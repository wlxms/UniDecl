using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitLabelRenderer : IElementRenderer<W.Label, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.Label, VisualElement>
    {
        public VisualElement Render(W.Label element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;
            return new Label(element.Text)
            {
                enableRichText = element.EnableRichText,
                parseEscapeSequences = element.ParseEscapeSequences,
            };
        }

        public bool TryUpdate(W.Label element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is Label ve)
            {
                ve.text = element.Text;
                ve.enableRichText = element.EnableRichText;
                ve.parseEscapeSequences = element.ParseEscapeSequences;
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.Label label && TryUpdate(label, existing, manager, state);
    }
}
