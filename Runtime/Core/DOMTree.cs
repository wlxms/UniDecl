using System;
using System.Collections.Generic;
using UniDecl.Runtime.Navigation;
using UnityEngine;

namespace UniDecl.Runtime.Core
{
    /// <summary>
    /// DOM 树
    /// 负责将声明式元素树展开为 DOM 节点树，供 Render 阶段遍历渲染
    /// </summary>
    public class DOMTree
    {
        private readonly List<DOMNode> _allNodes = new();
        private readonly Dictionary<IElement, DOMNode> _elementToNode = new();
        private readonly Dictionary<IElement, IStateManager> _containerStateManagers = new();
        private readonly Dictionary<string, DOMNode> _anchorToNode = new();
        
        private IElementRenderHostBase _manager;
        private ContextStack _contextStack;
        private StateStack _stateStack;

        /// <summary>
        /// 根节点
        /// </summary>
        public DOMNode Root { get; private set; }

        /// <summary>
        /// 所有节点的扁平列表
        /// </summary>
        public IReadOnlyList<DOMNode> AllNodes => _allNodes;

        /// <summary>
        /// 根据锚点 ID 查找 DOM 节点
        /// </summary>
        public DOMNode GetNodeByAnchor(string anchorId)
        {
            if (string.IsNullOrEmpty(anchorId)) return null;
            _anchorToNode.TryGetValue(anchorId, out var node);
            return node;
        }

        private void SubscribeNodeListeners(DOMNode node)
        {
            if (node.Element is IEventListener listener)
                _manager.Subscribe(listener);
            if (node.Element is Element e)
                foreach (var comp in e.Components)
                    if (comp is IEventListener compListener)
                        _manager.Subscribe(compListener);
        }

        private void UnsubscribeNodeListeners(DOMNode node)
        {
            if (node.Element is IEventListener listener)
                _manager.Unsubscribe(listener);
            if (node.Element is Element e)
                foreach (var comp in e.Components)
                    if (comp is IEventListener compListener)
                        _manager.Unsubscribe(compListener);
        }

        private void RegisterAnchor(DOMNode node)
        {
            if (node.Element is Element e && e.Get<Anchor>() is { } anchor)
            {
                if (_anchorToNode.ContainsKey(anchor.Id))
                    throw new InvalidOperationException($"重复的 Anchor ID: '{anchor.Id}'");
                _anchorToNode[anchor.Id] = node;
            }
        }

        private void UnregisterAnchor(DOMNode node)
        {
            if (node.Element is Element e && e.Get<Anchor>() is { } anchor)
                _anchorToNode.Remove(anchor.Id);
        }

        private void UnregisterAnchorSubtree(DOMNode node)
        {
            UnregisterAnchor(node);
            foreach (var child in node.Children)
                UnregisterAnchorSubtree(child);
        }

        /// <summary>
        /// 展开元素树为 DOM 树
        /// </summary>
        public void Build(IElement element, IElementRenderHostBase manager,
            ContextStack contextStack, StateStack stateStack)
        {
            _manager = manager;
            _contextStack = contextStack;
            _stateStack = stateStack;

            Clear();
            if (element == null) return;

            Root = CreateRootNode();
            BuildElement(element, Root);
        }

        protected virtual DOMNode CreateRootNode()
        {
            return new DOMNode();
        }

        protected virtual DOMNode CreateDOMNode()
        {
            return new DOMNode();
        }

        // ==== 核心构建逻辑 ====

        /// <summary>
        /// 构建单个元素及其子节点
        /// </summary>
        private void BuildElement(IElement element, DOMNode parent)
        {
            if (element == null) return;

            // 重复检测：同一元素不能在树中出现多次（同时防止循环引用 A→B→A）
            if (_elementToNode.ContainsKey(element))
                throw new InvalidOperationException(
                    $"{element.GetType().Name} 已在 DOM 树中存在。" +
                    "同一元素实例不能在多个位置被引用，也不能形成循环引用。");

            // 互斥检查：三个特殊接口不能同时实现任意两个
            int specialCount = 0;
            if (element is IContextProvider) specialCount++;
            if (element is IContextConsumer) specialCount++;
            if (element is IContainerElement) specialCount++;
            if (specialCount > 1)
                throw new InvalidOperationException(
                    $"{element.GetType().Name} 同时实现了多个互斥接口（ContextProvider/ContextConsumer/ContainerElement）。" +
                    "请使用嵌套方式组合，例如 new Container() {{ new Provider() {{ child }} }}");

            var node = CreateNode(element, parent);
            ProcessElement(element, node);
        }

