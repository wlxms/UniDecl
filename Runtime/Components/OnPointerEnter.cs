using System;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Components
{
    public sealed class OnPointerEnter : IElementEventComponent
    {
        public Action Handler { get; }
        public OnPointerEnter(Action handler) => Handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }
}
