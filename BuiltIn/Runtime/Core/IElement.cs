using System;
using System.Collections;
using System.Collections.Generic;
using UniDecl.BuiltIn.Runtime.Snapshot;

namespace UniDecl.BuiltIn.Runtime.Core
{
    /// <summary>
    /// 声明式元素接口——所有 Widget 的基类。
    /// 继承 IDisposable：元素被 DOMTree 移除时由框架调用 Dispose()，
    /// 子类可重写 Element.OnDispose() 执行自定义清理（反注册 Snapshot 订阅等）。
    /// </summary>
    public interface IElement : IDisposable
    {
        public string Key { get; }
        public IElement Render();
        public void Initialize(int index, IElementRenderHostBase manager);
        public void Rebuild();
        public IElement With<T>(T component) where T : IElementComponent;
        public T Get<T>() where T : IElementComponent;
    }

    /// <summary>
    /// Scope 提供者标记——声明此元素的子树需要 Undo/Redo 作用域。
    /// 仅作标记，实际 Scope 挂在 ElementState.Scope 上（由 Host 注入）。
    /// </summary>
    public interface IScopeProvider
    {
    }

    public interface IElementComponent
    {

    }

    public interface IStatefulElement : IElement
    {
        public object BuildState();
    }

    public interface IElement<TState> : IStatefulElement where TState : struct
    {
        object IStatefulElement.BuildState() => BuildState();

        IElement Render(TState state);

        public new TState BuildState();
    }

    /// <summary>
    /// 容器元素接口。继承 IScopeProvider 标记：容器的子树需要 Undo/Redo 作用域。
    /// </summary>
    public interface IContainerElement : IElement, IEnumerable, IScopeProvider
    {
        public IEnumerable<IElement> Children { get; }
        public void Add(IElement element);
    }

}