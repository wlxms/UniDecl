using System;
using UniDecl.Core;

namespace UniDecl.Widgets
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
