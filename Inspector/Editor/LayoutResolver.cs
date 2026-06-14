using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniDecl.Inspector.Runtime;

namespace UniDecl.Inspector.Editor
{
    // =========================================================================
    // 布局项模型
    // =========================================================================

    /// <summary>
    /// 布局项基类——字段和类级控件在布局系统中的统一抽象
    /// </summary>
    public abstract class LayoutItem
    {
        public string GroupPath = "Root";
        public int Order;
    }

    /// <summary>
    /// 字段布局项
    /// </summary>
    public class FieldItem : LayoutItem
    {
        public FieldInfo Field;
        public InspectorAttribute[] Attributes;
    }

    /// <summary>
    /// 类级控件布局项（Button/Label/InfoBox 等与字段无关的 UI 元素）
    /// </summary>
    public class ClassElementItem : LayoutItem
    {
        public InspectorAttribute Source;
        public string GroupBy;
    }

    // =========================================================================
    // 组类型
    // =========================================================================

    /// <summary>
    /// 组容器类型
    /// </summary>
    public enum GroupType
    {
        Root,       // 根级——垂直排列
        Horizontal, // 水平排列
        Vertical,   // 垂直排列
        Box,        // 边框区块
        Foldout,    // 折叠组
        Header,     // 标题组
        Tab,        // 标签页
    }

    // =========================================================================
    // 布局树
    // =========================================================================

    /// <summary>
    /// 布局节点——组的树结构
    /// </summary>
    public class LayoutNode
    {
        public string Path;
        public GroupType Type = GroupType.Vertical;
        public string Title;
        public int Order;
        public bool Expanded = true;
        public List<LayoutItem> Items = new List<LayoutItem>();
        public List<LayoutNode> Children = new List<LayoutNode>();
    }

    /// <summary>
    /// 布局树——根节点
    /// </summary>
    public class LayoutTree
    {
        public LayoutNode Root;
    }

    // =========================================================================
    // 布局解析器
    // =========================================================================

    /// <summary>
    /// 统一布局解析器——实现传染式布局扩散和类级控件分配
    /// 
    /// 管线：TypeMeta → LayoutResolver.Resolve() → LayoutTree
    /// 
    /// 算法：
    /// 1. 收集所有 LayoutItem（字段项 + 类级控件项）
    /// 2. 对字段项应用传染式布局扩散
    /// 3. 按 GroupPath 分组，构建 LayoutTree
    /// 4. 组内按 Order 排序
    /// </summary>
    public static class LayoutResolver
    {
        private static int _autoGroupCounter;

        /// <summary>
        /// 解析类型元数据为布局树
        /// </summary>
        public static LayoutTree Resolve(TypeMeta meta)
        {
            _autoGroupCounter = 0;
            var tree = new LayoutTree { Root = new LayoutNode { Path = "Root", Type = GroupType.Root } };

            var items = new List<LayoutItem>();
            var groupTypeMap = new Dictionary<string, GroupType>(); // 组路径 → 组类型（锁定）
            var groupMetaMap = new Dictionary<string, GroupMeta>(); // 组路径 → 元数据

            // 复制类级组元数据
            foreach (var kv in meta.ClassGroupMeta)
            {
                groupMetaMap[kv.Key] = kv.Value;
            }

            // 1. 遍历字段，生成 FieldItem，应用传染式扩散
            var currentGroup = "Root";
            foreach (var field in meta.Fields)
            {
                var fieldAttrs = field.GetCustomAttributes<InspectorAttribute>(false).ToArray();
                var item = new FieldItem { Field = field, Attributes = fieldAttrs };

                // 检查是否有布局属性（改变当前组）
                var layoutAttr = GetLayoutAttribute(fieldAttrs);
                if (layoutAttr != null)
                {
                    var path = !string.IsNullOrEmpty(layoutAttr.Path)
                        ? layoutAttr.Path
                        : $"_AutoGroup_{_autoGroupCounter++}";

                    currentGroup = path;

                    // 确定组类型
                    var groupType = GetGroupType(layoutAttr);
                    if (groupTypeMap.TryGetValue(path, out var existingType))
                    {
                        // 类型锁定检查
                        if (existingType != groupType)
                        {
                            UnityEngine.Debug.LogWarning(
                                $"[Inspector] Group type mismatch for '{path}': " +
                                $"expected {existingType}, got {groupType}. Using first declared type.");
                            groupType = existingType;
                        }
                    }
                    else
                    {
                        groupTypeMap[path] = groupType;
                    }

                    // 合并元数据
                    if (layoutAttr.Title != null)
                    {
                        if (!groupMetaMap.ContainsKey(path))
                            groupMetaMap[path] = new GroupMeta();
                        groupMetaMap[path].Title = layoutAttr.Title;
                    }
                    if (layoutAttr is FoldoutGroupAttribute foldoutAttr)
                    {
                        if (!groupMetaMap.ContainsKey(path))
                            groupMetaMap[path] = new GroupMeta();
                        groupMetaMap[path].Expanded = foldoutAttr.Expanded;
                    }
                }

                // PropertyOrder
                var orderAttr = field.GetCustomAttribute<PropertyOrderAttribute>();
                if (orderAttr != null)
                    item.Order = orderAttr.Order;

                item.GroupPath = currentGroup;
                items.Add(item);
            }

            // 2. 收集类级控件，生成 ClassElementItem
            if (meta.ClassAttributes != null)
            {
                foreach (var attr in meta.ClassAttributes)
                {
                    ClassElementItem classItem = null;

                    if (attr is ButtonAttribute btnAttr && !string.IsNullOrEmpty(btnAttr.Label))
                    {
                        classItem = new ClassElementItem { Source = attr, GroupBy = btnAttr.GroupBy ?? "Root", Order = btnAttr.Order };
                    }
                    else if (attr is InspectorLabelAttribute labelAttr)
                    {
                        classItem = new ClassElementItem { Source = attr, GroupBy = labelAttr.GroupBy ?? "Root", Order = labelAttr.Order };
                    }
                    else if (attr is InspectorInfoBoxAttribute infoAttr)
                    {
                        classItem = new ClassElementItem { Source = attr, GroupBy = infoAttr.GroupBy ?? "Root", Order = infoAttr.Order };
                    }
                    else if (attr is InfoBoxAttribute fieldInfoAttr)
                    {
                        // 字段级 InfoBox 也可能出现在类上
                        classItem = new ClassElementItem { Source = attr, GroupBy = fieldInfoAttr.GroupBy ?? "Root", Order = fieldInfoAttr.Order };
                    }

                    if (classItem != null)
                    {
                        classItem.GroupPath = classItem.GroupBy;
                        items.Add(classItem);
                    }
                }
            }

            // 3. 构建 LayoutTree
            BuildTree(tree.Root, items, groupTypeMap, groupMetaMap);

            return tree;
        }

