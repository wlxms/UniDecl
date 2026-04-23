using System;
using System.Collections.Generic;
using System.Diagnostics;
using UniDecl.Runtime.Navigation;
using UnityEngine;

namespace UniDecl.Runtime.Core
{
    /// <summary>
    /// 渲染管理器共享基类
    /// 提供 DOM 构建、事件分发、上下文栈、状态栈等核心基础设施
    /// </summary>
    public abstract class ElementRenderHostBase : IElementRenderHostBase, IEventListener<ElementChangeEvent>, IEventListener<AutoRebuildRequestEvent>
    {
        protected EventDispatcher _eventDispatcher;
        private readonly ContextStack _contextStack = new ContextStack();
        private readonly StateStack _stateStack = new StateStack();
        private readonly DOMTree _domTree = new DOMTree();
        private IStateManager _rootStateManager;
        private bool _initialized;
        private readonly HashSet<IElement> _pendingRebuilds = new HashSet<IElement>();
        private readonly HashSet<IElement> _rebuiltThisFlush = new HashSet<IElement>();
        private bool _flushScheduled;

        /// <summary>
        /// 重建性能监控开关。开启后会在关键重建路径分发 RebuildPerformanceEvent。
        /// </summary>
        public bool EnableRebuildPerformanceMonitoring { get; set; }

        protected ContextStack ContextStack => _contextStack;
        protected StateStack StateStack => _stateStack;
        protected virtual DOMTree ActiveDOMTree => _domTree;
        protected DOMTree DOMTree => ActiveDOMTree;

        // ---- Navigation ----

        public virtual void NavigateTo(string anchorId) { }
        public virtual void NavigateURL(string url) { }

        protected ElementRenderHostBase()
        {
        }

        // ---- EventDispatcher factory ----

        protected virtual EventDispatcher CreateEventDispatcher() => new EventDispatcher();

        // ---- IEventDispatcher ----

        public void Dispatch<T>(T @event) where T : struct => _eventDispatcher.Dispatch(@event);

        public void Subscribe(IEventListener listener) => _eventDispatcher.Subscribe(listener);

        public void Unsubscribe(IEventListener listener) => _eventDispatcher.Unsubscribe(listener);

        // ---- IContextReader ----

        public T Get<T>() where T : class, IContextProvider => _contextStack.Get<T>();
        public bool TryGet<T>(out T value) where T : class, IContextProvider => _contextStack.TryGet(out value);
        public bool Has<T>() where T : class, IContextProvider => _contextStack.Has<T>();

        // ==== 第一阶段: BuildDOM ====

        public void BuildDOM(IElement element)
        {
            EnsureInitialized();
            if (element == null) return;

            long totalStart = 0;
            long buildStart = 0;
            long buildEnd = 0;
            long afterEnd = 0;

            if (EnableRebuildPerformanceMonitoring)
            {
                totalStart = Stopwatch.GetTimestamp();
            }

            try
            {
                EnsureRootStateManager();
                BeginBuildFrame();
                PushRootState();

                if (EnableRebuildPerformanceMonitoring)
                {
                    buildStart = Stopwatch.GetTimestamp();
                }

                ActiveDOMTree.Build(element, this, _contextStack, _stateStack);

                if (EnableRebuildPerformanceMonitoring)
                {
                    buildEnd = Stopwatch.GetTimestamp();
                }

                OnBuildDOMComplete();

                if (EnableRebuildPerformanceMonitoring)
                {
                    afterEnd = Stopwatch.GetTimestamp();

                    var evt = new RebuildPerformanceEvent
                    {
                        Element = element,
                        Trigger = RebuildTrigger.FullRebuild,
                        BeforeRebuildMs = 0,
                        DomRebuildMs = ElapsedMs(buildStart, buildEnd),
                        AfterRebuildMs = ElapsedMs(buildEnd, afterEnd),
                        TotalMs = ElapsedMs(totalStart, afterEnd),
                    };
                    OnRebuildPerformanceMeasured(evt);
                }
            }
            finally
            {
                PopRootState();
                EndBuildFrame();
            }
        }

