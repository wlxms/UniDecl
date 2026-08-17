using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.BuiltIn.Runtime.Navigation;
using UniDecl.Editor.UIToolKit.Effects;
using W = UniDecl.BuiltIn.Runtime.Widgets;using UniDecl.Editor.UIToolKit.Style;
namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitLabelRenderer : IElementRenderer<W.Label, VisualElement>,
        IRendererEventListener<VisualElement, NavigationEvent>
    {
        private const float HighlightDuration = 0.5f;

        public VisualElement Render(W.Label element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is Label reused)
            {
                reused.text = element.Text;
                reused.enableRichText = element.EnableRichText;
                reused.parseEscapeSequences = element.ParseEscapeSequences;
                return reused;
            }

            var label = new Label(element.Text)
            {
                enableRichText = element.EnableRichText,
                parseEscapeSequences = element.ParseEscapeSequences,
            };
            UIToolkitStyleApplier.ApplyElementStyles(element, label);
            return label;
        }

        public void OnEvent(NavigationEvent @event, DOMNode<VisualElement> node, DOMTree<VisualElement> tree)
        {
            if (!@event.IsTarget) return;
            var ve = node.RenderResult;
            if (ve == null) return;

            OverlayEffectManager.Ping(ve);
        }
    }
}
