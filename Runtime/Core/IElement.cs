using System;
using System.Collections;
using System.Collections.Generic;

namespace UniDecl.Runtime.Core
{
    public interface IElement
    {
        public string Key { get; }
        public IElement Render();
        public void Initialize(int index, IElementRenderHostBase manager);
        public void Rebuild();
        public IElement With<T>(T component) where T : IElementComponent;
        public T Get<T>() where T : IElementComponent;
    }

    public interface IElementComponent
    {
        
    }

    public interface IStatefulElement : IElement
    {
        public object BuildState();
    }

    public interface IElement<TState> : IStatefulElement where TState : class
    {
        object IStatefulElement.BuildState() => BuildState();

        IElement Render(TState state);

        public new TState BuildState();
    }

    public interface IContainerElement : IElement, IEnumerable
    {
        public IEnumerable<IElement> Children { get; }
        public void Add(IElement element);
    }

}