using System;

namespace UniDecl.BuiltIn.Runtime.Snapshot
{
    /// <summary>
    /// 快照管理器接口——支持值类型快照、对象深拷贝 diff、事务分组、Scope 生命周期管理。
    /// </summary>
    public interface ISnapshotManager
    {
        int MaxSteps { get; set; }
        bool EnableMerge { get; set; }
        long MergeWindowMs { get; set; }

        /// <summary>新 step 入栈时触发。</summary>
        event Action<IStep> StepCommitted;

        /// <summary>Undo 执行完成时触发，参数为反向生成的 redo step。</summary>
        event Action<IStep> StepUndone;

        /// <summary>Redo 执行完成时触发，参数为反向生成的 undo step。</summary>
        event Action<IStep> StepRedone;

        /// <summary>Scope 被 Dispose 时触发，参数为 scopeId。</summary>
        event Action<int> ScopeDisposed;

        int UndoCount { get; }
        int RedoCount { get; }

        int CreateScope(int parentScopeId = 0);
        void DisposeScope(int scopeId);

        void Register<T>(string key, Func<T, T> setter, int scopeId = 0);
        /// <summary>
        /// 反注册指定 key：移除其 setter、merge 时间戳，以及 undo/redo/pending/group 栈中所有相关 steps。
        /// 用于单个 Widget 销毁时清理自身历史，不影响同 Scope 下其他 key。
        /// </summary>
        void Unregister(string key, int scopeId = 0);
        void Record(object oldValue, string key, int scopeId = 0);
        void RecordObject(object target, string key, int scopeId = 0);

        void BeginGroup(string key);
        void EndGroup();

        bool CommitPending();
        bool Undo();
        bool Redo();

        void Clear();
        void ClearRedo();
    }
}