        /// <summary>
        /// BuildDOM 完成后回调，子类可 override 执行清理逻辑
        /// </summary>
        protected virtual void OnBuildDOMComplete() { }

        // ==== Context 管理（Render 阶段使用）====

        /// <summary>
        /// 推入节点的 Context（Render 阶段由子类调用）
        /// </summary>
        protected bool PushNodeContext(DOMNode node)
        {
            if (node?.ContextToPush == null) return false;
            _contextStack.Push(node.ContextToPush);
            return true;
        }

        /// <summary>
        /// 弹出节点的 Context（Render 阶段由子类调用）
        /// </summary>
        protected void PopNodeContext(DOMNode node, bool wasPushed)
        {
            if (wasPushed)
                _contextStack.Pop(node.ContextType);
        }

        // ==== 事件处理 ====

        void IEventListener<ElementChangeEvent>.OnEvent(ElementChangeEvent @event)
        {
            if (@event.Element != null)
            {
                RebuildWithProfiling(@event.Element, RebuildTrigger.Immediate);
                _rebuiltThisFlush.Add(@event.Element);
            }
        }

        protected virtual void OnBeforeRebuild(IElement element) { }

        /// <summary>
        /// Rebuild 后回调，子类可 override 执行后置逻辑（如 VisualElement 同步）
        /// </summary>
        protected virtual void OnAfterRebuild(IElement element) { }

        // ==== 自动重建（延迟到帧尾）====

        void IEventListener<AutoRebuildRequestEvent>.OnEvent(AutoRebuildRequestEvent @event)
        {
            if (@event.Element == null) return;
            var node = ActiveDOMTree.GetNode(@event.Element);
            if (node == null) return;
            var target = ActiveDOMTree.FindRebuildTarget(node);
            if (target == null) return;

            _pendingRebuilds.Add(target);
            if (!_flushScheduled)
            {
                _flushScheduled = true;
                ScheduleFlush();
            }
        }

        /// <summary>
        /// 执行所有待处理的延迟重建
        /// </summary>
        protected void FlushPendingRebuilds()
        {
            _flushScheduled = false;
            foreach (var element in _pendingRebuilds)
            {
                if (!_rebuiltThisFlush.Contains(element))
                {
                    RebuildWithProfiling(element, RebuildTrigger.DeferredFlush);
                }
            }
            _pendingRebuilds.Clear();
            _rebuiltThisFlush.Clear();
        }

        /// <summary>
        /// 强制立即执行所有待处理的延迟重建
        /// </summary>
        public void ForceFlush()
        {
            _flushScheduled = false;
            FlushPendingRebuilds();
        }

        /// <summary>
        /// 调度帧尾执行 FlushPendingRebuilds，子类根据后端实现
        /// </summary>
        protected abstract void ScheduleFlush();

        // ==== State 查询 ====

        public object GetElementState(IElement element)
        {
            var node = ActiveDOMTree.GetNode(element);
            return node?.State?.Value;
        }

        // ==== 帧管理 ====

        private void EnsureRootStateManager()
        {
            if (_rootStateManager == null)
                _rootStateManager = new StateManager();
        }

        private void PushRootState() => _stateStack.Push(_rootStateManager);
        private void PopRootState() => _stateStack.Pop();
        private void BeginBuildFrame() => _rootStateManager?.MarkAllUnused();

        private void EndBuildFrame()
        {
            _rootStateManager?.ClearUnused();
            _stateStack.Clear();
        }

        private void EnsureInitialized()
        {
            if (!_initialized)
            {
                DiscoverRenderers();
                _eventDispatcher = CreateEventDispatcher();
                _eventDispatcher.Subscribe(this);
                _initialized = true;
            }
        }

        protected abstract void DiscoverRenderers();
        protected virtual void OnRendererError(Exception ex, IElement element) { }

