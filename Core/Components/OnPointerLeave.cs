using System;
using UniDecl.Core;

namespace UniDecl.Core.Components
{
    public sealed class OnPointerLeave : IElementEventComponent
    {
        public Action Handler { get; }
        public OnPointerLeave(Action handler) => Handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }
}
