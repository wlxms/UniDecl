using System;
using UniDecl.Core;

namespace UniDecl.Core.Components
{
    public sealed class OnClick : IElementEventComponent
    {
        public Action Handler { get; }
        public OnClick(Action handler) => Handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }
}
