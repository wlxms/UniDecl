using System;
using UnityEngine;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class CurveField : Element
    {
        public string Label { get; set; }
        public AnimationCurve Value { get; set; }
        public float MinX { get; set; }
        public float MaxX { get; set; }
        public float MinY { get; set; }
        public float MaxY { get; set; }
        public Color? CurveColor { get; set; }
        public Action<AnimationCurve> OnValueChanged { get; set; }

        public override IElement Render() => null;

        public CurveField(string label, AnimationCurve value = null, float minX = 0f, float maxX = 1f, float minY = 0f, float maxY = 1f, Color? color = null)
        {
            Label = label;
            Value = value ?? new AnimationCurve();
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
            CurveColor = color;
        }
    }
}
