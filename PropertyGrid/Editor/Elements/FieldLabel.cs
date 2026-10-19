using UniDecl.BuiltIn.Runtime.Components;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.BuiltIn.Runtime.Widgets;
using UniDecl.UIToolKit.Runtime;
using UnityEngine.UIElements;

namespace UniDecl.PropertyGrid.Editor.Elements
{
    /// <summary>
    /// 字段标签——模拟 Unity BaseField 内置 label 区域的宽度。
    /// 用于让独立 Label 与 TextField/FloatField/EnumField 等各种 Field 的 label 列对齐，
    /// 避免在 Hor{label, value} 显式布局模式下 label 宽度参差不齐。
    ///
    /// 实现：直接实现 IElement（不继承 Label），Render() 返回一个带 InlineStyle 的 Label 实例。
    /// 这样运行时精确类型是 Label，会被 Label 渲染器命中；FieldLabel 本身只是"结构包装节点"。
    ///
    /// 皮肤（文字色 / 字号 / 对齐）由 .ud-field-label 承担，宽度等尺寸仍走 InlineStyle。
    /// </summary>
    public class FieldLabel : Element
    {
        /// <summary>
        /// 默认 label 列宽度（与 BaseField 内置 label 对齐）。
        /// 项目主题通常将 .unity-base-field__label 定义为 128~136px，取折中值 130。
        /// 启动时修改即可全局生效，便于适配自定义主题。
        /// </summary>
        public static float DefaultWidth = 130f;

        public string Text { get; set; }

        /// <summary>只读字段的 label 一并置灰，与 Unity Inspector 的表现一致</summary>
        public bool Disabled { get; set; }

        public FieldLabel(string text) { Text = text; }

        public override IElement Render()
        {
            var style = new UITKStyle
            {
                Width = DefaultWidth,
                MinWidth = DefaultWidth,
                MaxWidth = DefaultWidth,
                FlexGrow = 0,
                FlexShrink = 0,
                AlignSelf = Align.Center,
                MarginRight = 6,
            };
            style.AddClass("ud-field-label");
            if (Disabled)
                style.AddClass("ud-field-label--disabled");

            return new UniDecl.BuiltIn.Runtime.Widgets.Label(Text).With(style);
        }
    }
}
