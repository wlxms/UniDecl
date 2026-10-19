using System;
using System.Linq;
using NUnit.Framework;
using UniDecl.BuiltIn.Runtime.Widgets;
using UniDecl.PropertyGrid.Editor.Decorators;
using UniDecl.PropertyGrid.Editor.Examples;
using UniDecl.PropertyGrid.Runtime;
using UnityEngine;
using IAttr = UniDecl.PropertyGrid.Runtime;
using PropertyField = UniDecl.PropertyGrid.Editor.Elements.PropertyField;
using RangeAttribute = UniDecl.PropertyGrid.Runtime.RangeAttribute;
using SpaceAttribute = UniDecl.PropertyGrid.Runtime.SpaceAttribute;

namespace UniDecl.PropertyGrid.Editor.Tests
{
    /// <summary>
    /// 字段装饰器行为——OnValueChanged 单参回调、Validate 回写、Step 吸附、
    /// 行级布局元数据（Space/FlexGrow/Width）与 EnumToggleButtons 按钮组。
    /// </summary>
    public class DecoratorBehaviorTests
    {
        [SetUp]
        public void SetUp() => PropertyGridModule.Initialize();

        static DecoratorContext MakeContext(PropertyField pf, PropertyAccessor accessor,
            PropertyGridAttribute[] attrs, object renderer, object target)
        {
            var ctx = BuildContext.CreateRoot(null, target, null);
            ctx.Renderer = renderer;
            return new DecoratorContext(accessor, attrs, ctx, pf);
        }

        static PropertyAccessor AccessorOf(object target, string fieldName)
        {
            var f = target.GetType().GetField(fieldName);
            return new PropertyAccessor(target, new[] { f }, f.Name, f.Name);
        }

        // ------------------------------------------------------------------
        // OnValueChanged：Method(T target) 签名必须被调用（P0 回归）
        // ------------------------------------------------------------------

        private class OnChangedProbe
        {
            public bool enabled;
            public int invoked;
        }

        private class OnChangedProbeRenderer
        {
            public void HandleEnabled(OnChangedProbe target) => target.invoked++;
        }

        [Test]
        public void OnValueChanged_SingleTargetParam_IsInvoked()
        {
            var target = new OnChangedProbe();
            var accessor = AccessorOf(target, nameof(OnChangedProbe.enabled));
            var pf = new PropertyField("enabled") { Editor = new Toggle(null, false), Accessor = accessor };

            var dc = MakeContext(pf, accessor,
                new PropertyGridAttribute[] { new OnValueChangedAttribute(nameof(OnChangedProbeRenderer.HandleEnabled)) },
                new OnChangedProbeRenderer(), target);
            new OnValueChangedDecorator().Process(pf, dc);

            accessor.SetValue(true);

            Assert.That(target.invoked, Is.EqualTo(1));
        }

        // ------------------------------------------------------------------
        // Validate：构建即执行并回写结果；字段变更后重新校验
        // （经 WidgetFactory 全链路构建，字段访问器绑定 BuildContext 以触发 FieldChanged）
        // ------------------------------------------------------------------

        private class ValidateProbe
        {
            public string name = "ok";

            [IAttr.Validate("Check")]
            public string result = "";
        }

        private class ValidateProbeRenderer
        {
            public string Check(ValidateProbe target) =>
                string.IsNullOrEmpty(target.name) ? "❌ 名称为空" : "✅ 通过";
        }

        [Test]
        public void Validate_WritesResultBack_OnBuildAndOnChange()
        {
            var target = new ValidateProbe();
            var meta = ReflectionCache.GetOrCreateMeta(typeof(ValidateProbe));
            var ctx = BuildContext.CreateRoot(null, target, meta);
            ctx.Renderer = new ValidateProbeRenderer();

            var root = (VerticalLayout)WidgetFactory.CreateTree(LayoutResolver.Resolve(meta, target), ctx);
            Assert.That(target.result, Is.EqualTo("✅ 通过"));

            // 依赖字段变化 → 重新校验
            var nameField = root.Children.OfType<PropertyField>().Single(p => p.Accessor.FullPath == "name");
            nameField.Accessor.SetValue("");
            Assert.That(target.result, Is.EqualTo("❌ 名称为空"));
        }

        // ------------------------------------------------------------------
        // Step：Range+Step 吸附网格
        // ------------------------------------------------------------------

        [Test]
        public void NumericStep_SnapsToGrid()
        {
            Assert.That(NumericStep.Snap(7.3f, 0f, 100f, 0.5), Is.EqualTo(7.5f).Within(1e-4));
            Assert.That(NumericStep.Snap(7.2f, 0f, 100f, 0.5), Is.EqualTo(7.0f).Within(1e-4));
            Assert.That(NumericStep.Snap(0.07f, 0f, 1f, 0.05), Is.EqualTo(0.05f).Within(1e-4));
            // 越界夹回
            Assert.That(NumericStep.Snap(120f, 0f, 100f, 0.5), Is.EqualTo(100f).Within(1e-4));
        }

