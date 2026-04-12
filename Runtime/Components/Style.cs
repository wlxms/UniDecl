using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Components
{
    public struct Style : IElementComponent
    {
        public string Name { get; set; }
        public Style(string name)
        {
            Name = name;
        }
    }
}