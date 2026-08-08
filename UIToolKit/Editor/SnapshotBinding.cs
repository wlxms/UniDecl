using System;
using UniDecl.BuiltIn.Runtime.Snapshot;

namespace UniDecl.Editor.UIToolKit
{
    /// <summary>
    /// Snapshot 绑定辅助类——封装 Register setter + Record + Commit 的样板逻辑。
    /// 每个 InputField Renderer 只需创建一个 SnapshotBinding，然后在提交点调用 Commit()。
    ///
    /// 用法（连续输入型，Blur 时提交）：
    /// <code>
    /// var binding = new SnapshotBinding&lt;float&gt;(scope, element.Key, element.Value,
    ///     () => element.Value,
    ///     v => { field.SetValueWithoutNotify(v); element.Value = v; });
    ///
    /// field.RegisterValueChangedCallback(evt => {
    ///     element.Value = evt.newValue;
    ///     element.OnValueChanged?.Invoke(evt.newValue, evt.previousValue);
    /// });
    /// field.RegisterCallback&lt;BlurEvent&gt;(_ => binding.Commit());
    /// </code>
    ///
    /// 用法（瞬时选择型，ChangeEvent 即提交）：
    /// <code>
    /// field.RegisterValueChangedCallback(evt => {
    ///     element.Value = evt.newValue;
    ///     binding.Commit();
    /// });
    /// </code>
    /// </summary>
    public sealed class SnapshotBinding<T>
    {
        private readonly UndoScope _scope;
        private readonly string _key;
        private readonly Func<T> _getCurrentValue;
        private readonly Action<T> _setValueWithoutNotify;
        private readonly Action<T, T> _onExternalChange;
        private T _lastCommitted;

        /// <summary>
        /// 创建绑定。scope 或 key 为空时绑定不生效（Commit 为空操作）。
        /// </summary>
        /// <param name="scope">UndoScope（来自 ElementState.Scope），可为 null</param>
        /// <param name="key">字段唯一标识（来自 element.Key）</param>
        /// <param name="initialValue">初始值</param>
        /// <param name="getCurrentValue">读取当前值（从 Widget 层）</param>
        /// <param name="setValueWithoutNotify">写回值（VE SetValueWithoutNotify + Widget 同步），不触发 ChangeEvent</param>
        /// <param name="onExternalChange">外部值变化通知（Undo/Redo 恢复时触发），参数为 (newValue, oldValue)</param>
        public SnapshotBinding(UndoScope scope, string key, T initialValue,
            Func<T> getCurrentValue, Action<T> setValueWithoutNotify,
            Action<T, T> onExternalChange = null)
        {
            _scope = scope;
            _key = key;
            _getCurrentValue = getCurrentValue;
            _setValueWithoutNotify = setValueWithoutNotify;
            _onExternalChange = onExternalChange;
            _lastCommitted = initialValue;

            // 注册 setter——Undo/Redo 时被 SnapshotManager 调用
            if (scope != null && !string.IsNullOrEmpty(key))
            {
                scope.Register<T>(key, restore =>
                {
                    var current = _getCurrentValue();
                    _setValueWithoutNotify(restore);
                    _lastCommitted = restore;
                    // Undo/Redo 恢复后通知业务层——与用户手动编辑等效
                    _onExternalChange?.Invoke(restore, current);
                    return current;
                });
            }
        }

        /// <summary>
        /// 提交当前值为一个 undo step。
        /// Record 旧值（_lastCommitted），Commit 到栈，然后更新 _lastCommitted。
        /// 连续输入型在 Blur/Enter 调用；瞬时选择型在 ChangeEvent 调用。
        /// </summary>
        public void Commit()
        {
            if (_scope == null || string.IsNullOrEmpty(_key)) return;
            _scope.Record(_lastCommitted, _key);
            _scope.Commit();
            _lastCommitted = _getCurrentValue();
        }

        /// <summary>绑定是否生效（scope 和 key 都有效）</summary>
        public bool IsActive => _scope != null && !string.IsNullOrEmpty(_key);
    }
}
