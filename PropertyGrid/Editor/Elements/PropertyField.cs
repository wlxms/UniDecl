using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.BuiltIn.Runtime.Contexts;
using UniDecl.BuiltIn.Runtime.Widgets;
using UniDecl.UIToolKit.Runtime;
using UnityEngine.UIElements;

namespace UniDecl.PropertyGrid.Editor.Elements
{
    /// <summary>
    /// 字段 Element——统一采用 Hor{label, value} 显式布局模式。
    /// Label 由 PropertyField 自己渲染（new Label(LabelText) 或 LabelWidgetOverride），
    /// 不再依赖 Unity BaseField 的内置 label——Editor setter 不会触碰 widget 的 Label 字段。
    /// 渲染时会清空 editor 内部的 Label，避免 Decorator 传入 LabelText 造成双 label 重复。
    ///
    /// 行级布局元数据（[FlexGrow]/[Width]/[AlignRight]/[AlignCenter]/[Space]）由对应
    /// Metadata Decorator 写入下列属性，Render 时统一落到行容器的 UITKStyle。
    /// </summary>
    public class PropertyField : Element
    {
        public string LabelText { get; set; }
        public bool ShowLabel { get; set; } = true;
        public string Tooltip { get; set; }
        public bool IsReadOnly { get; set; }
        public int IndentLevel { get; set; }
        public bool Visible { get; set; } = true;

        /// <summary>行在水平布局中的弹性占比（[FlexGrow]，默认 1 = 等分）</summary>
        public float FlexGrow { get; set; } = 1f;

        /// <summary>固定像素宽度（[Width]）；null = 未指定</summary>
        public float? Width { get; set; }

        /// <summary>水平行内的对齐（[AlignRight]/[AlignCenter]）；null = 跟随布局</summary>
        public Align? AlignSelf { get; set; }

        /// <summary>行前/行后间距（[Space]，px）</summary>
        public int SpaceBefore { get; set; }
        public int SpaceAfter { get; set; }

        public IElement LabelWidgetOverride { get; set; }
        public PropertyAccessor Accessor { get; set; }

        IElement _editor;
        // setter 仅保存引用——label 由 Render() 显式管理
        public IElement Editor { get => _editor; set => _editor = value; }

        public PropertyField(string labelText) { LabelText = labelText; }

        public override IElement Render()
        {
            // 始终返回带相同 Key 的 HorizontalLayout——保证 diff 时容器 DOMNode 复用，
            // 避免 Visible 切换时容器类型变化导致整棵子树销毁重建。
            // Visible=false 时返回空容器（不 Add 子元素），子元素的增减由 DiffChildren 增量处理。
            var row = new HorizontalLayout();
            var rowKey = Accessor != null && !string.IsNullOrEmpty(Accessor.FullPath)
                ? $"pf_{Accessor.FullPath}"
                : $"pf_{LabelText}";
            row.WithKey(rowKey);
            row.With(new UITKStyle
            {
                FlexDirection = FlexDirection.Row,
                AlignItems = Align.Center,
                // FlexGrow 语义：本字段在父级水平行中所占的剩余空间份额（[FlexGrow]）
                FlexGrow = FlexGrow,
                FlexShrink = 1,
                MinWidth = 0,
                AlignSelf = AlignSelf,
                Tooltip = Tooltip,
                MarginTop = SpaceBefore > 0 ? (float?)SpaceBefore : null,
                // 默认行间距 2px，[Space(After)] 覆盖
                MarginBottom = SpaceAfter > 0 ? (float?)SpaceAfter : 2f,
            }.AddClass("ud-property-row"));   // 行高 / 内边距来自主题（见 DefaultStyle.uss

            if (!Visible) return row;

            // 1) Label 槽：Override 优先；其次 ShowLabel + 非空 LabelText 才渲染
            if (LabelWidgetOverride != null)
                row.Add(LabelWidgetOverride);
            else if (ShowLabel && !string.IsNullOrEmpty(LabelText))
                row.Add(new FieldLabel(LabelText) { Disabled = IsReadOnly });

            // 2) Value 槽：清空 editor 内置 label（避免与显式 Label 重复）后加入。
            //    [Width] 固定的是编辑器宽度而非整行——否则 label+值一起被压扁。
            if (_editor != null)
            {
                WidgetLabelHelper.SetLabel(_editor, null);
                _editor.With(new UITKStyle
                {
                    FlexGrow = Width.HasValue ? 0 : 1, // 定宽字段不参与拉伸
                    FlexShrink = 1,
                    MinWidth = 0,
                    MaxWidth = Width,
                });
                row.Add(IsReadOnly
                    ? new DisableContext(true, _editor).WithKey($"readonly_{LabelText}")
                    : _editor);
            }

            return row;
        }
    }
}
