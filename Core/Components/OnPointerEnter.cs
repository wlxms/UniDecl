using System;
using UniDecl.Core;

namespace UniDecl.Core.Components
{
    public sealed class OnPointerEnter : IElementEventComponent
    {
        public Action Handler { get; }
        public OnPointerEnter(Action handler) => Handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }
}
