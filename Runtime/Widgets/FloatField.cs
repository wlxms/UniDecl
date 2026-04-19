using System;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class FloatField : Element
    {
        public float Value { get; set; }
        public Action<float, float> OnValueChanged { get; set; }
        public Action<float> OnCommit { get; set; }

        public override IElement Render() => null;

        public FloatField(float value = 0f) { Value = value; }
        public FloatField(float value = 0f, params IElementComponent[] components) : base(components) { Value = value; }
    }
}
