using System;
using System.Collections.Generic;

namespace UniDecl.Runtime.Core
{
    /// <summary>
    /// 状态管理器接口
    /// 基于 Key 管理元素状态的创建、获取和清理
    /// </summary>
    public interface IStateManager
    {
        /// <summary>
        /// 获取或创建指定 Key 的元素状态
        /// </summary>
        /// <param name="key">元素的唯一标识</param>
        /// <param name="factory">状态不存在时的创建工厂</param>
        ElementState GetOrCreateState(string key, Func<ElementState> factory);

        /// <summary>
        /// 获取指定 Key 的元素状态
        /// </summary>
        ElementState GetState(string key);

        /// <summary>
        /// 标记所有状态为未使用，用于帧结束时清理
        /// </summary>
        void MarkAllUnused();

        /// <summary>
        /// 清理未使用的状态
        /// </summary>
        void ClearUnused();
    }
}
