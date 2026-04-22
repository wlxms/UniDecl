namespace UniDecl.Runtime.Core
{
    public interface IEventListener
    {

    }
    public interface IEventListener<T> : IEventListener where T : struct
    {
        void OnEvent(T @event);
    }

    public interface IRendererEventListener<TRenderResult, TEvent> : IEventListener
        where TEvent : struct
    {
        void OnEvent(TEvent @event, DOMNode<TRenderResult> node, DOMTree<TRenderResult> tree);
    }
}