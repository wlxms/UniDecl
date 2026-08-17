using System;

namespace UniDecl.BuiltIn.Runtime.Snapshot
{
    /// <summary>
    /// 统一快照步骤——取代 ValueStep/ObjectDiffStep 的单轨 step。
    /// 存储值快照 + Binding 定位（Guid），不再使用用户字符串 key。
    /// </summary>
    public sealed class SnapshotStep : IStep
    {
        public string Key { get; }          // 兼容 IStep（= Path）
        public Guid BindingId { get; }
        public string Path { get; }
        public object Value { get; }        // 旧值快照（值类型装箱 / 集合深拷贝）
        public int ScopeId { get; }

        public SnapshotStep(Guid bindingId, string path, object value, int scopeId)
        {
            BindingId = bindingId;
            Path = path;
            Value = value;
            ScopeId = scopeId;
            Key = path;
        }
    }
}
