using System;
using UnityEngine;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class Vector2Field : Element
    {
        public string Label { get; set; }
        public Vector2 Value { get; set; }
        public Action<Vector2> OnValueChanged { get; set; }

        public override IElement Render() => null;

        public Vector2Field(string label, Vector2 value = default)
        {
            Label = label;
            Value = value;
        }
    }
}
