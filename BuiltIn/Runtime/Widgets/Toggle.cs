using System;
using UniDecl.BuiltIn.Runtime.Core;

namespace UniDecl.BuiltIn.Runtime.Widgets
{
    public class Toggle : Element
    {
        public string Label { get; set; }
        public bool Value { get; set; }
        public Action<bool> OnValueChanged { get; set; }

        public override IElement Render() => null;

        public Toggle(string label, bool value = false) { Label = label; Value = value; }
        public Toggle(string label, bool value = false, params IElementComponent[] components) : base(components) { Label = label; Value = value; }
    }
}
