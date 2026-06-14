using System;
using UniDecl.Runtime.Core;

namespace UniDecl.Inspector.Editor.Elements
{
    /// <summary>
    /// Inspector 按钮 Element——类级或字段级按钮
    /// </summary>
    public class InspectorButtonElement : Element
    {
        public string Label { get; }
        public Action OnClick { get; set; }
        public string GroupPath { get; set; }

        public override IElement Render() => null;

        public InspectorButtonElement(string label)
        {
            Label = label;
        }
    }
}