        /// <summary>
        /// 处理元素的初始化、状态恢复、子树构建和状态写回（BuildElement 和 RebuildNode 共用）
        /// </summary>
        private void ProcessElement(IElement element, DOMNode node)
        {
            element.Initialize(node.Parent.Children.Count - 1, _manager);

            if (element is IStatefulElement sf)
            {
                var elementState = GetOrCreateElementState(element);
                RestoreOrBuildState(sf, elementState);
                node.State = elementState;
            }

            switch (element)
            {
                case IContextProvider contextProvider:
                    BuildContextProvider(contextProvider, node);
                    break;
                case IContextConsumer contextConsumer:
                    BuildContextConsumer(contextConsumer, node);
                    break;
                case IContainerElement containerElement:
                    BuildContainerElement(containerElement, node);
                    break;
                default:
                    BuildNormalElement(element, node);
                    break;
            }
        }

        /// <summary>
        /// 构建上下文提供者
        /// </summary>
        private void BuildContextProvider(IContextProvider context, DOMNode node)
        {
            node.ContextToPush = context;
            node.ContextType = context.GetType();
            
            _contextStack.Push(context);
            try
            {
                BuildElement(context.Child, node);
            }
            finally
            {
                _contextStack.Pop(node.ContextType);
            }
        }