        [Test]
        public void RangeWithStep_CommitsSnappedValue()
        {
            var target = new StepProbe();
            var accessor = AccessorOf(target, nameof(StepProbe.speed));
            var pf = new PropertyField("speed") { Editor = new FloatField(0), Accessor = accessor };
            var dc = MakeContext(pf, accessor, new PropertyGridAttribute[]
            {
                new RangeAttribute(0f, 5f),
                new StepAttribute(0.5),
            }, null, target);
            new RangeDecorator().Process(pf, dc);

            ((Slider)pf.Editor).OnValueChanged(3.3f);
            Assert.That(target.speed, Is.EqualTo(3.5f).Within(1e-4));
        }

        private class StepProbe { public float speed = 1f; }

        // ------------------------------------------------------------------
        // 行级布局元数据：Space / FlexGrow / Width 写入 PropertyField
        // ------------------------------------------------------------------

        [Test]
        public void SpaceAndFlexMetadata_AppliedToPropertyField()
        {
            var target = new LayoutMetaProbe();
            var accessor = AccessorOf(target, nameof(LayoutMetaProbe.weighted));
            var pf = new PropertyField("weighted") { Editor = new FloatField(0), Accessor = accessor };
            var dc = MakeContext(pf, accessor, new PropertyGridAttribute[]
            {
                new SpaceAttribute(4, 8),
                new FlexGrowAttribute(2),
                new WidthAttribute(120),
            }, null, target);

            new SpaceDecorator().Process(pf, dc);
            new FlexSizeDecorator().Process(pf, dc);

            Assert.That(pf.SpaceBefore, Is.EqualTo(4));
            Assert.That(pf.SpaceAfter, Is.EqualTo(8));
            Assert.That(pf.FlexGrow, Is.EqualTo(2));
            Assert.That(pf.Width, Is.EqualTo(120));
        }

        private class LayoutMetaProbe { public float weighted = 1f; }

        // ------------------------------------------------------------------
        // EnumToggleButtons：替换为按钮组控件
        // ------------------------------------------------------------------

        [Test]
        public void EnumToggleButtons_ReplacesEditorWithToggleGroup()
        {
            var target = new EnumProbe();
            var accessor = AccessorOf(target, nameof(EnumProbe.clazz));
            var pf = new PropertyField("clazz") { Editor = new EnumField(null, typeof(CharClass), 0), Accessor = accessor };
            var dc = MakeContext(pf, accessor,
                new PropertyGridAttribute[] { new EnumToggleButtonsAttribute() }, null, target);

            new EnumToggleButtonsDecorator().Process(pf, dc);

            var group = (EnumToggleGroup)pf.Editor;
            Assert.That(group.EnumType, Is.EqualTo(typeof(CharClass)));
            Assert.That(group.GetChoiceNames().Length, Is.EqualTo(Enum.GetNames(typeof(CharClass)).Length));

            group.OnValueChanged((int)CharClass.Mage);
            Assert.That(target.clazz, Is.EqualTo(CharClass.Mage));
        }

        private class EnumProbe { public CharClass clazz = CharClass.Warrior; }

        // ------------------------------------------------------------------
        // InlineProperty(StripOwnGroups)：子字段组声明被剥离
        // ------------------------------------------------------------------

        [Test]
        public void InlineProperty_StripOwnGroups_RemovesChildGroupDeclarations()
        {
            var kept = ResolveChildGroups(new KeepGroupsProbe());
            Assert.That(kept, Is.EqualTo(1)); // 子字段 HGroup 保留 → 1 个组节点

            var stripped = ResolveChildGroups(new StripGroupsProbe());
            Assert.That(stripped, Is.EqualTo(0)); // 组声明被剥离 → 字段直挂
        }

        static int ResolveChildGroups(object target)
        {
            PropertyGridModule.Initialize();
            var meta = ReflectionCache.GetOrCreateMeta(target.GetType());
            var root = (ObjectLayoutNode)LayoutResolver.Resolve(meta, target).Root;
            var childRoot = root.Children.OfType<ObjectLayoutNode>().Single();
            return childRoot.Children.OfType<HGroupLayoutNode>().Count();
        }

        [Serializable]
        private class GroupedChild
        {
            [IAttr.HGroup("Inner")]
            public int a = 1;
            [IAttr.HGroup("Inner")]
            public int b = 2;
        }

        [Serializable]
        private class KeepGroupsProbe
        {
            [InlineProperty]
            public GroupedChild child = new GroupedChild();
        }

        [Serializable]
        private class StripGroupsProbe
        {
            [InlineProperty(StripOwnGroups = true)]
            public GroupedChild child = new GroupedChild();
        }
    }
}
