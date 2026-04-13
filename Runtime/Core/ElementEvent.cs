namespace UniDecl.Runtime.Core
{
    public enum RebuildTrigger
    {
        FullRebuild,
        Immediate,
        DeferredFlush,
    }

    public struct ElementChangeEvent
    {
        public IElement Element { get; set; }
    }

    public struct AutoRebuildRequestEvent
    {
        public IElement Element { get; set; }
    }

    public struct RebuildPerformanceEvent
    {
        public IElement Element { get; set; }
        public RebuildTrigger Trigger { get; set; }
        public double BeforeRebuildMs { get; set; }
        public double DomRebuildMs { get; set; }
        public double AfterRebuildMs { get; set; }
        public double TotalMs { get; set; }
    }
}