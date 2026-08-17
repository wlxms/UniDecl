using System;

namespace UniDecl.BuiltIn.Runtime.Snapshot
{
    /// <summary>
    /// 统一写回入口——Undo/Redo 恢复时由 SnapshotManager 调用。
    /// 叶子：restore=还原值，current=当前值，changes=单条自身；职责为写回字段 + 业务层刷新。
    /// 容器：restore=整体替换值（仅截断/引用替换场景，否则 null），current=当前对象引用，
    ///       changes=子树变更聚合清单；职责为业务层刷新（子树字段已由框架写回）。
    /// </summary>
    public delegate void SnapshotSetter(object restore, object current, ChangeSet changes);

    /// <summary>
    /// 快照绑定接口——绑定树节点。构造时自动向 Manager 注册（弱引用），
    /// Dispose 或 GC 后由 Manager 惰性清理反注册。
    /// </summary>
    public interface ISnapshotBinding : IDisposable
    {
        /// <summary>绑定唯一标识（自动生成，用户无需传 key）</summary>
        Guid Id { get; }

        /// <summary>字段路径（"config.Child.X"、"items[0]"），changeSet 展示与局部刷新用</summary>
        string Path { get; }

        /// <summary>所属 Scope（DisposeScope 时级联清理注册与历史）</summary>
        int ScopeId { get; }

        /// <summary>
        /// 提交当前值。叶子：与基线对比，变更才产生 step；容器：递归子节点并自动打包组。
        /// 恢复（Undo/Redo）期间调用将抛出 InvalidOperationException。
        /// </summary>
        void Commit();

        /// <summary>
        /// 恢复入口（仅 SnapshotManager 调用）：写回旧值并返回当前值（用于生成反向 step）。
        /// changes 为本次聚合的变更清单（叶子追加自身，容器冒泡通知）。
        /// </summary>
        object Restore(object value, ChangeSet changes);
    }
}
