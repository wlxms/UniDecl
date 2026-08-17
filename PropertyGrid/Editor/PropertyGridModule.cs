using System;
using UniDecl.BuiltIn.Runtime.Core;
using UnityEditor;
using UnityEngine;

namespace UniDecl.PropertyGrid.Editor
{
    [InitializeOnLoad]
    public static class PropertyGridModule
    {
        static bool _init, _pluginsInit;

        static PropertyGridModule() { Initialize(); }

        public static void Initialize() { if (_init) return; _init = true; InitPlugins(); }

        static void InitPlugins() { if (_pluginsInit) return; _pluginsInit = true; foreach (var p in PluginDiscovery.Discover()) { var r = new PluginRegistry(p.Name); try { p.OnInit(r); r.MountToGlobal(); } catch (Exception ex) { Debug.LogError($"[PropertyGrid] Plugin '{p.Name}' failed: {ex}"); } } }

        public static IElement CreateElementTree(PropertyGridElement element)
        {
            if (element?.Target == null) return null;
            var target = element.Target;
            var meta = ReflectionCache.GetOrCreateMeta(target.GetType());
            var tree = LayoutResolver.Resolve(meta, target);
            object renderer = null;
            if (meta.RendererType != null) try { renderer = Activator.CreateInstance(meta.RendererType); } catch (Exception ex) { Debug.LogWarning($"[PropertyGrid] Renderer failed: {ex.Message}"); }
            var ctx = BuildContext.CreateRoot(element, target, meta);
            ctx.Renderer = renderer;
            return WidgetFactory.CreateTree(tree, ctx);
        }
    }
}
