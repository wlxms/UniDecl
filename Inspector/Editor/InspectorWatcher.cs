using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.ComponentModel;

namespace UniDecl.Inspector.Editor
{
    /// <summary>
    /// 外源变更监控器——自动检测 Inspector 之外的字段修改
    /// 分层策略：
    /// 1. INotifyPropertyChanged → 即时通知
    /// 2. MonoBehaviour/ScriptableObject → DirtyFlag 检测
    /// 3. 普通 C# → 快照轮询
    /// </summary>
    public class InspectorWatcher
    {
        private object _target;
        private List<FieldInfo> _fields;
        private FieldSnapshot _lastSnapshot;
        private bool _isWatching;
        private bool _supportsINPC;
        private bool _isUnityObject;

        /// <summary>
        /// 当检测到外源变更时触发
        /// </summary>
        public event Action<List<string>> OnChanged;

        /// <summary>
        /// 开始监控
        /// </summary>
        public void StartWatching(object target, List<FieldInfo> fields)
        {
            StopWatching();

            _target = target;
            _fields = fields;
            _isUnityObject = target is UnityEngine.Object;
            _supportsINPC = target is INotifyPropertyChanged;

            // 拍摄初始快照
            _lastSnapshot = FieldSnapshot.Take(target, fields);

            if (_supportsINPC)
            {
                ((INotifyPropertyChanged)target).PropertyChanged += OnPropertyChangeded;
            }

            _isWatching = true;
            EditorApplication.update += OnEditorUpdate;
        }

        /// <summary>
        /// 停止监控
        /// </summary>
        public void StopWatching()
        {
            if (!_isWatching) return;

            if (_supportsINPC && _target is INotifyPropertyChanged inpc)
                inpc.PropertyChanged -= OnPropertyChangeded;

            EditorApplication.update -= OnEditorUpdate;
            _isWatching = false;
        }

        /// <summary>
        /// 通知：值即将变更（由 Inspector 内部编辑触发，防止自身变更被检测）
        /// </summary>
        public void NotifySelfChange()
        {
            // Inspector 内部编辑后立即更新快照，避免下次轮询误报
            if (_target != null && _fields != null)
                _lastSnapshot = FieldSnapshot.Take(_target, _fields);
        }

        private void OnPropertyChangeded(object sender, PropertyChangedEventArgs e)
        {
            // INPC 即时通知——只通知变化的字段
            if (_fields != null)
            {
                var changed = new List<string>();
                // INPC 的 PropertyName 对应字段名
                if (!string.IsNullOrEmpty(e.PropertyName))
                    changed.Add(e.PropertyName);
                else
                {
                    // 通知所有字段
                    foreach (var f in _fields)
                        changed.Add(f.Name);
                }

                // 更新快照
                _lastSnapshot = FieldSnapshot.Take(_target, _fields);
                OnChanged?.Invoke(changed);
            }
        }

        private void OnEditorUpdate()
        {
            if (!_isWatching || _target == null) return;

            // INPC 对象不需要轮询
            if (_supportsINPC) return;

            // Unity Object: DirtyFlag 检测
            if (_isUnityObject && _target is UnityEngine.Object obj)
            {
                if (!EditorUtility.IsDirty(obj)) return;
                // Dirty 时对比快照找变更字段
            }

            // 快照对比
            if (_lastSnapshot == null) return;

            if (_lastSnapshot.DiffersFrom(_target, _fields))
            {
                var changed = _lastSnapshot.GetChangedFields(_target, _fields);
                _lastSnapshot = FieldSnapshot.Take(_target, _fields);
                OnChanged?.Invoke(changed);
            }
        }
    }
}
