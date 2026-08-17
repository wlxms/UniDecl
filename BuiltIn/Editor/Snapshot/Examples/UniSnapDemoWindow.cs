using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Snapshot;

namespace UniDecl.BuiltIn.Editor.Snapshot.Examples
{
    /// <summary>
    /// UniSnap Demo 窗口——单轨快照演示：对象绑定自动展开 / 递归 Commit / GroupStep / Merge / changeSet
    /// 菜单: Window > UniDecl > UniSnap Demo
    /// </summary>
    public class UniSnapDemoWindow : EditorWindow
    {
        [MenuItem("Window/UniDecl/UniSnap Demo")]
        public static void ShowWindow() => GetWindow<UniSnapDemoWindow>("UniSnap Demo");

        // ─── 测试数据模型 ───

        [Serializable]
        public class ConfigData
        {
            public float size = 1.0f;
            public string name = "demo";
            public Color color = Color.white;
            public NestedData nested = new NestedData();

            public override string ToString()
                => $"size={size:F2}, name={name}, color={color}, nested.count={nested.count}";
        }

        [Serializable]
        public class NestedData
        {
            public int count = 0;
            public string tag = "";
        }

        // ─── 状态 ───

        private ConfigData _config;
        private EditorSnapshotManager _manager;
        private UndoScope _scope;
        private SnapshotBinding _configBinding; // 对象绑定：自动展开 size/name/color/nested.*

        private FloatField _sizeField;
        private TextField _nameField;
        private IntegerField _countField;
        private TextField _tagField;
        private ColorField _colorField;

        // GroupStep 日志
        private Label _groupLogLabel;

        // 状态栏
        private Label _statusLabel;
        private Label _configLabel;

        // ─── 生命周期 ───

        public void OnEnable()
        {
            _config = new ConfigData();
            _manager = new EditorSnapshotManager(new SnapshotManager());
            _scope = new UndoScope(_manager);

            // 单轨：绑定整个对象，字段自动展开（size/name/color/nested.count/nested.tag），
            // 根 Commit 递归对比，变化的字段各自成 step 并自动打包 group。
            _configBinding = new SnapshotBinding(_scope, "config", () => _config);

            _manager.OnUndoRedoPerformed += RebuildUI;

            BuildUI();
        }

        public void OnDisable()
        {
            _manager?.Dispose();
            _scope?.Dispose();
            _configBinding?.Dispose();
            _manager = null;
            _scope = null;
            _configBinding = null;
        }

        // ─── UI 构建 ───

        private void BuildUI()
        {
            rootVisualElement.Clear();
            var root = new ScrollView();
            var container = new VisualElement();
            container.style.paddingLeft = 10;
            container.style.paddingRight = 10;
            container.style.paddingTop = 10;

            // ── ① 值字段（自动展开的叶子）──
            AddHeader(container, "① 值字段 — 对象绑定自动展开的叶子");
            AddHint(container,
                "修改 Size / Name / Color，Ctrl+Z 撤销。同 binding 500ms 内连续修改自动合并。");

            _sizeField = new FloatField("Size") { value = _config.size };
            _sizeField.RegisterValueChangedCallback(evt =>
            {
                _config.size = evt.newValue;
                _configBinding.Commit(); // 根 commit：递归对比，只记录变化的字段
                UpdateStatus();
            });
            container.Add(_sizeField);

            _nameField = new TextField("Name") { value = _config.name };
            _nameField.RegisterValueChangedCallback(evt =>
            {
                _config.name = evt.newValue;
                _configBinding.Commit();
                UpdateStatus();
            });
            container.Add(_nameField);

            _colorField = new ColorField("Color") { value = _config.color };
            _colorField.RegisterValueChangedCallback(evt =>
            {
                _config.color = evt.newValue;
                _configBinding.Commit();
                UpdateStatus();
            });
            container.Add(_colorField);

            // ── ② 嵌套对象（自动递归展开）──
            AddSeparator(container);
            AddHeader(container, "② 嵌套对象 — 自动递归展开");
            AddHint(container,
                "修改 Count / Tag，Ctrl+Z 只撤销对应字段（changeSet 精确刷新）。");

            _countField = new IntegerField("Nested Count") { value = _config.nested.count };
            _countField.RegisterValueChangedCallback(evt =>
            {
                _config.nested.count = evt.newValue;
                _configBinding.Commit();
                UpdateStatus();
            });
            container.Add(_countField);

            _tagField = new TextField("Nested Tag") { value = _config.nested.tag };
            _tagField.RegisterValueChangedCallback(evt =>
            {
                _config.nested.tag = evt.newValue;
                _configBinding.Commit();
                UpdateStatus();
            });
            container.Add(_tagField);

            // ── ③ GroupStep ──
            AddSeparator(container);
            AddHeader(container, "③ GroupStep — 事务分组");
            AddHint(container,
                "BeginGroup/EndGroup 将多次 Record 合并为一个原子操作。" +
                "Ctrl+Z 一次撤销整组变更。");

            var groupBtnRow = new VisualElement();
            groupBtnRow.style.flexDirection = FlexDirection.Row;

            var randomizeBtn = new Button(OnRandomizeAll)
            {
                text = "Randomize All (Group)",
                tooltip = "BeginGroup → 修改 size+name+count → EndGroup → 一次 Ctrl+Z 全部撤销"
            };
            randomizeBtn.style.marginRight = 5;
            groupBtnRow.Add(randomizeBtn);

            var batchBtn = new Button(OnBatchColorAndCount)
            {
                text = "Batch Color+Count (Group)",
                tooltip = "同时修改 color 和 count 并分组"
            };
            groupBtnRow.Add(batchBtn);

            container.Add(groupBtnRow);

            _groupLogLabel = new Label("Group log: (none)");
            _groupLogLabel.style.whiteSpace = WhiteSpace.Normal;
            container.Add(_groupLogLabel);

            // ── ④ 数据快照 ──
            AddSeparator(container);
            AddHeader(container, "④ 当前数据快照");
            _configLabel = new Label();
            _configLabel.style.whiteSpace = WhiteSpace.Normal;
            container.Add(_configLabel);

            // ── 状态 ──
            AddSeparator(container);
            AddHeader(container, "状态");
            _statusLabel = new Label();
            container.Add(_statusLabel);

            container.Add(new Label("快捷键: Ctrl+Z Undo / Ctrl+Y Redo"));
            container.Add(new Label("关闭窗口 → Scope Dispose 自动清理所有历史"));

            root.Add(container);
            rootVisualElement.Add(root);
            UpdateStatus();
        }

