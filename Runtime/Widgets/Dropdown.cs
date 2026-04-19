using System;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class Dropdown : Element
    {
        public string Label { get; set; }
        public string[] Choices { get; set; }
        public int Index { get; set; }
        public Action<int> OnSelectionChanged { get; set; }

        public override IElement Render() => null;

        public Dropdown(string label, string[] choices, int index = 0)
        {
            Label = label;
            Choices = choices;
            Index = index;
        }
        public Dropdown(string label, string[] choices, int index = 0, params IElementComponent[] components) : base(components)
        {
            Label = label;
            Choices = choices;
            Index = index;
        }
    }
}
