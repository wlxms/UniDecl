using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Runtime.Navigation;
using W = UniDecl.Runtime.Widgets;
using UniDecl.Editor.UIToolKit.Style;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;
namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitFoldoutRenderer : IElementRenderer<W.Foldout, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.Foldout, VisualElement>,
        IRendererEventListener<VisualElement, NavigationEvent>
    {
        public VisualElement Render(W.Foldout element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var foldout = new UnityEngine.UIElements.Foldout
            {
                text = element.Text,
                value = element.Value,
            };

            foreach (var child in element.Children)
            {
                var childElement = manager.RenderElement(child);
                if (childElement != null)
                    foldout.Add(childElement);
            }

            UIToolkitStyleApplier.ApplyElementStyles(element, foldout);
            return foldout;
        }

        public bool TryUpdate(W.Foldout element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is UnityEngine.UIElements.Foldout ve)
            {
                ve.text = element.Text;
                ve.value = element.Value;
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.Foldout foldout && TryUpdate(foldout, existing, manager, state);

        public void OnEvent(NavigationEvent @event, DOMNode<VisualElement> node)
        {
            if (@event.IsTarget) return;
            var ve = node.RenderResult;
            if (ve is UnityEngine.UIElements.Foldout foldout)
                foldout.value = true;
        }
    }
}
