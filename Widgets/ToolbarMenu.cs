using System;
using UniDecl.Core;

namespace UniDecl.Widgets
{
    public class ToolbarMenu : Element
    {
        public string Text { get; set; }
        public Action OnClick { get; set; }

        public override IElement Render() => null;

        public ToolbarMenu(string text) { Text = text; }
        public ToolbarMenu(string text, Action onClick) { Text = text; OnClick = onClick; }
    }
}
