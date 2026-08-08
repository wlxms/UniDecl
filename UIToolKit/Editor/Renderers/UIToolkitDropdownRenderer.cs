using System.Collections.Generic;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitDropdownRenderer : IElementRenderer<W.Dropdown, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.Dropdown, VisualElement>
    {
        public VisualElement Render(W.Dropdown element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var choices = element.Choices != null
                ? new List<string>(element.Choices)
                : new List<string>();

            var dropdown = new DropdownField(element.Label, choices, element.Index);

            // Snapshot 绑定——瞬时选择型，ChangeEvent 即提交（按 Index 记录）
            var binding = new SnapshotBinding<int>(state?.Scope, element.Key, element.Index,
                () => element.Index,
                v =>
                {
                    var displayValue = (v >= 0 && v < choices.Count) ? choices[v] : null;
                    dropdown.SetValueWithoutNotify(displayValue);
                    element.Index = v;
                });

            dropdown.RegisterValueChangedCallback(evt =>
            {
                element.Index = dropdown.index;
                element.OnSelectionChanged?.Invoke(dropdown.index);
                manager.Dispatch(new DropdownChangeEvent(element, dropdown.index, evt.newValue));
                binding.Commit();  // 瞬时型：ChangeEvent 即提交
                element.NotifyChanged();
            });

            UIToolkitStyleApplier.ApplyElementStyles(element, dropdown);
            return dropdown;
        }

        public bool TryUpdate(W.Dropdown element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is DropdownField dropdown)
            {
                var displayValue = (element.Index >= 0 && element.Index < dropdown.choices.Count)
                    ? dropdown.choices[element.Index]
                    : null;
                dropdown.SetValueWithoutNotify(displayValue);
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.Dropdown f && TryUpdate(f, existing, manager, state);
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
