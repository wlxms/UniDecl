using System;
using System.Collections.Generic;
using System.Reflection;
using UniDecl.Inspector.Runtime;
using UniDecl.Runtime.Widgets;
using UnityEngine;

namespace UniDecl.Inspector.Editor
{
    /// <summary>
    /// 类型→Widget 映射器
    /// 将字段类型映射到对应的 UniDecl Widget Element 类型
    /// 
    /// 覆盖规则：
    /// 1. 属性覆盖优先：[Range] → Slider, [Dropdown] → Dropdown Widget 等
    /// 2. 无覆盖属性时按字段类型映射：int → IntegerField, float → FloatField 等
    /// 3. 未识别类型 → Label（显示类型名）
    /// </summary>
    public static class TypeWidgetMapper
    {
        /// <summary>
        /// 根据字段信息和属性决定应该使用哪个 Element 创建函数
        /// 返回 null 表示无法映射（应该不会发生）
        /// </summary>
        public static WidgetFactory.ElementCreator MapToCreator(FieldInfo field, InspectorAttribute[] attrs)
        {
            var fieldType = field.FieldType;

            // ---- 属性覆盖优先 ----

            // [Button] → 按钮替换字段编辑器
            if (GetAttribute<ButtonAttribute>(attrs) != null)
                return WidgetFactory.ElementCreators.Button;

            // [Range] → Slider（float/int）
            var rangeAttr = GetAttribute<UniDecl.Inspector.Runtime.RangeAttribute>(attrs);
            if (rangeAttr != null)
            {
                if (fieldType == typeof(int))
                    return (name, val, target, renderer) => new Slider(name, Convert.ToSingle(val), (float)rangeAttr.Min, (float)rangeAttr.Max);
                return (name, val, target, renderer) => new Slider(name, Convert.ToSingle(val), (float)rangeAttr.Min, (float)rangeAttr.Max);
            }

            // [MinMaxSlider] → MinMaxSlider（Vector2）
            var minMaxAttr = GetAttribute<MinMaxSliderAttribute>(attrs);
            if (minMaxAttr != null && fieldType == typeof(Vector2))
                return (name, val, target, renderer) => new MinMaxSlider(name,
                    ((Vector2)val).x, ((Vector2)val).y, (float)minMaxAttr.Min, (float)minMaxAttr.Max);

            // [TextArea] → ResizableTextArea
            if (GetAttribute<UniDecl.Inspector.Runtime.TextAreaAttribute>(attrs) != null && fieldType == typeof(string))
                return WidgetFactory.ElementCreators.TextArea;

            // [EnumToggleButtons] → 自定义枚举按钮组
            if (GetAttribute<EnumToggleButtonsAttribute>(attrs) != null && fieldType.IsEnum)
                return WidgetFactory.ElementCreators.EnumToggleButtons;

            // ---- 按字段类型映射 ----

            // 数值类型
            if (fieldType == typeof(int)) return WidgetFactory.ElementCreators.IntField;
            if (fieldType == typeof(float)) return WidgetFactory.ElementCreators.FloatField;
            if (fieldType == typeof(double)) return WidgetFactory.ElementCreators.DoubleField;
            if (fieldType == typeof(long)) return WidgetFactory.ElementCreators.LongField;

            // 字符串
            if (fieldType == typeof(string)) return WidgetFactory.ElementCreators.TextField;

            // 布尔
            if (fieldType == typeof(bool)) return WidgetFactory.ElementCreators.Toggle;

            // 枚举
            if (fieldType.IsEnum) return WidgetFactory.ElementCreators.EnumField;

            // Unity 类型
            if (fieldType == typeof(Vector2)) return WidgetFactory.ElementCreators.Vector2Field;
            if (fieldType == typeof(Vector3)) return WidgetFactory.ElementCreators.Vector3Field;
            if (fieldType == typeof(Vector4)) return WidgetFactory.ElementCreators.Vector4Field;
            if (fieldType == typeof(Vector2Int)) return WidgetFactory.ElementCreators.Vector2IntField;
            if (fieldType == typeof(Vector3Int)) return WidgetFactory.ElementCreators.Vector3IntField;
            if (fieldType == typeof(Color)) return WidgetFactory.ElementCreators.ColorField;
            if (fieldType == typeof(Rect)) return WidgetFactory.ElementCreators.RectField;
            if (fieldType == typeof(RectInt)) return WidgetFactory.ElementCreators.RectIntField;
            if (fieldType == typeof(Bounds)) return WidgetFactory.ElementCreators.BoundsField;
            if (fieldType == typeof(BoundsInt)) return WidgetFactory.ElementCreators.BoundsIntField;
            if (fieldType == typeof(AnimationCurve)) return WidgetFactory.ElementCreators.CurveField;
            if (fieldType == typeof(Gradient)) return WidgetFactory.ElementCreators.GradientField;
            if (fieldType == typeof(LayerMask)) return WidgetFactory.ElementCreators.LayerField;

            // UnityEngine.Object 引用
            if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
            {
                // 捕获字段类型用于 ObjectField.ObjectType
                return (name, val, target, renderer) => new ObjectField(name, fieldType, val as UnityEngine.Object);
            }

            // List<T> / Array → 用 InspectorElement 显示（简化版，后续可用 ListView）
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
                return WidgetFactory.ElementCreators.FallbackLabel;

            if (fieldType.IsArray)
                return WidgetFactory.ElementCreators.FallbackLabel;

            // 嵌套 [Serializable]
            if (fieldType.GetCustomAttribute<SerializableAttribute>() != null ||
                (fieldType.IsValueType && fieldType.IsSerializable))
                return WidgetFactory.ElementCreators.NestedInspector;

            // 未识别 → Label（显示类型名）
            return WidgetFactory.ElementCreators.FallbackLabel;
        }

        private static T GetAttribute<T>(InspectorAttribute[] attrs) where T : InspectorAttribute
        {
            for (int i = 0; i < attrs.Length; i++)
            {
                if (attrs[i] is T t) return t;
            }
            return null;
        }
    }
}
