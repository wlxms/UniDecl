using UnityEngine.UIElements;
using UniDecl.Core.Components;
using UniDecl.Core;

namespace UniDecl.Backend.UIToolKit.Editor
{
    public static class ElementEventApplier
    {
        public static void Apply(IElement element, VisualElement ve)
        {
            if (!(element is UniDecl.Core.Element e) || ve == null) return;
            foreach (var comp in e.Components)
            {
                if (comp is IUITKEventComponent evt)
                {
                    evt.RegisterTo(ve);
                    continue;
                }
                if (comp is OnClick click) { ve.RegisterCallback<ClickEvent>(_ => click.Handler?.Invoke()); continue; }
                if (comp is OnPointerEnter enter) { ve.RegisterCallback<PointerEnterEvent>(_ => enter.Handler?.Invoke()); continue; }
                if (comp is OnPointerLeave leave) { ve.RegisterCallback<PointerLeaveEvent>(_ => leave.Handler?.Invoke()); continue; }
            }
        }
    }
}
