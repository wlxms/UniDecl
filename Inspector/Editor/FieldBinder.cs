using System;
using System.Linq;
using System.Reflection;
using UniDecl.Inspector.Runtime;

namespace UniDecl.Inspector.Editor
{
    /// <summary>
    /// 字段绑定器——解析 @ 引用和方法绑定
    /// 
    /// @ 引用解析链：Renderer → 数据类 → 父级 → ... → 字面量
    /// 方法绑定：在 Renderer 类中查找匹配签名的方法
    /// </summary>
    public static class FieldBinder
    {
        /// <summary>
        /// 解析 @ 引用——从 Renderer 或数据类获取值
        /// </summary>
        public static string ResolveReference(string key, object renderer, object target)
        {
            if (string.IsNullOrEmpty(key) || !key.StartsWith("@"))
                return key;

            var name = key.Substring(1);

            // 1. 在 Renderer 中查找字段/属性/方法
            if (renderer != null)
            {
                var result = ResolveFromObject(renderer, name);
                if (result != null) return result.ToString();
            }

            // 2. 在数据类自身查找
            if (target != null)
            {
                var result = ResolveFromObject(target, name);
                if (result != null) return result.ToString();
            }

            // 3. 都没找到 → 去掉 @ 前缀作字面量
            return name;
        }

        /// <summary>
        /// 在 Renderer 中查找方法
        /// 签名约定：
        /// - 优先匹配带 target 参数的版本：Method(T target)
        /// - 回退到无参版本：Method()
        /// </summary>
        public static MethodInfo FindMethod(Type rendererType, string methodName, Type targetType)
        {
            if (rendererType == null || string.IsNullOrEmpty(methodName))
                return null;

            // 优先匹配带 target 参数的版本
            var withTarget = rendererType.GetMethod(methodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { targetType },
                null);

            if (withTarget != null) return withTarget;

            // 回退到无参版本
            return rendererType.GetMethod(methodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null);
        }

        /// <summary>
        /// 解析条件属性的值——读取数据类成员的当前值
        /// </summary>
        public static object ResolveConditionValue(string memberName, object target)
        {
            if (string.IsNullOrEmpty(memberName) || target == null)
                return null;

            // 优先查找字段
            var field = target.GetType().GetField(memberName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
                return field.GetValue(target);

            // 回退到属性
            var prop = target.GetType().GetProperty(memberName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null)
                return prop.GetValue(target);

            return null;
        }

        private static object ResolveFromObject(object obj, string name)
        {
            var type = obj.GetType();

            // 字段
            var field = type.GetField(name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
                return field.GetValue(obj);

            // 属性
            var prop = type.GetProperty(name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null)
                return prop.GetValue(obj);

            // 无参方法
            var method = type.GetMethod(name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null, Type.EmptyTypes, null);
            if (method != null)
            {
                try { return method.Invoke(obj, null); }
                catch { return null; }
            }

            return null;
        }
    }
}
