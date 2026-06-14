using System;
using System.Collections.Generic;

namespace UniDecl.Snapshot
{
    /// <summary>
    /// UniSnap 核心——通用 Undo/Redo 管理器
    /// 支持值类型快照、对象深拷贝 diff、事务分组、步骤合并、Scope 生命周期管理
    /// </summary>
    public class SnapshotManager : ISnapshotManager
    {
        private const int DefaultMaxSteps = 50;
        private const long DefaultMergeWindowMs = 500;

        // Scope 注册表
        private int _nextScopeId = 1;
        private readonly Dictionary<int, ScopeInfo> _scopes = new();

        private readonly Dictionary<string, SetterEntry> _setters = new();
        private readonly List<IStep> _undoStack = new();
        private readonly List<IStep> _redoStack = new();
        private readonly Stack<List<IStep>> _groupStack = new();
        private readonly List<IStep> _pendingSteps = new();
        private readonly Dictionary<string, long> _lastRecordTime = new();

        public int MaxSteps { get; set; } = DefaultMaxSteps;
        public bool EnableMerge { get; set; } = true;
        public long MergeWindowMs { get; set; } = DefaultMergeWindowMs;

        // ─── 事件（供外部宿主订阅，如 EditorSnapshotManager 与 Unity Undo 同步） ───

        /// <summary>
        /// 新 step 入栈时触发。step.Key 可作为宿主侧的撤销点描述。
        /// 合并或 pending 为空时不会触发。
        /// </summary>
        public event Action<IStep> StepCommitted;

        /// <summary>
        /// Undo 执行完成时触发，参数为反向生成的 redo step。
        /// </summary>
        public event Action<IStep> StepUndone;

        /// <summary>
        /// Redo 执行完成时触发，参数为反向生成的 undo step。
        /// </summary>
        public event Action<IStep> StepRedone;

        /// <summary>
        /// Scope 被 Dispose 时触发，参数为 scopeId。
        /// 宿主可据此感知栈结构变化（部分 steps 被移除）。
        /// </summary>
        public event Action<int> ScopeDisposed;

        public int UndoCount
        {
            get
            {
                var count = _undoStack.Count + _pendingSteps.Count;
                foreach (var group in _groupStack)
                    count += group.Count;
                return count;
            }
        }

        public int RedoCount => _redoStack.Count;

        // ─── Scope 管理 ───

        /// <summary>
        /// 创建一个新的 UndoScope，返回 scopeId。
        /// parentScopeId=0 表示顶层 Scope。
        /// </summary>
        public int CreateScope(int parentScopeId = 0)
        {
            var id = _nextScopeId++;
            _scopes[id] = new ScopeInfo
            {
                Id = id,
                ParentId = parentScopeId,
                Children = new List<int>()
            };
            if (parentScopeId > 0 && _scopes.TryGetValue(parentScopeId, out var parent))
                parent.Children.Add(id);
            return id;
        }

        /// <summary>
        /// Dispose Scope：移除该 Scope 下所有 steps + setters，级联移除子 Scope。
        /// 先移除 steps（此时 setter 仍可用），再移除 setters（引用计数）。
        /// </summary>
        public void DisposeScope(int scopeId)
        {
            if (!_scopes.TryGetValue(scopeId, out var scope)) return;
            _scopes.Remove(scopeId); // 防止重入

            // 级联 Dispose 子 Scope
            foreach (var childId in scope.Children.ToArray())
                DisposeScope(childId);

            // 从父 Scope 的 children 中移除自己
            if (scope.ParentId > 0 && _scopes.TryGetValue(scope.ParentId, out var parent))
                parent.Children.Remove(scopeId);

            // 先移除 steps（此时 setter 仍可用）
            FilterStepsByScopePrefix(scopeId);

            // 再移除 setters
            RemoveSettersByScope(scopeId);

            ScopeDisposed?.Invoke(scopeId);
        }

        // ─── Register / Record ───

        /// <summary>
        /// 注册一个 key 的 setter 回调。setter 接收新值，返回被覆盖的旧值。
        /// 内部使用 $scopeId:userKey 作为存储 key，不同 Scope 的同名 userKey 天然隔离。
        /// </summary>
        public void Register<T>(string key, Func<T, T> setter, int scopeId = 0)
        {
            var scopedKey = ScopedKey(key, scopeId);
            _setters[scopedKey] = new SetterEntry
            {
                ValueType = typeof(T),
                BoxedSetter = boxed => setter((T)boxed),
                OwnerScopeId = scopeId
            };
        }

