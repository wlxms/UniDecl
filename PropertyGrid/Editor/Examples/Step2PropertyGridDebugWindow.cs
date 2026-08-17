using System;
using System.IO;
using System.Text;
using UniDecl.BuiltIn.Editor.Snapshot;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.BuiltIn.Runtime.Snapshot;
using UniDecl.Editor.UIToolKit;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.PropertyGrid.Editor.Examples
{
    /// <summary>
    /// Step2 分层调试窗口——PropertyGrid + 简单对象，插桩 NodeRemoved/ScopeDisposed/Step 事件，
    /// 结果写入 persistentDataPath/SnapshotStep2Result.txt。菜单: Window > UniDecl/Step2 PG Debug
    /// </summary>
    public class Step2PropertyGridDebugWindow : EditorWindow
    {
        [MenuItem("Window/UniDecl/Step2 PG Debug")]
        public static void Open() => GetWindow<Step2PropertyGridDebugWindow>("Step2 PG Debug");

        [Serializable]
        public class SimpleData
        {
            public string name = "原始";
            public int level = 10;
        }

        private SimpleData _data = new SimpleData();
        private UIToolkitRenderManager _manager;
        private EditorSnapshotManager _snap;
        private StringBuilder _log = new StringBuilder();

        private void CreateGUI()
        {
            _manager = new UIToolkitRenderManager(new EditorSnapshotManager(new SnapshotManager()));
            _snap = (EditorSnapshotManager)FindField(_manager.GetType(), "_snapshotManager").GetValue(_manager);
            Instrument();
            Rebuild();
        }

        private static System.Reflection.FieldInfo FindField(System.Type start, string name)
        {
            var t = start;
            while (t != null)
            {
                var f = t.GetField(name,
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (f != null) return f;
                t = t.BaseType;
            }
            return null;
        }

        private void Instrument()
        {
            _snap.ScopeDisposed += id => _log.AppendLine("  [ScopeDisposed] id=" + id);
            _snap.StepCommitted += s => _log.AppendLine("  [StepCommitted] key=" + s.Key);
            _snap.StepUndone += s => _log.AppendLine("  [StepUndone] key=" + s.Key);
            _snap.OnUndoRedoPerformed += cs => _log.AppendLine("  [OnUndoRedoPerformed] changes=" + cs.Changes.Count);

            var tree = (DOMTree)FindField(_manager.GetType(), "_domTree").GetValue(_manager);
            tree.NodeRemoved += n =>
                _log.AppendLine("  [NodeRemoved] type=" + (n.Element?.GetType().Name ?? "null") +
                                " key='" + (n.Element?.Key ?? "") + "'");
            tree.NodeCreated += n =>
                _log.AppendLine("  [NodeCreated] type=" + (n.Element?.GetType().Name ?? "null") +
                                " key='" + (n.Element?.Key ?? "") + "'");
        }

        private void Rebuild()
        {
            rootVisualElement.Clear();
            var propGrid = new PropertyGridElement(_data);
            var root = _manager.RenderRoot(propGrid);
            if (root != null) rootVisualElement.Add(root);

            var run = new Button(RunScenario) { text = "Run Scenario (2 edits + 2 undos)" };
            rootVisualElement.Add(run);
        }

        private TextField FindTextField()
        {
            TextField f = null;
            void Walk(VisualElement ve)
            {
                if (f == null) f = ve as TextField;
                for (int i = 0; i < ve.childCount; i++) Walk(ve[i]);
            }
            Walk(rootVisualElement);
            return f;
        }

        private void RunScenario()
        {
            _log.AppendLine("=== Step2 scenario start ===");
            var tf = FindTextField();
            _log.AppendLine("initial: value=" + (tf != null ? tf.value : "NULL") + " undo=" + _snap.UndoCount);

            tf.value = "第一次"; tf.Focus(); tf.Blur();
            _log.AppendLine("after edit1: value=" + tf.value + " undo=" + _snap.UndoCount);

            tf.value = "第二次"; tf.Focus(); tf.Blur();
            _log.AppendLine("after edit2: value=" + tf.value + " undo=" + _snap.UndoCount);

            Undo.PerformUndo();
            tf = FindTextField();
            _log.AppendLine("after undo1: value=" + (tf != null ? tf.value : "NULL") + " undo=" + _snap.UndoCount);

            Undo.PerformUndo();
            tf = FindTextField();
            _log.AppendLine("after undo2: value=" + (tf != null ? tf.value : "NULL") + " undo=" + _snap.UndoCount);

            File.WriteAllText(Path.Combine(Application.persistentDataPath, "SnapshotStep2Result.txt"), _log.ToString());
            Debug.Log("[Step2] result -> " + Path.Combine(Application.persistentDataPath, "SnapshotStep2Result.txt"));
        }
    }
}
