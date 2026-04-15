using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using W = UniDecl.Runtime.Widgets;
using UniDecl.Editor.UIToolKit.Style;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitProgressBarRenderer : IElementRenderer<W.ProgressBar, VisualElement>
    {
        public VisualElement Render(W.ProgressBar element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;

            if (!string.IsNullOrEmpty(element.Label))
            {
                var label = new Label(element.Label);
                label.style.marginBottom = 2;
                container.Add(label);
            }

            var track = new VisualElement();
            track.AddToClassList("ud-progress-track");
            track.style.flexGrow = 1;
            track.style.position = Position.Relative;

            var fill = new VisualElement();
            fill.AddToClassList("ud-progress-fill");
            fill.style.position = Position.Absolute;
            fill.style.left = 0;
            fill.style.top = 0;
            fill.style.width = Length.Percent(Mathf.Clamp01(element.Value) * 100f);

            track.Add(fill);
            container.Add(track);
            UIToolkitStyleApplier.ApplyElementStyles(element, container);
            return container;
        }
    }
}
