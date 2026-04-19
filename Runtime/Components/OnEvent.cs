using System;

namespace UniDecl.Runtime.Components
{
    public sealed class OnEvent<TEvent> : IOnEventComponent
    {
        public Action<TEvent> Handler { get; }
        public OnEvent(Action<TEvent> handler) => Handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }
}
