using System;
using System.Collections;
using System.Collections.Generic;

namespace UniDecl.Runtime.Core
{
    /// <summary>
    /// 响应式列表
    /// 列表内容发生变化时，自动触发 onChange 回调（通常用于触发 UI 重建）
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public class ReactiveList<T> : IList<T>
    {
        private readonly List<T> _inner = new List<T>();
        private Action _onChange;

        /// <summary>
        /// 内部方法：绑定 onChange 回调
        /// </summary>
        internal void SetOnChange(Action onChange) => _onChange = onChange;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ReactiveList()
        {
        }

        /// <summary>
        /// 构造函数（使用初始元素）
        /// </summary>
        public ReactiveList(IEnumerable<T> items)
        {
            if (items != null)
                _inner.AddRange(items);
        }

        /// <summary>
        /// 触发变更通知
        /// </summary>
        private void NotifyChange() => _onChange?.Invoke();

        #region IList<T> Implementation

        public T this[int index]
        {
            get => _inner[index];
            set
            {
                _inner[index] = value;
                NotifyChange();
            }
        }

        public int Count => _inner.Count;

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            _inner.Add(item);
            NotifyChange();
        }

        public void Clear()
        {
            if (_inner.Count > 0)
            {
                _inner.Clear();
                NotifyChange();
            }
        }

        public bool Contains(T item) => _inner.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _inner.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();

        public int IndexOf(T item) => _inner.IndexOf(item);

        public void Insert(int index, T item)
        {
            _inner.Insert(index, item);
            NotifyChange();
        }

        public bool Remove(T item)
        {
            var result = _inner.Remove(item);
            if (result)
                NotifyChange();
            return result;
        }

        public void RemoveAt(int index)
        {
            _inner.RemoveAt(index);
            NotifyChange();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        /// <summary>
        /// 批量添加（只触发一次变更通知）
        /// </summary>
        public void AddRange(IEnumerable<T> items)
        {
            if (items == null) return;
            _inner.AddRange(items);
            NotifyChange();
        }

        /// <summary>
        /// 批量移除（只触发一次变更通知）
        /// </summary>
        public int RemoveAll(Predicate<T> match)
        {
            var removed = _inner.RemoveAll(match);
            if (removed > 0)
                NotifyChange();
            return removed;
        }

        /// <summary>
        /// 排序（触发变更通知）
        /// </summary>
        public void Sort()
        {
            _inner.Sort();
            NotifyChange();
        }

        /// <summary>
        /// 排序（触发变更通知）
        /// </summary>
        public void Sort(Comparison<T> comparison)
        {
            _inner.Sort(comparison);
            NotifyChange();
        }
    }
}
