namespace UniDecl.Runtime.Core
{
    /// <summary>
    /// 上下文读取器接口
    /// 提供从上下文栈中读取上下文值的能力
    /// </summary>
    public interface IContextReader
    {
        /// <summary>
        /// 获取指定类型的上下文（最近的）
        /// </summary>
        T Get<T>() where T : class, IContextProvider;

        /// <summary>
        /// 尝试获取指定类型的上下文
        /// </summary>
        bool TryGet<T>(out T value) where T : class, IContextProvider;

        /// <summary>
        /// 检查是否存在指定类型的上下文
        /// </summary>
        bool Has<T>() where T : class, IContextProvider;
    }
}
