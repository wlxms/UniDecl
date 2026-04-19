using System;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class TextField : Element
    {
        public string Value { get; set; }
        public string Placeholder { get; set; }
        public bool IsPassword { get; set; }
        public bool IsMultiline { get; set; }
        public bool IsReadOnly { get; set; }
        public int MaxLength { get; set; } = -1;
        public bool IsDelayed { get; set; } = true;
        public Action<string, string> OnValueChange { get; set; }
        public Action<string> OnCommit { get; set; }

        public override IElement Render() => null;

        public TextField(string value = "", string placeholder = "")
        {
            Value = value;
            Placeholder = placeholder;
        }
        public TextField(string value = "", string placeholder = "", params IElementComponent[] components) : base(components)
        {
            Value = value;
            Placeholder = placeholder;
        }
    }
}
