using System.Collections;

namespace UniDecl.Runtime.Core
{
    /// <summary>
    /// 上下文提供者接口
    /// 声明式地在元素树中提供上下文信息
    /// </summary>
    public interface IContextProvider : IElement, IEnumerable
    {
        /// <summary>
        /// 子元素
        /// </summary>
        IElement Child { get; }

        /// <summary>
        /// 添加子元素
        /// </summary>
        void Add(IElement child);
    }
}
