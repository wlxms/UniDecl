using System;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Navigation
{
    public class Anchor : IElementComponent
    {
        public string Id { get; }
        public Anchor(string id) => Id = id ?? throw new ArgumentNullException(nameof(id));
    }
}
