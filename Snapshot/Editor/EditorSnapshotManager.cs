using System;
using UnityEngine;

namespace UniDecl.Snapshot.Editor
{
#if UNITY_EDITOR
    /// <summary>
    /// SnapshotManager 的装饰者——为任意 ISnapshotManager 增加 Unity Undo/Redo 接入能力。
    ///
    /// 使用方式：
    /// <code>
    /// var editor = new EditorSnapshotManager(new SnapshotManager());
    /// // editor 同时是 ISnapshotManager（可传给 UndoScope）和 Unity 接入器
    /// </code>
    ///
    /// 装饰者职责：
    /// - 将所有 ISnapshotManager 成员委托给 inner
    /// - 订阅 inner.StepCommitted：业务侧提交新 step 时，对隐藏 SO 的 Version 字段
    ///   做 RecordObject + ++，让 Unity 栈与 inner 栈保持 1:1。
    /// - 监听 Unity.undoRedoPerformed：用户按 Ctrl+Z/Y 时，按 Version 差值反向驱动 inner.Undo()/Redo()。
    /// </summary>
    public class EditorSnapshotManager : ISnapshotManager, IDisposable
    {
        private class SnapshotBridgeHost : ScriptableObject
        {
            [SerializeField] public int Version;
        }

        private readonly ISnapshotManager _inner;
        private readonly SnapshotBridgeHost _host;
        private int _lastVersion;
        private bool _isSyncing;

        /// <summary>
        /// Undo/Redo 执行后触发，UI 据此刷新。
        /// </summary>
        public event Action OnUndoRedoPerformed;

        public EditorSnapshotManager(ISnapshotManager inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _host = ScriptableObject.CreateInstance<SnapshotBridgeHost>();
            _host.hideFlags = HideFlags.HideAndDontSave;
            _lastVersion = _host.Version;

            // 业务 → Unity 同步
            _inner.StepCommitted += OnStepCommitted;
            // Unity → 业务 同步
            UnityEditor.Undo.undoRedoPerformed += OnUnityUndoRedoPerformed;
        }

        // ─── 装饰者：事件直接转发给 inner ───

        public event Action<IStep> StepCommitted
        {
            add => _inner.StepCommitted += value;
            remove => _inner.StepCommitted -= value;
        }

        public event Action<IStep> StepUndone
        {
            add => _inner.StepUndone += value;
            remove => _inner.StepUndone -= value;
        }

        public event Action<IStep> StepRedone
        {
            add => _inner.StepRedone += value;
            remove => _inner.StepRedone -= value;
        }

        public event Action<int> ScopeDisposed
        {
            add => _inner.ScopeDisposed += value;
            remove => _inner.ScopeDisposed -= value;
        }

        // ─── 装饰者：属性直接转发 ───

        public int MaxSteps
        {
            get => _inner.MaxSteps;
            set => _inner.MaxSteps = value;
        }

        public bool EnableMerge
        {
            get => _inner.EnableMerge;
            set => _inner.EnableMerge = value;
        }

        public long MergeWindowMs
        {
            get => _inner.MergeWindowMs;
            set => _inner.MergeWindowMs = value;
        }

        public int UndoCount => _inner.UndoCount;
        public int RedoCount => _inner.RedoCount;

        // ─── 装饰者：方法委托 ───

        public int CreateScope(int parentScopeId = 0) => _inner.CreateScope(parentScopeId);
        public void DisposeScope(int scopeId) => _inner.DisposeScope(scopeId);

        public void Register<T>(string key, Func<T, T> setter, int scopeId = 0)
            => _inner.Register(key, setter, scopeId);

        public void Record(object oldValue, string key, int scopeId = 0)
            => _inner.Record(oldValue, key, scopeId);

        public void RecordObject(object target, string key, int scopeId = 0)
            => _inner.RecordObject(target, key, scopeId);

        public void BeginGroup(string key) => _inner.BeginGroup(key);
        public void EndGroup() => _inner.EndGroup();
        public bool CommitPending() => _inner.CommitPending();

        // Undo/Redo 在 Unity 触发路径上走 _inner，避免与 OnUnityUndoRedoPerformed 循环
        public bool Undo() => _inner.Undo();
        public bool Redo() => _inner.Redo();

        public void Clear() => _inner.Clear();
        public void ClearRedo() => _inner.ClearRedo();

        // ─── Unity 同步逻辑 ───

        private void OnStepCommitted(IStep step)
        {
            if (_isSyncing) return; // 防止 Unity 触发的 undo/redo 再回到这里
            UnityEditor.Undo.RecordObject(_host, step?.Key ?? "Snapshot");
            _host.Version++;
            _lastVersion = _host.Version;
        }

        private void OnUnityUndoRedoPerformed()
        {
            if (_isSyncing) return;
            _isSyncing = true;
            try
            {
                var current = _host.Version;
                if (current == _lastVersion) return;

                // Unity 可能将相邻帧对同一对象的多次 RecordObject 合并为一个 undo 点，
                // 导致 Version 一次跳变多个值。按实际差值循环，保持两边栈一一对应。
                if (current < _lastVersion)
                {
                    int steps = _lastVersion - current;
                    for (int i = 0; i < steps; i++)
                        _inner.Undo();
                }
                else
                {
                    int steps = current - _lastVersion;
                    for (int i = 0; i < steps; i++)
                        _inner.Redo();
                }

                _lastVersion = current;
                OnUndoRedoPerformed?.Invoke();
            }
            finally
            {
                _isSyncing = false;
            }
        }

        public void Dispose()
        {
            _inner.StepCommitted -= OnStepCommitted;
            UnityEditor.Undo.undoRedoPerformed -= OnUnityUndoRedoPerformed;
            if (_host != null)
                UnityEngine.Object.DestroyImmediate(_host);
            if (_inner is IDisposable disposable)
                disposable.Dispose();
        }
    }
#endif
}
