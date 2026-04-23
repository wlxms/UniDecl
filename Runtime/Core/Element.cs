using System;
using System.Collections;
using System.Collections.Generic;

namespace UniDecl.Runtime.Core
{
    public abstract class Element : IElement
    {
        protected IElementRenderHostBase _manager;
        private readonly Dictionary<Type, IElementComponent> _components = new Dictionary<Type, IElementComponent>();
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
    }

    public abstract class ContainerElement : Element, IContainerElement
    {
        public abstract IEnumerable<IElement> Children { get; }
        public abstract void Add(IElement element);

        protected ContainerElement(params IElementComponent[] components) : base(components) { }

        public IEnumerator GetEnumerator()
        {
            return Children?.GetEnumerator();
        }
    }

    /// <summary>
    /// 统一的状态化元素抽象基类
    /// 支持 struct 和 class 两种状态类型
    ///
    /// - struct 状态：强制不可变，通过 SetState() 更新，自动触发重建
    /// - class 状态：可变，需要手动调用 NotifyChanged() 触发重建
    /// </summary>
    /// <typeparam name="TState">状态类型（struct 或 class）</typeparam>
    public abstract class Element<TState> : Element, IElement<TState>
    {
        private TState _state;
        private bool _stateInitialized;
        private readonly bool _isValueType;

        protected Element()
        {
            _isValueType = typeof(TState).IsValueType;
        }

        public abstract TState BuildState();
        public abstract IElement Render(TState state);

        /// <summary>
        /// 更新状态（仅当 TState 是 struct 时可用）
        /// 使用 updater 函数接收旧状态并返回新状态
        /// 如果新旧状态不同，会自动触发 UI 重建
        /// </summary>
        /// <param name="updater">状态更新函数</param>
        protected void SetState(Func<TState, TState> updater)
        {
            if (updater == null)
                throw new ArgumentNullException(nameof(updater));

            if (!_isValueType)
                throw new InvalidOperationException(
                    $"SetState() 只能用于 struct 状态。当前状态类型 {typeof(TState).Name} 是 class。" +
                    "请使用 ReactiveStateElement 或直接修改状态后调用 NotifyChanged()。");

            var newState = updater(_state);
            if (!EqualityComparer<TState>.Default.Equals(_state, newState))
            {
                _state = newState;
                NotifyChanged();
            }
        }

        /// <summary>
        /// 直接设置新状态（仅当 TState 是 struct 时可用）
        /// </summary>
        /// <param name="newState">新状态</param>
        protected void SetState(TState newState)
        {
            if (!_isValueType)
                throw new InvalidOperationException(
                    $"SetState() 只能用于 struct 状态。当前状态类型 {typeof(TState).Name} 是 class。" +
                    "请使用 ReactiveStateElement 或直接修改状态后调用 NotifyChanged()。");

            if (!EqualityComparer<TState>.Default.Equals(_state, newState))
            {
                _state = newState;
                NotifyChanged();
            }
        }

        /// <summary>
        /// 获取当前状态
        /// - struct 状态：返回副本（不可变）
        /// - class 状态：返回引用（可变，但需要手动调用 NotifyChanged）
        /// </summary>
        protected TState State => _state;

        /// <summary>
        /// 框架调用的 Render 入口（密封，用户不应 override）
        /// </summary>
        public sealed override IElement Render()
        {
            if (_isValueType)
            {
                // Struct 模式：自己管理状态
                if (!_stateInitialized)
                {
                    _state = BuildState();
                    _stateInitialized = true;
                }
                return Render(_state);
            }
            else
            {
                // Class 模式：通过 Manager 管理状态（保持向后兼容）
                var state = (TState)_manager.GetElementState(this);
                if (state == null)
                {
                    if (!_stateInitialized)
                    {
                        _state = BuildState();
                        _stateInitialized = true;
                    }
                    state = _state;
                }
                else
                {
                    _state = state;
                }
                return Render(state);
            }
        }
    }
}