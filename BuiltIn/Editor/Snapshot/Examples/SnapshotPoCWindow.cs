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
    /// PoC 测试窗口——验证 ContainerElement（VerticalLayout）作为 IScopeProvider，
    /// 让子 Widget 的 Renderer 通过 ElementState.Scope 接入 Undo/Redo。
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
        private UndoScope _scope;
        private W.FloatField _floatField;

        protected override IElement BuildContent()
        {
            // 创建 Snapshot 基础设施
            _snapshotManager = new EditorSnapshotManager(new SnapshotManager());
            // Undo/Redo 后由 Renderer 的 setter 自动回写 VE（SetValueWithoutNotify）
            // 无需重建子树

            _scope = new UndoScope(_snapshotManager);

            // 测试用数据
            var data = new PoCData { value = 42f };

            // 创建 FloatField，手动设置 Key（Renderer 用它做 Register key）
            _floatField = new W.FloatField(data.value) { Value = data.value };
            _floatField.WithKey("poc_float");
            _floatField.OnValueChanged = (newVal, oldVal) =>
            {
                data.value = newVal;
            };

            // VerticalLayout 作为 IScopeProvider——设置 Scope 后，
            // DOMTree 展开 Children 时自动 Push，子元素 ElementState 携带此 Scope。
            var layout = new W.VerticalLayout
            {
                new W.Label("Snapshot PoC — FloatField Undo/Redo"),
                new W.Label("修改下方数值后按 Ctrl+Z/Ctrl+Y 验证 Undo/Redo"),
                _floatField,
            };
            layout.Scope = _scope;

            return new W.Panel { layout };
        }

        private void OnDestroy()
        {
            _scope?.Dispose();
            _snapshotManager?.Dispose();
        }

        [Serializable]
        public class PoCData
        {
            public float value;
        }
    }
}
