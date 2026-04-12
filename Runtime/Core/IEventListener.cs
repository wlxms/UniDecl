namespace UniDecl.Runtime.Core
{
    public interface IEventListener
    {

    }
    public interface IEventListener<T> : IEventListener where T : struct
    {
        void OnEvent(T @event);
    }
}