using System;
using System.Collections.Generic;
using System.Reflection;

namespace UniDecl.BuiltIn.Runtime.Core
{
    /// <summary>
    /// RenderHost Plugin 反射发现——扫描所有程序集中带 [RenderHostPlugin] 的类，
    /// 按 FullName 字典序稳定排序，实例化后返回。
    /// </summary>
    public static class RenderHostPluginDiscovery
    {
        private static List<IElementRenderHostPlugin> _cached;

        public static List<IElementRenderHostPlugin> Discover()
        {
            if (_cached != null) return _cached;

            var result = new List<IElementRenderHostPlugin>();
            var pluginTypes = new List<Type>();

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch { continue; }

                foreach (var t in types)
                {
                    try
                    {
                        if (!t.IsClass || t.IsAbstract) continue;
                        if (t.GetCustomAttribute<RenderHostPluginAttribute>() == null) continue;
                        if (!typeof(IElementRenderHostPlugin).IsAssignableFrom(t)) continue;
                        pluginTypes.Add(t);
                    }
                    catch { }
                }
            }

            pluginTypes.Sort((a, b) => string.Compare(a.FullName, b.FullName, StringComparison.Ordinal));

            foreach (var t in pluginTypes)
            {
                try
                {
                    result.Add((IElementRenderHostPlugin)Activator.CreateInstance(t));
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"[RenderHost] Failed to instantiate plugin '{t.FullName}': {ex.Message}");
                }
            }

            _cached = result;
            return result;
        }

        /// <summary>清空缓存（仅测试用）</summary>
        public static void ClearCache() => _cached = null;
    }
}
