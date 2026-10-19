using System.Linq;
using NUnit.Framework;
using UniDecl.PropertyGrid.Editor.Examples;
using UniDecl.PropertyGrid.Runtime;
using IAttr = UniDecl.PropertyGrid.Runtime;

namespace UniDecl.PropertyGrid.Editor.Tests
{
    /// <summary>
    /// 布局结构——行组传染逃逸、字段级组标题、节点命名兜底。
    /// 均以 GameCharacterConfig 生产用例为断言对象。
    /// </summary>
    public class LayoutStructureTests
    {
        ObjectLayoutNode ResolveRoot(object target)
        {
            PropertyGridModule.Initialize();
            var meta = ReflectionCache.GetOrCreateMeta(target.GetType());
            return (ObjectLayoutNode)LayoutResolver.Resolve(meta, target).Root;
        }

        static TabLayoutNode Page(ObjectLayoutNode root, string path) =>
            root.Children.OfType<TabViewLayoutNode>().Single()
                .Children.Cast<TabLayoutNode>().Single(t => t.Path == path);

        // ------------------------------------------------------------------
        // 传染式行组：无组字段不挤进行
        // ------------------------------------------------------------------

        [Test]
        public void FlowGroupContagion_LaterGrouplessFields_EscapeToParent()
        {
            // AIConfig: AITarget 行组(targetPriority/threatDecay=2 项)之后,
            // skillRotation/enableDodge/dodgeCooldown/minDodgeCooldown/hasDodgeSkill 无组字段应回到纵向流
            var root = ResolveRoot(new GameCharacterConfig());
            var aiObj = Page(root, "AI").Children.OfType<ObjectLayoutNode>().Single(o => o.ValueAccessor.FullPath == "ai");
            var aiTargetRow = aiObj.Children.OfType<HGroupLayoutNode>().Single(g => g.Path == "AITarget");

            Assert.That(aiTargetRow.Children.OfType<FieldLayoutNode>().Select(f => f.Accessor.FullPath).ToArray(),
                Is.EqualTo(new[] { "ai/targetPriority", "ai/threatDecay" }),
                "行组之后的字段仍在行内——传染逃逸失效");

            var escaped = aiObj.Children.OfType<FieldLayoutNode>().Select(f => f.Accessor.FullPath).ToArray();
            Assert.That(escaped, Does.Contain("ai/skillRotation"));
            Assert.That(escaped, Does.Contain("ai/hasDodgeSkill"));
        }

        [Test]
        public void ArmorStats_RowContainsOnlyDeclaredMembers()
        {
            // ArmorConfig: ArmorStats 行 3 项；repairable 无组字段应在其后单独成行（截图回归）
            var root = ResolveRoot(new GameCharacterConfig());
            var equipmentTab = Page(root, "Equipment");
            var armorObj = FindObjectNode(equipmentTab, "armor");

            var armorRow = armorObj.Children.OfType<HGroupLayoutNode>().Single(g => g.Path == "ArmorStats");
            Assert.That(armorRow.Children.OfType<FieldLayoutNode>().Select(f => f.Accessor.FullPath).ToArray(),
                Is.EqualTo(new[] { "equipment/armor/baseDefense", "equipment/armor/weight", "equipment/armor/durability" }));

            var afterRow = armorObj.Children.SkipWhile(c => c != armorRow)
                .Skip(1).OfType<FieldLayoutNode>().Select(f => f.Accessor.FullPath).ToArray();
            Assert.That(afterRow, Does.Contain("equipment/armor/repairable"), "可维修字段被吸附进行内");
        }

        static ObjectLayoutNode FindObjectNode(ILayoutNode node, string accessorLeaf)
        {
            foreach (var c in node.Children)
            {
                if (c is ObjectLayoutNode o && o.ValueAccessor != null)
                {
                    var segs = o.ValueAccessor.FullPath.Split('/');
                    if (segs[segs.Length - 1] == accessorLeaf) return o;
                }
                var r = FindObjectNode(c, accessorLeaf);
                if (r != null) return r;
            }
            return null;
        }

        // ------------------------------------------------------------------
        // 组标题：字段级 Title 生效，兜底为末段友好名而非裸路径
        // ------------------------------------------------------------------

        [Test]
        public void FieldLevelGroupTitles_AppliedToNodes()
        {
            var root = ResolveRoot(new GameCharacterConfig());
            var basic = Page(root, "Basic");

            var identity = basic.Children.OfType<BoxGroupLayoutNode>().Single(b => b.Path == "Basic/Identity");
            Assert.That(identity.DisplayName, Is.EqualTo("身份信息"));

            var attributes = basic.Children.OfType<GroupLayoutNode>().Single(b => b.Path == "Basic/Attributes");
            Assert.That(attributes.DisplayName, Is.EqualTo("基础属性"),
                "字段级 BoxGroup Title 未收集——显示原始路径");
        }

        [Test]
        public void GroupWithoutTitle_FallsBackToNicifiedLastSegment()
        {
            var root = ResolveRoot(new GroupNameProbe());
            var stats = root.Children.OfType<GroupLayoutNode>().Single(g => g.Path == "PlayerStats");
            Assert.That(stats.DisplayName, Is.EqualTo("Player Stats"));
        }

        private class GroupNameProbe
        {
            [IAttr.VGroup("PlayerStats")]
            public int hp = 1;
        }
    }
}
