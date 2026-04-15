using System;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class EnumFlagsField : Element
    {
        public string Label { get; set; }
        public Type EnumType { get; set; }
        public int Value { get; set; }
        public Action<int> OnValueChanged { get; set; }

        public override IElement Render() => null;

        public EnumFlagsField(string label, Type enumType, int value = 0)
        {
            Label = label;
            EnumType = enumType;
            Value = value;
        }
    }
}
