using System.Collections.Generic;
using UnityEngine.UIElements;
using UniDecl.Core;
using UniDecl.Widgets;
using UniDecl.Backend.UIToolKit.Editor.Style;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers
{
    public class UIToolkitPanelRenderer : IElementRenderer<Panel, VisualElement>
    {
        public VisualElement Render(Panel element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var container = new VisualElement();
            // 轴向由 .ud-panel 承担（DefaultStyle.uss）

            foreach (var child in element.Children)
            {
                var childElement = manager.RenderElement(child);
                if (childElement != null)
                    container.Add(childElement);
            }

            UIToolkitStyleApplier.ApplyElementStyles(element, container);
            return container;
        }
    }
}
