using System;
using System.Collections.Generic;

namespace UniDecl.BuiltIn.Runtime.Snapshot
{
    /// <summary>
    /// UniSnap 核心——单轨 Undo/Redo 管理器。
    /// 绑定树（SnapshotBinding）统一注册（弱引用）/提交/撤销，step 统一为 SnapshotStep。
    /// 防重入：Undo/Redo 期间 IsRestoring=true，此时 Commit 抛异常。
    /// 惰性清理：每次操作扫描失效弱引用（视图销毁的 binding），自动反注册并清理其历史。
    /// </summary>
    public class SnapshotManager : ISnapshotManager
    {
        private const int DefaultMaxSteps = 50;
        private const long DefaultMergeWindowMs = 500;

        private int _nextScopeId = 1;
        private readonly Dictionary<int, ScopeInfo> _scopes = new();
        // 绑定强引用持有——生命周期由显式 Dispose/DisposeScope 清理。
        // 不用弱引用：Renderer 创建的 binding 无强引用会被过早 GC，导致 step 无法定位而 undo 失效。
        private readonly Dictionary<Guid, ISnapshotBinding> _bindings = new();
        private readonly Dictionary<string, ISnapshotBinding> _bindingsByPath = new(); // Path → 当代 binding（换代覆盖）
        private readonly Dictionary<int, HashSet<Guid>> _scopeBindings = new();

        private readonly List<IStep> _undoStack = new();
        private readonly List<IStep> _redoStack = new();
        private readonly Stack<List<IStep>> _groupStack = new();
        private readonly List<IStep> _pendingSteps = new();
        private readonly Dictionary<Guid, long> _lastRecordTime = new();
        private readonly Dictionary<string, long> _mergeBreakTimes = new(); // "scopeId:path" → 手势打断时刻

        private bool _restoring;

        public int MaxSteps { get; set; } = DefaultMaxSteps;
        public bool EnableMerge { get; set; } = true;
        public long MergeWindowMs { get; set; } = DefaultMergeWindowMs;
        public bool IsRestoring => _restoring;

        // ─── 事件 ───

        public event Action<IStep> StepCommitted;
        public event Action<IStep> StepUndone;
        public event Action<IStep> StepRedone;
        public event Action<int> ScopeDisposed;
        public event Action<ChangeSet> OnUndoRedoPerformed;

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
        /// Dispose Scope：移除其下所有绑定注册 + steps，级联子 Scope。
        /// </summary>
        public void DisposeScope(int scopeId)
        {
            if (!_scopes.TryGetValue(scopeId, out var scope)) return;
            _scopes.Remove(scopeId); // 防重入

            // 级联 Dispose 子 Scope
            foreach (var childId in scope.Children.ToArray())
                DisposeScope(childId);

            // 从父 Scope 的 children 中移除自己
            if (scope.ParentId > 0 && _scopes.TryGetValue(scope.ParentId, out var parent))
                parent.Children.Remove(scopeId);

            // 移除该 scope 下所有绑定注册（历史保留：跨结构 rebuild 的 undo 靠 Path 兜底）
            if (_scopeBindings.TryGetValue(scopeId, out var ids))
            {
                foreach (var id in ids)
                {
                    if (_bindings.TryGetValue(id, out var b) &&
                        _bindingsByPath.TryGetValue(b.Path, out var cur) && ReferenceEquals(cur, b))
                        _bindingsByPath.Remove(b.Path);
                    _bindings.Remove(id);
                    _lastRecordTime.Remove(id);
                }
                _scopeBindings.Remove(scopeId);
            }

            ScopeDisposed?.Invoke(scopeId);
        }

        // ─── 绑定注册（SnapshotBinding 调用）───

        public void RegisterBinding(ISnapshotBinding binding)
        {
            _bindings[binding.Id] = binding;
            _bindingsByPath[binding.Path] = binding; // 换代覆盖：step 恢复按 Path 找当代 binding
            if (!_scopeBindings.TryGetValue(binding.ScopeId, out var ids))
                _scopeBindings[binding.ScopeId] = ids = new HashSet<Guid>();
            ids.Add(binding.Id);
        }

