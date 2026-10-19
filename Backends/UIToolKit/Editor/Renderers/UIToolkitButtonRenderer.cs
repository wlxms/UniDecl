using UnityEngine.UIElements;
using UniDecl.Core;
using UniDecl.Backend.UIToolKit.Editor.Style;
using UniDecl.Snapshot;
using UITKStyle = UniDecl.Backend.UIToolKit.UITKStyle;
using W = UniDecl.Widgets;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers
{
    public class UIToolkitButtonRenderer : IElementRenderer<W.Button, VisualElement>
    {
        public VisualElement Render(W.Button element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var button = new Button { text = element.Text };
            button.SetEnabled(element.Enabled);

            button.clicked += () =>
            {
                manager.Dispatch(new ButtonClickEvent(element));
                element.OnClick?.Invoke();
                element.NotifyChanged();
            };

            UIToolkitStyleApplier.ApplyElementStyles(element, button);
            return button;
        }
    }

    public struct ButtonClickEvent
    {
        public W.Button SourceButton { get; }
        public ButtonClickEvent(W.Button source) { SourceButton = source; }
    }
}
