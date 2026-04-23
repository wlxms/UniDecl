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
    /// 状态化元素抽象基类（Class-based State）
    /// 用户继承此类，只需实现 BuildState() 和 Render(TState state) 两个方法
    /// 框架自动通过 Manager 引用获取状态并传入 Render
    ///
    /// 注意：这是传统的 class-based 状态模式，状态变更需要手动调用 NotifyChanged()
    ///
    /// 推荐使用以下新模式：
    /// - StructStateElement&lt;TState&gt; - Struct + SetState 模式，强制不可变状态
    /// - ReactiveStateElement&lt;TState&gt; - ReactiveValue 模式，自动变更检测
    /// </summary>
    public abstract class Element<TState> : Element, IElement<TState> where TState : class
    {
        private TState _cachedState;

        public abstract TState BuildState();
        public abstract IElement Render(TState state);

        public sealed override IElement Render()
        {
            var state = (TState)_manager.GetElementState(this);
            if (state == null)
            {
                _cachedState ??= BuildState();
                state = _cachedState;
            }
            return Render(state);
        }
    }
}