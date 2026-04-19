using System.Collections.Generic;
using UniDecl.Runtime.Navigation;

namespace UniDecl.Runtime.Core
{
    public static class HostManager
    {
        private static readonly Dictionary<string, IElementRenderHostBase> _hosts = new();

        public static void Register(string name, IElementRenderHostBase host) => _hosts[name] = host;
        public static void Unregister(string name) => _hosts.Remove(name);
        public static IElementRenderHostBase GetHost(string name) =>
            _hosts.TryGetValue(name, out var h) ? h : null;

        public static void NavigateURL(string url)
        {
            var p = NavigationURL.Parse(url);
            if (p?.Fragment == null) return;
            var host = string.IsNullOrEmpty(p.Host) ? null : GetHost(p.Host);
            host?.NavigateTo(p.Fragment);
        }
    }
}
