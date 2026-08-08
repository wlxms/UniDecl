using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using W = UniDecl.BuiltIn.Runtime.Widgets;
using UniDecl.Editor.UIToolKit.Style;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitToggleRenderer : IElementRenderer<W.Toggle, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.Toggle, VisualElement>
    {
        public VisualElement Render(W.Toggle element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var toggle = new UnityEngine.UIElements.Toggle(element.Label) { value = element.Value };

            // Snapshot 绑定——瞬时选择型，ChangeEvent 即提交
            var binding = new SnapshotBinding<bool>(state?.Scope, element.Key, element.Value,
                () => element.Value,
                v => { toggle.SetValueWithoutNotify(v); element.Value = v; });

            UIToolkitStyleApplier.ApplyElementStyles(element, toggle);
            RegisterToggleCallbacks(toggle, element, manager, binding);
            return toggle;
        }

        public bool TryUpdate(W.Toggle element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is UnityEngine.UIElements.Toggle ve)
            {
                // SetValueWithoutNotify 避免触发 ChangeEvent 导致误 Commit（外部更新不应进 Undo 栈）
                ve.SetValueWithoutNotify(element.Value);
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.Toggle toggle && TryUpdate(toggle, existing, manager, state);

        private static void RegisterToggleCallbacks(UnityEngine.UIElements.Toggle toggle, W.Toggle element, IElementRenderHost<VisualElement> manager, SnapshotBinding<bool> binding)
        {
            toggle.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new ToggleChangeEvent(element, evt.newValue));
                binding.Commit();  // 瞬时型：ChangeEvent 即提交
                element.NotifyChanged();
            });
        }
    }

    public struct ToggleChangeEvent
    {
        public W.Toggle Source { get; }
        public bool NewValue { get; }
        public ToggleChangeEvent(W.Toggle source, bool newValue) { Source = source; NewValue = newValue; }
    }
}
