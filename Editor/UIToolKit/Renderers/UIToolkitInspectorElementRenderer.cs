using UnityEditor;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitInspectorElementRenderer : IElementRenderer<W.InspectorElement, VisualElement>
    {
        public VisualElement Render(W.InspectorElement element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var container = new VisualElement();

            if (element.Target != null)
            {
                var editor = UnityEditor.Editor.CreateEditor(element.Target);
                if (editor != null)
                {
                    container.Add(new IMGUIContainer(() => editor.OnInspectorGUI()));
                }
            }

            UIToolkitStyleApplier.ApplyElementStyles(element, container);
            return container;
        }
    }
}