        /// <summary>
        /// 反注册绑定：移除注册与 merge 时间戳。历史 steps 保留——
        /// 结构 rebuild（条件显隐等）会让行容器 dispose scope，但 undo 需要跨重建的历史；
        /// 失效 step 的 Guid 找不到时 ApplyStep 按 Path 兜底到当代 binding。
        /// </summary>
        public void UnregisterBinding(Guid bindingId)
        {
            if (_bindings.TryGetValue(bindingId, out var b) &&
                _bindingsByPath.TryGetValue(b.Path, out var cur) && ReferenceEquals(cur, b))
                _bindingsByPath.Remove(b.Path);
            _bindings.Remove(bindingId);
            _lastRecordTime.Remove(bindingId);
        }

        /// <summary>记录一次值变更（创建 SnapshotStep）。合并按 Path+时间窗（可被 BreakMerge 打断）。</summary>
        public void RecordValue(object oldValue, Guid bindingId, string path, int scopeId)
        {
            if (EnableMerge && TryMergeValueStep(bindingId, path, scopeId))
                return;

            var step = new SnapshotStep(bindingId, path, oldValue, scopeId);
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
        /// 提交 pending steps。手动组由 BeginGroup/EndGroup 显式闭合（EndGroup 时组入 pending）。
        /// 返回 true 表示有新 step 提交；false 表示 pending 为空（如 Record 被 Merge）。
        /// </summary>
        public bool CommitPending()
        {
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

            _restoring = true;
            try
            {
                var changes = new ChangeSet();
                var redoStep = ApplyStep(step, changes);
                if (redoStep != null) _redoStack.Add(redoStep);
                StepUndone?.Invoke(redoStep);
                OnUndoRedoPerformed?.Invoke(changes);
            }
            finally
            {
                _restoring = false;
            }
            return true;
        }

        public bool Redo()
        {
            if (_redoStack.Count == 0) return false;

            _restoring = true;
            try
            {
                var step = PopLast(_redoStack);
                var changes = new ChangeSet();
                var undoStep = ApplyStep(step, changes);
                if (undoStep != null)
                {
                    _undoStack.Add(undoStep);
                    TrimStack(_undoStack);
                }
                StepRedone?.Invoke(undoStep);
                OnUndoRedoPerformed?.Invoke(changes);
            }
            finally
            {
                _restoring = false;
            }
            return true;
        }

        /// <summary>
        /// 执行一个 step，聚合变更清单，返回反向 step。
        /// binding 已失效（视图销毁）时跳过。
        /// </summary>
        private IStep ApplyStep(IStep step, ChangeSet changes)
        {
            switch (step)
            {
                case SnapshotStep ss:
                    // Path 始终指向当前视图 binding。重建会生成新 Guid，而旧 binding
                    // 可能仍留在注册表中；优先 Path 才不会把历史恢复写回旧闭包。
                    if (!_bindingsByPath.TryGetValue(ss.Path, out var binding) &&
                        !_bindings.TryGetValue(ss.BindingId, out binding))
                        return null; // 无当代 binding（面板已销毁），跳过
                    var current = binding.Restore(ss.Value, changes);
                    return new SnapshotStep(binding.Id, ss.Path, current, ss.ScopeId);

                case GroupStep gs:
                    var redoSteps = new List<IStep>();
                    for (int i = gs.Steps.Count - 1; i >= 0; i--)
                    {
                        var redoStep = ApplyStep(gs.Steps[i], changes);
                        if (redoStep != null) redoSteps.Insert(0, redoStep);
                    }
                    return redoSteps.Count > 0 ? new GroupStep(gs.Key, redoSteps) : null;

                default: return null;
            }
        }

        // ─── 合并 / buffer / stack ───

        /// <summary>
        /// 合并检查：当前 buffer 末尾或（buffer 空）栈顶，同 Path+Scope 的 SnapshotStep（按 Path 匹配，
        /// 拖动中 rebuild 换代 binding 也不受影响）。时间窗内且栈顶 step 属于当前手势
        /// （晚于最近一次 BreakMerge）才合并——手势间隔离，手势内聚合。
        /// </summary>
        private bool TryMergeValueStep(Guid bindingId, string path, int scopeId)
        {
            var buffer = CurrentBuffer;
            SnapshotStep last = null;
            if (buffer.Count > 0 && buffer[buffer.Count - 1] is SnapshotStep s1)
                last = s1;
            else if (buffer.Count == 0 && _undoStack.Count > 0 && _undoStack[_undoStack.Count - 1] is SnapshotStep s2)
                last = s2; // 栈顶合并：连续事件流（拖动/拾色）窗口内聚合为起终点；显式提交点由 Renderer 调 BreakMerge 隔离
            if (last == null || last.Path != path)
                return false; // 只按 Path 匹配（insp_字段名 跨 rebuild 稳定；换代会换 binding Guid 与 ScopeId）

            if (!_lastRecordTime.TryGetValue(last.BindingId, out var lastStepTime))
                return false;
            var now = NowTicks();
            if ((now - lastStepTime) * TicksToMs > MergeWindowMs)
                return false; // 时间窗外

            // 手势隔离：栈顶 step 产生于最近一次 BreakMerge 之前 → 属于上一手势，不合并
            if (_mergeBreakTimes.TryGetValue(scopeId + ":" + path, out var breakTime) && lastStepTime <= breakTime)
                return false;

            _lastRecordTime[bindingId] = now;
            return true; // 保留最早旧值，不做任何修改
        }

        /// <summary>打断合并链（新手势开始，如 Slider PointerDown）：时间窗内后续记录不与已有 step 合并</summary>
        public void BreakMerge(string path, int scopeId)
        {
            _mergeBreakTimes[scopeId + ":" + path] = NowTicks();
        }

        // 高分辨率时间戳（Stopwatch/QPC 纳秒级）——DateTime.UtcNow 在 Windows 粒度 ~10ms，
        // 无法区分手势内 Down→Change 的先后边界
        private static long NowTicks() => System.Diagnostics.Stopwatch.GetTimestamp();
        private static readonly double TicksToMs = 1000.0 / System.Diagnostics.Stopwatch.Frequency;

        private List<IStep> CurrentBuffer =>
            _groupStack.Count > 0 ? _groupStack.Peek() : _pendingSteps;

        private void AddToCurrentBuffer(IStep step)
        {
            CurrentBuffer.Add(step);
            if (step is SnapshotStep ss)
                _lastRecordTime[ss.BindingId] = NowTicks();
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

        // ─── step 过滤 ───

        private static bool MatchBinding(IStep step, Guid bindingId) =>
            step is SnapshotStep ss && ss.BindingId == bindingId;

        private static bool MatchScope(IStep step, int scopeId) =>
            step is SnapshotStep ss && ss.ScopeId == scopeId;

        /// <summary>按谓词过滤 steps（GroupStep 递归，剩余为空则整体移除）</summary>
        private static void FilterSteps(List<IStep> steps, Func<IStep, bool> remove)
        {
            for (int i = steps.Count - 1; i >= 0; i--)
            {
                var filtered = FilterStep(steps[i], remove);
                if (filtered == null)
                    steps.RemoveAt(i);
                else if (!ReferenceEquals(filtered, steps[i]))
                    steps[i] = filtered;
            }
        }

        private static IStep FilterStep(IStep step, Func<IStep, bool> remove)
        {
            if (step is not GroupStep gs)
                return remove(step) ? null : step;

            var remaining = new List<IStep>();
            foreach (var child in gs.Steps)
            {
                var filtered = FilterStep(child, remove);
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

        private class ScopeInfo
        {
            public int Id;
            public int ParentId;
            public List<int> Children;
        }
    }
}
