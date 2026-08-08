using System;
using UnityEngine;
using UniDecl.BuiltIn.Runtime.Core;

namespace UniDecl.BuiltIn.Runtime.Widgets
{
    public class GradientField : Element
    {
        public string Label { get; set; }
        public Gradient Value { get; set; }
        public Action<Gradient> OnValueChanged { get; set; }

        public override IElement Render() => null;

        public GradientField(string label, Gradient value = null)
        {
            Label = label;
            Value = value ?? new Gradient();
        }
    }
}
