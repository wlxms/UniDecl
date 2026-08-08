using System;

namespace UniDecl.BuiltIn.Runtime.Core
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DeclHostWindowAttribute : Attribute
    {
        public string HostName { get; }

        public DeclHostWindowAttribute() { }

        public DeclHostWindowAttribute(string hostName) => HostName = hostName;
    }
}
