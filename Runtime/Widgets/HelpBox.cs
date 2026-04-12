using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public enum HelpBoxMessageType { None, Info, Warning, Error }

    public class HelpBox : Element
    {
        public string Text { get; set; }
        public HelpBoxMessageType MessageType { get; set; } = HelpBoxMessageType.Info;

        public override IElement Render() => null;

        public HelpBox(string text, HelpBoxMessageType type = HelpBoxMessageType.Info)
        {
            Text = text;
            MessageType = type;
        }
    }
}
