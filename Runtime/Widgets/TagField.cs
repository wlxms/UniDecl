using System;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class TagField : Element
    {
        public string Label { get; set; }
        public string Value { get; set; }
        public Action<string> OnValueChanged { get; set; }

        public override IElement Render() => null;

        public TagField(string label, string value = null)
        {
            Label = label;
            Value = value ?? string.Empty;
        }
    }
}
