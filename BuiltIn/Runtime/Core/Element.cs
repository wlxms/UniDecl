using System;
using System.Collections;
using System.Collections.Generic;
using UniDecl.BuiltIn.Runtime.Snapshot;

namespace UniDecl.BuiltIn.Runtime.Core
{
    public abstract class Element : IElement
    {
        protected IElementRenderHostBase _manager;
        private readonly Dictionary<Type, IElementComponent> _components = new Dictionary<Type, IElementComponent>();
        private bool _disposed;
        public string Key { get; private set; }
        public abstract IElement Render();

        public IEnumerable<IElementComponent> Components => _components.Values;

        public Element WithKey(string key)
        {
            Key = key;
            return this;
        }
        public Element() { }
        protected Element(params IElementComponent[] components)
        {
            if (components != null)
                foreach (var c in components)
                    _components[c.GetType()] = c;
        }
        public void Initialize(int index, IElementRenderHostBase manager)
        {
            _manager = manager;
            Key = string.IsNullOrEmpty(Key) ? $"__{GetType().Name}_{index}__" : Key;
        }

        public void Rebuild()
        {
            _manager.Dispatch(new ElementChangeEvent { Element = this });
        }

        public void NotifyChanged()
        {
            _manager?.Dispatch(new AutoRebuildRequestEvent { Element = this });
        }

        public IElement With<T>(T component) where T : IElementComponent
        {
            _components[typeof(T)] = component;
            return this;
        }



        public T Get<T>() where T : IElementComponent
        {
            if (_components.TryGetValue(typeof(T), out var component))
            {
                return (T)component;
            }
            return default;
        }

        /// <summary>
        /// 释放元素资源——由 DOMTree 在移除节点时调用。
        /// 幂等：多次调用安全。子类重写 OnDispose() 执行自定义清理。
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            OnDispose();
        }

        /// <summary>
        /// 子类重写以执行自定义清理（反注册 Snapshot 订阅、取消事件订阅等）。
        /// 基类实现为空。调用时 _disposed 已为 true。
        /// </summary>
        protected virtual void OnDispose() { }
    }

    /// <summary>
    /// 容器元素抽象基类。
    /// 实现 IScopeProvider：子类或调用方可设置 Scope 属性，
    /// DOMTree 展开 Children 时会自动 Push，让所有子元素的 ElementState 携带此 Scope。
    /// 默认 Scope=null（不提供），不影响现有行为。
    /// </summary>
    public abstract class ContainerElement : Element, IContainerElement, IScopeProvider
    {
        public abstract IEnumerable<IElement> Children { get; }
        public abstract void Add(IElement element);

        /// <summary>
        /// 为子元素提供的 Undo/Redo 作用域。
        /// 设置后，DOMTree 展开 Children 时会自动 Push 到 ScopeStack。
        /// 默认 null——不提供 Scope，行为与之前一致。
        /// </summary>
        public UndoScope Scope { get; set; }

        protected ContainerElement(params IElementComponent[] components) : base(components) { }

        public IEnumerator GetEnumerator()
        {
            return Children?.GetEnumerator();
        }
    }

    /// <summary>
    /// 状态化元素抽象基类（仅支持 struct 状态）
    /// State 必须是 struct，强制不可变，通过 SetState() 更新，自动触发重建
    /// </summary>
    /// <typeparam name="TState">状态类型（必须是 struct）</typeparam>
    public abstract class Element<TState> : Element, IElement<TState> where TState : struct
    {
        private TState _state;
        private bool _stateInitialized;

        public abstract TState BuildState();
        public abstract IElement Render(TState state);

        /// <summary>
        /// 更新状态。使用 updater 函数接收旧状态并返回新状态。
        /// 如果新旧状态不同，会自动触发 UI 重建。
        /// </summary>
        /// <param name="updater">状态更新函数</param>
        protected void SetState(Func<TState, TState> updater)
        {
            if (updater == null)
                throw new ArgumentNullException(nameof(updater));

            var newState = updater(_state);
            if (!EqualityComparer<TState>.Default.Equals(_state, newState))
            {
                _state = newState;
                NotifyChanged();
            }
        }

        /// <summary>
        /// 直接设置新状态
        /// </summary>
        /// <param name="newState">新状态</param>
        protected void SetState(TState newState)
        {
            if (!EqualityComparer<TState>.Default.Equals(_state, newState))
            {
                _state = newState;
                NotifyChanged();
            }
        }

        /// <summary>
        /// 获取当前状态（返回副本，不可变）
        /// </summary>
        protected TState State => _state;

        /// <summary>
        /// 框架调用的 Render 入口（密封，用户不应 override）
        /// </summary>
        public sealed override IElement Render()
        {
            if (!_stateInitialized)
            {
                _state = BuildState();
                _stateInitialized = true;
            }
            return Render(_state);
        }
    }
}