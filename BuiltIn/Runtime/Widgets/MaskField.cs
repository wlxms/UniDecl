using System;
using UniDecl.BuiltIn.Runtime.Core;

namespace UniDecl.BuiltIn.Runtime.Widgets
{
    public class MaskField : Element
    {
        public string Label { get; set; }
        public int Value { get; set; }
        public string[] Choices { get; set; }
        public Action<int> OnValueChanged { get; set; }

        public override IElement Render() => null;

        public MaskField(string label, int value = 0, string[] choices = null)
        {
            Label = label;
            Value = value;
            Choices = choices;
        }
    }
}
