using System;
using System.Collections;
using System.Collections.Generic;

namespace UniDecl.BuiltIn.Runtime.Snapshot
{
    /// <summary>
    /// List 特化展开——按索引展开子绑定（path[i]）。
    /// 元素数量变化 → 容器整体记录一个 step（旧值为元素深拷贝列表），恢复时整体替换；
    /// 数量不变 → 逐元素基线对比（替换元素值）。
    /// </summary>
    public class ListSnapshotBinding : SnapshotBinding
    {
        private int _lastCount = -1; // 首次 Commit 时初始化为当前数量

        public ListSnapshotBinding(ISnapshotManager manager, int scopeId, string path,
            Func<object> getter, SnapshotSetter setter = null)
            : base(manager, scopeId, path, getter, setter)
        {
            var list = getter() as IList;
            _lastCount = list != null ? list.Count : 0; // 构造即基线，首 Commit 不误判
            _baseline = Snapshot(list);                 // 整体基线（旧值快照），数量变化时作为 undo 值
        }

        internal ListSnapshotBinding(ISnapshotManager manager, int scopeId, string path,
            Func<object> getter, SnapshotSetter setter, HashSet<object> visited)
            : base(manager, scopeId, path, getter, setter, visited)
        {
            var list = getter() as IList;
            _lastCount = list != null ? list.Count : 0;
            _baseline = Snapshot(list);
        }

        private IList ListGetter() => _getter() as IList;

        protected override void ExpandObject(object value, List<ISnapshotBinding> sink, HashSet<object> visited)
        {
            if (!(value is IList list)) return;
            for (int i = 0; i < list.Count; i++)
                sink.Add(CreateElementBinding(list, i, visited));
        }

        public override void Commit()
        {
            if (!_active) return;
            if (_manager.IsRestoring)
                throw new InvalidOperationException(
                    $"Cannot commit binding '{_path}' during undo/redo restore.");
            var list = ListGetter();
            if (list == null) return;

            if (list.Count != _lastCount)
            {
                RecordWholeList(list);
                _lastCount = list.Count;
                RebuildChildren(list);
                _manager.CommitPending();
                return;
            }
            base.Commit(); // 数量不变：递归元素对比（自动组）
        }

        private static List<object> Snapshot(IList list)
        {
            if (list == null) return null;
            var snapshot = new List<object>(list.Count);
            foreach (var element in list)
                snapshot.Add(DeepCopyUtility.DeepCopy(element));
            return snapshot;
        }

        private void RecordWholeList(IList list)
        {
            var snapshot = Snapshot(list);
            _manager.RecordValue(_baseline, Id, _path, _scopeId); // 旧值 = 上次提交的整体基线
            _baseline = snapshot;
        }

        private void RebuildChildren(IList list)
        {
            foreach (var child in _children) child.Dispose();
            _children.Clear();
            var visited = new HashSet<object>();
            for (int i = 0; i < list.Count; i++)
                _children.Add(CreateElementBinding(list, i, visited));
        }

        private ISnapshotBinding CreateElementBinding(IList list, int index, HashSet<object> visited)
        {
            var elementPath = $"{_path}[{index}]";
            return CreateValueBinding(list[index], elementPath, visited,
                () => ListGetter()[index],
                (restore, current, changes) => ListGetter()[index] = restore);
        }

        /// <summary>整体恢复：框架默认替换（Clear + 重建），再通知用户 setter</summary>
        public override object Restore(object value, ChangeSet changes)
        {
            var current = _getter();
            changes.Add(_path, current, value);
            if (current is IList list)
            {
                list.Clear();
                if (value is IEnumerable items)
                    foreach (var item in items) list.Add(item);
            }
            _setter?.Invoke(value, current, changes);
            _baseline = value;
            if (current is IList rebuilt)
            {
                _lastCount = rebuilt.Count; // 恢复后重建计数基线，防误判
                RebuildChildren(rebuilt);
            }
            Parent?.Notify(changes);
            return current;
        }
    }
}
