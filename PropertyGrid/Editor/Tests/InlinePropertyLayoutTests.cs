using System;
using System.Linq;
using NUnit.Framework;
using UniDecl.BuiltIn.Runtime.Widgets;
using UniDecl.PropertyGrid.Editor.Examples;
using UniDecl.PropertyGrid.Runtime;

namespace UniDecl.PropertyGrid.Editor.Tests
{
    public class InlinePropertyLayoutTests
    {
        [Serializable]
        private class InlineBoundaryProbe
        {
            [InlineProperty]
            public FloatRangeConfig InlineValue = new FloatRangeConfig();

            public int AfterInline = 1;
        }

        [Serializable]
        private class HorizontalBoundaryProbe
        {
            [HGroup("Row")]
            public float BeforeInline = 1;

            [InlineProperty]
            public FloatRangeConfig InlineValue = new FloatRangeConfig();

            public int AfterInline = 1;
        }

        [Test]
        public void InlineProperty_OnlyChangesCurrentValueInitialFlow()
        {
            var data = new InlineBoundaryProbe();
            var meta = ReflectionCache.GetOrCreateMeta(typeof(InlineBoundaryProbe));
            var root = (ObjectLayoutNode)LayoutResolver.Resolve(meta, data).Root;
            var inlineValue = root.Children.OfType<ObjectLayoutNode>().Single();
            var sibling = root.Children.OfType<FieldLayoutNode>().Single();

            Assert.That(root.InitialValueDirection, Is.EqualTo(InlineDirection.Default));
            Assert.That(inlineValue.InitialValueDirection, Is.EqualTo(InlineDirection.Horizontal));
            Assert.That(inlineValue.ValueAccessor.FullPath, Is.EqualTo("InlineValue"));
            Assert.That(inlineValue.Parent, Is.SameAs(root));
            Assert.That(sibling.Parent, Is.SameAs(root));

            var handler = new ObjectLayoutHandler();
            Assert.That(handler.Build(root, new BuildContext()), Is.TypeOf<VerticalLayout>());
            Assert.That(handler.Build(inlineValue, new BuildContext()), Is.TypeOf<HorizontalLayout>());
        }

        [Test]
        public void InlineProperty_DoesNotEnterActiveHorizontalGroup()
        {
            var data = new HorizontalBoundaryProbe();
            var meta = ReflectionCache.GetOrCreateMeta(typeof(HorizontalBoundaryProbe));
            var root = (ObjectLayoutNode)LayoutResolver.Resolve(meta, data).Root;
            var inlineValue = root.Children.OfType<ObjectLayoutNode>().Single();
            var row = root.Children.OfType<HGroupLayoutNode>().Single();
            var sibling = root.Children.OfType<FieldLayoutNode>().Single(field => field.Accessor.FullPath == "AfterInline");

            Assert.That(inlineValue.InitialValueDirection, Is.EqualTo(InlineDirection.Horizontal));
            Assert.That(inlineValue.Parent, Is.SameAs(root));
            // 无布局特性的后续字段从传染式行组逃逸回父级纵向流，不再挤进行
            Assert.That(row.Children.OfType<FieldLayoutNode>().Select(f => f.Accessor.FullPath),
                Is.EqualTo(new[] { "BeforeInline" }));
            Assert.That(sibling.Parent, Is.SameAs(root));
        }
    }
}