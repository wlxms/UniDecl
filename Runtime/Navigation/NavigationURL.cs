namespace UniDecl.Runtime.Navigation
{
    public class NavigationURL
    {
        public string Scheme { get; }
        public string Host { get; }
        public string Path { get; }
        public string Query { get; }
        public string Fragment { get; }

        private NavigationURL(string scheme, string host, string path, string query, string fragment)
        {
            Scheme = scheme;
            Host = host;
            Path = path;
            Query = query;
            Fragment = fragment;
        }

        public static NavigationURL Parse(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;

            // #frag → Fragment only
            if (url.StartsWith("#"))
                return new NavigationURL(null, null, null, null, url.Substring(1));

            // scheme://host/path?query#frag
            var fragmentStart = url.IndexOf('#');
            string fragment = null;
            string remainder = url;
            if (fragmentStart >= 0)
            {
                fragment = url.Substring(fragmentStart + 1);
                remainder = url.Substring(0, fragmentStart);
            }

            var queryStart = remainder.IndexOf('?');
            string query = null;
            if (queryStart >= 0)
            {
                query = remainder.Substring(queryStart + 1);
                remainder = remainder.Substring(0, queryStart);
            }

            // Check for scheme://
            var schemeEnd = remainder.IndexOf("://");
            if (schemeEnd >= 0)
            {
                var scheme = remainder.Substring(0, schemeEnd);
                var afterScheme = remainder.Substring(schemeEnd + 3);
                var hostEnd = afterScheme.IndexOf('/');
                string host;
                string path;
                if (hostEnd >= 0)
                {
                    host = afterScheme.Substring(0, hostEnd);
                    path = afterScheme.Substring(hostEnd);
                }
                else
                {
                    host = afterScheme;
                    path = null;
                }
                return new NavigationURL(scheme, host, path, query, fragment);
            }

            // Plain id (no scheme, no #) → treat as Fragment
            return new NavigationURL(null, null, null, null, url);
        }
    }
}
