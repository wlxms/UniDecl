using System.Collections.Generic;

namespace UniDecl.PropertyGrid.Editor
{
    /// <summary>
    /// 统一布局节点接口——替代旧的 LayoutItem/FieldItem/ClassElementItem 双层结构。
    /// 字段、组、对象、类级控件都是 ILayoutNode 的具体子类。
    /// </summary>
    public interface ILayoutNode
    {
        /// <summary>唯一标识（如 "Character/stats/hp"）</summary>
        string Path { get; }

        /// <summary>UI 显示名</summary>
        string DisplayName { get; set; }

        /// <summary>排序权重</summary>
        int Order { get; set; }

        /// <summary>父节点（顶层为 null）</summary>
        ILayoutNode Parent { get; }

        /// <summary>子节点（只读）</summary>
        IReadOnlyList<ILayoutNode> Children { get; }
    }
}
