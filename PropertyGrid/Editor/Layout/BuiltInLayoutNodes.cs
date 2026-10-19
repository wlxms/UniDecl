using System;
using System.Collections.Generic;
using System.Reflection;
using UniDecl.PropertyGrid;

namespace UniDecl.PropertyGrid.Editor
{
    /// <summary>嵌套对象 Value 的初始布局方向</summary>
    public enum InlineDirection { Default, Horizontal }

    // =========================================================================
    // 叶子节点
    // =========================================================================

    /// <summary>叶子：原子字段节点（int/string/Vector3...）</summary>
    public sealed class FieldLayoutNode : LayoutNodeBase
    {
        public PropertyAccessor Accessor;
        public PropertyGridAttribute[] Attributes;
    }

    /// <summary>叶子：类级控件节点（Button/Label/InfoBox）</summary>
    public sealed class ClassElementLayoutNode : LayoutNodeBase
    {
        public PropertyGridAttribute Source;
    }

    // =========================================================================
    // 容器节点：组
    // =========================================================================

    /// <summary>组节点抽象基类——持有 StyleClass。</summary>
    public abstract class GroupLayoutNode : LayoutNodeBase
    {
        public string StyleClass { get; set; }
    }

    public class HGroupLayoutNode : GroupLayoutNode { }
    public class VGroupLayoutNode : GroupLayoutNode { }
    public class BoxGroupLayoutNode : GroupLayoutNode { }
    public class HeaderLayoutNode : GroupLayoutNode { }
    public class TabLayoutNode : GroupLayoutNode { }

    /// <summary>
    /// 页签视图——同一父级下连续 TabLayoutNode 的复合容器（Normalize 阶段合成）。
    /// Children 即各 TabLayoutNode 页，DisplayName 为页标题。渲染为带页签条的 Tabs 控件。
    /// </summary>
    public sealed class TabViewLayoutNode : LayoutNodeBase
    {
        /// <summary>初始选中的页索引</summary>
        public int SelectedIndex;
    }

    /// <summary>折叠组——持有 Expanded 特有字段</summary>
    public class FoldoutLayoutNode : GroupLayoutNode
    {
        public bool Expanded = true;
    }

    // =========================================================================
    // 容器节点：可展开对象
    // =========================================================================

    /// <summary>可展开对象节点——每个 Serializable 子对象对应一个 ObjectLayoutNode。</summary>
    public sealed class ObjectLayoutNode : LayoutNodeBase
    {
        /// <summary>子对象实例</summary>
        public object Target;

        /// <summary>
        /// 当前对象作为父级字段 Value 时对应的字段访问器；根对象为 null。
        /// 该字段用于区分对象 Value 边界，不参与父级布局状态传播。
        /// </summary>
        public PropertyAccessor ValueAccessor { get; internal set; }

        /// <summary>子对象类型元数据</summary>
        public TypeMeta Meta;

        /// <summary>子对象的 Renderer 类型（由 ReflectionCache 发现，可能为 null）</summary>
        public Type RendererType;

        /// <summary>该类级组声明 map（key = 组路径，value = 组元数据，Layout 阶段独立计算）</summary>
        public Dictionary<string, GroupMeta> ClassGroupMap;

        /// <summary>从父级传播过来的 Attribute（PropagateOnInline=true 者）</summary>
        public PropertyGridAttribute[] PropagatedAttributes;

        /// <summary>
        /// 当前对象 Value 自身的初始布局方向。
        /// 这是字段级 InlineProperty 的局部结果，不是父级布局状态，也不会向兄弟节点传播。
        /// </summary>
        public InlineDirection InitialValueDirection { get; internal set; } = InlineDirection.Default;
    }
}
