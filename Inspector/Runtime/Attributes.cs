using System;
using System.Diagnostics;

namespace UniDecl.Inspector.Runtime
{
    // =========================================================================
    // 基类
    // =========================================================================

    /// <summary>
    /// 所有 Inspector 属性标记的抽象基类
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public abstract class InspectorAttribute : Attribute { }

    // =========================================================================
    // §4.1 类级：预定义分组 + 类级控件
    // =========================================================================

    /// <summary>
    /// 声明一个带标题的折叠组（类级元数据声明，不产生渲染效果）
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class HeaderGroupAttribute : LayoutGroupAttribute
    {
        public HeaderGroupAttribute(string path) : base(path) { }
    }

    /// <summary>
    /// 声明一个带边框的区块（类级元数据声明，不产生渲染效果）
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class BoxGroupAttribute : LayoutGroupAttribute
    {
        public BoxGroupAttribute(string path) : base(path) { }
    }

    /// <summary>
    /// 声明一个可折叠的组（类级元数据声明，不产生渲染效果）
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class FoldoutGroupAttribute : LayoutGroupAttribute
    {
        public bool Expanded { get; set; } = true;
        public FoldoutGroupAttribute(string path) : base(path) { }
    }

    /// <summary>
    /// 声明一个标签页容器（类级元数据声明，不产生渲染效果）
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class TabGroupAttribute : LayoutGroupAttribute
    {
        public TabGroupAttribute(string path) : base(path) { }
    }

    /// <summary>
    /// 类级/字段级按钮——声明一个按钮，点击回调 Renderer 的 method 方法。
    /// 字段级使用时替换字段编辑器为按钮。
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class ButtonAttribute : InspectorAttribute
    {
        public string Label { get; }
        public string Method { get; }
        public string GroupBy { get; set; }
        public int Order { get; set; }
        public ButtonAttribute(string label, string method) { Label = label; Method = method; }
    }

    // =========================================================================
    // §4.2 字段级：布局（传染式）
    // =========================================================================

    /// <summary>
    /// 布局组属性基类——HGroup/VGroup/BoxGroup/FoldoutGroup/HeaderGroup/TabGroup 的共同基类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public abstract class LayoutGroupAttribute : InspectorAttribute
    {
        public string Path { get; }
        public string Title { get; set; }
        public int Order { get; set; }
        protected LayoutGroupAttribute(string path) { Path = path; }
    }

    /// <summary>
    /// 启动水平排列，后续字段自动加入同一行；空 path 自动命名组
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class HGroupAttribute : LayoutGroupAttribute
    {
        public HGroupAttribute() : base(null) { }
        public HGroupAttribute(string path) : base(path) { }
    }

    /// <summary>
    /// 启动垂直排列，后续字段自动垂直排序；空 path 自动命名组
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class VGroupAttribute : LayoutGroupAttribute
    {
        public VGroupAttribute() : base(null) { }
        public VGroupAttribute(string path) : base(path) { }
    }

    /// <summary>
    /// 在组内指定排序位置，越小越靠前
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class PropertyOrderAttribute : InspectorAttribute
    {
        public int Order { get; }
        public PropertyOrderAttribute(int order) { Order = order; }
    }

    // =========================================================================
    // §4.3 字段级：显示
    // =========================================================================

    /// <summary>
    /// 自定义标签文本；@ 前缀运行时查 Renderer/数据类
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class LabelTextAttribute : InspectorAttribute
    {
        public string Text { get; }
        public LabelTextAttribute(string text) { Text = text; }
    }

    /// <summary>
    /// 隐藏标签
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class HideLabelAttribute : InspectorAttribute { }

    /// <summary>
    /// 悬停提示
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class TooltipAttribute : InspectorAttribute
    {
        public string Text { get; }
        public TooltipAttribute(string text) { Text = text; }
    }

    /// <summary>
    /// 编辑控件后追加文字（如 "m/s"）
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class SuffixLabelAttribute : InspectorAttribute
    {
        public string Text { get; }
        public SuffixLabelAttribute(string text) { Text = text; }
    }

    /// <summary>
    /// 块级标题
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class TitleAttribute : InspectorAttribute
    {
        public string Text { get; }
        public TitleAttribute(string text) { Text = text; }
    }

