using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitTagFieldRenderer : IElementRenderer<W.TagField, VisualElement>
    {
        public VisualElement Render(W.TagField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var field = new TagField(element.Label) { value = element.Value };
            
            field.RegisterValueChangedCallback<string>(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new TagFieldChangeEvent(element, evt.newValue, evt.previousValue));
                element.NotifyChanged();
            });

            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct TagFieldChangeEvent
    {
        public W.TagField Source { get; }
        public string NewValue { get; }
        public string PreviousValue { get; }
        
        public TagFieldChangeEvent(W.TagField source, string newV, string prevV)
        {
            Source = source;
            NewValue = newV;
            PreviousValue = prevV;
        }
    }
}
