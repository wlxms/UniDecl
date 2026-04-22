using UniDecl.Runtime.Navigation;

namespace UniDecl.Runtime.Core
{
    public class RuntimeHostManager : HostManager<RuntimeHostManager>
    {
        public override void NavigateURL(string url)
        {
            var p = NavigationURL.Parse(url);
            if (p?.Fragment == null) return;
            if (string.IsNullOrEmpty(p.Host)) return;
            var host = GetHost(p.Host);
            host?.NavigateTo(p.Fragment);
        }
    }
}
