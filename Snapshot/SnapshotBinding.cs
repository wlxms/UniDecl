using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace UniDecl.Snapshot
{
    /// <summary>
    /// 单轨快照绑定——统一叶子与对象展开。
    /// 叶子：值类型 / string / Unity 对象 / 循环截断引用，setter 写回（用户或框架提供）。
    /// 对象：自动按字段展开子绑定（反射读写），自身 setter 为容器通知型（restore=null，
    ///       current=当前对象引用，changes=子树聚合清单）。
    /// 字段中的 List/Dict 自动特化为 ListSnapshotBinding / DictSnapshotBinding。
    /// 自定义拓展：继承并覆写 IsLeaf / ExpandObject。
    /// </summary>
    public class SnapshotBinding : ISnapshotBinding
    {
        internal const BindingFlags FieldFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        protected readonly ISnapshotManager _manager;
        protected readonly int _scopeId;
        protected readonly string _path;
        protected readonly Func<object> _getter;
        protected readonly SnapshotSetter _setter;   // 叶子：写回；容器：通知（restore=null）
        protected readonly List<ISnapshotBinding> _children = new();
        protected readonly bool _active;             // false = 游离绑定（scope 为空，不注册）
        protected object _baseline;
        protected object _deferredBaseline;           // CommitDeferred 手势起点值（Flush 后清空）
        protected bool _disposed;

        public Guid Id { get; } = Guid.NewGuid();
        public string Path => _path;
        public int ScopeId => _scopeId;

        /// <summary>
        /// 构造绑定。getter 返回叶子值（值类型/string/Unity 对象）→ 叶子节点，setter 用于写回；
        /// 返回普通对象 → 自动按字段展开（setter 为容器通知型，仅收到子树变更聚合）。
        /// </summary>
        public SnapshotBinding(ISnapshotManager manager, int scopeId, string path,
            Func<object> getter, SnapshotSetter setter = null)
            : this(manager, scopeId, path, getter, setter, null, true) { }

        /// <summary>
        /// 便捷构造：从 UndoScope 取 manager/scopeId。scope 为 null 时绑定不生效（Commit 无操作）。
        /// </summary>
        public SnapshotBinding(UndoScope scope, string path, Func<object> getter, SnapshotSetter setter = null)
            : this(scope?.Manager, scope?.ScopeId ?? 0, path, getter, setter, null, scope != null) { }

        internal SnapshotBinding(ISnapshotManager manager, int scopeId, string path,
            Func<object> getter, SnapshotSetter setter, HashSet<object> visited, bool active = true)
        {
            _active = active;
            _scopeId = scopeId;
            _path = path ?? "";
            _getter = getter ?? throw new ArgumentNullException(nameof(getter));
            _setter = setter;
            if (!active)
            {
                _manager = null; // 游离绑定：不注册不展开
                return;
            }
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _baseline = getter();

            if (IsLeaf(_baseline) || visited != null && visited.Contains(_baseline))
            {
                // 叶子，或循环引用截断为整体引用叶子（字段级 setter 整体替换）
                _manager.RegisterBinding(this);
                return;
            }

            visited ??= new HashSet<object>();
            visited.Add(_baseline);
            ExpandObject(_baseline, _children, visited);
            _manager.RegisterBinding(this);
        }

        internal SnapshotBinding Parent { get; set; }

        // ─── 引用漂移追踪（对象字段被整体替换时退化为整体引用叶子）───
        // 对象字段（如 parent.Child = new Child()）替换引用后，子绑定展开的仍是旧对象，
        // 字段级 diff 无法感知引用变化。展开目标引用 + 字段写回 setter 使其可记录/恢复。
        private object _expandedTarget;
        private SnapshotSetter _referenceWriteback;

        /// <summary>登记展开目标引用与字段写回（仅由 CreateValueBinding 的对象字段分支调用）。</summary>
        internal void AttachReferenceTracking(object target, SnapshotSetter writeback)
        {
            _expandedTarget = target;
            _referenceWriteback = writeback;
        }

        /// <summary>
        /// 引用漂移提交 guard：getter 引用已不是展开目标 → 记整体引用 step（旧引用→新引用），
        /// 并按新对象重建子树。返回 true 表示已处理，调用方中止常规提交。
        /// </summary>
        protected bool CommitReferenceDrift()
        {
            if (_referenceWriteback == null) return false;
            var target = _getter();
            if (ReferenceEquals(target, _expandedTarget)) return false;

            _manager.RecordValue(_baseline, Id, _path, _scopeId);
            _baseline = target;
            RebuildChildren(target);
            if (Parent == null) _manager.CommitPending();
            return true;
        }

        /// <summary>
        /// 叶子判定：交由注入的 ISnapshotLeafPolicy 决定。
        /// 默认策略把 null / 值类型 / string 视为叶子；引擎适配层（如 Unity）可注入
        /// 识别引擎对象类型的策略。子类仍可重写以支持自定义展开规则。
        /// </summary>
        protected virtual bool IsLeaf(object value)
        {
            return _manager != null && _manager.IsLeaf(value);
        }

        /// <summary>
        /// 展开点：把对象值展开为子绑定列表（默认字段反射；List/Dict 子类覆写为索引/key）。
        /// </summary>
        protected virtual void ExpandObject(object value, List<ISnapshotBinding> sink, HashSet<object> visited)
        {
            foreach (var field in value.GetType().GetFields(FieldFlags))
            {
                if (field.IsInitOnly) continue; // readonly 不可写回，跳过
                var fieldPath = _path.Length > 0 ? $"{_path}.{field.Name}" : field.Name;
                sink.Add(CreateFieldBinding(field, fieldPath, visited));
            }
        }

        /// <summary>
        /// 构建字段子绑定：字段值按类型分派（叶子 / List / Dict / 对象递归）。
        /// </summary>
        protected virtual ISnapshotBinding CreateFieldBinding(FieldInfo field, string fieldPath, HashSet<object> visited)
        {
            var value = field.GetValue(_getter());
            return CreateValueBinding(value, fieldPath, visited,
                () => field.GetValue(_getter()),
                (restore, current, changes) => field.SetValue(_getter(), restore));
        }

        /// <summary>
        /// 构建值子绑定：按值类型分派。getter/setter 为读写入口（叶子写回 / 容器读引用）。
        /// 容器分支不传 setter——字段写回由子叶子完成，容器 setter 仅用户显式构造时的通知型。
        /// </summary>
        protected ISnapshotBinding CreateValueBinding(object value, string childPath, HashSet<object> visited,
            Func<object> getter, SnapshotSetter setter)
        {
            ISnapshotBinding child;
            if (value == null || IsLeaf(value) || visited.Contains(value))
                child = new SnapshotBinding(_manager, _scopeId, childPath, getter, setter, visited);
            else if (value is IList)
                child = new ListSnapshotBinding(_manager, _scopeId, childPath, getter, null, visited);
            else if (value is IDictionary)
                child = new DictSnapshotBinding(_manager, _scopeId, childPath, getter, null, visited);
            else
            {
                child = new SnapshotBinding(_manager, _scopeId, childPath, getter, null, visited);
                // 对象字段登记引用追踪：字段引用被整体替换时可记/恢复整体引用
                ((SnapshotBinding)child).AttachReferenceTracking(value, setter);
            }
            if (child is SnapshotBinding sb)
                sb.Parent = this; // 恢复时 changeSet 冒泡链
            return child;
        }

        // ─── 提交 ───

        public virtual void Commit()
        {
            if (!_active) return; // 游离绑定（无 scope）
            if (_manager.IsRestoring)
                throw new InvalidOperationException(
                    $"Cannot commit binding '{_path}' during undo/redo restore.");
            if (_children.Count == 0) CommitLeaf();
            else if (CommitReferenceDrift()) { /* 引用被整体替换：整体 step，子树已重建 */ }
            else CommitContainer();
        }

        /// <summary>叶子提交：与基线对比，变更才产生 step</summary>
        protected virtual void CommitLeaf()
        {
            var current = _getter();
            if (Equals(current, _baseline)) return;
            _manager.RecordValue(_baseline, Id, _path, _scopeId);
            _baseline = current;
            if (Parent == null) _manager.CommitPending(); // 叶子独立使用：立即提交
        }

        /// <summary>
        /// 延迟提交（瞬时型控件拖动用）：手势中的连续变化在 binding 层聚合——
        /// 只记住首个基线（手势起点值），不触碰 manager（避免 pending 计数噪音与
        /// Unity Version 不同步）。手势结束（PointerUp 等）调 Flush() 一次成 step。
        /// </summary>
        public void CommitDeferred()
        {
            if (!_active) return;
            if (_manager.IsRestoring)
                throw new InvalidOperationException(
                    $"Cannot commit binding '{_path}' during undo/redo restore.");
            var current = _getter();
            if (Equals(current, _baseline)) return;
            _deferredBaseline ??= _baseline; // 手势起点旧值（整段拖动一个 step 的 undo 值）
            _baseline = current;             // 基线跟进：后续变化不重复记
        }

        /// <summary>提交 deferred（配合 CommitDeferred：手势结束点调用，一次拖动 = 一个 step）</summary>
        public void Flush()
        {
            if (!_active) return;
            if (_deferredBaseline == null) return;
            _manager.RecordValue(_deferredBaseline, Id, _path, _scopeId);
            _deferredBaseline = null;
            _manager.CommitPending();
        }

        /// <summary>打断合并链（新手势开始）：后续提交不与已有 step 按时间窗合并</summary>
        public void BreakMerge()
        {
            if (_active) _manager.BreakMerge(_path, _scopeId);
        }

        /// <summary>容器提交：递归子节点，自动打包组（与手动组嵌套共存）</summary>
        protected virtual void CommitContainer()
        {
            _manager.BeginGroup(_path);
            foreach (var child in _children)
                child.Commit();
            _manager.EndGroup();
            _manager.CommitPending();
        }

        // ─── 恢复（SnapshotManager 调用）───

        /// <summary>恢复（仅 SnapshotManager 调用）：写回旧值并冒泡通知容器，返回当前值（生成 redo step）</summary>
        public virtual object Restore(object value, ChangeSet changes)
        {
            // 引用漂移 step：整体写回旧引用，并按旧对象重建子树
            if (_children.Count > 0 && _referenceWriteback != null && !ReferenceEquals(value, _expandedTarget))
            {
                var currentRef = _getter();
                changes.Add(_path, currentRef, value);
                _referenceWriteback(value, currentRef, changes);
                _baseline = value;
                RebuildChildren(value);
                Parent?.Notify(changes);
                return currentRef;
            }

            var current = _getter();
            changes.Add(_path, current, value); // 追加自身变更（容器聚合由此积累）
            _setter?.Invoke(value, current, changes);
            _baseline = value; // 基线同步：恢复后再 Commit 不误判
            Parent?.Notify(changes);
            return current;
        }

        /// <summary>按新目标对象重建子树（引用漂移后子绑定指向新对象）。</summary>
        private void RebuildChildren(object target)
        {
            foreach (var child in _children) child.Dispose();
            _children.Clear();
            if (target == null) { _expandedTarget = null; return; }
            var visited = new HashSet<object> { target };
            ExpandObject(target, _children, visited);
            _expandedTarget = target;
        }

        /// <summary>容器通知：收到子树变更聚合，向上冒泡</summary>
        internal void Notify(ChangeSet changes)
        {
            if (_children.Count > 0)
                _setter?.Invoke(null, _getter(), changes); // 容器通知型 setter：restore=null
            Parent?.Notify(changes);
        }

        public virtual void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            foreach (var child in _children)
                child.Dispose();
            if (_active)
                _manager.UnregisterBinding(Id);
        }
    }
}
