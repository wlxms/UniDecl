using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UniDecl.BuiltIn.Runtime.Widgets;
using UniDecl.PropertyGrid.Runtime;
using PropertyField = UniDecl.PropertyGrid.Editor.Elements.PropertyField;

namespace UniDecl.PropertyGrid.Editor.Tests
{
    /// <summary>
    /// List/数组字段渲染——注册表路由、行结构、元素写透与递归展开。
    /// </summary>
    public class ListFieldElementTests
    {
        [SetUp]
        public void SetUp() => PropertyGridModule.Initialize();

        [Test]
        public void CollectionTypes_RouteToListTypeRenderer()
        {
            Assert.That(FieldTypeRendererRegistry.TryResolve(typeof(List<string>), out var lr), Is.True);
            Assert.That(lr, Is.InstanceOf<ListTypeRenderer>());

            Assert.That(FieldTypeRendererRegistry.TryResolve(typeof(int[]), out lr), Is.True);
            Assert.That(lr, Is.InstanceOf<ListTypeRenderer>());

            // string 自身实现 IList，必须仍路由到字符串渲染器
            Assert.That(FieldTypeRendererRegistry.TryResolve(typeof(string), out var sr), Is.True);
            Assert.That(sr, Is.InstanceOf<StringTypeRenderer>());
        }

        [Test]
        public void ValueList_RendersRowPerElementPlusAddButton()
        {
            var data = new ValueListProbe { numbers = { 1, 2, 3 } };
            var element = BuildListEditor(data, nameof(ValueListProbe.numbers));

            var rendered = (VerticalLayout)element.Render();
            // 3 行 + 1 个添加按钮
            Assert.That(rendered.Children.Count(), Is.EqualTo(4));
            var firstRow = (HorizontalLayout)rendered.Children.First();
            Assert.That(firstRow.Children.First(), Is.InstanceOf<IntegerField>());
        }

        [Test]
        public void ObjectList_RendersRecursiveSubtreePerElement()
        {
            var data = new ObjectListProbe
            {
                entries = { new NestedEntry { name = "甲", power = 5 } },
            };
            var element = BuildListEditor(data, nameof(ObjectListProbe.entries));

            var rendered = (VerticalLayout)element.Render();
            Assert.That(rendered.Children.Count(), Is.EqualTo(2)); // 1 元素行 + 添加按钮

            var row = (HorizontalLayout)rendered.Children.First();
            var foldout = row.Children.OfType<Foldout>().Single();
            // 元素标题取首个 string 字段
            Assert.That(foldout.Text, Is.EqualTo("[0] 甲"));
            // 子树包含元素自身字段（递归 LayoutResolver 展开）
            var subtree = foldout.Children.OfType<VerticalLayout>().Single();
            Assert.That(subtree.Children.OfType<PropertyField>().Count(), Is.EqualTo(2));
        }

        [Test]
        public void ListItemAccessor_WritesThroughToList()
        {
            var list = new List<int> { 1, 2 };
            var accessor = new ListItemAccessor(list, 1, "probe[1]");
            accessor.SetValue(9);
            Assert.That(list, Is.EqualTo(new[] { 1, 9 }));
            Assert.That(accessor.GetValue(), Is.EqualTo(9));
        }

        static ListFieldElement BuildListEditor(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName);
            var accessor = new PropertyAccessor(target, new[] { field }, field.Name, field.Name);
            return (ListFieldElement)new ListTypeRenderer().CreateWidget(accessor, new BuildContext());
        }

        [Serializable]
        private class ValueListProbe
        {
            public List<int> numbers = new List<int>();
        }

        [Serializable]
        private class NestedEntry
        {
            public string name;
            public float power;
        }

        [Serializable]
        private class ObjectListProbe
        {
            public List<NestedEntry> entries = new List<NestedEntry>();
        }
    }
}
