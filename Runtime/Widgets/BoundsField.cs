using System;
using UnityEngine;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class BoundsField : Element
    {
        public string Label { get; set; }
        public Bounds Value { get; set; }
        public Action<Bounds> OnValueChanged { get; set; }

        public override IElement Render() => null;

        public BoundsField(string label, Bounds value = default)
        {
            Label = label;
            Value = value;
        }
    }
}
