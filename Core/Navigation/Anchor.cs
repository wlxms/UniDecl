using System;
using UniDecl.Core;

namespace UniDecl.Core.Navigation
{
    public class Anchor : IElementComponent
    {
        public string Id { get; }
        public Anchor(string id) => Id = id ?? throw new ArgumentNullException(nameof(id));
    }
}
