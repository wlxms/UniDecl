using System;
using System.Collections.Generic;
using System.Reflection;

namespace UniDecl.Inspector.Editor
{
    /// <summary>
    /// 字段值快照——用于外源变更检测
    /// 对 target 的指定字段集合拍摄快照，对比差异
    /// </summary>
    public class FieldSnapshot
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        /// <summary>
        /// 从 target 的指定字段中拍摄快照
        /// </summary>
        public static FieldSnapshot Take(object target, List<FieldInfo> fields)
        {
            if (target == null) return null;

            var snapshot = new FieldSnapshot();
            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                try
                {
                    var value = field.GetValue(target);
                    snapshot._values[field.Name] = value;
                }
                catch
                {
                    // 无法读取的字段跳过
                }
            }
            return snapshot;
        }

        /// <summary>
        /// 检查当前 target 的字段值是否与此快照不同
        /// </summary>
        public bool DiffersFrom(object target, List<FieldInfo> fields)
        {
            if (target == null) return _values.Count > 0;

            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                if (!_values.TryGetValue(field.Name, out var oldVal))
                    return true; // 新字段出现

                try
                {
                    var newVal = field.GetValue(target);
                    if (!AreEqual(oldVal, newVal))
                        return true;
                }
                catch
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取发生变更的字段名列表
        /// </summary>
        public List<string> GetChangedFields(object target, List<FieldInfo> fields)
        {
            var changed = new List<string>();
            if (target == null) return changed;

            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                if (!_values.TryGetValue(field.Name, out var oldVal))
                {
                    changed.Add(field.Name);
                    continue;
                }

                try
                {
                    var newVal = field.GetValue(target);
                    if (!AreEqual(oldVal, newVal))
                        changed.Add(field.Name);
                }
                catch
                {
                    changed.Add(field.Name);
                }
            }
            return changed;
        }

        private static bool AreEqual(object a, object b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            return a.Equals(b);
        }
    }
}
