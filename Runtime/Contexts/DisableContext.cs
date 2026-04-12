using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Contexts
{
    /// <summary>
    /// 禁用上下文
    /// 控制子元素的禁用状态
    /// 用法: new DisableContext(true) { new Button("Submit") }
    /// </summary>
    public class DisableContext : ContextProvider
    {
        public bool Value { get; }

        public DisableContext(bool value, IElement child = null)
        {
            Value = value;
            if (child != null)
                Add(child);
        }
    }
}
