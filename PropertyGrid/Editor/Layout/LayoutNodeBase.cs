using System.Collections.Generic;

namespace UniDecl.PropertyGrid.Editor
{
    /// <summary>
    /// ILayoutNode 的抽象基类——提供 Parent/Children 管理与 AddChild。
    /// </summary>
    public abstract class LayoutNodeBase : ILayoutNode
    {
        public string Path { get; internal set; }
        public string DisplayName { get; set; }
        public int Order { get; set; }

        private ILayoutNode _parent;
        public ILayoutNode Parent => _parent;

        private readonly List<ILayoutNode> _children = new List<ILayoutNode>();
        public IReadOnlyList<ILayoutNode> Children => _children;

        /// <summary>添加子节点并设置 Parent。</summary>
        internal void AddChild(ILayoutNode child)
        {
            if (child is LayoutNodeBase childBase)
                childBase._parent = this;
            _children.Add(child);
        }

        /// <summary>批量添加子节点。</summary>
        internal void AddChildren(IEnumerable<ILayoutNode> children)
        {
            foreach (var child in children)
                AddChild(child);
        }
    }
}
