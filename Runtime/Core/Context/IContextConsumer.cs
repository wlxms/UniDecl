using System;

namespace UniDecl.Runtime.Core
{
    /// <summary>
    /// 上下文消费者接口
    /// 通过回调函数消费上下文信息并渲染结果
    /// </summary>
    public interface IContextConsumer : IElement
    {
        /// <summary>
        /// 渲染函数，接收上下文读取器作为参数
        /// </summary>
        Func<IContextReader, IElement> ContextRender { get; }
    }
}
