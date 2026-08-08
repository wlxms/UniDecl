using System;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.BuiltIn.Runtime.Snapshot;
using UniDecl.Editor.UIToolKit;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.BuiltIn.Editor.Snapshot.Examples
{
    /// <summary>
    /// 综合测试窗口——验证所有可编辑 Field 的 SnapshotBinding 接入。
    /// 覆盖数值类、文本类、Slider 类、瞬时选择型、选择型、复合型。
    /// 菜单: Window > UniDecl > Snapshot Showcase
    /// </summary>
    public class SnapshotShowcaseWindow : UIToolkitHostEditorWindow<UIToolkitRenderManager>
    {
        [MenuItem("Window/UniDecl/Snapshot Showcase")]
        public static void ShowWindow() => GetWindow<SnapshotShowcaseWindow>("Snapshot Showcase");

        private EditorSnapshotManager _snapshotManager;
        private UndoScope _scope;
        private W.Label _statusLabel;

        protected override IElement BuildContent()
        {
            _snapshotManager = new EditorSnapshotManager(new SnapshotManager());
            _scope = new UndoScope(_snapshotManager);

            _statusLabel = new W.Label("Undo: 0 | Redo: 0 | 按下 Ctrl+Z/Ctrl+Y 测试");
            _statusLabel.WithKey("status");

            var layout = new W.VerticalLayout
            {
                new W.Label("Snapshot Showcase — 全类型 Undo/Redo 验证"),
                _statusLabel,
                new W.Label("— 连续输入型（Blur/Enter 提交）—"),
                CreateFloatFields(),
                CreateTextFields(),
                new W.Label("— Slider 类（PointerUp 提交）—"),
                CreateSliders(),
                new W.Label("— 瞬时选择型（ChangeEvent 即提交）—"),
                CreateInstantFields(),
                new W.Label("— 复合型（ChangeEvent 即提交）—"),
                CreateCompositeFields(),
            };

            layout.Scope = _scope;
            return new W.Panel { layout };
        }

        private W.VerticalLayout CreateFloatFields()
        {
            var v = new W.FloatField(3.14f) { Value = 3.14f };
            v.WithKey("showcase_float");
            var d = new W.DoubleField("Double", 2.71828) { Value = 2.71828 };
            d.WithKey("showcase_double");
            var i = new W.IntegerField(42) { Value = 42 };
            i.WithKey("showcase_int");
            var l = new W.LongField("Long", 9_999_999_999L) { Value = 9_999_999_999L };
            l.WithKey("showcase_long");
            return new W.VerticalLayout { v, d, i, l };
        }

        private W.VerticalLayout CreateTextFields()
        {
            var t = new W.TextField("Hello", "输入文本") { Value = "Hello" };
            t.WithKey("showcase_text");
            // ResizableTextArea 构造函数顺序为 (value, label)
            var r = new W.ResizableTextArea("Line1\nLine2", "多行文本") { Value = "Line1\nLine2" };
            r.WithKey("showcase_textarea");
            var s = new W.ToolbarSearchField { Value = "搜索" };
            s.WithKey("showcase_search");
            return new W.VerticalLayout { t, r, s };
        }

        private W.HorizontalLayout CreateSliders()
        {
            var s = new W.Slider("Slider", 50f, 0f, 100f) { Value = 50f };
            s.WithKey("showcase_slider");
            var si = new W.SliderInt("SliderInt", 5, 0, 10) { Value = 5 };
            si.WithKey("showcase_slider_int");
            return new W.HorizontalLayout { s, si };
        }

        private W.VerticalLayout CreateInstantFields()
        {
            var tog = new W.Toggle("启用") { Value = false };
            tog.WithKey("showcase_toggle");
            var color = new W.ColorField("Color", Color.red) { Value = Color.red };
            color.WithKey("showcase_color");
            var obj = new W.ObjectField("对象", typeof(UnityEngine.Object), null);
            obj.WithKey("showcase_object");
            var curve = new W.CurveField("Curve", AnimationCurve.Linear(0, 0, 1, 1)) { Value = AnimationCurve.Linear(0, 0, 1, 1) };
            curve.WithKey("showcase_curve");
            var grad = new W.GradientField("Gradient", new Gradient()) { Value = new Gradient() };
            grad.WithKey("showcase_gradient");
            var enumF = new W.EnumField("Enum", typeof(DayOfWeek), 0) { Value = 0 };
            enumF.WithKey("showcase_enum");
            var layer = new W.LayerField("Layer", 0) { Value = 0 };
            layer.WithKey("showcase_layer");
            var tag = new W.TagField("Tag", "Untagged") { Value = "Untagged" };
            tag.WithKey("showcase_tag");
            return new W.VerticalLayout { tog, color, obj, curve, grad, enumF, layer, tag };
        }

        private W.VerticalLayout CreateCompositeFields()
        {
            var v2 = new W.Vector2Field("Vector2", Vector2.one) { Value = Vector2.one };
            v2.WithKey("showcase_v2");
            var v3 = new W.Vector3Field("Vector3", Vector3.one) { Value = Vector3.one };
            v3.WithKey("showcase_v3");
            var v4 = new W.Vector4Field("Vector4", Vector4.one) { Value = Vector4.one };
            v4.WithKey("showcase_v4");
            var r = new W.RectField("Rect", new Rect(0, 0, 100, 50)) { Value = new Rect(0, 0, 100, 50) };
            r.WithKey("showcase_rect");
            var b = new W.BoundsField("Bounds", new Bounds(Vector3.zero, Vector3.one)) { Value = new Bounds(Vector3.zero, Vector3.one) };
            b.WithKey("showcase_bounds");
            return new W.VerticalLayout { v2, v3, v4, r, b };
        }

        private void UpdateStatus()
        {
            if (_statusLabel != null)
                _statusLabel.Text = $"Undo: {_snapshotManager.UndoCount} | Redo: {_snapshotManager.RedoCount} | 按下 Ctrl+Z/Ctrl+Y 测试";
        }

        private void OnDestroy()
        {
            _scope?.Dispose();
            _snapshotManager?.Dispose();
        }
    }
}
