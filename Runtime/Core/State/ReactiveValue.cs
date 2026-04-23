using System;
using System.Collections.Generic;

namespace UniDecl.Runtime.Core
{
    /// <summary>
    /// 响应式值包装器
    /// 当值发生变化时，自动触发 onChange 回调（通常用于触发 UI 重建）
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    public class ReactiveValue<T>
    {
        private T _value;
        private Action _onChange;

        /// <summary>
        /// 获取或设置值。设置时如果值发生变化，会自动触发 onChange 回调。
        /// </summary>
        public T Value
        {
            get => _value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(_value, value))
                {
                    _value = value;
                    _onChange?.Invoke();
                }
            }
        }

        /// <summary>
        /// 内部方法：绑定 onChange 回调
        /// </summary>
        internal void SetOnChange(Action onChange) => _onChange = onChange;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="initialValue">初始值</param>
        public ReactiveValue(T initialValue = default)
        {
            _value = initialValue;
        }

        /// <summary>
        /// 隐式转换：ReactiveValue&lt;T&gt; → T（便于读取）
        /// </summary>
        public static implicit operator T(ReactiveValue<T> rv) => rv?.Value ?? default;

        /// <summary>
        /// 获取值的字符串表示
        /// </summary>
        public override string ToString() => _value?.ToString() ?? "null";
    }
}
