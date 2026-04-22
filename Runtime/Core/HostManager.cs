using System.Collections.Generic;
using UniDecl.Runtime.Navigation;

namespace UniDecl.Runtime.Core
{
    public abstract class HostManager<TSingleton> where TSingleton : HostManager<TSingleton>, new()
    {
        private static TSingleton _instance;
        public static TSingleton Instance => _instance ??= new TSingleton();

        protected readonly Dictionary<string, IElementRenderHostBase> _hosts = new();

        public virtual void Register(string name, IElementRenderHostBase host) => _hosts[name] = host;
        public virtual void Unregister(string name) => _hosts.Remove(name);
        public virtual IElementRenderHostBase GetHost(string name) =>
            _hosts.TryGetValue(name, out var h) ? h : null;

        public abstract void NavigateURL(string url);
    }
}
