using System;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class DoubleField : Element
    {
        public string Label { get; set; }
        public double Value { get; set; }
        public Action<double, double> OnValueChanged { get; set; }
        public Action<double> OnCommit { get; set; }

        public override IElement Render() => null;

        public DoubleField(string label = null, double value = 0)
        {
            Label = label;
            Value = value;
        }
    }
}
