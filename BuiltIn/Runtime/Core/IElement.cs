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
    /// Scope 提供者——DOMTree 展开此元素的子树时，会自动 Push 此 Scope，
    /// 让子元素的 ElementState 携带该 Scope。子树展开完毕后自动 Pop。
    /// </summary>
    public interface IScopeProvider : IElement
    {
        /// <summary>
        /// 为子元素提供的 Undo/Redo 作用域。
        /// 返回 null 表示当前不提供 Scope（DOMTree 会跳过 Push）。
        /// </summary>
        UndoScope Scope { get; }
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

    public interface IContainerElement : IElement, IEnumerable
    {
        public IEnumerable<IElement> Children { get; }
        public void Add(IElement element);
    }

}