using System.Collections.Generic;

namespace UniDecl.BuiltIn.Runtime.Snapshot
{
    /// <summary>
    /// 对象差异步骤——存储深拷贝字段快照，适用于 class 和含可变引用的 struct
    /// </summary>
    public sealed class ObjectDiffStep : IStep
    {
        public string Key { get; }
        public object Target { get; }
        public Dictionary<string, object> FieldSnapshots { get; }

        public ObjectDiffStep(string key, object target, Dictionary<string, object> fieldSnapshots)
        {
            Key = key;
            Target = target;
            FieldSnapshots = fieldSnapshots;
        }
    }
}
