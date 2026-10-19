namespace UniDecl.Snapshot
{
    /// <summary>
    /// 快照步骤接口——所有 Undo/Redo 操作的基本单元
    /// </summary>
    public interface IStep
    {
        string Key { get; }
    }
}
