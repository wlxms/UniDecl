using System;
using System.Collections.Generic;

namespace UniDecl.Runtime.Core
{
    /// <summary>
    /// 上下文栈管理器
    /// 管理不同类型上下文的栈结构，支持嵌套覆盖
    /// </summary>
    public class ContextStack : IContextReader
    {
        private readonly Dictionary<Type, Stack<IContextProvider>> _contextStacks =
            new Dictionary<Type, Stack<IContextProvider>>();

        /// <summary>
        /// 压入上下文（按运行时类型存储）
        /// </summary>
        public void Push(IContextProvider context)
        {
            var contextType = context.GetType();
            if (!_contextStacks.TryGetValue(contextType, out var stack))
            {
                stack = new Stack<IContextProvider>();
                _contextStacks[contextType] = stack;
            }
            stack.Push(context);
        }

        /// <summary>
        /// 弹出上下文
        /// </summary>
        public void Pop(Type contextType)
        {
            if (_contextStacks.TryGetValue(contextType, out var stack) && stack.Count > 0)
            {
                stack.Pop();
                if (stack.Count == 0)
                {
                    _contextStacks.Remove(contextType);
                }
            }
        }

        /// <summary>
        /// 获取指定类型的上下文（最近的）
        /// </summary>
        public T Get<T>() where T : class, IContextProvider
        {
            if (_contextStacks.TryGetValue(typeof(T), out var stack) && stack.Count > 0)
            {
                return stack.Peek() as T;
            }
            return null;
        }

        /// <summary>
        /// 尝试获取指定类型的上下文
        /// </summary>
        public bool TryGet<T>(out T value) where T : class, IContextProvider
        {
            value = null;
            if (_contextStacks.TryGetValue(typeof(T), out var stack) && stack.Count > 0)
            {
                value = stack.Peek() as T;
                return value != null;
            }
            return false;
        }

        /// <summary>
        /// 检查是否存在指定类型的上下文
        /// </summary>
        public bool Has<T>() where T : class, IContextProvider
        {
            return _contextStacks.TryGetValue(typeof(T), out var stack) && stack.Count > 0;
        }

        /// <summary>
        /// 清空所有上下文栈
        /// </summary>
        public void Clear()
        {
            _contextStacks.Clear();
        }
    }
}
