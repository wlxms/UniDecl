using System;
using UniDecl.BuiltIn.Runtime.Core;

namespace UniDecl.BuiltIn.Runtime.Components
{
    public sealed class OnPointerEnter : IElementEventComponent
    {
        public Action Handler { get; }
        public OnPointerEnter(Action handler) => Handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }
}
