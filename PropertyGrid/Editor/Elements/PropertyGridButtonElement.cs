using System;
using UniDecl.BuiltIn.Runtime.Core;

namespace UniDecl.PropertyGrid.Editor.Elements
{
    /// <summary>
    /// PropertyGrid 按钮 Element——类级或字段级按钮
    /// </summary>
    public class PropertyGridButtonElement : Element
    {
        public string Label { get; }
        public Action OnClick { get; set; }
        public string GroupPath { get; set; }

        public override IElement Render() => null;

        public PropertyGridButtonElement(string label)
        {
            Label = label;
        }
    }
}
