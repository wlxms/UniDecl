using System;
using UnityEngine;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class ColorField : Element
    {
        public string Label { get; set; }
        public Color Value { get; set; }
        public bool ShowAlpha { get; set; } = true;
        public bool ShowEyeDropper { get; set; } = true;
        public Action<Color> OnValueChanged { get; set; }

        public override IElement Render() => null;

        public ColorField(string label, Color value = default) { Label = label; Value = value; }
    }
}
