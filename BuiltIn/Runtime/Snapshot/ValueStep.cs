namespace UniDecl.BuiltIn.Runtime.Snapshot
{
    /// <summary>
    /// 值类型步骤——存储装箱拷贝，适用于基础类型、string、纯值类型 struct
    /// </summary>
    public sealed class ValueStep : IStep
    {
        public string Key { get; }
        public object Value { get; }

        public ValueStep(string key, object value)
        {
            Key = key;
            Value = value;
        }
    }
}
