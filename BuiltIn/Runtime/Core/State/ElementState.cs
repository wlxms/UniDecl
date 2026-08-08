using System;
using System.Collections.Generic;
using UniDecl.BuiltIn.Runtime.Snapshot;

namespace UniDecl.BuiltIn.Runtime.Core
{
    /// <summary>
    /// 元素状态
    /// 存储元素在多次渲染之间需要保持的数据
    /// </summary>
    public class ElementState
    {
        /// <summary>
        /// 元素的自定义状态数据（对应 IStatefulElement.State）
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 元素关联的 Undo/Redo 作用域。
        /// 由上层（如 PropertyGridModule）在创建/更新元素时注入。
        /// Renderer 通过此字段在 Render() 内直接访问所属 Scope，
        /// 无需依赖 IElementRenderHost 全局查询。
        /// 生命周期随 ElementState——ClearUnused 时被丢弃，
        /// 对应 UndoScope.Dispose() 由上层负责调用。
        /// </summary>
        public UndoScope Scope { get; set; }

        /// <summary>
        /// 本帧是否被访问过（用于清理未使用的状态）
        /// </summary>
        internal bool Used { get; set; }
    }

    /// <summary>
    /// 状态管理器默认实现
    /// </summary>
    public class StateManager : IStateManager
    {
        private readonly Dictionary<string, ElementState> _states = new Dictionary<string, ElementState>();

        public ElementState GetOrCreateState(string key, Func<ElementState> factory)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            if (_states.TryGetValue(key, out var state))
            {
                state.Used = true;
                return state;
            }

            state = factory?.Invoke() ?? new ElementState();
            state.Used = true;
            _states[key] = state;
            return state;
        }

        public ElementState GetState(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            if (_states.TryGetValue(key, out var state))
            {
                state.Used = true;
                return state;
            }
            return null;
        }

        public void MarkAllUnused()
        {
            foreach (var state in _states.Values)
            {
                state.Used = false;
            }
        }

        public void ClearUnused()
        {
            var keysToRemove = new List<string>();
            foreach (var kv in _states)
            {
                if (!kv.Value.Used)
                {
                    keysToRemove.Add(kv.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _states.Remove(key);
            }
        }
    }
}
