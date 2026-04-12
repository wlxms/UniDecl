using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitHelpBoxRenderer : IElementRenderer<W.HelpBox, VisualElement>
    {
        public VisualElement Render(W.HelpBox element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var msgType = ConvertMessageType(element.MessageType);
            return new HelpBox(element.Text ?? "", msgType);
        }

        private static HelpBoxMessageType ConvertMessageType(W.HelpBoxMessageType type)
        {
            return type switch
            {
                W.HelpBoxMessageType.None => HelpBoxMessageType.None,
                W.HelpBoxMessageType.Info => HelpBoxMessageType.Info,
                W.HelpBoxMessageType.Warning => HelpBoxMessageType.Warning,
                W.HelpBoxMessageType.Error => HelpBoxMessageType.Error,
                _ => HelpBoxMessageType.Info,
            };
        }
    }
}
