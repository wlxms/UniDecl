using System;
using System.Collections.Generic;
using System.Reflection;
using UniDecl.Runtime.Core;
using UniDecl.Runtime.Navigation;
using UnityEditor;
using UnityEngine;

namespace UniDecl.Editor
{
    public class EditorHostManager : HostManager<EditorHostManager>
    {
        private readonly Dictionary<string, Type> _windowTypes = new Dictionary<string, Type>();
        private readonly Dictionary<string, string> _pendingNav = new Dictionary<string, string>();

        public EditorHostManager()
        {
            ScanDeclHostWindowAttributes();
        }

        [InitializeOnLoadMethod]
        private static void EnsureInitialized()
        {
            var _ = Instance;
        }

        private void ScanDeclHostWindowAttributes()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException) { continue; }

                foreach (var type in types)
                {
                    if (type.IsAbstract) continue;
                    var attr = type.GetCustomAttribute<DeclHostWindowAttribute>();
                    if (attr == null) continue;
                    var key = !string.IsNullOrEmpty(attr.HostName) ? attr.HostName : type.Name;
                    _windowTypes[key] = type;
                }
            }
        }

        public override void Register(string name, IElementRenderHostBase host)
        {
            base.Register(name, host);
            // 自动回放待处理导航：注册即可用，调用方无需单独 NotifyHostReady
            if (_pendingNav.TryGetValue(name, out var anchor))
            {
                _pendingNav.Remove(name);
                host.NavigateTo(anchor);
            }
        }

        public override void NavigateURL(string url)
        {
            var p = NavigationURL.Parse(url);
            if (p?.Host == null) return;

            var hostName = p.Host;
            var anchor = p.Fragment;

            // Case 1: already registered host
            var existingHost = GetHost(hostName);
            if (existingHost != null)
            {
                if (anchor != null) existingHost.NavigateTo(anchor);
                return;
            }

            // Case 2: type route lookup
            if (!_windowTypes.TryGetValue(hostName, out var type)) return;

            if (typeof(EditorWindow).IsAssignableFrom(type))
            {
                // EditorWindow path
                var window = EditorWindow.GetWindow(type);

                if (window is IDeclHostWindow declWindow)
                {
                    var windowHost = declWindow.GetHost();
                    if (windowHost != null)
                    {
                        // Case A: host ready, navigate directly
                        if (anchor != null) windowHost.NavigateTo(anchor);
                    }
                    else
                    {
                        // Case B: host not ready, store pending
                        if (anchor != null)
                            _pendingNav[hostName] = anchor;
                    }
                }
                else
                {
                    // Case C: not IDeclHostWindow, just open window
                }
            }
            else
            {
                // Non-EditorWindow path
                if (typeof(IDeclHostWindow).IsAssignableFrom(type))
                {
                    // Case D: instantiate and get host
                    try
                    {
                        var instance = Activator.CreateInstance(type) as IDeclHostWindow;
                        var instanceHost = instance?.GetHost();
                        if (instanceHost != null)
                        {
                            if (anchor != null) instanceHost.NavigateTo(anchor);
                        }
                        else
                        {
                            // Case E: host is null after instantiation
                            Debug.LogError($"[EditorHostManager] IDeclHostWindow '{type.Name}' returned null host after instantiation.");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[EditorHostManager] Failed to instantiate '{type.Name}': {e.Message}");
                    }
                }
                else
                {
                    // Case F: not IDeclHostWindow
                    Debug.LogError($"[EditorHostManager] Type '{type.Name}' is not an EditorWindow or IDeclHostWindow.");
                }
            }
        }
    }
}
