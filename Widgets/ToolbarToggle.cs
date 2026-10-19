using System;
using UniDecl.Core;

namespace UniDecl.Widgets
{
    public class ToolbarToggle : Element
    {
        public string Text { get; set; }
        public bool Value { get; set; }
        public Action<bool> OnValueChanged { get; set; }

        public override IElement Render() => null;

        public ToolbarToggle(string text, bool value = false)
        {
            Text = text;
            Value = value;
        }
    }
}
