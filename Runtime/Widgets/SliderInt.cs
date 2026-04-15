using System;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class SliderInt : Element
    {
        public string Label { get; set; }
        public int Value { get; set; }
        public int LowValue { get; set; } = 0;
        public int HighValue { get; set; } = 100;
        public Action<int> OnValueChanged { get; set; }
        public Action<int> OnCommit { get; set; }

        public override IElement Render() => null;

        public SliderInt(string label = "", int value = 0, int low = 0, int high = 100)
        {
            Label = label;
            Value = value;
            LowValue = low;
            HighValue = high;
        }
    }
}
