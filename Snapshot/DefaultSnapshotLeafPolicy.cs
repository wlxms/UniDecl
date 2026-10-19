using System;

namespace UniDecl.Snapshot
{
    /// <summary>
    /// 默认叶子策略：null、值类型、string 视为叶子。
    /// 引擎适配层可组合本策略并追加引擎特有类型判断。
    /// </summary>
    public sealed class DefaultSnapshotLeafPolicy : ISnapshotLeafPolicy
    {
        public static readonly DefaultSnapshotLeafPolicy Instance = new DefaultSnapshotLeafPolicy();

        public bool IsLeaf(object value)
        {
            if (value == null) return true;
            var type = value.GetType();
            return type.IsValueType || type == typeof(string);
        }
    }
}
