using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitFoldoutRenderer : IElementRenderer<W.Foldout, VisualElement>, IElementUpdater<W.Foldout, VisualElement>
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

            return foldout;
        }

        public bool Update(W.Foldout element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is UnityEngine.UIElements.Foldout ve)
            {
                ve.text = element.Text;
                ve.value = element.Value;
                return true;
            }
            return false;
        }
    }
}
