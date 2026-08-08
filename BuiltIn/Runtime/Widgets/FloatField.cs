using System;
using UniDecl.BuiltIn.Runtime.Core;

namespace UniDecl.BuiltIn.Runtime.Widgets
{
    public class FloatField : Element
    {
        public string Label { get; set; }
        public float Value { get; set; }
        public Action<float, float> OnValueChanged { get; set; }
        public Action<float> OnCommit { get; set; }

        public override IElement Render() => null;

        public FloatField(float value = 0f) { Value = value; }
        public FloatField(string label, float value = 0f) { Label = label; Value = value; }
        public FloatField(float value = 0f, params IElementComponent[] components) : base(components) { Value = value; }
    }
}
