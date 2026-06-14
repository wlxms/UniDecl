using System;
using System.Collections.Generic;
using UniDecl.Runtime.Core;
using UniDecl.Runtime.Widgets;
using UniDecl.Snapshot;
using UniDecl.Snapshot.Editor;
using UnityEditor;
using UnityEngine;

namespace UniDecl.Inspector.Editor
{
    /// <summary>
    /// Inspector 模块入口
    /// 
    /// 职责：
    /// 1. 初始化时注册 DOM expander，让 InspectorElement 在 BuildDOM 阶段展开
    /// 2. 提供 CreateElementTree() 构建 Element 子树
    /// 3. 提供 Open() API 打开 Inspector 窗口
    /// </summary>
    [InitializeOnLoad]
    public static class InspectorModule
    {
        private static bool _initialized;

        /// <summary>
        /// InspectorElement 与 EditorSnapshotManager 的映射。
        /// 嵌套 InspectorElement（共享 HostObject）复用父级 manager。
        /// </summary>
        private static readonly Dictionary<InspectorElement, EditorSnapshotManager> _snapshotManagers =
            new Dictionary<InspectorElement, EditorSnapshotManager>();

        static InspectorModule()
        {
            Initialize();
        }

        /// <summary>
        /// 获取或创建 InspectorElement 对应的 EditorSnapshotManager（跨 Rebuild 复用）。
        /// 嵌套 InspectorElement（HostObject 相同）共享同一个 manager。
        /// </summary>
        internal static EditorSnapshotManager GetOrCreateSnapshotManager(InspectorElement element)
        {
            if (_snapshotManagers.TryGetValue(element, out var existing))
                return existing;

            var hostObject = element.HostObject;
            if (hostObject == null)
            {
                UnityEngine.Debug.LogWarning("[Inspector] InspectorElement.HostObject is null, Undo will not work");
                return null;
            }

            // 复用同 hostObject 的已有 manager（嵌套 InspectorElement 继承父级 HostObject）
            foreach (var kvp in _snapshotManagers)
            {
                if (kvp.Value != null && ReferenceEquals(kvp.Key.HostObject, hostObject))
                {
                    _snapshotManagers[element] = kvp.Value;
                    return kvp.Value;
                }
            }

            var manager = new EditorSnapshotManager(new SnapshotManager());
            // OnUndoRedoPerformed: Rebuild + 强制窗口重绘
            manager.OnUndoRedoPerformed += () =>
            {
                element.Rebuild();
                if (hostObject is EditorWindow window && window.rootVisualElement != null)
                    window.rootVisualElement.MarkDirtyRepaint();
            };
            _snapshotManagers[element] = manager;
            return manager;
        }

        /// <summary>
        /// 释放 InspectorElement 对应的 SnapshotManager（取消事件订阅、清理缓存）。
        /// 只对 manager 的持有者（顶层 element）执行 Dispose。
        /// </summary>
        internal static void ReleaseSnapshotManager(InspectorElement element)
        {
            if (!_snapshotManagers.TryGetValue(element, out var mgr))
                return;

            _snapshotManagers.Remove(element);

            // 检查是否还有其他 element 共享此 manager
            foreach (var kvp in _snapshotManagers)
            {
                if (ReferenceEquals(kvp.Value, mgr))
                    return; // 仍有使用者，不 Dispose
            }

            // 最后一个使用者，执行清理
            mgr?.Dispose();
        }

        /// <summary>
        /// 初始化 Inspector 模块——注册 DOM expander
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            // DOM 期展开：InspectorElement 在 BuildDOM 阶段展开为普通 Element 子树。
            ElementDomExpanderRegistry.Register<InspectorElement>((element, _) => CreateElementTree(element));
        }

        /// <summary>
        /// 为 InspectorElement 构建 Element 子树
        /// 
        /// 管线：InspectorElement(Target) 
        ///   → ReflectionCache.GetOrCreateMeta(Target.GetType())
        ///   → LayoutResolver.Resolve(TypeMeta)
        ///   → WidgetFactory.CreateTree(LayoutTree, BuildContext)
        ///   → Element 子树
        /// </summary>
        public static IElement CreateElementTree(InspectorElement element)
        {
            if (element?.Target == null) return null;

            var target = element.Target;
            var meta = ReflectionCache.GetOrCreateMeta(target.GetType());

            // 解析布局
            var layoutTree = LayoutResolver.Resolve(meta);

            // 创建 Renderer 实例（如果有）
            object renderer = null;
            if (meta.RendererType != null)
            {
                try { renderer = Activator.CreateInstance(meta.RendererType); }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Inspector] Failed to create renderer {meta.RendererType.Name}: {ex.Message}");
                }
            }

            // 构建上下文
            var ctx = new WidgetFactory.BuildContext
            {
                Target = target,
                Renderer = renderer,
                Meta = meta,
                SnapshotManager = GetOrCreateSnapshotManager(element),
                InspectorElement = element,
                OnRebuildNeeded = () =>
                {
                    // 触发 InspectorElement 的 Rebuild
                    element.Rebuild();
                },
            };

            // 构建 Element 子树
            return WidgetFactory.CreateTree(layoutTree, ctx);
        }
    }
}
