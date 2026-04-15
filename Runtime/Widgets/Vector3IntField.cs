using System;
using UnityEngine;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class Vector3IntField : Element
    {
        public string Label { get; set; }
        public Vector3Int Value { get; set; }
        public Action<Vector3Int> OnValueChanged { get; set; }

        public override IElement Render() => null;

        public Vector3IntField(string label, Vector3Int value = default)
        {
            Label = label;
            Value = value;
        }
    }
}
