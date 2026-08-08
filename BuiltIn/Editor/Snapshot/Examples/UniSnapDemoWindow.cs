using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Snapshot;

namespace UniDecl.BuiltIn.Editor.Snapshot.Examples
{
    /// <summary>
    /// UniSnap Demo 窗口——完整演示 ValueStep / ObjectDiffStep / GroupStep / Merge
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
        // ValueStep 字段
        private FloatField _sizeField;
        private TextField _nameField;

        // ObjectDiffStep 字段
        private IntegerField _countField;
        private TextField _tagField;
        private ColorField _colorField;

        // GroupStep 日志
        private Label _groupLogLabel;

        // 状态栏
        private Label _statusLabel;
        private Label _configLabel;

        // Color 防抖：连续拖拽时延迟一帧提交，让 Merge 合并同 key Record
        private int _colorDebounceId;

        // ─── 生命周期 ───

        public void OnEnable()
        {
            _config = new ConfigData();
            _manager = new EditorSnapshotManager(new SnapshotManager());
            _scope = new UndoScope(_manager);

            // ① ValueStep setter：Func<T, T> 接收新值恢复，返回被覆盖的旧值
            _scope.Register<float>("size", v =>
            {
                float old = _config.size;
                _config.size = v;
                return old;
            });
            _scope.Register<string>("name", v =>
            {
                string old = _config.name;
                _config.name = v;
                return old;
            });
            _scope.Register<Color>("color", v =>
            {
                var old = _config.color;
                _config.color = v;
                return old;
            });

            // ② ObjectDiffStep setter：深拷贝快照整体恢复嵌套对象
            _scope.Register<object>("config", v =>
            {
                DeepCopyUtility.RestoreFields(_config,
                    (System.Collections.Generic.Dictionary<string, object>)v);
                return null;
            });

            _manager.OnUndoRedoPerformed += RebuildUI;

            BuildUI();
        }

        public void OnDisable()
        {
            _manager?.Dispose();
            _scope?.Dispose();
            _manager = null;
            _scope = null;
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

            // ── ① ValueStep ──
            AddHeader(container, "① ValueStep — 值类型快照");
            AddHint(container,
                "修改 Size / Name / Color，Ctrl+Z 撤销。同 key 500ms 内连续修改自动合并。");

            _sizeField = new FloatField("Size") { value = _config.size };
            _sizeField.RegisterValueChangedCallback(evt =>
            {
                _scope.Record(evt.previousValue, "size");
                _config.size = evt.newValue;
                _scope.Commit();
                UpdateStatus();
            });
            container.Add(_sizeField);

            _nameField = new TextField("Name") { value = _config.name };
            _nameField.RegisterValueChangedCallback(evt =>
            {
                _scope.Record(evt.previousValue, "name");
                _config.name = evt.newValue;
                _scope.Commit();
                UpdateStatus();
            });
            container.Add(_nameField);

            _colorField = new ColorField("Color") { value = _config.color };
            _colorField.RegisterValueChangedCallback(evt =>
            {
                _scope.Record(evt.previousValue, "color");
                _config.color = evt.newValue;
                UpdateStatus();
                // 防抖：延迟一帧提交。连续拖拽时每次新变更重置 debounceId，
                // 只有最后一次停顿后的 delayCall 才提交，此时 Merge 已合并同 key Record
                int id = ++_colorDebounceId;
                EditorApplication.delayCall += () =>
                {
                    if (_colorDebounceId == id)
                        _scope.Commit();
                };
            });
            container.Add(_colorField);

            // ── ② ObjectDiffStep ──
            AddSeparator(container);
            AddHeader(container, "② ObjectDiffStep — 对象深拷贝快照");
            AddHint(container,
                "RecordObject 对整个 _config 做深拷贝快照。修改 Count / Tag 后，" +
                "Ctrl+Z 一次性恢复所有字段（包括 size/name/color/nested）到快照时的状态。");

            _countField = new IntegerField("Nested Count") { value = _config.nested.count };
            _countField.RegisterValueChangedCallback(evt =>
            {
                _scope.RecordObject(_config, "config");
                _config.nested.count = evt.newValue;
                _scope.Commit();
                UpdateStatus();
            });
            container.Add(_countField);

            _tagField = new TextField("Nested Tag") { value = _config.nested.tag };
            _tagField.RegisterValueChangedCallback(evt =>
            {
                _scope.RecordObject(_config, "config");
                _config.nested.tag = evt.newValue;
                _scope.Commit();
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
            _manager.BeginGroup("randomize");

            var oldSize = _config.size;
            var oldName = _config.name;
            var oldCount = _config.nested.count;

            var newSize = UnityEngine.Random.Range(0f, 100f);
            var newName = "item_" + UnityEngine.Random.Range(1000, 9999);
            var newCount = UnityEngine.Random.Range(0, 50);

            // ValueStep x2
            _scope.Record(oldSize, "size");
            _config.size = newSize;

            _scope.Record(oldName, "name");
            _config.name = newName;

            // ObjectDiffStep x1（快照当前 state 再修改）
            _scope.RecordObject(_config, "config");
            _config.nested.count = newCount;

            _manager.EndGroup();
            _scope.Commit();

            _groupLogLabel.text =
                $"Group: size {oldSize:F1}→{newSize:F1}, " +
                $"name '{oldName}'→'{newName}', count {oldCount}→{newCount}";
            RebuildUI();
        }

        private void OnBatchColorAndCount()
        {
            _manager.BeginGroup("batch-color-count");

            var oldColor = _config.color;
            var oldCount = _config.nested.count;

            var newColor = UnityEngine.Random.ColorHSV();
            var newCount = UnityEngine.Random.Range(0, 100);

            // ValueStep x1 (Color)
            _scope.Record(oldColor, "color");
            _config.color = newColor;

            // ObjectDiffStep x1 (整个 config 快照)
            _scope.RecordObject(_config, "config");
            _config.nested.count = newCount;

            _manager.EndGroup();
            _scope.Commit();

            _groupLogLabel.text =
                $"Group: color {oldColor}→{newColor}, count {oldCount}→{newCount}";
            RebuildUI();
        }

        // ─── UI 辅助 ───

        private void RebuildUI()
        {
            if (_sizeField != null) _sizeField.SetValueWithoutNotify(_config.size);
            if (_nameField != null) _nameField.SetValueWithoutNotify(_config.name);
            if (_colorField != null) _colorField.SetValueWithoutNotify(_config.color);
            if (_countField != null) _countField.SetValueWithoutNotify(_config.nested.count);
            if (_tagField != null) _tagField.SetValueWithoutNotify(_config.nested.tag);
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
