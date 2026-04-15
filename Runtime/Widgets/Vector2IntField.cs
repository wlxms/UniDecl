using System;
using UnityEngine;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class Vector2IntField : Element
    {
        public string Label { get; set; }
        public Vector2Int Value { get; set; }
        public Action<Vector2Int> OnValueChanged { get; set; }

        public override IElement Render() => null;

        public Vector2IntField(string label, Vector2Int value = default)
        {
            Label = label;
            Value = value;
        }
    }
}
