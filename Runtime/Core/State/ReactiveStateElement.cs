using System;
using System.Reflection;

namespace UniDecl.Runtime.Core
{
    /// <summary>
    /// 响应式状态元素抽象基类
    /// 自动绑定状态对象中所有 ReactiveValue&lt;T&gt; 字段/属性，使其变更时自动触发 UI 重建
    /// </summary>
    /// <typeparam name="TState">状态类型（必须是 class，且有无参构造函数）</typeparam>
    public abstract class ReactiveStateElement<TState> : Element where TState : class, new()
    {
        private TState _state;
        private bool _batchMode;

        /// <summary>
        /// 抽象方法：构建初始状态
        /// </summary>
        public abstract TState BuildState();

        /// <summary>
        /// 抽象方法：基于当前状态渲染 UI
        /// </summary>
        /// <param name="state">当前状态</param>
        public abstract IElement Render(TState state);

        /// <summary>
        /// 批量更新：在 action 内的多次 ReactiveValue 修改只会触发一次 UI 重建
        /// </summary>
        /// <param name="updates">更新操作</param>
        protected void BatchUpdate(Action<TState> updates)
        {
            if (updates == null)
                throw new ArgumentNullException(nameof(updates));

            _batchMode = true;
            try
            {
                updates(_state);
            }
            finally
            {
                _batchMode = false;
                NotifyChanged();
            }
        }

        /// <summary>
        /// 获取当前状态对象
        /// </summary>
        protected TState State => _state;

        /// <summary>
        /// 框架调用的 Render 入口（密封，用户不应 override）
        /// </summary>
        public sealed override IElement Render()
        {
            if (_state == null)
            {
                _state = BuildState();
                BindReactiveProperties(_state);
            }
            return Render(_state);
        }

        /// <summary>
        /// 响应式属性变更时的回调
        /// </summary>
        private void OnReactiveChange()
        {
            if (!_batchMode)
                NotifyChanged();
        }

        /// <summary>
        /// 自动绑定状态对象中所有 ReactiveValue&lt;T&gt; 的 onChange 回调
        /// </summary>
        private void BindReactiveProperties(TState state)
        {
            if (state == null) return;

            var type = state.GetType();
            var onChange = (Action)OnReactiveChange;

            // 绑定字段
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (IsReactiveValueType(field.FieldType))
                {
                    var reactiveValue = field.GetValue(state);
                    if (reactiveValue != null)
                    {
                        BindReactiveValue(reactiveValue, field.FieldType, onChange);
                    }
                }
            }

            // 绑定属性
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                if (prop.CanRead && IsReactiveValueType(prop.PropertyType))
                {
                    var reactiveValue = prop.GetValue(state);
                    if (reactiveValue != null)
                    {
                        BindReactiveValue(reactiveValue, prop.PropertyType, onChange);
                    }
                }
            }
        }

        /// <summary>
        /// 判断类型是否为 ReactiveValue&lt;T&gt;
        /// </summary>
        private static bool IsReactiveValueType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ReactiveValue<>);
        }

        /// <summary>
        /// 绑定单个 ReactiveValue 对象的 onChange 回调
        /// </summary>
        private static void BindReactiveValue(object reactiveValue, Type reactiveType, Action onChange)
        {
            var setOnChangeMethod = reactiveType.GetMethod("SetOnChange", BindingFlags.NonPublic | BindingFlags.Instance);
            setOnChangeMethod?.Invoke(reactiveValue, new object[] { onChange });
        }
    }
}
