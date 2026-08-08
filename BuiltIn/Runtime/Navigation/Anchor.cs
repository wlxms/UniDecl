using System;
using UniDecl.BuiltIn.Runtime.Core;

namespace UniDecl.BuiltIn.Runtime.Navigation
{
    public class Anchor : IElementComponent
    {
        public string Id { get; }
        public Anchor(string id) => Id = id ?? throw new ArgumentNullException(nameof(id));
    }
}
