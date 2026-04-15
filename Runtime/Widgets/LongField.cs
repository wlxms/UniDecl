using System;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class LongField : Element
    {
        public string Label { get; set; }
        public long Value { get; set; }
        public Action<long, long> OnValueChanged { get; set; }
        public Action<long> OnCommit { get; set; }

        public override IElement Render() => null;

        public LongField(string label = null, long value = 0)
        {
            Label = label;
            Value = value;
        }
    }
}