        // ─── Group 演示回调 ───

        private void OnRandomizeAll()
        {
            // 手动组包裹提交事务：BeginGroup → Commit（自动组嵌套）→ EndGroup → 提交
            _manager.BeginGroup("randomize");

            _config.size = UnityEngine.Random.Range(0f, 100f);
            _config.name = "item_" + UnityEngine.Random.Range(1000, 9999);
            _config.nested.count = UnityEngine.Random.Range(0, 50);

            _configBinding.Commit(); // 自动组嵌套进手动组
            _manager.EndGroup();
            _manager.CommitPending(); // 提交手动组 → 一步撤销整组

            _groupLogLabel.text =
                $"Group: size/name/count → {_config.size:F1} / {_config.name} / {_config.nested.count}";
            RebuildUI(null);
        }

        private void OnBatchColorAndCount()
        {
            _manager.BeginGroup("batch-color-count");

            _config.color = UnityEngine.Random.ColorHSV();
            _config.nested.count = UnityEngine.Random.Range(0, 100);

            _configBinding.Commit();
            _manager.EndGroup();
            _manager.CommitPending();

            _groupLogLabel.text =
                $"Group: color → {_config.color}, count → {_config.nested.count}";
            RebuildUI(null);
        }

        // ─── UI 辅助 ───

        // changeSet 局部刷新：只更新实际变更的字段
        private void RebuildUI(ChangeSet changes)
        {
            if (changes != null)
            {
                foreach (var c in changes.Changes)
                {
                    switch (c.Path)
                    {
                        case "config.size": if (_sizeField != null) _sizeField.SetValueWithoutNotify(_config.size); break;
                        case "config.name": if (_nameField != null) _nameField.SetValueWithoutNotify(_config.name); break;
                        case "config.color": if (_colorField != null) _colorField.SetValueWithoutNotify(_config.color); break;
                        case "config.nested.count": if (_countField != null) _countField.SetValueWithoutNotify(_config.nested.count); break;
                        case "config.nested.tag": if (_tagField != null) _tagField.SetValueWithoutNotify(_config.nested.tag); break;
                    }
                }
            }
            else
            {
                if (_sizeField != null) _sizeField.SetValueWithoutNotify(_config.size);
                if (_nameField != null) _nameField.SetValueWithoutNotify(_config.name);
                if (_colorField != null) _colorField.SetValueWithoutNotify(_config.color);
                if (_countField != null) _countField.SetValueWithoutNotify(_config.nested.count);
                if (_tagField != null) _tagField.SetValueWithoutNotify(_config.nested.tag);
            }
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (_statusLabel != null)
                _statusLabel.text = $"Undo: {_manager.UndoCount} | Redo: {_manager.RedoCount}";
            if (_configLabel != null)
                _configLabel.text = _config.ToString();
        }

        private static void AddHeader(VisualElement container, string text)
        {
            var label = new Label(text);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginTop = 5;
            container.Add(label);
        }

        private static void AddHint(VisualElement container, string text)
        {
            var label = new Label(text);
            label.style.fontSize = 10;
            label.style.unityFontStyleAndWeight = FontStyle.Italic;
            label.style.marginBottom = 5;
            label.style.whiteSpace = WhiteSpace.Normal;
            container.Add(label);
        }

        private static void AddSeparator(VisualElement container)
        {
            container.Add(new VisualElement
            {
                style =
                {
                    height = 1,
                    backgroundColor = new StyleColor(new Color(0.4f, 0.4f, 0.4f, 1f)),
                    marginTop = 10,
                    marginBottom = 5
                }
            });
        }
    }
}
