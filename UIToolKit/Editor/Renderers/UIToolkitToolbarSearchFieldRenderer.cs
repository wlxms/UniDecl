using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using W = UniDecl.BuiltIn.Runtime.Widgets;
using UniDecl.Editor.UIToolKit.Style;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitToolbarSearchFieldRenderer : IElementRenderer<W.ToolbarSearchField, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.ToolbarSearchField, VisualElement>
    {
        public VisualElement Render(W.ToolbarSearchField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var searchField = new UnityEditor.UIElements.ToolbarSearchField();

            // Snapshot 绑定——Register setter + 提供 Commit() 方法
            var binding = new SnapshotBinding<string>(state?.Scope, element.Key, element.Value ?? "",
                () => element.Value,
                v => { searchField.SetValueWithoutNotify(v ?? ""); element.Value = v; });

            searchField.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new ToolbarSearchFieldChangeEvent(element, evt.newValue));
                element.NotifyChanged();
            });

            searchField.RegisterCallback<BlurEvent>(_ =>
            {
                binding.Commit();
                element.OnCommit?.Invoke(element.Value);
                element.NotifyChanged();
            });

            UIToolkitStyleApplier.ApplyElementStyles(element, searchField);
            return searchField;
        }

        public bool TryUpdate(W.ToolbarSearchField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is UnityEditor.UIElements.ToolbarSearchField field)
            {
                field.SetValueWithoutNotify(element.Value ?? "");
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.ToolbarSearchField f && TryUpdate(f, existing, manager, state);
    }

    public struct ToolbarSearchFieldChangeEvent
    {
        public W.ToolbarSearchField Source { get; }
        public string NewValue { get; }
        public ToolbarSearchFieldChangeEvent(W.ToolbarSearchField source, string newValue) { Source = source; NewValue = newValue; }
    }
}
