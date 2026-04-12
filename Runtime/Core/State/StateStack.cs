using System.Collections.Generic;

namespace UniDecl.Runtime.Core
{
    /// <summary>
    /// 状态栈
    /// 按容器层级组织状态管理器，每层容器有独立的 StateManager
    /// </summary>
    public class StateStack
    {
        private readonly Stack<IStateManager> _stack = new Stack<IStateManager>();

        /// <summary>
        /// 当前（栈顶）状态管理器
        /// </summary>
        public IStateManager Current => _stack.Count > 0 ? _stack.Peek() : null;

        /// <summary>
        /// 栈是否为空
        /// </summary>
        public bool IsEmpty => _stack.Count == 0;

        public void Push(IStateManager manager)
        {
            _stack.Push(manager);
        }

        public void Pop()
        {
            if (_stack.Count > 0)
                _stack.Pop();
        }

        public void Clear()
        {
            _stack.Clear();
        }
    }
}
