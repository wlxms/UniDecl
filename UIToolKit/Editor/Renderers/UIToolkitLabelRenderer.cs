using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Runtime.Navigation;
using UniDecl.Editor.UIToolKit.Effects;
using W = UniDecl.Runtime.Widgets;using UniDecl.Editor.UIToolKit.Style;
namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitLabelRenderer : IElementRenderer<W.Label, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.Label, VisualElement>,
        IRendererEventListener<VisualElement, NavigationEvent>
    {
        private const float HighlightDuration = 0.5f;

        public VisualElement Render(W.Label element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;
            var label = new Label(element.Text)
            {
                enableRichText = element.EnableRichText,
                parseEscapeSequences = element.ParseEscapeSequences,
            };
            UIToolkitStyleApplier.ApplyElementStyles(element, label);
            return label;
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

        public void OnEvent(NavigationEvent @event, DOMNode<VisualElement> node, DOMTree<VisualElement> tree)
        {
            if (!@event.IsTarget) return;
            var ve = node.RenderResult;
            if (ve == null) return;

            OverlayEffectManager.Ping(ve);
        }
    }
}
