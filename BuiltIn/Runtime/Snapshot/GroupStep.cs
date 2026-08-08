using System.Collections.Generic;

namespace UniDecl.BuiltIn.Runtime.Snapshot
{
    /// <summary>
    /// 分组步骤——将多个步骤合并为一个事务，Undo/Redo 一步回退整组
    /// </summary>
    public sealed class GroupStep : IStep
    {
        public string Key { get; }
        public List<IStep> Steps { get; }

        public GroupStep(string key, List<IStep> steps)
        {
            Key = key;
            Steps = steps;
        }
    }
}
