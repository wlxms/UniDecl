using System;
using System.Collections.Generic;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.BuiltIn.Runtime.Snapshot;
using UniDecl.BuiltIn.Editor.Snapshot;
using UnityEditor;
using UnityEngine;

namespace UniDecl.PropertyGrid.Editor
{
    [InitializeOnLoad]
    public static class PropertyGridModule
    {
        static bool _init, _pluginsInit;
        static readonly Dictionary<PropertyGridElement, EditorSnapshotManager> _snaps = new Dictionary<PropertyGridElement, EditorSnapshotManager>();

        static PropertyGridModule() { Initialize(); }

        internal static EditorSnapshotManager GetOrCreateManager(PropertyGridElement e)
        {
            if (_snaps.TryGetValue(e, out var m)) return m;
            var ho = e.HostObject; if (ho == null) { Debug.LogWarning("[PropertyGrid] HostObject is null"); return null; }
            foreach (var kv in _snaps) if (kv.Value != null && ReferenceEquals(kv.Key.HostObject, ho)) { _snaps[e] = kv.Value; return kv.Value; }
            var mgr = new EditorSnapshotManager(new SnapshotManager());
            mgr.OnUndoRedoPerformed += () => { e.Rebuild(); if (ho is EditorWindow w && w.rootVisualElement != null) w.rootVisualElement.MarkDirtyRepaint(); };
            _snaps[e] = mgr; return mgr;
        }

        internal static void ReleaseManager(PropertyGridElement e)
        {
            if (!_snaps.TryGetValue(e, out var mgr)) return;
            _snaps.Remove(e);
            foreach (var kv in _snaps) if (ReferenceEquals(kv.Value, mgr)) return;
            mgr?.Dispose();
        }

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
            var mgr = GetOrCreateManager(element);
            if (mgr != null && element.Scope == null) element.Scope = new UndoScope(mgr);
            var ctx = BuildContext.CreateRoot(element, target, meta, mgr); ctx.Renderer = renderer;
            return WidgetFactory.CreateTree(tree, ctx);
        }
    }
}