        /// <summary>
        /// 从属性中获取布局属性（HGroup/VGroup/BoxGroup/FoldoutGroup/HeaderGroup/TabGroup）
        /// </summary>
        private static LayoutGroupAttribute GetLayoutAttribute(InspectorAttribute[] attrs)
        {
            for (int i = attrs.Length - 1; i >= 0; i--)
            {
                if (attrs[i] is LayoutGroupAttribute layout)
                    return layout;
            }
            return null;
        }

        /// <summary>
        /// 根据属性类型确定组类型
        /// </summary>
        private static GroupType GetGroupType(LayoutGroupAttribute attr)
        {
            if (attr is HGroupAttribute) return GroupType.Horizontal;
            if (attr is VGroupAttribute) return GroupType.Vertical;
            if (attr is BoxGroupAttribute) return GroupType.Box;
            if (attr is FoldoutGroupAttribute) return GroupType.Foldout;
            if (attr is HeaderGroupAttribute) return GroupType.Header;
            if (attr is TabGroupAttribute) return GroupType.Tab;
            return GroupType.Vertical;
        }

        /// <summary>
        /// 构建 LayoutTree 节点
        /// </summary>
        private static void BuildTree(LayoutNode root, List<LayoutItem> items,
            Dictionary<string, GroupType> groupTypeMap, Dictionary<string, GroupMeta> groupMetaMap)
        {
            // 按 GroupPath 分组
            var groups = new Dictionary<string, List<LayoutItem>>();
            foreach (var item in items)
            {
                if (!groups.TryGetValue(item.GroupPath, out var list))
                {
                    list = new List<LayoutItem>();
                    groups[item.GroupPath] = list;
                }
                list.Add(item);
            }

            // 为每个组创建 LayoutNode
            var nodeMap = new Dictionary<string, LayoutNode>();
            nodeMap["Root"] = root;

            foreach (var kv in groups)
            {
                if (kv.Key == "Root")
                {
                    root.Items = kv.Value.OrderBy(i => i.Order).ToList();
                    continue;
                }

                // 确保路径上所有祖先节点都存在
                EnsurePathExists(root, kv.Key, groupTypeMap, groupMetaMap, nodeMap);

                var node = nodeMap[kv.Key];
                node.Items = kv.Value.OrderBy(i => i.Order).ToList();
            }
        }

        /// <summary>
        /// 确保路径上所有节点都存在（如 "Stats/Row" 需要 Stats 和 Stats/Row 都有节点）
        /// </summary>
        private static void EnsurePathExists(LayoutNode root, string path,
            Dictionary<string, GroupType> groupTypeMap, Dictionary<string, GroupMeta> groupMetaMap,
            Dictionary<string, LayoutNode> nodeMap)
        {
            var segments = path.Split('/');
            var currentPath = "";
            LayoutNode parent = root;

            for (int i = 0; i < segments.Length; i++)
            {
                currentPath = i == 0 ? segments[i] : $"{currentPath}/{segments[i]}";

                if (nodeMap.TryGetValue(currentPath, out var existing))
                {
                    parent = existing;
                    continue;
                }

                // 创建新节点
                var groupType = groupTypeMap.TryGetValue(currentPath, out var gt) ? gt : GroupType.Vertical;
                var node = new LayoutNode
                {
                    Path = currentPath,
                    Type = gt,
                };

                // 应用元数据
                if (groupMetaMap.TryGetValue(segments[i], out var meta))
                {
                    node.Title = meta.Title;
                    node.Order = meta.Order;
                    node.Expanded = meta.Expanded;
                }

                parent.Children.Add(node);
                nodeMap[currentPath] = node;
                parent = node;
            }
        }
    }
}
