using System;
using UniDecl.BuiltIn.Runtime.Core;

namespace UniDecl.BuiltIn.Runtime.Components
{
    public sealed class OnClick : IElementEventComponent
    {
        public Action Handler { get; }
        public OnClick(Action handler) => Handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }
}
