using System;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class MinMaxSlider : Element
    {
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public float LowLimit { get; set; } = 0f;
        public float HighLimit { get; set; } = 100f;
        public Action<float, float> OnValueChanged { get; set; }
        public Action<float, float> OnCommit { get; set; }
        public string Label { get; set; }

        public override IElement Render() => null;

        public MinMaxSlider(string label = "", float min = 0f, float max = 100f, float lowLimit = 0f, float highLimit = 100f)
        {
            Label = label;
            MinValue = min;
            MaxValue = max;
            LowLimit = lowLimit;
            HighLimit = highLimit;
        }
    }
}
