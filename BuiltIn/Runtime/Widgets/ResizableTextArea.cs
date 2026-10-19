using System;
using UniDecl.BuiltIn.Runtime.Core;

namespace UniDecl.BuiltIn.Runtime.Widgets
{
    public class ResizableTextArea : Element
    {
        public string Value { get; set; }
        public string Label { get; set; }
        public Action<string, string> OnValueChanged { get; set; }
        public Action<string> OnCommit { get; set; }

        /// <summary>最小行数（[TextArea] MinLines，渲染层据此计算 minHeight）</summary>
        public int MinLines { get; set; }

        /// <summary>最大行数（[TextArea] MaxLines，0 = 不限制）</summary>
        public int MaxLines { get; set; }

        public override IElement Render() => null;

        public ResizableTextArea(string value = "", string label = null)
        {
            Value = value;
            Label = label;
        }
    }
}
