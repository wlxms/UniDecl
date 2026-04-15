using System;
using UnityEngine;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class RectField : Element
    {
        public string Label { get; set; }
        public Rect Value { get; set; }
        public Action<Rect> OnValueChanged { get; set; }

        public override IElement Render() => null;

        public RectField(string label, Rect value = default)
        {
            Label = label;
            Value = value;
        }
    }
}
