using System;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.BuiltIn.Runtime.Snapshot;
using UniDecl.Editor.UIToolKit;
using UnityEditor;
using UnityEngine.UIElements;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.BuiltIn.Editor.Snapshot.Examples
{
    /// <summary>
    /// PoC 测试窗口——验证 Host 注入 SnapshotManager 后，
    /// ContainerElement（VerticalLayout）作为 IScopeProvider 标记，
    /// 子 Widget 的 Renderer 通过 ElementState.Scope 自动接入 Undo/Redo。
    ///
    /// 不依赖 PropertyGridModule/PropertyGridElement，纯声明式 Element 树。
    /// 菜单: Window > UniDecl > Snapshot PoC
    ///
    /// 验证步骤：
    /// 1. 修改 FloatField 的值
    /// 2. Ctrl+Z 应回滚到旧值
    /// 3. Ctrl+Y 应重做到新值
    /// 4. 控制台日志显示 Register/Record/Commit/Undo/Redo 流转
    /// </summary>
    public class SnapshotPoCWindow : UIToolkitHostEditorWindow<UIToolkitRenderManager>
    {
        [MenuItem("Window/UniDecl/Snapshot PoC")]
        public static void ShowWindow() => GetWindow<SnapshotPoCWindow>("Snapshot PoC");

        private EditorSnapshotManager _snapshotManager;
        private W.FloatField _floatField;

        protected override UIToolkitRenderManager CreateManager()
        {
            _snapshotManager = new EditorSnapshotManager(new SnapshotManager());
            return new UIToolkitRenderManager(_snapshotManager);
        }

        protected override IElement BuildContent()
        {
            // 测试用数据
            var data = new PoCData { value = 42f };

            // 创建 FloatField，手动设置 Key（Renderer 用它做 Register key）
            _floatField = new W.FloatField(data.value) { Value = data.value };
            _floatField.WithKey("poc_float");
            _floatField.OnValueChanged = (newVal, oldVal) =>
            {
                data.value = newVal;
            };

            // VerticalLayout 作为 IScopeProvider 标记——Host 自动为它创建
            // UndoScope 并注入 ElementState.Scope，子树 Widget 自动获得。
            return new W.Panel
            {
                new W.VerticalLayout
                {
                    new W.Label("Snapshot PoC — FloatField Undo/Redo"),
                    new W.Label("修改下方数值后按 Ctrl+Z/Ctrl+Y 验证 Undo/Redo"),
                    _floatField,
                },
            };
        }

        private void OnDestroy()
        {
            _snapshotManager?.Dispose();
        }

        [Serializable]
        public class PoCData
        {
            public float value;
        }
    }
}
