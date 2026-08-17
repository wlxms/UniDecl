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
using IAttr = UniDecl.PropertyGrid.Runtime;

namespace UniDecl.PropertyGrid.Editor.Examples
{
    /// <summary>
    /// Step3 分层调试——PropertyGridShowcase 完整数据（@表达式/Button/条件/嵌套），插桩同 Step2。
    /// 菜单: Window > UniDecl/Step3 Showcase Debug
    /// </summary>
    public class Step3ShowcaseDebugWindow : EditorWindow
    {
        [MenuItem("Window/UniDecl/Step3 Showcase Debug")]
        public static void Open() => GetWindow<Step3ShowcaseDebugWindow>("Step3 Showcase Debug");

        private PropertyGridShowcase _data = new PropertyGridShowcase();
        private UIToolkitRenderManager _manager;
        private EditorSnapshotManager _snap;
        private StringBuilder _log = new StringBuilder();

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

        private void CreateGUI()
        {
            _manager = new UIToolkitRenderManager(new EditorSnapshotManager(new SnapshotManager()));
            _snap = (EditorSnapshotManager)FindField(_manager.GetType(), "_snapshotManager").GetValue(_manager);
            Instrument();
            Rebuild();
        }

        private void Instrument()
        {
            _snap.ScopeDisposed += id => _log.AppendLine("  [ScopeDisposed] id=" + id);
            _snap.StepCommitted += s => _log.AppendLine("  [StepCommitted] key=" + s.Key);
            _snap.StepUndone += s => _log.AppendLine("  [StepUndone] key=" + s.Key);

            var tree = (DOMTree)FindField(_manager.GetType(), "_domTree").GetValue(_manager);
            tree.NodeRemoved += n =>
                _log.AppendLine("  [NodeRemoved] type=" + (n.Element?.GetType().Name ?? "null") +
                                " key='" + (n.Element?.Key ?? "") + "'");
        }

        private void Rebuild()
        {
            rootVisualElement.Clear();
            var propGrid = new PropertyGridElement(_data);
            var root = _manager.RenderRoot(propGrid);
            if (root != null) rootVisualElement.Add(root);
            var run = new Button(RunScenario) { text = "Run Scenario" };
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
            _log.AppendLine("=== Step3 scenario ===");
            var tf = FindTextField();
            _log.AppendLine("initial: value=" + (tf != null ? tf.value : "NULL") + " undo=" + _snap.UndoCount);

            tf.value = "第一次"; tf.Focus(); tf.Blur();
            tf = FindTextField();
            _log.AppendLine("edit1: value=" + (tf != null ? tf.value : "NULL") + " undo=" + _snap.UndoCount);

            tf.value = "第二次"; tf.Focus(); tf.Blur();
            tf = FindTextField();
            _log.AppendLine("edit2: value=" + (tf != null ? tf.value : "NULL") + " undo=" + _snap.UndoCount);

            Undo.PerformUndo();
            tf = FindTextField();
            _log.AppendLine("undo1: value=" + (tf != null ? tf.value : "NULL") + " undo=" + _snap.UndoCount);

            Undo.PerformUndo();
            tf = FindTextField();
            _log.AppendLine("undo2: value=" + (tf != null ? tf.value : "NULL") + " undo=" + _snap.UndoCount);

            File.WriteAllText(Path.Combine(Application.persistentDataPath, "SnapshotStep3Result.txt"), _log.ToString());
            Debug.Log("[Step3] done");
        }
    }
}
