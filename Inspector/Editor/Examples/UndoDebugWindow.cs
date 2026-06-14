using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.Snapshot;
using UniDecl.Snapshot.Editor;

namespace UniDecl.Inspector.Editor.Examples
{
    /// <summary>
    /// 最小 Undo 调试窗口——直接测试 EditorSnapshotManager 机制，不经过 Inspector 系统
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
        private TextField _field;

        private const string Key = "text";

        public void CreateGUI()
        {
            _data = new TestData();
            _mgr = new EditorSnapshotManager(new SnapshotManager());
            _mgr.OnUndoRedoPerformed += RebuildField;

            // Register setter：接收新值恢复，返回被覆盖的旧值
            _mgr.Register<string>(Key, v =>
            {
                var old = _data.text;
                _data.text = v;
                return old;
            });

            rootVisualElement.Clear();
            var container = new VisualElement();

            var label = new Label("输入文本后按 Ctrl+Z:");
            container.Add(label);

            _field = new TextField("Text") { value = _data.text };
            _field.RegisterValueChangedCallback(evt =>
            {
                _mgr.Record(evt.previousValue, Key);
                _data.text = evt.newValue;
                _mgr.CommitPending();
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

        private void RebuildField()
        {
            if (_field != null)
                _field.SetValueWithoutNotify(_data.text);
        }

        private void OnDestroy()
        {
            _mgr?.Dispose();
        }
    }
}
