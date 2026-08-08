using System;
using System.Collections.Generic;
using System.Reflection;
using UniDecl.PropertyGrid.Runtime;

namespace UniDecl.PropertyGrid.Editor
{
    /// <summary>Inline 展开方向枚举</summary>
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

        /// <summary>子对象类型元数据</summary>
        public TypeMeta Meta;

        /// <summary>子对象的 Renderer 类型（由 ReflectionCache 发现，可能为 null）</summary>
        public Type RendererType;

        /// <summary>该类级组声明 map（key = 组路径，value = 组元数据，Layout 阶段独立计算）</summary>
        public Dictionary<string, GroupMeta> ClassGroupMap;

        /// <summary>从父级传播过来的 Attribute（PropagateOnInline=true 者）</summary>
        public PropertyGridAttribute[] PropagatedAttributes;

        /// <summary>Inline 展开方向（InlineProperty 决定初始方向）</summary>
        public InlineDirection Direction = InlineDirection.Default;
    }
}