        /// <summary>
        /// 检查 key 是否已注册，未注册抛出异常
        /// </summary>
        private void EnsureRegistered(string key)
        {
            if (!_setters.ContainsKey(key))
                throw new InvalidOperationException(
                    $"Key '{key}' has not been registered. Call Register() before Record().");
        }

        /// <summary>
        /// 记录一次值变更（创建 ValueStep）。合并仅在当前 buffer 内生效。
        /// scopeId 用于隔离不同 Scope 的同名 key。
        /// </summary>
        public void Record(object oldValue, string key, int scopeId = 0)
        {
            var scopedKey = ScopedKey(key, scopeId);
            EnsureRegistered(scopedKey);

            if (EnableMerge && TryMergeValueStep(scopedKey))
                return;

            var step = new ValueStep(scopedKey, oldValue);
            AddToCurrentBuffer(step);
            ClearRedo();
        }

        /// <summary>
        /// 记录一次对象变更（创建 ObjectDiffStep，深拷贝字段快照）。
        /// </summary>
        public void RecordObject(object target, string key, int scopeId = 0)
        {
            var scopedKey = ScopedKey(key, scopeId);
            var snapshots = DeepCopyUtility.SnapshotFields(target);
            var step = new ObjectDiffStep(scopedKey, target, snapshots);
            AddToCurrentBuffer(step);
            ClearRedo();
        }

        // ─── Group ───

        public void BeginGroup(string key)
        {
            _groupStack.Push(new List<IStep>());
        }

        public void EndGroup()
        {
            if (_groupStack.Count == 0) return;
            var steps = _groupStack.Pop();
            if (steps.Count == 0) return;
            var group = new GroupStep("group", steps);
            AddToCurrentBuffer(group);
        }

        /// <summary>
        /// 提交 pending steps。先处理未关闭的 group，再提交到 undoStack。
        /// 未关闭 group 从外层到内层收集，保持 Record 时间顺序。
        /// 返回 true 表示有新 step 提交到 undoStack；false 表示 pending 为空（如 Record 被 Merge）。
        /// </summary>
        public bool CommitPending()
        {
            // 收集所有未关闭 group 的 steps，保持 Record 时间顺序
            var groups = new List<List<IStep>>();
            while (_groupStack.Count > 0)
                groups.Add(_groupStack.Pop());
            for (int i = groups.Count - 1; i >= 0; i--)
            {
                if (groups[i].Count > 0)
                    _pendingSteps.AddRange(groups[i]);
            }

            if (_pendingSteps.Count == 0) return false;
            IStep committed = _pendingSteps.Count == 1
                ? _pendingSteps[0]
                : new GroupStep("auto", new List<IStep>(_pendingSteps));
            _pendingSteps.Clear();
            _undoStack.Add(committed);
            TrimStack(_undoStack);
            StepCommitted?.Invoke(committed);
            return true;
        }

        // ─── Undo / Redo ───

        public bool Undo()
        {
            CommitPending();
            if (_undoStack.Count == 0) return false;
            var step = PopLast(_undoStack);
            var redoStep = ApplyStep(step);
            if (redoStep != null) _redoStack.Add(redoStep);
            StepUndone?.Invoke(redoStep);
            return true;
        }

        public bool Redo()
        {
            if (_redoStack.Count == 0) return false;
            var step = PopLast(_redoStack);
            var undoStep = ApplyStep(step);
            if (undoStep != null)
            {
                _undoStack.Add(undoStep);
                TrimStack(_undoStack);
            }
            StepRedone?.Invoke(undoStep);
            return true;
        }

        // ─── 内部方法 ───

        private IStep ApplyStep(IStep step)
        {
            switch (step)
            {
                case ValueStep vs:
                    if (!_setters.TryGetValue(vs.Key, out var entry))
                        return null; // setter 已被 Scope Dispose 移除，跳过
                    var currentVal = entry.BoxedSetter(vs.Value);
                    return new ValueStep(vs.Key, currentVal);

                case ObjectDiffStep ods:
                    var currentSnap = DeepCopyUtility.SnapshotFields(ods.Target);
                    DeepCopyUtility.RestoreFields(ods.Target, ods.FieldSnapshots);
                    return new ObjectDiffStep(ods.Key, ods.Target, currentSnap);

                case GroupStep gs:
                    var redoSteps = new List<IStep>();
                    for (int i = gs.Steps.Count - 1; i >= 0; i--)
                    {
                        var redoStep = ApplyStep(gs.Steps[i]);
                        if (redoStep != null) redoSteps.Insert(0, redoStep);
                    }
                    return redoSteps.Count > 0 ? new GroupStep(gs.Key, redoSteps) : null;

                default: return null;
            }
        }

