using System;
using UniDecl.BuiltIn.Runtime.Core;

namespace UniDecl.BuiltIn.Runtime.Widgets
{
    public class ToolbarButton : Element
    {
        public string Text { get; set; }
        public Action OnClick { get; set; }
        public bool Enabled { get; set; } = true;

        public override IElement Render() => null;

        public ToolbarButton(string text) { Text = text; }
        public ToolbarButton(string text, Action onClick) { Text = text; OnClick = onClick; }
    }
}