        /// <summary>
        /// 构建上下文消费者
        /// </summary>
        private void BuildContextConsumer(IContextConsumer consumer, DOMNode node)
        {
            if (consumer.ContextRender == null) return;

            try
            {
                var rendered = consumer.ContextRender(_contextStack);
                if (rendered != null)
                {
                    BuildElement(rendered, node);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DOMTree] ContextRender failed for {consumer.GetType().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// 构建容器元素
        /// </summary>
        private void BuildContainerElement(IContainerElement container, DOMNode node)
        {
            _stateStack.Push(GetOrCreateContainerStateManager(container));
            try
            {
                // Render() 优先：如果返回非自身元素，使用返回值展开
                var rendered = container.Render();
                if (rendered != null && rendered != container)
                {
                    BuildElement(rendered, node);
                    return;
                }

                // 否则展开 Children 列表
                if (container.Children != null)
                {
                    foreach (var child in container.Children)
                    {
                        BuildElement(child, node);
                    }
                }
            }
            finally
            {
                _stateStack.Pop();
            }
        }

        /// <summary>
        /// 构建普通元素
        /// </summary>
        private void BuildNormalElement(IElement element, DOMNode node)
        {
            var rendered = element.Render();
            if (rendered != null && rendered != element)
            {
                BuildElement(rendered, node);
            }
        }

        // ==== 节点创建 ====

        /// <summary>
        /// 创建节点并添加到父节点
        /// </summary>
        protected virtual DOMNode CreateNode(IElement element, DOMNode parent)
        {
            var node = CreateDOMNode();
            node.Element = element;
            node.Parent = parent;

            parent.Children.Add(node);
            _allNodes.Add(node);

            if (element != null)
            {
                _elementToNode[element] = node;
                SubscribeNodeListeners(node);
                RegisterAnchor(node);
            }

            return node;
        }

        /// <summary>
        /// 根据元素实例查找对应的 DOM 节点
        /// </summary>
        public DOMNode GetNode(IElement element)
        {
            if (element == null) return null;
            _elementToNode.TryGetValue(element, out var node);
            return node;
        }

        // ==== 状态管理 ====

        private ElementState GetOrCreateElementState(IElement element)
        {
            if (string.IsNullOrEmpty(element.Key) || _stateStack.IsEmpty)
                return null;
            return _stateStack.Current.GetOrCreateState(element.Key, () => new ElementState());
        }

        private void RestoreOrBuildState(IStatefulElement sf, ElementState es)
        {
            if (es == null) return;
            es.Value ??= sf.BuildState();
        }

        private IStateManager GetOrCreateContainerStateManager(IContainerElement container)
        {
            if (_containerStateManagers.TryGetValue(container, out var existing))
                return existing;
            var manager = new StateManager();
            _containerStateManagers[container] = manager;
            return manager;
        }

        // ==== 重建与清理 ====

        /// <summary>
        /// 局部重建指定元素的子节点
        /// </summary>
        public void RebuildNode(IElement element)
        {
            if (element == null) return;
            var node = GetNode(element);
            if (node == null) return;

            // 互斥检查：确保重建的元素仍符合接口规则
            int specialCount = 0;
            if (element is IContextProvider) specialCount++;
            if (element is IContextConsumer) specialCount++;
            if (element is IContainerElement) specialCount++;
            if (specialCount > 1)
                throw new InvalidOperationException(
                    $"{element.GetType().Name} 同时实现了多个互斥接口（ContextProvider/ContextConsumer/ContainerElement）。");

            var contextChain = CollectContextChain(node);
            var stateChain = CollectStateChain(node);

            PushContextChain(contextChain);
            PushStateChain(stateChain);

            try
            {
                // 初始化元素（分配 Key）
                element.Initialize(node.Parent.Children.Count - 1, _manager);

                // 状态恢复
                if (element is IStatefulElement sf)
                {
                    var elementState = GetOrCreateElementState(element);
                    RestoreOrBuildState(sf, elementState);
                    node.State = elementState;
                }

                // 获取新子元素并 diff
                var newChildren = GetChildren(element);
                DiffChildren(node, newChildren);
            }
            finally
            {
                PopStateChain(stateChain);
                PopContextChain(contextChain);
            }
        }

        /// <summary>
        /// 沿父链向上收集所有 ContextProvider 节点的上下文信息
        /// </summary>
        private List<(IContextProvider context, Type type)> CollectContextChain(DOMNode node)
        {
            var chain = new List<(IContextProvider, Type)>();
            var current = node.Parent;
            while (current != null)
            {
                if (current.ContextToPush != null)
                    chain.Add((current.ContextToPush, current.ContextType));
                current = current.Parent;
            }
            chain.Reverse(); // 从根到当前节点顺序
            return chain;
        }

        /// <summary>
        /// 沿父链向上收集所有 ContainerElement 节点
        /// </summary>
        private List<IContainerElement> CollectStateChain(DOMNode node)
        {
            var chain = new List<IContainerElement>();
            var current = node.Parent;
            while (current != null)
            {
                if (current.Element is IContainerElement container)
                    chain.Add(container);
                current = current.Parent;
            }
            chain.Reverse(); // 从根到当前节点顺序
            return chain;
        }

        private void PushContextChain(List<(IContextProvider context, Type type)> chain)
        {
            foreach (var (context, type) in chain)
                _contextStack.Push(context);
        }

        private void PopContextChain(List<(IContextProvider context, Type type)> chain)
        {
            for (int i = chain.Count - 1; i >= 0; i--)
                _contextStack.Pop(chain[i].type);
        }

        private void PushStateChain(List<IContainerElement> chain)
        {
            foreach (var container in chain)
                _stateStack.Push(GetOrCreateContainerStateManager(container));
        }

        private void PopStateChain(List<IContainerElement> chain)
        {
            for (int i = 0; i < chain.Count; i++)
                _stateStack.Pop();
        }

        /// <summary>
        /// 沿父链向上查找自动重建的目标元素
        /// 跳过结构性节点（IContextProvider / IContextConsumer / IContainerElement），返回第一个非结构性节点
        /// </summary>
        public IElement FindRebuildTarget(DOMNode node)
        {
            var current = node.Parent;
            while (current != null)
            {
                if (current.Element == null) { current = current.Parent; continue; }

                bool isStructural = current.Element is IContextProvider
                    || current.Element is IContextConsumer
                    || current.Element is IContainerElement;

                if (!isStructural)
                    return current.Element;

                current = current.Parent;
            }
            return Root?.Element;
        }

        /// <summary>
        /// 递归清理子节点并取消事件订阅
        /// </summary>
        private void ClearChildren(DOMNode node)
        {
            if (node.Children == null || node.Children.Count == 0) return;

            var nodesToRemove = new List<DOMNode>();
            CollectDescendants(node, nodesToRemove);

            foreach (var n in nodesToRemove)
            {
                UnsubscribeNodeListeners(n);
                UnregisterAnchor(n);
                if (n.Element != null)
                {
                    _elementToNode.Remove(n.Element);
                    if (n.Element is IContainerElement container)
                        _containerStateManagers.Remove(container);
                }
                n.Parent = null;
                n.Children.Clear();
            }

            var removeSet = new HashSet<DOMNode>(nodesToRemove);
            _allNodes.RemoveAll(n => removeSet.Contains(n));
            node.Children.Clear();
        }

        // ==== Diff 重建 ====

        /// <summary>
        /// 判断元素是否为结构性节点（ContextProvider / ContextConsumer / ContainerElement）
        /// 结构性节点的子树不做 diff，整体替换
        /// </summary>
        private static bool IsStructural(IElement element)
        {
            return element is IContextProvider || element is IContextConsumer || element is IContainerElement;
        }

        /// <summary>
        /// 从旧子节点构建局部 Key→Node 映射（兄弟作用域）
        /// </summary>
        private static Dictionary<string, DOMNode> BuildKeyToNodeMap(List<DOMNode> oldChildren)
        {
            var map = new Dictionary<string, DOMNode>();
            for (int i = 0; i < oldChildren.Count; i++)
            {
                var child = oldChildren[i];
                if (child.Element != null && !string.IsNullOrEmpty(child.Element.Key))
                    map[child.Element.Key] = child;
            }
            return map;
        }

        /// <summary>
        /// 对父节点的子列表执行 diff，根据匹配结果复用/创建/移除 DOMNode
        /// </summary>
        private void DiffChildren(DOMNode parentNode, List<IElement> newElements)
        {
            var oldChildren = new List<DOMNode>(parentNode.Children);
            parentNode.Children.Clear();

            var keyMap = BuildKeyToNodeMap(oldChildren);
            var usedOldNodes = new HashSet<DOMNode>();
            var newPositionalIndex = 0;

            foreach (var newElement in newElements)
            {
                if (newElement == null) continue;

                DOMNode matched = null;

                // 策略1: Key 查找
                if (!string.IsNullOrEmpty(newElement.Key) && keyMap.TryGetValue(newElement.Key, out var keyMatch))
                {
                    matched = keyMatch;
                }

                // 策略2: 位置回退
                if (matched == null && newPositionalIndex < oldChildren.Count)
                {
                    var positionalCandidate = oldChildren[newPositionalIndex];
                    if (positionalCandidate != null && !usedOldNodes.Contains(positionalCandidate))
                        matched = positionalCandidate;
                }

                newPositionalIndex++;

                if (matched != null && !usedOldNodes.Contains(matched))
                {
                    usedOldNodes.Add(matched);
                    var matchedType = matched.Element?.GetType();
                    var newType = newElement.GetType();

                    if (matchedType == newType)
                    {
                        // 类型相同 → 复用 DOMNode，递归 diff
                        ReuseNode(matched, newElement);
                        parentNode.Children.Add(matched);
                    }
                    else
                    {
                        // 类型不同 → 替换
                        RemoveSubtree(matched);
                        BuildNewNode(newElement, parentNode);
                    }
                }
                else
                {
                    // 未匹配 → 新建
                    BuildNewNode(newElement, parentNode);
                }
            }

            // 移除未匹配的旧节点
            foreach (var oldChild in oldChildren)
            {
                if (!usedOldNodes.Contains(oldChild))
                    RemoveSubtree(oldChild);
            }
        }

        /// <summary>
        /// 复用已有 DOMNode：更新 Element 引用、恢复状态、递归 diff 子节点
        /// </summary>
        private void ReuseNode(DOMNode node, IElement newElement)
        {
            var oldElement = node.Element;

            // 更新 _elementToNode 映射
            if (oldElement != null)
            {
                _elementToNode.Remove(oldElement);
                UnsubscribeNodeListeners(node);
                UnregisterAnchor(node);
            }

            node.Element = newElement;

            if (newElement != null)
            {
                // 设置 _manager（确保后续 GetChildren 中 Render() 调用不会 NPE）
                // Key 不会被覆盖（Initialize 检测到 Key 非空时不重新生成）
                newElement.Initialize(node.Parent.Children.Count - 1, _manager);

                _elementToNode[newElement] = node;
                SubscribeNodeListeners(node);
                RegisterAnchor(node);

                // 状态恢复
                if (newElement is IStatefulElement sf)
                {
                    var elementState = GetOrCreateElementState(newElement);
                    RestoreOrBuildState(sf, elementState);
                    node.State = elementState;
                }

                // 递归 diff 子节点
                var newChildren = GetChildren(newElement);
                DiffChildren(node, newChildren);
            }
        }

        /// <summary>
        /// 创建新 DOMNode 并执行 ProcessElement
        /// </summary>
        private void BuildNewNode(IElement element, DOMNode parent)
        {
            var node = CreateNode(element, parent);
            ProcessElement(element, node);
        }

        /// <summary>
        /// 移除单个节点及其子树，清理映射和事件订阅
        /// </summary>
        private void RemoveSubtree(DOMNode node)
        {
            // 递归清理子节点
            CollectAndCleanupDescendants(node);

            // 清理节点本身
            UnsubscribeNodeListeners(node);
            UnregisterAnchor(node);
            if (node.Element != null)
            {
                _elementToNode.Remove(node.Element);
                if (node.Element is IContainerElement container)
                    _containerStateManagers.Remove(container);
            }

            // 从父节点和全局列表中移除
            if (node.Parent != null)
                node.Parent.Children.Remove(node);
            _allNodes.Remove(node);

            node.Parent = null;
            node.Children.Clear();
        }

        private void CollectAndCleanupDescendants(DOMNode node)
        {
            for (int i = node.Children.Count - 1; i >= 0; i--)
            {
                CollectAndCleanupDescendants(node.Children[i]);
            }

            foreach (var child in node.Children)
            {
                UnsubscribeNodeListeners(child);
                UnregisterAnchor(child);
                if (child.Element != null)
                {
                    _elementToNode.Remove(child.Element);
                    if (child.Element is IContainerElement container)
                        _containerStateManagers.Remove(container);
                }
                _allNodes.Remove(child);
            }
        }

        /// <summary>
        /// 获取元素的子元素列表
        /// </summary>
        private static List<IElement> GetChildren(IElement element)
        {
            switch (element)
            {
                case IContainerElement container:
                    var rendered = container.Render();
                    if (rendered != null && rendered != container)
                        return new List<IElement> { rendered };
                    return container.Children != null
                        ? new List<IElement>(container.Children)
                        : new List<IElement>();
                case IContextProvider provider:
                    return provider.Child != null
                        ? new List<IElement> { provider.Child }
                        : new List<IElement>();
                default:
                    var normalRendered = element.Render();
                    return normalRendered != null && normalRendered != element
                        ? new List<IElement> { normalRendered }
                        : new List<IElement>();
            }
        }

        private void CollectDescendants(DOMNode node, List<DOMNode> result)
        {
            foreach (var child in node.Children)
            {
                CollectDescendants(child, result);
                result.Add(child);
            }
        }

        /// <summary>
        /// 清空树
        /// </summary>
        public void Clear()
        {
            foreach (var node in _allNodes)
            {
                UnsubscribeNodeListeners(node);
                node.Parent = null;
                node.Children.Clear();
            }

            Root = null;
            _allNodes.Clear();
            _elementToNode.Clear();
            _containerStateManagers.Clear();
            _anchorToNode.Clear();
        }
    }

    public class DOMTree<TRenderResult> : DOMTree
    {
        protected override DOMNode CreateRootNode()
        {
            return new DOMNode<TRenderResult>();
        }

        protected override DOMNode CreateDOMNode()
        {
            return new DOMNode<TRenderResult>();
        }

        public new DOMNode<TRenderResult> GetNode(IElement element)
        {
            return base.GetNode(element) as DOMNode<TRenderResult>;
        }

        public new DOMNode<TRenderResult> GetNodeByAnchor(string anchorId)
        {
            return base.GetNodeByAnchor(anchorId) as DOMNode<TRenderResult>;
        }
    }
}
