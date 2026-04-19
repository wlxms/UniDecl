using System;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Components
{
    public sealed class OnClick : IElementEventComponent
    {
        public Action Handler { get; }
        public OnClick(Action handler) => Handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }
}
