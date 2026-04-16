using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using W = UniDecl.Runtime.Widgets;
using UniDecl.Editor.UIToolKit.Style;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitToolbarSearchFieldRenderer : IElementRenderer<W.ToolbarSearchField, VisualElement>
    {
        public VisualElement Render(W.ToolbarSearchField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var searchField = new UnityEditor.UIElements.ToolbarSearchField();

            searchField.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new ToolbarSearchFieldChangeEvent(element, evt.newValue));
                element.NotifyChanged();
            });

            searchField.RegisterCallback<BlurEvent>(_ =>
            {
                element.OnCommit?.Invoke(element.Value);
                element.NotifyChanged();
            });

            UIToolkitStyleApplier.ApplyElementStyles(element, searchField);
            return searchField;
        }
    }

    public struct ToolbarSearchFieldChangeEvent
    {
        public W.ToolbarSearchField Source { get; }
        public string NewValue { get; }
        public ToolbarSearchFieldChangeEvent(W.ToolbarSearchField source, string newValue) { Source = source; NewValue = newValue; }
    }
}
