using System;
using UniDecl.Core;

namespace UniDecl.Unity.Widgets
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
