using System;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class ResizableTextArea : Element
    {
        public string Value { get; set; }
        public string Label { get; set; }
        public Action<string, string> OnValueChanged { get; set; }
        public Action<string> OnCommit { get; set; }

        public override IElement Render() => null;

        public ResizableTextArea(string value = "", string label = null)
        {
            Value = value;
            Label = label;
        }
    }
}
