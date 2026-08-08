using System;
using System.Collections.Generic;
using UniDecl.BuiltIn.Runtime.Core;

namespace UniDecl.PropertyGrid.Editor.Elements
{
    /// <summary>
    /// 条件容器 Element——根据条件显示/隐藏子元素
    /// </summary>
    public class PropertyGridConditionalElement : ContainerElement
    {
        public bool IsVisible { get; set; } = true;

        private readonly List<IElement> _children = new List<IElement>();
        public override IEnumerable<IElement> Children => _children;
        public override void Add(IElement element) => _children.Add(element);
        public override IElement Render() => null;

        public PropertyGridConditionalElement() { }
    }
}
