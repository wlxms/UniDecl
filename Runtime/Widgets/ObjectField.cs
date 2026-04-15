using System;
using UnityEngine;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class ObjectField : Element
    {
        public string Label { get; set; }
        public Type ObjectType { get; set; }
        public UnityEngine.Object Value { get; set; }
        public bool AllowSceneObjects { get; set; }
        public Action<UnityEngine.Object> OnValueChanged { get; set; }

        public override IElement Render() => null;

        public ObjectField(string label, Type objectType, UnityEngine.Object value = null)
        {
            Label = label;
            ObjectType = objectType;
            Value = value;
        }
    }
}
