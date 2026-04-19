using System;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class Slider : Element
    {
        public float Value { get; set; }
        public float LowValue { get; set; } = 0f;
        public float HighValue { get; set; } = 100f;
        public Action<float> OnValueChanged { get; set; }
        public Action<float> OnCommit { get; set; }
        public string Label { get; set; }

        public override IElement Render() => null;

        public Slider(string label = "", float value = 0f, float low = 0f, float high = 100f)
        {
            Label = label;
            Value = value;
            LowValue = low;
            HighValue = high;
        }
        public Slider(string label = "", float value = 0f, float low = 0f, float high = 100f, params IElementComponent[] components) : base(components)
        {
            Label = label;
            Value = value;
            LowValue = low;
            HighValue = high;
        }
    }
}
