using System;
using System.Collections;
using System.Collections.Generic;

namespace UniDecl.BuiltIn.Runtime.Snapshot
{
    /// <summary>
    /// Dictionary 特化展开——按 key 展开子绑定（path[key]），key 天然稳定。
    /// key 集合变化（增删）→ 容器整体记录一个 step（旧值为 key/元素深拷贝字典），恢复时整体替换；
    /// key 稳定 → 逐元素基线对比。
    /// </summary>
    public class DictSnapshotBinding : SnapshotBinding
    {
        private HashSet<object> _lastKeys; // 首次 Commit 时初始化为当前 keys

        public DictSnapshotBinding(ISnapshotManager manager, int scopeId, string path,
            Func<object> getter, SnapshotSetter setter = null)
            : base(manager, scopeId, path, getter, setter)
        {
            var dict = getter() as IDictionary;
            _lastKeys = CollectKeys(dict);  // 构造即基线，首 Commit 不误判
            _baseline = Snapshot(dict);     // 整体基线（旧值快照），key 变化时作为 undo 值
        }

        internal DictSnapshotBinding(ISnapshotManager manager, int scopeId, string path,
            Func<object> getter, SnapshotSetter setter, HashSet<object> visited)
            : base(manager, scopeId, path, getter, setter, visited)
        {
            var dict = getter() as IDictionary;
            _lastKeys = CollectKeys(dict);
            _baseline = Snapshot(dict);
        }

        private IDictionary DictGetter() => _getter() as IDictionary;

        protected override void ExpandObject(object value, List<ISnapshotBinding> sink, HashSet<object> visited)
        {
            if (!(value is IDictionary dict)) return;
            foreach (DictionaryEntry entry in dict)
                sink.Add(CreateElementBinding(dict, entry.Key, visited));
        }

        public override void Commit()
        {
            if (!_active) return;
            if (_manager.IsRestoring)
                throw new InvalidOperationException(
                    $"Cannot commit binding '{_path}' during undo/redo restore.");
            var dict = DictGetter();
            if (dict == null) return;

            var currentKeys = CollectKeys(dict);
            if (!_lastKeys.SetEquals(currentKeys))
            {
                RecordWholeDict(dict);
                _lastKeys = currentKeys;
                RebuildChildren(dict);
                _manager.CommitPending();
                return;
            }
            base.Commit(); // key 稳定：递归元素对比（自动组）
        }

        private static HashSet<object> CollectKeys(IDictionary dict)
        {
            var keys = new HashSet<object>();
            foreach (DictionaryEntry entry in dict)
                keys.Add(entry.Key);
            return keys;
        }

        private static Dictionary<object, object> Snapshot(IDictionary dict)
        {
            if (dict == null) return null;
            var snapshot = new Dictionary<object, object>();
            foreach (DictionaryEntry entry in dict)
                snapshot[entry.Key] = DeepCopyUtility.DeepCopy(entry.Value);
            return snapshot;
        }

        private void RecordWholeDict(IDictionary dict)
        {
            var snapshot = Snapshot(dict);
            _manager.RecordValue(_baseline, Id, _path, _scopeId); // 旧值 = 上次提交的整体基线
            _baseline = snapshot;
        }

        private void RebuildChildren(IDictionary dict)
        {
            foreach (var child in _children) child.Dispose();
            _children.Clear();
            var visited = new HashSet<object>();
            foreach (DictionaryEntry entry in dict)
                _children.Add(CreateElementBinding(dict, entry.Key, visited));
        }

        private ISnapshotBinding CreateElementBinding(IDictionary dict, object key, HashSet<object> visited)
        {
            var elementPath = $"{_path}[{key}]";
            return CreateValueBinding(dict[key], elementPath, visited,
                () => DictGetter()[key],
                (restore, current, changes) => DictGetter()[key] = restore);
        }

        /// <summary>整体恢复：框架默认替换（Clear + 重建），再通知用户 setter</summary>
        public override object Restore(object value, ChangeSet changes)
        {
            var current = _getter();
            changes.Add(_path, current, value);
            if (current is IDictionary dict)
            {
                dict.Clear();
                if (value is IDictionary snapshot)
                    foreach (DictionaryEntry entry in snapshot)
                        dict[entry.Key] = entry.Value;
            }
            _setter?.Invoke(value, current, changes);
            _baseline = value;
            if (current is IDictionary rebuilt)
            {
                _lastKeys = CollectKeys(rebuilt); // 恢复后重建 keys 基线，防误判
                RebuildChildren(rebuilt);
            }
            Parent?.Notify(changes);
            return current;
        }
    }
}
