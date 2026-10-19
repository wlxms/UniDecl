using System;
using System.Linq;
using NUnit.Framework;
using UniDecl.PropertyGrid.Editor.Examples;
using UniDecl.PropertyGrid;
using IAttr = UniDecl.PropertyGrid;

namespace UniDecl.PropertyGrid.Editor.Tests
{
    /// <summary>
    /// TabGroup 布局解析——类级声明为中间路径段提供节点类型（Basic/Debug 不再退化为 VGroup），
    /// 同级连续 TabGroup 被合成为 TabViewLayoutNode，类级控件按 Order 排在字段区之前。
    /// </summary>
    public class TabGroupLayoutTests
    {
        ObjectLayoutNode ResolveRoot(object target)
        {
            PropertyGridModule.Initialize();
            var meta = ReflectionCache.GetOrCreateMeta(target.GetType());
            return (ObjectLayoutNode)LayoutResolver.Resolve(meta, target).Root;
        }

        [Test]
        public void ClassDeclaredTabs_MiddlePathSegment_UpgradesToTabNode()
        {
            // Basic 仅作为 "Basic/Identity" 等路径的中间段出现，类型应由类级 [TabGroup("Basic")] 决定
            var root = ResolveRoot(new GameCharacterConfig());
            var basic = root.Children.OfType<TabViewLayoutNode>().Single()
                .Children.Cast<TabLayoutNode>().Single(t => t.Path == "Basic");
            Assert.That(basic.DisplayName, Is.EqualTo("📋 基础"));

            // Basic 页内应包含身份信息与基础属性两个 Box 子组（而非平铺字段）
            var boxes = basic.Children.OfType<BoxGroupLayoutNode>().Select(b => b.Path).ToArray();
            Assert.That(boxes, Does.Contain("Basic/Identity"));
            Assert.That(boxes, Does.Contain("Basic/Attributes"));
            Assert.That(basic.Children.OfType<FieldLayoutNode>(), Is.Empty);
        }

        [Test]
        public void ConsecutiveTabs_AreWrappedIntoTabView()
        {
            var root = ResolveRoot(new GameCharacterConfig());
            var tabView = root.Children.OfType<TabViewLayoutNode>().Single();

            var pagePaths = tabView.Children.Cast<TabLayoutNode>().Select(t => t.Path).ToArray();
            Assert.That(pagePaths, Is.EqualTo(new[] { "Basic", "Equipment", "AI", "Appearance", "Debug" }));

            var titles = tabView.Children.Cast<TabLayoutNode>().Select(t => t.DisplayName).ToArray();
            Assert.That(titles, Is.EqualTo(new[] { "📋 基础", "⚔ 装备", "🧠 AI", "🎨 外观", "🔧 调试" }));
        }

        [Test]
        public void ClassElements_WithNegativeOrder_RenderBeforeTabs()
        {
            var root = ResolveRoot(new GameCharacterConfig());
            var firstTabViewIndex = root.Children.TakeWhile(c => !(c is TabViewLayoutNode)).Count();

            // 标题/InfoBox/三个按钮（Order -1000..-960）应全部位于页签区之前
            var headers = root.Children.Take(firstTabViewIndex).ToArray();
            Assert.That(headers.Length, Is.EqualTo(5));
            Assert.That(headers[0], Is.InstanceOf<ClassElementLayoutNode>());
            Assert.That(headers.All(c => c is ClassElementLayoutNode), Is.True);
        }

        [Test]
        public void SingleTabGroup_IsNotWrappedIntoTabView()
        {
            var root = ResolveRoot(new SingleTabProbe());
            Assert.That(root.Children.OfType<TabLayoutNode>().Single().Path, Is.EqualTo("Only"));
            Assert.That(root.Children.OfType<TabViewLayoutNode>(), Is.Empty);
        }

        [IAttr.TabGroup("Only", Title = "唯一页")]
        [Serializable]
        private class SingleTabProbe
        {
            [IAttr.TabGroup("Only")]
            public int value = 1;
        }
    }
}
