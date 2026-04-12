namespace UniDecl.Runtime.Core
{
    public struct ElementChangeEvent
    {
        public IElement Element { get; set; }
    }

    public struct AutoRebuildRequestEvent
    {
        public IElement Element { get; set; }
    }
}