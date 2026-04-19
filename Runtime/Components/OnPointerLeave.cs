using System;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Components
{
    public sealed class OnPointerLeave : IElementEventComponent
    {
        public Action Handler { get; }
        public OnPointerLeave(Action handler) => Handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }
}
