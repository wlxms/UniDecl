using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.PropertyGrid.Editor.Elements
{
    /// <summary>
    /// 字段 Element——统一采用 Hor{label, value} 显式布局模式。
    /// Label 由 PropertyField 自己渲染（new Label(LabelText) 或 LabelWidgetOverride），
    /// 不再依赖 Unity BaseField 的内置 label——Editor setter 不会触碰 widget 的 Label 字段。
    /// 渲染时会清空 editor 内部的 Label，避免 Decorator 传入 LabelText 造成双 label 重复。
    /// </summary>
    public class PropertyField : Element
    {
        public string LabelText { get; set; }
        public bool ShowLabel { get; set; } = true;
        public string Tooltip { get; set; }
        public bool IsReadOnly { get; set; }
        public int IndentLevel { get; set; }
        public bool Visible { get; set; } = true;
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
            row.WithKey($"pf_{LabelText}");

            if (!Visible) return row;

            // 1) Label 槽：Override 优先；其次 ShowLabel + 非空 LabelText 才渲染
            if (LabelWidgetOverride != null)
                row.Add(LabelWidgetOverride);
            else if (ShowLabel && !string.IsNullOrEmpty(LabelText))
                row.Add(new FieldLabel(LabelText));

            // 2) Value 槽：清空 editor 内置 label（避免与显式 Label 重复）后加入
            if (_editor != null)
            {
                WidgetLabelHelper.SetLabel(_editor, null);
                row.Add(_editor);
            }

            return row;
        }
    }
}
