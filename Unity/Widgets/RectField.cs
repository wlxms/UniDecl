using System;
using UnityEngine;
using UniDecl.Core;

namespace UniDecl.Unity.Widgets
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
