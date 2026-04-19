using System;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class Button : Element
    {
        public string Text { get; set; }
        public Action OnClick { get; set; }
        public bool Enabled { get; set; } = true;

        public override IElement Render() => null;

        public Button(string text) { Text = text; }
        public Button(string text, Action onClick) { Text = text; OnClick = onClick; }
        public Button(string text, params IElementComponent[] components) : base(components) { Text = text; }
    }
}
