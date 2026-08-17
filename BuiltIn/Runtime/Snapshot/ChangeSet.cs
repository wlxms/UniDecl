using System.Collections.Generic;

namespace UniDecl.BuiltIn.Runtime.Snapshot
{
    /// <summary>
    /// 单条字段变更——Path 供渲染层局部刷新定位，OldValue/NewValue 供展示。
    /// Undo 与 Redo 方向自动对调：undo 时 Old=恢复前、New=恢复后；redo 反之。
    /// </summary>
    public struct FieldChange
    {
        public string Path;
        public object OldValue;
        public object NewValue;

        public FieldChange(string path, object oldValue, object newValue)
        {
            Path = path;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    /// <summary>
    /// 变更清单——一次 Undo/Redo 实际变更的字段集合，由执行过的 step 聚合产生（非 diff）。
    /// </summary>
    public class ChangeSet
    {
        public List<FieldChange> Changes { get; } = new List<FieldChange>();

        public bool IsEmpty => Changes.Count == 0;

        public void Add(string path, object oldValue, object newValue)
            => Changes.Add(new FieldChange(path, oldValue, newValue));
    }
}
