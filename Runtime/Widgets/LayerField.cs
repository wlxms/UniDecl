using System;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class LayerField : Element
    {
        public string Label { get; set; }
        public int Value { get; set; }
        public Action<int> OnValueChanged { get; set; }

        public override IElement Render() => null;

        public LayerField(string label, int value = 0)
        {
            Label = label;
            Value = value;
        }
    }
}
