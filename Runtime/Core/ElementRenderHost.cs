using System;
using System.Collections.Generic;

namespace UniDecl.Runtime.Core
{
    /// <summary>
    /// 渲染管理器共享基类
    /// 提供 DOM 构建、事件分发、上下文栈、状态栈等核心基础设施
    /// </summary>
    public abstract class ElementRenderHostBase : IElementRenderHostBase, IEventListener<ElementChangeEvent>, IEventListener<AutoRebuildRequestEvent>
    {
        private readonly EventDispatcher _eventDispatcher = new EventDispatcher();
        private readonly ContextStack _contextStack = new ContextStack();
        private readonly StateStack _stateStack = new StateStack();
        private readonly DOMTree _domTree = new DOMTree();
        private IStateManager _rootStateManager;
        private bool _initialized;
        private readonly HashSet<IElement> _pendingRebuilds = new HashSet<IElement>();
        private readonly HashSet<IElement> _rebuiltThisFlush = new HashSet<IElement>();
        private bool _flushScheduled;

        protected ContextStack ContextStack => _contextStack;
        protected StateStack StateStack => _stateStack;
        protected DOMTree DOMTree => _domTree;

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

            try
            {
                EnsureRootStateManager();
                BeginBuildFrame();
                PushRootState();

                _domTree.Build(element, this, GetRendererForBuild, _contextStack, _stateStack);
                OnBuildDOMComplete();
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
                _domTree.RebuildNode(@event.Element);
                _rebuiltThisFlush.Add(@event.Element);
                OnAfterRebuild(@event.Element);
            }
        }

        /// <summary>
        /// Rebuild 后回调，子类可 override 执行后置逻辑（如 VisualElement 同步）
        /// </summary>
        protected virtual void OnAfterRebuild(IElement element) { }

        // ==== 自动重建（延迟到帧尾）====

        void IEventListener<AutoRebuildRequestEvent>.OnEvent(AutoRebuildRequestEvent @event)
        {
            if (@event.Element == null) return;
            var node = _domTree.GetNode(@event.Element);
            if (node == null) return;
            var target = _domTree.FindRebuildTarget(node);
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
                    _domTree.RebuildNode(element);
                    OnAfterRebuild(element);
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
            var node = _domTree.GetNode(element);
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
                _eventDispatcher.Subscribe(this);
                _initialized = true;
            }
        }

        protected abstract void DiscoverRenderers();
        protected abstract IElementRender GetRendererForBuild(IElement element);
        protected virtual void OnRendererError(Exception ex, IElement element) { }
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
                if (node.HasRenderer && node.Element != null)
                {
                    try
                    {
                        node.Renderer.Render(node.Element, this, node.State);
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

        protected override IElementRender GetRendererForBuild(IElement element) => GetRenderer(element);

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
        private readonly Dictionary<DOMNode, TRenderResult> _renderCache = new();

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
        /// 渲染指定元素，返回 TRenderResult（供泛型渲染器回调，递归渲染子节点）
        /// diff 模式下：优先尝试 IElementUpdater 增量更新，失败则完全渲染
        /// </summary>
        public TRenderResult RenderElement(IElement element)
        {
            if (element == null) return default;

            var node = DOMTree.GetNode(element);
            if (node == null) return default;

            var pushedContext = PushNodeContext(node);
            try
            {
                var typedRenderer = GetTypedRenderer(element);
                if (typedRenderer != null)
                {
                    // 复用路径：查缓存 + 尝试 Update
                    if (TryGetCachedRender(node, out var cached))
                    {
                        if (typedRenderer is IElementUpdater<IElement, TRenderResult> updater
                            && updater.Update(element, cached, this, node.State))
                        {
                            return cached;
                        }
                        // Update 失败或渲染器不支持 Update，清除旧缓存
                        RemoveCachedRender(node);
                    }

                    // 完全渲染
                    var result = typedRenderer.Render(element, this, node.State);
                    OnElementRendered(element, result);
                    return result;
                }

                // 结构节点（无渲染器）：穿透到第一个 DOM 子节点
                if (node.Children != null && node.Children.Count > 0 && node.Children[0]?.Element != null)
                {
                    var result = RenderElement(node.Children[0].Element);
                    OnElementRendered(element, result);
                    return result;
                }

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

        protected override void OnBuildDOMComplete() => ClearRenderCache();

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

        protected override IElementRender GetRendererForBuild(IElement element) => null;

        protected override void ScheduleFlush() => FlushPendingRebuilds();

        /// <summary>
        /// RenderElement 返回渲染结果后回调，自动写入渲染缓存
        /// </summary>
        protected virtual void OnElementRendered(IElement element, TRenderResult result)
        {
            if (result != null)
            {
                var node = DOMTree.GetNode(element);
                if (node != null)
                    _renderCache[node] = result;
            }
        }

        /// <summary>
        /// 获取指定 DOMNode 的缓存渲染结果
        /// </summary>
        protected bool TryGetCachedRender(DOMNode node, out TRenderResult result)
            => _renderCache.TryGetValue(node, out result);

        /// <summary>
        /// 移除指定 DOMNode 的缓存渲染结果
        /// </summary>
        protected void RemoveCachedRender(DOMNode node) => _renderCache.Remove(node);

        /// <summary>
        /// 清除所有缓存渲染结果
        /// </summary>
        protected void ClearRenderCache() => _renderCache.Clear();

        /// <summary>
        /// 同步容器的 VE 子节点，根据 DOMNode 子列表变化增删 VE
        /// 供容器渲染器在 Update 中调用
        /// </summary>
        protected void SyncChildren<TContainer>(TContainer containerElement, Action<TRenderResult> add,
            Action<TRenderResult> remove) where TContainer : IContainerElement
        {
            var node = DOMTree.GetNode(containerElement);
            if (node == null) return;

            foreach (var childNode in node.Children)
            {
                if (childNode.Element == null) continue;
                var childVE = RenderElement(childNode.Element);
                if (childVE != null)
                    add(childVE);
            }
        }
    }
}
