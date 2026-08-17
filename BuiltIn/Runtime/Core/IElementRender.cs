using System;

namespace UniDecl.BuiltIn.Runtime.Core
{
    /// <summary>
    /// 元素渲染器接口（类型擦除）
    /// 无状态渲染器，负责将元素绘制到屏幕
    /// 渲染器通过 IUniDeclRenderManager 获取上下文信息
    /// 继承 IDisposable：渲染结果被废弃时由框架调用 Dispose()，
    /// Renderer 可释放后端资源（VisualElement 的事件订阅、Snapshot Register 等）。
    /// 默认实现为空——无资源的 Renderer 无需重写。
    /// </summary>
    public interface IElementRender : IDisposable
    {
        /// <summary>
        /// 渲染元素
        /// </summary>
        bool Render(IElement element, IElementRenderHost manager, ElementState state);

        /// <summary>
        /// 默认空实现。需要释放资源的 Renderer 重写此方法。
        /// </summary>
        void IDisposable.Dispose() { }
    }

    /// <summary>
    /// 带元素类型的渲染器接口
    /// 提供 TElement 类型的 Render 方法，方便子类直接使用具体元素类型
    /// 默认实现将 IElementRender.Render 委托给 Render(TElement, ...)
    /// </summary>
    /// <typeparam name="TElement">元素类型</typeparam>
    public interface IElementRenderer<TElement> : IElementRender where TElement : IElement
    {
        /// <summary>
        /// 渲染指定类型的元素
        /// </summary>
        bool Render(TElement element, IElementRenderHost manager, ElementState state);

        bool IElementRender.Render(IElement element, IElementRenderHost manager, ElementState state)
            => Render((TElement)element, manager, state);
    }

    /// <summary>
    /// 泛型元素渲染器接口（类型擦除）
    /// 用于 UI Toolkit 等需要返回渲染结果的后端
    /// 渲染器通过 IElementRenderHost&lt;TRenderResult&gt;.RenderElement 回调框架渲染子节点
    /// 继承 IDisposable：渲染结果被废弃时释放后端资源。
    /// 默认实现为空——无资源的 Renderer 无需重写。
    /// </summary>
    public interface IElementRender<TRenderResult> : IDisposable
    {
        /// <summary>
        /// 将元素投影到渲染结果。
        /// existing 为该元素上次的渲染结果（首次为 default）。可复用则原地更新并返回 existing 本身，
        /// 返回新对象则宿主替换缓存并触发渲染结果变更回调。
        /// </summary>
        TRenderResult Render(IElement element, TRenderResult existing, IElementRenderHost<TRenderResult> manager, ElementState state);

        /// <summary>
        /// 默认空实现。需要释放资源的 Renderer 重写此方法。
        /// </summary>
        void IDisposable.Dispose() { }
    }

    /// <summary>
    /// 带元素类型的泛型渲染器接口
    /// 同时提供 TElement 类型安全和 TRenderResult 返回值
    /// 默认实现将 IElementRender&lt;TRenderResult&gt;.Render 委托给 Render(TElement, ...)
    /// </summary>
    public interface IElementRenderer<TElement, TRenderResult> : IElementRender<TRenderResult> where TElement : IElement
    {
        /// <summary>
        /// 将元素投影到渲染结果。existing 语义同 IElementRender&lt;TRenderResult&gt;.Render。
        /// </summary>
        TRenderResult Render(TElement element, TRenderResult existing, IElementRenderHost<TRenderResult> manager, ElementState state);

        TRenderResult IElementRender<TRenderResult>.Render(IElement element, TRenderResult existing, IElementRenderHost<TRenderResult> manager, ElementState state)
            => Render((TElement)element, existing, manager, state);
    }
}
