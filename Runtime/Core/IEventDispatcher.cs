namespace UniDecl.Runtime.Core
{
    public interface IEventDispatcher
    {
        void Dispatch<T>(T @event) where T : struct;
        void Subscribe(IEventListener listener);
        void Unsubscribe(IEventListener listener);
    }
}