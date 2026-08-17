using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Snapshot;
using UniDecl.BuiltIn.Editor.Snapshot;

namespace UniDecl.PropertyGrid.Editor.Examples
{
    /// <summary>
    /// 最小 Undo 调试窗口——直接测试 EditorSnapshotManager 机制，不经过 PropertyGrid 系统
    /// 菜单: Window > UniDecl > Undo Debug
    /// </summary>
    public class UndoDebugWindow : EditorWindow
    {
        [MenuItem("Window/UniDecl/Undo Debug")]
        public static void ShowWindow() => GetWindow<UndoDebugWindow>("Undo Debug");

        // 测试用数据类——只有一个字段
        [Serializable]
        public class TestData
        {
            public string text = "hello";
        }

        private TestData _data;
        private EditorSnapshotManager _mgr;
        private SnapshotBinding _binding;
        private TextField _field;

        public void CreateGUI()
        {
            _data = new TestData();
            _mgr = new EditorSnapshotManager(new SnapshotManager());
            _mgr.OnUndoRedoPerformed += RebuildField;

            // 单轨叶子绑定：getter/setter 统一入口，Commit 时基线对比（变更才记录）
            _binding = new SnapshotBinding(_mgr, 0, "text",
                () => _data.text,
                (restore, current, changes) => _data.text = (string)restore);

            rootVisualElement.Clear();
            var container = new VisualElement();

            var label = new Label("输入文本后按 Ctrl+Z:");
            container.Add(label);

            _field = new TextField("Text") { value = _data.text };
            _field.RegisterValueChangedCallback(evt =>
            {
                _data.text = evt.newValue;
                _binding.Commit();
            });

            container.Add(_field);

            var showBtn = new Button(() =>
            {
                Debug.Log($"[UndoDebug] data.text = '{_data.text}', field.value = '{_field.value}'");
            });
            showBtn.text = "Show Current Value";
            container.Add(showBtn);

            rootVisualElement.Add(container);
        }

        private void RebuildField(ChangeSet changes)
        {
            if (_field != null)
                _field.SetValueWithoutNotify(_data.text);
        }

        private void OnDestroy()
        {
            _binding?.Dispose();
            _mgr?.Dispose();
            _binding = null;
            _mgr = null;
        }
    }
}
