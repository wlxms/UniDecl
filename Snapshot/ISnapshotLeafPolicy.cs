namespace UniDecl.Snapshot
{
    /// <summary>
    /// 快照叶子判定策略。由引擎适配层注入 SnapshotManager，用于识别“整体作为引用叶子、
    /// 不继续反射展开”的引擎类型（如 UnityEngine.Object / AnimationCurve / Gradient）。
    /// 纯 C# 层只提供值类型、string 的默认实现，不感知任何引擎。
    /// </summary>
    public interface ISnapshotLeafPolicy
    {
        /// <summary>返回 true 表示该值应作为叶子整体记录，不再展开其字段。</summary>
        bool IsLeaf(object value);
    }
}