        protected virtual void OnRebuildPerformanceMeasured(RebuildPerformanceEvent @event)
        {
            Dispatch(@event);
        }

        private void RebuildWithProfiling(IElement element, RebuildTrigger trigger)
        {
            if (element == null) return;

            if (!EnableRebuildPerformanceMonitoring)
            {
                OnBeforeRebuild(element);
                ActiveDOMTree.RebuildNode(element);
                OnAfterRebuild(element);
                return;
            }

            var t0 = Stopwatch.GetTimestamp();
            OnBeforeRebuild(element);
            var t1 = Stopwatch.GetTimestamp();
            ActiveDOMTree.RebuildNode(element);
            var t2 = Stopwatch.GetTimestamp();
            OnAfterRebuild(element);
            var t3 = Stopwatch.GetTimestamp();

            var evt = new RebuildPerformanceEvent
            {
                Element = element,
                Trigger = trigger,
                BeforeRebuildMs = ElapsedMs(t0, t1),
                DomRebuildMs = ElapsedMs(t1, t2),
                AfterRebuildMs = ElapsedMs(t2, t3),
                TotalMs = ElapsedMs(t0, t3),
            };
            OnRebuildPerformanceMeasured(evt);
        }

        private static double ElapsedMs(long start, long end)
        {
            return (end - start) * 1000.0 / Stopwatch.Frequency;
        }
    }

    /// <summary>
    /// UniDecl 非泛型渲染管理器
    /// 用于 IMGUI 等不需要返回渲染结果的后端
    /// Render 阶段由框架遍历 DOM 树，逐节点调用渲染器
    /// </summary>
    public abstract class ElementRenderHost : ElementRenderHostBase, IElementRenderHost
    {
        private readonly Dictionary<Type, IElementRender> _renderers = new Dictionary<Type, IElementRender>();

        // ---- 渲染器注册 ----

        public void RegisterRenderer<T>(IElementRenderer<T> renderer) where T : IElement
        {
            _renderers[typeof(T)] = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        public IElementRender GetRenderer(IElement element)
        {
            if (element == null) return null;

            var elementType = element.GetType();
            if (_renderers.TryGetValue(elementType, out var renderer))
                return renderer;

            if (elementType.IsGenericType)
            {
                var genericDef = elementType.GetGenericTypeDefinition();
                if (_renderers.TryGetValue(genericDef, out renderer))
                    return renderer;
            }

            return null;
        }

        // ==== 第二阶段: Render（DOM 树遍历）====

        public void Render()
        {
            if (DOMTree.Root == null) return;
            try
            {
                RenderNode(DOMTree.Root);
            }
            finally
            {
                ContextStack.Clear();
            }
        }

        private void RenderNode(DOMNode node)
        {
            if (node == null) return;

            var pushedContext = PushNodeContext(node);

            try
            {
                var renderer = node.Element != null ? GetRenderer(node.Element) : null;
                if (renderer != null && node.Element != null)
                {
                    try
                    {
                        renderer.Render(node.Element, this, node.State);
                    }
                    catch (Exception ex)
                    {
                        OnRendererError(ex, node.Element);
                    }
                }
                else
                {
                    // 无 Renderer 的结构节点：框架自动遍历子节点
                    if (node.Children != null)
                    {
                        for (int i = 0; i < node.Children.Count; i++)
                        {
                            try
                            {
                                RenderNode(node.Children[i]);
                            }
                            catch (Exception ex)
                            {
                                OnRendererError(ex, node.Children[i]?.Element);
                            }
                        }
                    }
                }
            }
            finally
            {
                PopNodeContext(node, pushedContext);
            }
        }

        /// <summary>
        /// 渲染指定元素（供渲染器回调框架渲染子节点）
        /// </summary>
        public void RenderElement(IElement element)
        {
            if (element == null) return;
            var node = DOMTree.GetNode(element);
            if (node != null)
                RenderNode(node);
        }

        protected override void ScheduleFlush() => FlushPendingRebuilds();
    }

