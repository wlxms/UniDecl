using UniDecl.Snapshot;

namespace UniDecl.Unity.Snapshot
{
    /// <summary>
    /// Unity 叶子策略：在默认策略（null / 值类型 / string）之上，
    /// 把 UnityEngine.Object 及含原生指针的包装类型（AnimationCurve / Gradient）
    /// 整体视为叶子，避免反射展开 m_Ptr 导致 undo 后野引用。
    /// 通过 <c>new SnapshotManager(new UnitySnapshotLeafPolicy())</c> 显式注入。
    /// </summary>
    public sealed class UnitySnapshotLeafPolicy : ISnapshotLeafPolicy
    {
        private readonly ISnapshotLeafPolicy _fallback;

        public UnitySnapshotLeafPolicy(ISnapshotLeafPolicy fallback = null)
        {
            _fallback = fallback ?? DefaultSnapshotLeafPolicy.Instance;
        }

        public bool IsLeaf(object value)
        {
            if (_fallback.IsLeaf(value)) return true;

            var type = value.GetType();
            if (typeof(UnityEngine.Object).IsAssignableFrom(type)) return true;

            // Unity 原生包装类（非 UnityEngine.Object 但含 m_Ptr 指针）：
            // 反射展开会拿到 IntPtr，undo 写回旧指针产生野引用
            // （如 AnimationCurve undo 后曲线消失）——整体作引用叶子。
            if (value is UnityEngine.AnimationCurve || value is UnityEngine.Gradient) return true;

            return false;
        }
    }
}
