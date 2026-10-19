using System;
using System.Reflection;
using UniDecl.BuiltIn.Runtime.Core;

namespace UniDecl.PropertyGrid.Editor.Elements
{
    /// <summary>
    /// 编辑器值同步——外部（按钮回调、校验器等）绕过编辑器直接修改数据后，
    /// 把 accessor 当前值回写到编辑器元素的 Value 属性，驱动界面刷新。
    /// 用户正在编辑的场景天然安全：自身触发的 FieldChanged 回读到的是同一值，判等跳过。
    /// </summary>
    internal static class EditorValueSync
    {
        /// <summary>若 editor 暴露可写 Value 属性且与 accessor 当前值不一致，则回写并请求重渲染。</summary>
        public static void Sync(IElement editor, PropertyAccessor accessor)
        {
            if (editor == null || accessor == null) return;

            try
            {
                var prop = editor.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                if (prop == null || !prop.CanWrite) return;

                var current = accessor.GetValue();
                if (current == null) return;

                var converted = ConvertTo(current, prop.PropertyType);
                if (converted == null) return;

                if (!Equals(prop.GetValue(editor), converted))
                {
                    prop.SetValue(editor, converted);
                    (editor as Element)?.NotifyChanged();
                }
            }
            catch
            {
                // 同步是尽力而为的显示补偿，任何类型不匹配都静默跳过
            }
        }

        /// <summary>枚举 → 底层 int、数值互转等常见转换；不可转换返回 null。</summary>
        static object ConvertTo(object value, Type targetType)
        {
            if (targetType.IsInstanceOfType(value)) return value;
            try
            {
                if (targetType == typeof(int) && value is Enum e) return Convert.ToInt32(e);
                if (targetType.IsEnum && value is int i) return Enum.ToObject(targetType, i);
                if (value is IConvertible) return Convert.ChangeType(value, targetType);
            }
            catch { }
            return null;
        }
    }
}
