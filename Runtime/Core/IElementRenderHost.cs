namespace UniDecl.Runtime.Core
{
    /// <summary>
    /// 渲染管理器基础接口
    /// 提供 BuildDOM、Render、渲染器注册等核心能力
    /// </summary>
    public interface IElementRenderHostBase : IEventDispatcher, IContextReader
    {
        /// <summary>
        /// 展开声明式元素树为 DOM 树
        /// 当 DOM 为空时调用，处理 Context、Stateful、Consumer 等逻辑
        /// </summary>
        void BuildDOM(IElement element);

        /// <summary>
        /// 获取元素的缓存状态对象
        /// 用于 StatefulElement 在 Render 时获取 State 引用
        /// </summary>
        object GetElementState(IElement element);

        /// <summary>
        /// 导航到指定锚点
        /// </summary>
        void NavigateTo(string anchorId);

        /// <summary>
        /// 导航到指定 URL
        /// </summary>
        void NavigateURL(string url);

        /// <summary>
        /// Host 名称，用于 HostManager 注册和 URL 路由
        /// </summary>
        string HostName { get; }
    }

    /// <summary>
    /// UniDecl 渲染管理器接口（非泛型）
    /// 用于 IMGUI 等不需要返回渲染结果的后端
    /// </summary>
    public interface IElementRenderHost : IElementRenderHostBase
    {
        /// <summary>
        /// 基于 DOM 树进行实际渲染
        /// </summary>
        void Render();

        /// <summary>
        /// 注册渲染器
        /// </summary>
        void RegisterRenderer<T>(IElementRenderer<T> renderer) where T : IElement;

        /// <summary>
        /// 获取元素的渲染器
        /// </summary>
        IElementRender GetRenderer(IElement element);

        /// <summary>
        /// 渲染指定元素（由渲染器回调，用于渲染子节点）
        /// </summary>
        void RenderElement(IElement element);
    }

    /// <summary>
    /// UniDecl 渲染管理器泛型接口
    /// 用于 UI Toolkit 等需要返回渲染结果的后端
    /// 容器渲染器通过 RenderElement(child) 获取子节点的 TRenderResult，自行组装
    /// </summary>
    public interface IElementRenderHost<TRenderResult> : IElementRenderHostBase
    {
        /// <summary>
        /// 基于 DOM 树进行实际渲染，返回渲染结果
        /// </summary>
        TRenderResult Render();

        /// <summary>
        /// 渲染指定元素，返回渲染结果（由渲染器回调，用于渲染子节点）
        /// </summary>
        TRenderResult RenderElement(IElement element);

        /// <summary>
        /// 注册泛型渲染器
        /// </summary>
        void RegisterRenderer<T>(IElementRenderer<T, TRenderResult> renderer) where T : IElement;

        /// <summary>
        /// 获取泛型渲染器
        /// </summary>
        IElementRender<TRenderResult> GetTypedRenderer(IElement element);
    }
}