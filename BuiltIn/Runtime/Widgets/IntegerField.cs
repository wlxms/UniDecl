using System;
using UniDecl.BuiltIn.Runtime.Core;

namespace UniDecl.BuiltIn.Runtime.Widgets
{
    public class IntegerField : Element
    {
        public string Label { get; set; }
        public int Value { get; set; }
        public Action<int, int> OnValueChanged { get; set; }
        public Action<int> OnCommit { get; set; }

        public override IElement Render() => null;

        public IntegerField(int value = 0) { Value = value; }
        public IntegerField(string label, int value = 0) { Label = label; Value = value; }
        public IntegerField(int value = 0, params IElementComponent[] components) : base(components) { Value = value; }
    }
}
