using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitTocViewRenderer : IElementRenderer<W.TocView, VisualElement>
    {
        public VisualElement Render(W.TocView element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;

            if (element.Items != null)
            {
                foreach (var entry in element.Items)
                {
                    if (entry == null) continue;

                    var item = new VisualElement();
                    item.style.flexDirection = FlexDirection.Row;
                    item.style.alignItems = Align.Center;

                    // Indent based on heading level (level 1 = no indent, level 6 = deep indent)
                    var indentPx = (entry.Level - 1) * 12f;
                    item.style.paddingLeft = indentPx;

                    item.AddToClassList("ud-toc-item");
                    item.AddToClassList($"ud-toc-item--l{Mathf.Clamp(entry.Level, 1, 6)}");

                    if (entry.IsSelected)
                        item.AddToClassList("ud-toc-item--selected");

                    var label = new Label(entry.Text);
                    label.AddToClassList("ud-toc-item-label");
                    item.Add(label);

                    if (entry.OnClick != null)
                    {
                        var clickAction = entry.OnClick;
                        item.RegisterCallback<ClickEvent>(_ => clickAction?.Invoke());
                        item.AddToClassList("ud-toc-item--clickable");
                    }

                    container.Add(item);
                }
            }

            UIToolkitStyleApplier.ApplyElementStyles(element, container);
            return container;
        }
    }
}
