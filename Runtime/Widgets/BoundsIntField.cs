using System;
using UnityEngine;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class BoundsIntField : Element
    {
        public string Label { get; set; }
        public BoundsInt Value { get; set; }
        public Action<BoundsInt> OnValueChanged { get; set; }

        public override IElement Render() => null;

        public BoundsIntField(string label, BoundsInt value = default)
        {
            Label = label;
            Value = value;
        }
    }
}