    /// <summary>
    /// UniDecl 泛型渲染管理器
    /// 用于 UI Toolkit 等需要返回渲染结果的后端
    /// 渲染器通过 RenderElement(child) 获取子节点的 TRenderResult，自行组装
    /// Render() 仅调用根节点渲染器，由渲染器负责递归渲染子节点
    /// </summary>
    public abstract class ElementRenderHost<TRenderResult> : ElementRenderHostBase, IElementRenderHost<TRenderResult>
    {
        private readonly Dictionary<Type, IElementRender<TRenderResult>> _typedRenderers =
            new Dictionary<Type, IElementRender<TRenderResult>>();
        private readonly DOMTree<TRenderResult> _typedDomTree = new DOMTree<TRenderResult>();
        private readonly Dictionary<IElement, (bool hasValue, TRenderResult value)> _preRebuildResults = new();

        protected override DOMTree ActiveDOMTree => _typedDomTree;

        protected override EventDispatcher CreateEventDispatcher() => new EventDispatcher<TRenderResult>(GetTypedRenderer);

        /// <summary>
        /// 注册泛型渲染器
        /// </summary>
        public void RegisterRenderer<T>(IElementRenderer<T, TRenderResult> renderer) where T : IElement
        {
            _typedRenderers[typeof(T)] = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        /// <summary>
        /// 获取泛型渲染器
        /// </summary>
        public IElementRender<TRenderResult> GetTypedRenderer(IElement element)
        {
            if (element == null) return null;

            var elementType = element.GetType();
            if (_typedRenderers.TryGetValue(elementType, out var renderer))
                return renderer;

            if (elementType.IsGenericType)
            {
                var genericDef = elementType.GetGenericTypeDefinition();
                if (_typedRenderers.TryGetValue(genericDef, out renderer))
                    return renderer;
            }

            return null;
        }

        /// <summary>
        /// 根据锚点 ID 查找泛型 DOM 节点
        /// </summary>
        protected DOMNode<TRenderResult> GetTypedNodeByAnchor(string anchorId)
        {
            return _typedDomTree.GetNodeByAnchor(anchorId);
        }

        /// <summary>
        /// 渲染指定元素，返回 TRenderResult（供泛型渲染器回调，递归渲染子节点）
        /// diff 模式下：优先尝试 IElementUpdater.TryUpdate 增量更新，失败则完全渲染
        /// </summary>
        public TRenderResult RenderElement(IElement element)
        {
            if (element == null) return default;

            var node = _typedDomTree.GetNode(element);
            if (node == null) return default;

            var pushedContext = PushNodeContext(node);
            try
            {
                var typedRenderer = GetTypedRenderer(element);
                if (typedRenderer != null)
                {
                    // 复用路径：节点缓存 + 尝试 TryUpdate
                    if (node.HasRenderResult
                        && typedRenderer is IElementUpdater<TRenderResult> updater
                        && updater.TryUpdate(element, node.RenderResult, this, node.State))
                    {
                        return node.RenderResult;
                    }

                    // 完全渲染
                    var result = typedRenderer.Render(element, this, node.State);
                    node.RenderResult = result;
                    return result;
                }

                // 结构节点（无渲染器）：穿透到第一个 DOM 子节点
                if (node.Children != null && node.Children.Count > 0 && node.Children[0]?.Element != null)
                {
                    var result = RenderElement(node.Children[0].Element);
                    node.RenderResult = result;
                    return result;
                }

                node.ClearRenderResult();
                return default;
            }
            catch (Exception ex)
            {
                OnRendererError(ex, element);
                return default;
            }
            finally
            {
                PopNodeContext(node, pushedContext);
            }
        }

        protected override void OnBuildDOMComplete() { }

        /// <summary>
        /// 基于 DOM 树进行实际渲染，返回根节点的渲染结果
        /// 仅调用根节点渲染器，由渲染器负责通过 RenderElement(child) 递归渲染子节点
        /// </summary>
        public TRenderResult Render()
        {
            var root = DOMTree.Root;
            if (root == null || root.Element == null) return default;
            try
            {
                return RenderElement(root.Element);
            }
            finally
            {
                ContextStack.Clear();
            }
        }

        protected override void ScheduleFlush() => FlushPendingRebuilds();

        protected override void OnBeforeRebuild(IElement element)
        {
            if (element == null) return;

            var node = _typedDomTree.GetNode(element);
            if (node != null && node.HasRenderResult)
            {
                _preRebuildResults[element] = (true, node.RenderResult);
                return;
            }

            _preRebuildResults[element] = (false, default);
        }

        protected override void OnAfterRebuild(IElement element)
        {
            if (element == null) return;

            _preRebuildResults.TryGetValue(element, out var old);
            _preRebuildResults.Remove(element);

            var newResult = RenderElement(element);
            var newNode = _typedDomTree.GetNode(element);
            var hasNew = newNode != null && newNode.HasRenderResult;

            if (old.hasValue && hasNew && !EqualityComparer<TRenderResult>.Default.Equals(old.value, newResult))
                OnRenderResultChanged(element, old.value, newResult);
        }

        protected virtual void OnRenderResultChanged(IElement element, TRenderResult oldResult, TRenderResult newResult) { }

        /// <summary>
        /// 同步容器的 VE 子节点，根据 DOMNode 子列表变化增删 VE
        /// 供容器渲染器在 Update 中调用
        /// </summary>
        protected void SyncChildren<TContainer>(TContainer containerElement, Action<TRenderResult> add,
            Action<TRenderResult> remove) where TContainer : IContainerElement
        {
            var node = _typedDomTree.GetNode(containerElement);
            if (node == null) return;

            foreach (var childNode in node.Children)
            {
                if (childNode.Element == null) continue;
                var childVE = RenderElement(childNode.Element);
                if (childVE != null)
                    add(childVE);
            }
        }

        // ---- Navigation ----

        private List<DOMNode<TRenderResult>> CollectPathToRoot(DOMNode<TRenderResult> target)
        {
            var path = new List<DOMNode<TRenderResult>>();
            var current = target;
            while (current != null)
            {
                path.Add(current);
                current = current.Parent as DOMNode<TRenderResult>;
            }
            path.Reverse();
            return path;
        }

        public override void NavigateTo(string anchorId)
        {
            var targetNode = _typedDomTree.GetNodeByAnchor(anchorId);
            if (targetNode == null) return;

            var path = CollectPathToRoot(targetNode);
            var dispatcher = _eventDispatcher as EventDispatcher<TRenderResult>;
            if (dispatcher == null) return;

            // 找到锚点节点下第一个有渲染器的后代作为目标
            var renderableTarget = FindFirstRenderable(targetNode) ?? targetNode;

            // 沿路径分发冒泡事件（祖先可据此展开/滚动）
            dispatcher.DispatchAlongPath(
                new NavigationEvent { AnchorId = anchorId, IsTarget = false }, path, _typedDomTree);

            // 目标节点收到 IsTarget=true 事件（高亮等）
            dispatcher.DispatchAlongPath(
                new NavigationEvent { AnchorId = anchorId, IsTarget = true },
                new[] { renderableTarget }, _typedDomTree);
        }

        /// <summary>
        /// 向下查找第一个有渲染器的后代节点（容器节点如 H1 会穿透到子 Label）
        /// </summary>
        private DOMNode<TRenderResult> FindFirstRenderable(DOMNode<TRenderResult> node)
        {
            if (node.Element != null && GetTypedRenderer(node.Element) != null)
                return node;
            if (node.Children != null)
                foreach (var child in node.Children)
                    if (child is DOMNode<TRenderResult> typedChild)
                    {
                        var found = FindFirstRenderable(typedChild);
                        if (found != null) return found;
                    }
            return null;
        }

        public override void NavigateURL(string url)
        {
            var parsed = NavigationURL.Parse(url);
            if (parsed?.Fragment == null) return;
            NavigateTo(parsed.Fragment);
        }
    }
}
