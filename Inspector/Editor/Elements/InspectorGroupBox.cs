using System.Collections.Generic;
using UniDecl.Runtime.Core;

namespace UniDecl.Inspector.Editor.Elements
{
    /// <summary>
    /// Inspector 组容器 Element——对应 Box/Foldout/Header 组
    /// </summary>
    public class InspectorGroupBox : ContainerElement
    {
        public string GroupPath { get; }
        public string Title { get; set; }
        public bool Expanded { get; set; } = true;
        public global::UniDecl.Inspector.Editor.GroupType Type { get; set; }

        private readonly List<IElement> _children = new List<IElement>();
        public override IEnumerable<IElement> Children => _children;
        public override void Add(IElement element) => _children.Add(element);
        public override IElement Render() => null;

        public InspectorGroupBox(string groupPath)
        {
            GroupPath = groupPath;
        }
    }
}
