using System;
using System.Collections.Generic;
using UniDecl.Runtime.Core;

namespace UniDecl.Inspector.Editor.Elements
{
    /// <summary>
    /// Inspector 字段包装 Element——包装一个字段的 Widget，携带字段元数据
    /// </summary>
    public class InspectorPropertyField : ContainerElement
    {
        public string FieldName { get; }
        public string Tooltip { get; set; }
        public bool IsReadOnly { get; set; }
        public string SuffixLabel { get; set; }
        public int IndentLevel { get; set; }
        public IElement FieldWidget { get; set; }

        private readonly List<IElement> _children = new List<IElement>();
        public override IEnumerable<IElement> Children => _children;
        public override void Add(IElement element) => _children.Add(element);
        public override IElement Render() => null;

        public InspectorPropertyField(string fieldName)
        {
            FieldName = fieldName;
        }
    }
}