    /// <summary>
    /// 分割线
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class DividerAttribute : InspectorAttribute { }

    /// <summary>
    /// 字段上下间距（px）
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class SpaceAttribute : InspectorAttribute
    {
        public int Before { get; set; }
        public int After { get; set; }
        public SpaceAttribute(int before = 0, int after = 0) { Before = before; After = after; }
    }

    /// <summary>
    /// 提示信息框
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class InfoBoxAttribute : InspectorAttribute
    {
        public string Text { get; }
        public InfoBoxType Type { get; set; } = InfoBoxType.Info;
        public string GroupBy { get; set; }
        public int Order { get; set; }
        public InfoBoxAttribute(string text) { Text = text; }
    }

    /// <summary>
    /// 信息框类型
    /// </summary>
    public enum InfoBoxType { Info, Warning, Error }

    /// <summary>
    /// 只读字段
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ReadOnlyAttribute : InspectorAttribute { }

    /// <summary>
    /// 字段颜色
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class GUIColorAttribute : InspectorAttribute
    {
        public float R { get; }
        public float G { get; }
        public float B { get; }
        public float A { get; }
        public GUIColorAttribute(float r, float g, float b, float a = 1f) { R = r; G = g; B = b; A = a; }
    }

    /// <summary>
    /// 缩进级别
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class IndentAttribute : InspectorAttribute
    {
        public int Level { get; }
        public IndentAttribute(int level = 1) { Level = level; }
    }

    /// <summary>
    /// Editor 运行模式下隐藏
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class HideInPlayModeAttribute : InspectorAttribute { }

    /// <summary>
    /// Editor 运行模式下禁用
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class DisableInPlayModeAttribute : InspectorAttribute { }

    // =========================================================================
    // §4.4 字段级：Flex 对齐
    // =========================================================================

    /// <summary>
    /// 水平行中占用剩余空间的 n 份比例
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class FlexGrowAttribute : InspectorAttribute
    {
        public int Value { get; }
        public FlexGrowAttribute(int value = 1) { Value = value; }
    }

    /// <summary>
    /// 固定像素宽度
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class WidthAttribute : InspectorAttribute
    {
        public float Value { get; }
        public WidthAttribute(float value) { Value = value; }
    }

    /// <summary>
    /// 水平行中靠右对齐
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class AlignRightAttribute : InspectorAttribute { }

    /// <summary>
    /// 水平行中居中对齐
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class AlignCenterAttribute : InspectorAttribute { }

    // =========================================================================
    // §4.5 字段级：数值约束
    // =========================================================================

    /// <summary>
    /// 显示为滑块，值限制在范围内
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class RangeAttribute : InspectorAttribute
    {
        public double Min { get; }
        public double Max { get; }
        public RangeAttribute(double min, double max) { Min = min; Max = max; }
    }

    /// <summary>
    /// 最小值约束
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class MinValueAttribute : InspectorAttribute
    {
        public double Value { get; }
        public MinValueAttribute(double value) { Value = value; }
    }

    /// <summary>
    /// 最大值约束
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class MaxValueAttribute : InspectorAttribute
    {
        public double Value { get; }
        public MaxValueAttribute(double value) { Value = value; }
    }

    /// <summary>
    /// 双端滑块（用于 Vector2）
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class MinMaxSliderAttribute : InspectorAttribute
    {
        public double Min { get; }
        public double Max { get; }
        public MinMaxSliderAttribute(double min, double max) { Min = min; Max = max; }
    }

    /// <summary>
    /// 步进值
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class StepAttribute : InspectorAttribute
    {
        public double Value { get; }
        public StepAttribute(double value) { Value = value; }
    }

    /// <summary>
    /// 越界回绕
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class WrapAttribute : InspectorAttribute
    {
        public double Min { get; }
        public double Max { get; }
        public WrapAttribute(double min, double max) { Min = min; Max = max; }
    }

    // =========================================================================
    // §4.6 字段级：条件控制
    // =========================================================================

    /// <summary>
    /// 条件为真时显示字段
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class ShowIfAttribute : InspectorAttribute
    {
        public string Member { get; }
        public object Value { get; }
        public ShowIfAttribute(string member) { Member = member; }
        public ShowIfAttribute(string member, object value) { Member = member; Value = value; }
    }

