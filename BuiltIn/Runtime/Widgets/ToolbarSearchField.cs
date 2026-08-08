using System;
using UniDecl.BuiltIn.Runtime.Core;

namespace UniDecl.BuiltIn.Runtime.Widgets
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