        /// <summary>
        /// 合并检查：仅在当前 buffer 内（pending 或 group）检查末尾同 key ValueStep。
        /// 合并 = 保留最早旧值（vs.Value 不变），本次 oldValue 被丢弃，不创建新 step。
        /// 时间窗口：同 key 两次 Record 间隔超过 MergeWindowMs 则不合并。
        /// </summary>
        private bool TryMergeValueStep(string key)
        {
            var buffer = CurrentBuffer;
            if (buffer.Count == 0) return false;
            var last = buffer[buffer.Count - 1];
            if (last is not ValueStep vs || vs.Key != key) return false;

            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (_lastRecordTime.TryGetValue(key, out var lastTime) && now - lastTime > MergeWindowMs)
                return false;
            _lastRecordTime[key] = now;
            return true; // 保留最早旧值，不做任何修改
        }

        private List<IStep> CurrentBuffer =>
            _groupStack.Count > 0 ? _groupStack.Peek() : _pendingSteps;

        private void AddToCurrentBuffer(IStep step)
        {
            CurrentBuffer.Add(step);
            _lastRecordTime[step.Key] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private void TrimStack(List<IStep> stack)
        {
            while (stack.Count > MaxSteps) stack.RemoveAt(0);
        }

        private static IStep PopLast(List<IStep> stack)
        {
            var step = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);
            return step;
        }

        // ─── Scoped Key 隔离 ───

        /// <summary>
        /// 将用户可见的 key 与 scopeId 组合为内部存储 key。
        /// scopeId > 0 时格式为 "$scopeId:userKey"，scopeId=0 为全局注册直接返回 userKey。
        /// </summary>
        private static string ScopedKey(string key, int scopeId) =>
            scopeId > 0 ? $"${scopeId}:{key}" : key;

        /// <summary>
        /// 按 scopeId 前缀过滤所有 buffer 中的 steps（Dispose 时调用）
        /// </summary>
        private void FilterStepsByScopePrefix(int scopeId)
        {
            var prefix = $"${scopeId}:";
            FilterByPrefix(_undoStack, prefix);
            FilterByPrefix(_redoStack, prefix);
            FilterByPrefix(_pendingSteps, prefix);
            foreach (var buffer in _groupStack)
                FilterByPrefix(buffer, prefix);
        }

        /// <summary>
        /// 按 scopeId 前缀移除所有 setter 和 merge 时间戳（Dispose 时调用）
        /// </summary>
        private void RemoveSettersByScope(int scopeId)
        {
            var prefix = $"${scopeId}:";
            var keysToRemove = new List<string>();
            foreach (var kvp in _setters)
            {
                if (kvp.Key.StartsWith(prefix))
                    keysToRemove.Add(kvp.Key);
            }
            foreach (var key in keysToRemove)
            {
                _setters.Remove(key);
                _lastRecordTime.Remove(key);
            }
        }

        private static void FilterByPrefix(List<IStep> steps, string prefix)
        {
            for (int i = steps.Count - 1; i >= 0; i--)
            {
                var filtered = FilterStepByPrefix(steps[i], prefix);
                if (filtered == null)
                    steps.RemoveAt(i);
                else if (!ReferenceEquals(filtered, steps[i]))
                    steps[i] = filtered;
            }
        }

        private static IStep FilterStepByPrefix(IStep step, string prefix)
        {
            return step switch
            {
                ValueStep vs => vs.Key.StartsWith(prefix) ? null : vs,
                ObjectDiffStep ods => ods.Key.StartsWith(prefix) ? null : ods,
                GroupStep gs => FilterGroupByPrefix(gs, prefix),
                _ => step
            };
        }

        private static IStep FilterGroupByPrefix(GroupStep gs, string prefix)
        {
            var remaining = new List<IStep>();
            foreach (var child in gs.Steps)
            {
                var filtered = FilterStepByPrefix(child, prefix);
                if (filtered != null) remaining.Add(filtered);
            }
            if (remaining.Count == 0) return null;
            if (remaining.Count == gs.Steps.Count) return gs; // 无变化，保留原引用
            return new GroupStep(gs.Key, remaining);
        }

        // ─── Clear ───

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            _pendingSteps.Clear();
            _groupStack.Clear();
            _lastRecordTime.Clear();
        }

        public void ClearRedo()
        {
            _redoStack.Clear();
        }

        // ─── 内部类型 ───

        internal struct SetterEntry
        {
            public Type ValueType;
            public Func<object, object> BoxedSetter;
            public int OwnerScopeId; // 注册此 key 的 scopeId（0 表示全局）
        }

        private class ScopeInfo
        {
            public int Id;
            public int ParentId;
            public List<int> Children;
        }
    }
}
