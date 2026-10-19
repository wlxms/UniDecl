using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.Core;
using W = UniDecl.Widgets;
using UniDecl.Backend.UIToolKit.Editor.Style;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers
{
    public class UIToolkitProgressBarRenderer : IElementRenderer<W.ProgressBar, VisualElement>
    {
        public VisualElement Render(W.ProgressBar element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var container = new VisualElement();
            // 轴向由 .ud-progress-bar 承担（DefaultStyle.uss）

            if (!string.IsNullOrEmpty(element.Label))
            {
                var label = new Label(element.Label);
                label.AddToClassList("ud-progress-bar__label");
                container.Add(label);
            }

            // 轨道 / 填充的定位与配色见 .ud-progress-track / .ud-progress-fill（DefaultStyle.uss）
            var track = new VisualElement();
            track.AddToClassList("ud-progress-track");

            var fill = new VisualElement();
            fill.AddToClassList("ud-progress-fill");
            // 进度比例是实例级数据，只能内联
            fill.style.width = Length.Percent(Mathf.Clamp01(element.Value) * 100f);

            track.Add(fill);
            container.Add(track);
            UIToolkitStyleApplier.ApplyElementStyles(element, container);
            return container;
        }
    }
}
