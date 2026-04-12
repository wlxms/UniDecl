using System;

namespace UniDecl.Runtime.Core
{
    /// <summary>
    /// 上下文消费者
    /// 通过回调函数消费上下文信息，返回渲染结果
    /// 用法: new ContextConsumer(reader => new Label(reader.Get&lt;UserName&gt;().Value))
    /// </summary>
    public class ContextConsumer : Element, IContextConsumer
    {
        public Func<IContextReader, IElement> ContextRender { get; }

        public ContextConsumer(Func<IContextReader, IElement> contextRender)
        {
            ContextRender = contextRender ?? throw new ArgumentNullException(nameof(contextRender));
        }

        public override IElement Render()
        {
            return null;
        }
    }
}
