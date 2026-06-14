using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace UniDecl.Snapshot
{
    /// <summary>
    /// 深拷贝工具——为 ObjectDiffStep 提供对象字段的深拷贝和恢复能力
    ///
    /// 已知限制：Add+Remove 树回溯模式下，同一对象被多个字段引用时（DAG），
    /// 深拷贝会产生多个独立副本。Undo 后这些字段不再指向同一对象，引用共享关系丢失。
    /// 对绝大多数实际场景（无共享引用的普通对象图）此限制可接受。
    /// 如未来需保持引用共享，需改为 Dictionary&lt;object, object&gt; 全局映射表模式。
    /// </summary>
    public static class DeepCopyUtility
    {
        private const BindingFlags FieldBindingFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>
        /// 对目标对象的所有实例字段做深拷贝，返回字段名→深拷贝值的字典
        /// </summary>
        public static Dictionary<string, object> SnapshotFields(object target)
        {
            var result = new Dictionary<string, object>();
            if (target == null) return result;

            var visited = new HashSet<object>();
            foreach (var field in target.GetType().GetFields(FieldBindingFlags))
            {
                var value = field.GetValue(target);
                result[field.Name] = DeepCopyValue(value, visited);
            }
            return result;
        }

        /// <summary>
        /// 从快照字典恢复目标对象的字段值
        /// </summary>
        public static void RestoreFields(object target, Dictionary<string, object> snapshots)
        {
            if (target == null || snapshots == null) return;

            foreach (var field in target.GetType().GetFields(FieldBindingFlags))
            {
                if (snapshots.TryGetValue(field.Name, out var value))
                    field.SetValue(target, value);
            }
        }

        /// <summary>
        /// 递归深拷贝值
        /// - null / 值类型 / string: 直接返回（装箱即深拷贝）
        /// - 引用类型: 创建未初始化实例 + 递归拷贝所有字段
        /// - 循环引用: visited 检测后返回 null 截断
        /// </summary>
        private static object DeepCopyValue(object value, HashSet<object> visited)
        {
            if (value == null) return null;

            var type = value.GetType();
            if (type.IsValueType || type == typeof(string))
                return value;

            if (visited.Contains(value))
                return null; // 循环引用截断

            visited.Add(value);

            var copy = FormatterServices.GetUninitializedObject(type);
            foreach (var field in type.GetFields(FieldBindingFlags))
            {
                var fieldValue = field.GetValue(value);
                field.SetValue(copy, DeepCopyValue(fieldValue, visited));
            }

            visited.Remove(value);
            return copy;
        }
    }
}
