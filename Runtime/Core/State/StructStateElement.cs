using System;
using System.Collections.Generic;

namespace UniDecl.Runtime.Core
{
    /// <summary>
    /// 基于 Struct 的状态化元素抽象基类
    /// 强制使用 SetState 方法更新状态，确保每次更新都触发 UI 重建
    /// </summary>
    /// <typeparam name="TState">状态类型（必须是 struct）</typeparam>
    public abstract class StructStateElement<TState> : Element where TState : struct
    {
        private TState _state;
        private bool _stateInitialized;

        /// <summary>
        /// 抽象方法：构建初始状态
        /// </summary>
        public abstract TState BuildInitialState();

        /// <summary>
        /// 抽象方法：基于当前状态渲染 UI
        /// </summary>
        /// <param name="state">当前状态</param>
        public abstract IElement Render(TState state);

        /// <summary>
        /// 更新状态。使用 updater 函数接收旧状态并返回新状态。
        /// 如果新旧状态不同，会自动触发 UI 重建。
        /// </summary>
        /// <param name="updater">状态更新函数</param>
        protected void SetState(Func<TState, TState> updater)
        {
            if (updater == null)
                throw new ArgumentNullException(nameof(updater));

            var newState = updater(_state);
            if (!EqualityComparer<TState>.Default.Equals(_state, newState))
            {
                _state = newState;
                NotifyChanged();
            }
        }

        /// <summary>
        /// 直接设置新状态（不推荐，建议使用 SetState(updater)）
        /// </summary>
        protected void SetState(TState newState)
        {
            if (!EqualityComparer<TState>.Default.Equals(_state, newState))
            {
                _state = newState;
                NotifyChanged();
            }
        }

        /// <summary>
        /// 获取当前状态（只读）
        /// </summary>
        protected TState State => _state;

        /// <summary>
        /// 框架调用的 Render 入口（密封，用户不应 override）
        /// </summary>
        public sealed override IElement Render()
        {
            if (!_stateInitialized)
            {
                _state = BuildInitialState();
                _stateInitialized = true;
            }
            return Render(_state);
        }
    }
}
