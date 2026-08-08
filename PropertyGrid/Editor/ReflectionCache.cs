using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniDecl.PropertyGrid.Runtime;
using UnityEngine;

namespace UniDecl.PropertyGrid.Editor
{
    /// <summary>
    /// 类级组元数据
    /// </summary>
    public class GroupMeta
    {
        public string Title;
        public int Order;
        public bool Expanded = true;
    }

    /// <summary>
    /// 类型元数据——缓存一个数据类型的反射结果
    /// </summary>
    public class TypeMeta
    {
        public Type Type;
        public List<FieldInfo> Fields = new List<FieldInfo>();
        public PropertyGridAttribute[] ClassAttributes;
        public Type RendererType;
    }

    /// <summary>
    /// 反射缓存——对数据类进行反射分析并缓存结果
    /// 包括：字段收集、属性标记解析、Renderer 发现、条件依赖反向索引
    /// </summary>
    public static class ReflectionCache
    {
        private static readonly Dictionary<Type, TypeMeta> _cache = new Dictionary<Type, TypeMeta>();

        /// <summary>
        /// 获取或创建类型的元数据
        /// </summary>
        public static TypeMeta GetOrCreateMeta(Type type)
        {
            if (_cache.TryGetValue(type, out var meta))
                return meta;

            meta = new TypeMeta { Type = type };
            AnalyzeType(meta);
            _cache[type] = meta;
            return meta;
        }

        /// <summary>
        /// 清除缓存（测试用）
        /// </summary>
        public static void Clear() => _cache.Clear();

        private static void AnalyzeType(TypeMeta meta)
        {
            var type = meta.Type;

            // 1. 收集字段：public + [SerializeField] private/protected
            var allFields = new List<FieldInfo>();
            CollectFields(type, allFields);

            // 过滤掉编译器生成的字段（如自动属性后备字段）
            meta.Fields = allFields
                .Where(f => !f.Name.StartsWith("<") && !f.Name.Contains("k__BackingField"))
                .ToList();

            // 2. 收集类级属性
            var classAttrs = new List<PropertyGridAttribute>();
            CollectClassAttributes(type, classAttrs);
            meta.ClassAttributes = classAttrs.ToArray();

            // 3. 发现 Renderer
            meta.RendererType = FindRendererType(type);
        }

        /// <summary>
        /// 递归收集字段（支持继承，父类字段排在前面）
        /// </summary>
        private static void CollectFields(Type type, List<FieldInfo> result)
        {
            if (type == null || type == typeof(object)) return;

            // 先收集父类字段
            CollectFields(type.BaseType, result);

            // 当前类声明的字段
            var declared = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);

            foreach (var field in declared)
            {
                // public 字段始终包含
                if (field.IsPublic)
                {
                    result.Add(field);
                    continue;
                }

                // 非 public 字段需要 [SerializeField]
                if (field.GetCustomAttribute<SerializeField>() != null)
                {
                    result.Add(field);
                }
            }
        }

        /// <summary>
        /// 收集类级 PropertyGrid 属性
        /// </summary>
        private static void CollectClassAttributes(Type type, List<PropertyGridAttribute> attrs)
        {
            var typeChain = new List<Type>();
            var current = type;
            while (current != null && current != typeof(object))
            {
                typeChain.Add(current);
                current = current.BaseType;
            }
            typeChain.Reverse();

            foreach (var t in typeChain)
            {
                foreach (var attr in t.GetCustomAttributes<PropertyGridAttribute>(false))
                {
                    attrs.Add(attr);
                }
            }
        }

        /// <summary>
        /// 发现 Renderer 类型：在数据类所在程序集和所有 Editor 程序集中搜索
        /// </summary>
        private static Type FindRendererType(Type targetType)
        {
            var assemblies = new HashSet<Assembly>();

            // 数据类所在程序集
            assemblies.Add(targetType.Assembly);

            // 所有 Editor 程序集
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var name = asm.GetName().Name;
                if (name != null && name.Contains("Editor"))
                    assemblies.Add(asm);
            }

            Type result = null;
            foreach (var asm in assemblies)
            {
                try
                {
                    foreach (var type in asm.GetTypes())
                    {
                        var attr = type.GetCustomAttribute<PropertyGridRendererAttribute>();
                        if (attr == null) continue;

                        if (attr.TargetType == targetType)
                        {
                            if (result != null)
                            {
                                Debug.LogWarning($"[PropertyGrid] Multiple renderers found for {targetType.Name}: {result.Name} and {type.Name}. Using first found: {result.Name}");
                            }
                            else
                            {
                                result = type;
                            }
                        }
                        // 父类匹配回退
                        else if (result == null && attr.TargetType != null && attr.TargetType.IsAssignableFrom(targetType))
                        {
                            result = type;
                        }
                    }
                }
                catch
                {
                    // 忽略无法加载的程序集
                }
            }

            return result;
        }
    }
}
