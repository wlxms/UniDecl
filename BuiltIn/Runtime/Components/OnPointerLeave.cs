using System;
using UniDecl.BuiltIn.Runtime.Core;

namespace UniDecl.BuiltIn.Runtime.Components
{
    public sealed class OnPointerLeave : IElementEventComponent
    {
        public Action Handler { get; }
        public OnPointerLeave(Action handler) => Handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }
}
