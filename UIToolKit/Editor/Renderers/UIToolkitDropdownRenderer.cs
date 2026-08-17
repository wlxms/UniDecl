using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using UniDecl.BuiltIn.Runtime.Snapshot;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitDropdownRenderer : IElementRenderer<W.Dropdown, VisualElement>
    {
        public VisualElement Render(W.Dropdown element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is DropdownField reused)
            {
                reused.SetValueWithoutNotify(element.Index >= 0 && element.Index < element.Choices.Length
                    ? element.Choices[element.Index] : null);
                return reused;
            }

            var field = new DropdownField(element.Label, new System.Collections.Generic.List<string>(element.Choices), element.Index);

            // Snapshot 绑定——瞬时选择型，ChangeEvent 即提交
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Index,
                (restore, current, changes) =>
                {
                    var v = (int)restore;
                    field.SetValueWithoutNotify(v >= 0 && v < element.Choices.Length ? element.Choices[v] : null);
                    element.Index = v;
                    element.OnSelectionChanged?.Invoke(v);
                });

            field.RegisterValueChangedCallback(evt =>
            {
                var newIndex = field.index;
                element.Index = newIndex;
                element.OnSelectionChanged?.Invoke(newIndex);
                manager.Dispatch(new DropdownChangeEvent(element, newIndex));
                binding.BreakMerge(); // 离散选择：每次独立 step
                binding.Commit();
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct DropdownChangeEvent
    {
        public W.Dropdown Source { get; }
        public int NewIndex { get; }

        public DropdownChangeEvent(W.Dropdown source, int newIndex)
        {
            Source = source;
            NewIndex = newIndex;
        }
    }
}
