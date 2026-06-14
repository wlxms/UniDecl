using System;

namespace UniDecl.Snapshot
{
    /// <summary>
    /// 生命周期 Scope——管理一组 undo/redo 操作的生命周期。
    /// Dispose 时自动清理所有关联 steps 和 setters。
    /// 支持父子关系：父 Scope Dispose 级联清理所有子 Scope。
    /// </summary>
    public class UndoScope : IDisposable
    {
        private readonly ISnapshotManager _manager;
        private readonly int _scopeId;
        private bool _disposed;

        public int ScopeId => _scopeId;
        public ISnapshotManager Manager => _manager;

        /// <summary>
        /// 创建顶层 Scope
        /// </summary>
        public UndoScope(ISnapshotManager manager)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _scopeId = manager.CreateScope(parentScopeId: 0);
        }

        /// <summary>
        /// 创建子 Scope，从 parent 推导 manager
        /// </summary>
        public UndoScope(UndoScope parent)
        {
            _manager = parent?.Manager ?? throw new ArgumentNullException(nameof(parent));
            _scopeId = _manager.CreateScope(parent._scopeId);
        }

        /// <summary>
        /// 在此 Scope 内注册 setter
        /// </summary>
        public void Register<T>(string key, Func<T, T> setter)
        {
            _manager.Register(key, setter, _scopeId);
        }

        /// <summary>
        /// 在此 Scope 内记录值变更
        /// </summary>
        public void Record(object oldValue, string key)
        {
            _manager.Record(oldValue, key, _scopeId);
        }

        /// <summary>
        /// 在此 Scope 内记录对象变更
        /// </summary>
        public void RecordObject(object target, string key)
        {
            _manager.RecordObject(target, key, _scopeId);
        }

        /// <summary>
        /// 提交当前 pending steps（包括未关闭的 group）。
        /// 若有新 step 入栈，SnapshotManager 会触发 StepCommitted 事件。
        /// 返回 true 表示实际提交了 step。
        /// </summary>
        public bool Commit()
        {
            return _manager.CommitPending();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _manager.DisposeScope(_scopeId);
        }
    }
}
