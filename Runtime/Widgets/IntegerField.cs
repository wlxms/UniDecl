using System;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class IntegerField : Element
    {
        public int Value { get; set; }
        public Action<int, int> OnValueChanged { get; set; }
        public Action<int> OnCommit { get; set; }

        public override IElement Render() => null;

        public IntegerField(int value = 0) { Value = value; }
    }
}
