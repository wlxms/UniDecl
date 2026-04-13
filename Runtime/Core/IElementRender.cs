namespace UniDecl.Runtime.Core
{
    /// <summary>
    /// 元素渲染器接口（类型擦除）
    /// 无状态渲染器，负责将元素绘制到屏幕
    /// 渲染器通过 IUniDeclRenderManager 获取上下文信息
    /// </summary>
    public interface IElementRender
    {
        /// <summary>
        /// 渲染元素
        /// </summary>
        bool Render(IElement element, IElementRenderHost manager, ElementState state);
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
    /// </summary>
    public interface IElementRender<TRenderResult>
    {
        TRenderResult Render(IElement element, IElementRenderHost<TRenderResult> manager, ElementState state);
    }

    /// <summary>
    /// 带元素类型的泛型渲染器接口
    /// 同时提供 TElement 类型安全和 TRenderResult 返回值
    /// 默认实现将 IElementRender&lt;TRenderResult&gt;.Render 委托给 Render(TElement, ...)
    /// </summary>
    public interface IElementRenderer<TElement, TRenderResult> : IElementRender<TRenderResult> where TElement : IElement
    {
        TRenderResult Render(TElement element, IElementRenderHost<TRenderResult> manager, ElementState state);

        TRenderResult IElementRender<TRenderResult>.Render(IElement element, IElementRenderHost<TRenderResult> manager, ElementState state)
            => Render((TElement)element, manager, state);
    }

    // ==== Updater 接口（diff 模式增量更新）====

    /// <summary>
    /// 泛型增量更新器接口（类型擦除）
    /// 供渲染管线在不知道具体 TElement 时调用
    /// </summary>
    public interface IElementUpdater<TRenderResult>
    {
        bool TryUpdate(IElement element, TRenderResult existing,
            IElementRenderHost<TRenderResult> manager, ElementState state);
    }

    /// <summary>
    /// 泛型增量更新器接口
    /// Renderer 可同时实现此接口，在 diff 模式下复用已有渲染结果
    /// 返回 true 表示成功复用，返回 false 则框架回退到 Render()
    /// </summary>
    public interface IElementUpdater<TElement, TRenderResult> : IElementUpdater<TRenderResult> where TElement : IElement
    {
        bool TryUpdate(TElement element, TRenderResult existing,
            IElementRenderHost<TRenderResult> manager, ElementState state);
    }
}
