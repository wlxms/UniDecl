using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitButtonRenderer : IElementRenderer<W.Button, VisualElement>
    {
        public VisualElement Render(W.Button element, IElementRenderHost<VisualElement> manager, ElementState state)
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
