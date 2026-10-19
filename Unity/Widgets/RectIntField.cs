using System;
using UnityEngine;
using UniDecl.Core;

namespace UniDecl.Unity.Widgets
{
    public class RectIntField : Element
    {
        public string Label { get; set; }
        public RectInt Value { get; set; }
        public Action<RectInt> OnValueChanged { get; set; }

        public override IElement Render() => null;

        public RectIntField(string label, RectInt value = default)
        {
            Label = label;
            Value = value;
        }
    }
}
