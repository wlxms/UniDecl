using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using W = UniDecl.Runtime.Widgets;
using UniDecl.Editor.UIToolKit.Style;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitToggleRenderer : IElementRenderer<W.Toggle, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.Toggle, VisualElement>
    {
        public VisualElement Render(W.Toggle element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var toggle = new UnityEngine.UIElements.Toggle(element.Label) { value = element.Value };
            UIToolkitStyleApplier.ApplyElementStyles(element, toggle);
            RegisterToggleCallbacks(toggle, element, manager);
            return toggle;
        }

        public bool TryUpdate(W.Toggle element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is UnityEngine.UIElements.Toggle ve)
            {
                ve.value = element.Value;
                // Callback 在 Render 时一次性注册，Update 不重复注册
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.Toggle toggle && TryUpdate(toggle, existing, manager, state);

        private static void RegisterToggleCallbacks(UnityEngine.UIElements.Toggle toggle, W.Toggle element, IElementRenderHost<VisualElement> manager)
        {
            toggle.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new ToggleChangeEvent(element, evt.newValue));
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
