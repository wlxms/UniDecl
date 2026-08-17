using System;

namespace UniDecl.BuiltIn.Runtime.Snapshot
{
    /// <summary>
    /// 生命周期 Scope——管理一组绑定与 undo/redo 操作的生命周期。
    /// Dispose 时自动清理其下所有绑定注册与历史 steps（级联子 Scope）。
    /// </summary>
    public class UndoScope : IDisposable
    {
        private readonly ISnapshotManager _manager;
        private readonly int _scopeId;
        private bool _disposed;

        public int ScopeId => _scopeId;
        public ISnapshotManager Manager => _manager;

        /// <summary>创建顶层 Scope</summary>
        public UndoScope(ISnapshotManager manager)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _scopeId = manager.CreateScope(parentScopeId: 0);
        }

        /// <summary>创建子 Scope，从 parent 推导 manager</summary>
        public UndoScope(UndoScope parent)
        {
            _manager = parent?.Manager ?? throw new ArgumentNullException(nameof(parent));
            _scopeId = _manager.CreateScope(parent._scopeId);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _manager.DisposeScope(_scopeId);
        }
    }
}
