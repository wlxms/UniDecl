using System;
using UnityEngine;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class Vector3Field : Element
    {
        public string Label { get; set; }
        public Vector3 Value { get; set; }
        public Action<Vector3> OnValueChanged { get; set; }

        public override IElement Render() => null;

        public Vector3Field(string label, Vector3 value = default)
        {
            Label = label;
            Value = value;
        }
    }
}
