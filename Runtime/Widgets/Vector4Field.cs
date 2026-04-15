using System;
using UnityEngine;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class Vector4Field : Element
    {
        public string Label { get; set; }
        public Vector4 Value { get; set; }
        public Action<Vector4> OnValueChanged { get; set; }

        public override IElement Render() => null;

        public Vector4Field(string label, Vector4 value = default)
        {
            Label = label;
            Value = value;
        }
    }
}
