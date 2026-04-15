using System.Collections.Generic;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitDropdownRenderer : IElementRenderer<W.Dropdown, VisualElement>
    {
        public VisualElement Render(W.Dropdown element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var choices = element.Choices != null
                ? new List<string>(element.Choices)
                : new List<string>();

            var dropdown = new DropdownField(element.Label, choices, element.Index);
            dropdown.RegisterValueChangedCallback(evt =>
            {
                element.Index = dropdown.index;
                element.OnSelectionChanged?.Invoke(dropdown.index);
                manager.Dispatch(new DropdownChangeEvent(element, dropdown.index, evt.newValue));
                element.NotifyChanged();
            });

            UIToolkitStyleApplier.ApplyElementStyles(element, dropdown);
            return dropdown;
        }
    }

    public struct DropdownChangeEvent
    {
        public W.Dropdown Source { get; }
        public int Index { get; }
        public string Value { get; }

        public DropdownChangeEvent(W.Dropdown source, int index, string value)
        {
            Source = source;
            Index = index;
            Value = value;
        }
    }
}