    /// <summary>
    /// 条件为真时隐藏字段
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class HideIfAttribute : InspectorAttribute
    {
        public string Member { get; }
        public object Value { get; }
        public HideIfAttribute(string member) { Member = member; }
        public HideIfAttribute(string member, object value) { Member = member; Value = value; }
    }

    /// <summary>
    /// 条件为真时可编辑
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class EnableIfAttribute : InspectorAttribute
    {
        public string Member { get; }
        public object Value { get; }
        public EnableIfAttribute(string member) { Member = member; }
        public EnableIfAttribute(string member, object value) { Member = member; Value = value; }
    }

    // =========================================================================
    // §4.7 字段级：Editor 绑定（方法引用）
    // =========================================================================

    /// <summary>
    /// 下拉选项来自 Renderer 的同名方法
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class DropdownAttribute : InspectorAttribute
    {
        public string Method { get; }
        public DropdownAttribute(string method) { Method = method; }
    }

    /// <summary>
    /// 验证器来自 Renderer 的同名方法
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ValidateAttribute : InspectorAttribute
    {
        public string Method { get; }
        public ValidateAttribute(string method) { Method = method; }
    }

    /// <summary>
    /// 值变更时回调 Renderer 的同名方法
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class OnValueChangedAttribute : InspectorAttribute
    {
        public string Method { get; }
        public OnValueChangedAttribute(string method) { Method = method; }
    }

    // =========================================================================
    // §4.8 字段级：资源
    // =========================================================================

    /// <summary>
    /// 对象选择器带方形预览图
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class PreviewFieldAttribute : InspectorAttribute
    {
        public int Height { get; set; } = 64;
    }

    /// <summary>
    /// 文件路径选择对话框
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class FilePathAttribute : InspectorAttribute
    {
        public string Extensions { get; set; }
        public string Parent { get; set; }
    }

    /// <summary>
    /// 文件夹路径选择对话框
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class FolderPathAttribute : InspectorAttribute
    {
        public string Parent { get; set; }
    }

    /// <summary>
    /// 限选项目资源
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class AssetsOnlyAttribute : InspectorAttribute { }

    /// <summary>
    /// 限选场景对象
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class SceneObjectsOnlyAttribute : InspectorAttribute { }

    // =========================================================================
    // §4.9 字段级：枚举/颜色/多行
    // =========================================================================

    /// <summary>
    /// 枚举选项显示为按钮组
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class EnumToggleButtonsAttribute : InspectorAttribute { }

    /// <summary>
    /// 为颜色字段提供预定义调色板
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ColorPaletteAttribute : InspectorAttribute
    {
        public string PaletteName { get; }
        public ColorPaletteAttribute(string paletteName) { PaletteName = paletteName; }
    }

    /// <summary>
    /// 字符串显示为可拉伸多行文本框
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class TextAreaAttribute : InspectorAttribute
    {
        public int MinLines { get; }
        public int MaxLines { get; }
        public TextAreaAttribute(int minLines = 3, int maxLines = 10) { MinLines = minLines; MaxLines = maxLines; }
    }

    // =========================================================================
    // 类级字段无关控件（用户扩展需求）
    // =========================================================================

    /// <summary>
    /// 类级 Label：字段无关的文本标签
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class InspectorLabelAttribute : InspectorAttribute
    {
        public string Text { get; }
        public string GroupBy { get; set; }
        public int Order { get; set; }
        public InspectorLabelAttribute(string text) { Text = text; }
    }

    /// <summary>
    /// 类级 InfoBox：字段无关的提示信息
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class InspectorInfoBoxAttribute : InspectorAttribute
    {
        public string Text { get; }
        public InfoBoxType Type { get; set; } = InfoBoxType.Info;
        public string GroupBy { get; set; }
        public int Order { get; set; }
        public InspectorInfoBoxAttribute(string text) { Text = text; }
    }

    // =========================================================================
    // Renderer 标记
    // =========================================================================

    /// <summary>
    /// 标记一个类为某个数据类型的 Inspector Renderer
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class InspectorRendererAttribute : Attribute
    {
        public Type TargetType { get; }
        public InspectorRendererAttribute(Type targetType) { TargetType = targetType; }
    }
}
