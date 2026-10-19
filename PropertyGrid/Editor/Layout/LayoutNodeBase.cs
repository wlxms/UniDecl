using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// 按 Order 稳定排序子节点（同 Order 保持声明顺序）。
        /// 布局顺序在此统一决定，渲染阶段按 Children 顺序直接构建。
        /// </summary>
        internal void SortChildrenStable()
        {
            // List.Sort 不稳定；用原始 index 作次序键实现稳定排序
            var sorted = _children
                .Select((c, i) => (node: c, index: i))
                .OrderBy(x => x.node.Order)
                .ThenBy(x => x.index)
                .Select(x => x.node)
                .ToList();
            _children.Clear();
            _children.AddRange(sorted);
        }

        /// <summary>用单个节点替换一段连续子节点（Tab 包装等结构性后处理用）。</summary>
        internal void ReplaceChildrenRange(int index, int count, ILayoutNode replacement)
        {
            _children.RemoveRange(index, count);
            var insertAt = Math.Max(0, Math.Min(index, _children.Count));
            _children.Insert(insertAt, replacement);
            if (replacement is LayoutNodeBase rb)
                rb._parent = this;
        }

        /// <summary>原位替换一个子节点（组节点类型升级用），保留其在兄弟中的位置。</summary>
        internal void ReplaceChild(ILayoutNode oldNode, ILayoutNode newNode)
        {
            var ix = _children.IndexOf(oldNode);
            if (ix < 0) return;
            _children[ix] = newNode;
            if (newNode is LayoutNodeBase nb)
                nb._parent = this;
        }
    }
}
