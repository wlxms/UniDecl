using System;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class ToolbarSearchField : Element
    {
        public string Value { get; set; }
        public Action<string> OnValueChanged { get; set; }
        public Action<string> OnCommit { get; set; }

        public override IElement Render() => null;

        public ToolbarSearchField() { }
    }
}
