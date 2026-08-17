using System;

namespace UniDecl.BuiltIn.Runtime.Snapshot
{
    /// <summary>
    /// 快照管理器接口——单轨：绑定树（ISnapshotBinding）统一注册/提交/撤销。
    /// 用户不直接 Record/Register，统一走 SnapshotBinding.Commit()。
    /// </summary>
    public interface ISnapshotManager
    {
        int MaxSteps { get; set; }
        bool EnableMerge { get; set; }
        long MergeWindowMs { get; set; }

        /// <summary>恢复（Undo/Redo）执行中为 true，此时任何 Commit 抛异常（防重入）</summary>
        bool IsRestoring { get; }

        /// <summary>新 step 入栈时触发。</summary>
        event Action<IStep> StepCommitted;

        /// <summary>Undo 执行完成时触发，参数为反向生成的 redo step。</summary>
        event Action<IStep> StepUndone;

        /// <summary>Redo 执行完成时触发，参数为反向生成的 undo step。</summary>
        event Action<IStep> StepRedone;

        /// <summary>Scope 被 Dispose 时触发，参数为 scopeId。</summary>
        event Action<int> ScopeDisposed;

        /// <summary>Undo/Redo 完成后触发，携带本次实际变更的字段清单（渲染层据此局部刷新）</summary>
        event Action<ChangeSet> OnUndoRedoPerformed;

        int UndoCount { get; }
        int RedoCount { get; }

        int CreateScope(int parentScopeId = 0);
        void DisposeScope(int scopeId);

        /// <summary>手动分组（可嵌套；自动组嵌套在手动组内）</summary>
        void BeginGroup(string key);
        void EndGroup();

        /// <summary>提交 pending steps。返回 true 表示实际提交了 step。</summary>
        bool CommitPending();
        bool Undo();
        bool Redo();

        void Clear();
        void ClearRedo();

        // ─── 绑定框架专用（SnapshotBinding 构造/提交时调用，用户勿直接调用）───

        void RegisterBinding(ISnapshotBinding binding);
        void UnregisterBinding(Guid bindingId);
        void RecordValue(object oldValue, Guid bindingId, string path, int scopeId);
        /// <summary>打断合并链（新手势开始，如 Slider PointerDown）</summary>
        void BreakMerge(string path, int scopeId);
    }
}
