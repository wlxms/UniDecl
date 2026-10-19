using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UniDecl.PropertyGrid.Editor.Examples;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.TestTools;

namespace UniDecl.PropertyGrid.Editor.Tests
{
    /// <summary>
    /// GameCharacterConfig / PropertyGridExample 端到端渲染——
    /// 真实打开 EditorWindow，走 CreateGUI → UIToolkitRenderManager 完整链路，
    /// 断言渲染出的 VisualElement 树：页签结构、页内容、枚举按钮组防溢出样式。
    /// </summary>
    public class GameCharacterConfigE2ETests
    {
        static readonly string[] ExpectedTabs = { "📋 基础", "⚔ 装备", "🧠 AI", "🎨 外观", "🔧 调试" };

        /// <summary>页签条上的 ToolbarToggle（父级是 Toolbar），排除枚举按钮组。</summary>
        static List<ToolbarToggle> GetTabToggles(VisualElement root) =>
            root.Query<ToolbarToggle>().Where(t => t.parent is Toolbar).ToList();

        /// <summary>切页并等待 delayCall 刷新（Rebuild 经 EditorApplication.delayCall 调度）。</summary>
        static IEnumerator SwitchTab(VisualElement root, int index)
        {
            GetTabToggles(root)[index].value = true;
            yield return null;
            yield return null;
        }

        // ------------------------------------------------------------------
        // Game Character Config 窗口
        // ------------------------------------------------------------------

        [UnityTest]
        public IEnumerator GCC_E2E_HeaderButtonsAndTabStripRender()
        {
            var w = EditorWindow.GetWindow<GameCharacterConfigWindow>();
            w.Repaint();
            yield return null;
            var root = w.rootVisualElement;
            Assert.That(root, Is.Not.Null);

            // @ 表达式头部：标题与状态提示
            var labelTexts = root.Query<Label>().ToList().Select(l => l.text).ToList();
            Assert.That(labelTexts, Does.Contain("【Warrior】无名英雄 - Lv.1"));
            Assert.That(labelTexts.Any(t => t.Contains("装备评分")), Is.True, "StatusHint InfoBox 未渲染");

            // 类级按钮
            var buttonTexts = root.Query<UnityEngine.UIElements.Button>().ToList().Select(b => b.text).ToList();
            Assert.That(buttonTexts, Does.Contain("🎲 随机生成"));
            Assert.That(buttonTexts, Does.Contain("📋 导出配置"));
            Assert.That(buttonTexts, Does.Contain("⚠ 恢复默认"));

            // 页签条：5 页 + 标题
            var toggles = GetTabToggles(root);
            Assert.That(toggles.Select(t => t.text).ToArray(), Is.EqualTo(ExpectedTabs));

            yield break;
        }

        [UnityTest]
        public IEnumerator GCC_E2E_DefaultTabHasFieldContent()
        {
            var w = EditorWindow.GetWindow<GameCharacterConfigWindow>();
            w.Repaint();
            yield return null;
            var root = w.rootVisualElement;

            yield return SwitchTab(root, 0); // 显式切到基础页（页签记忆跨测试持久）

            // 默认页（基础）应有真实编辑控件，而非空白
            var labels = root.Query<Label>().ToList().Select(l => l.text).ToList();
            Assert.That(labels, Does.Contain("角色名称"), "基础页缺少字段内容——Tabs 页内容未渲染");
            Assert.That(labels, Does.Contain("职业"));
            Assert.That(labels, Does.Contain("生命值"));

            Assert.That(root.Query<TextField>().ToList().Count, Is.GreaterThan(0), "应有文本输入框");
            Assert.That(root.Query<UnityEngine.UIElements.Slider>().ToList().Count, Is.GreaterThan(0), "Range 字段应渲染为滑条");
            // 职业为普通枚举（非 EnumToggleButtons），应渲染为下拉
            Assert.That(root.Query<EnumField>().ToList().Count, Is.GreaterThan(0), "职业字段应渲染为枚举下拉");

            yield break;
        }

        [UnityTest]
        public IEnumerator GCC_E2E_SwitchToEquipmentTabRendersNestedContent()
        {
            var w = EditorWindow.GetWindow<GameCharacterConfigWindow>();
            w.Repaint();
            yield return null;
            var root = w.rootVisualElement;

            yield return SwitchTab(root, 1); // ⚔ 装备

            var labels = root.Query<Label>().ToList().Select(l => l.text).ToList();
            Assert.That(labels, Does.Contain("武器名称"), "装备页三级嵌套内容未渲染");
            Assert.That(labels, Does.Contain("快捷栏物品"), "装备页背包区未渲染");

            // Foldout 组标题（武器/护甲/饰品）
            var foldouts = root.Query<UnityEngine.UIElements.Foldout>().ToList().Select(f => f.text).ToList();
            Assert.That(foldouts, Does.Contain("武器配置"));
            Assert.That(foldouts, Does.Contain("护甲配置"));

            yield break;
        }

        [UnityTest]
        public IEnumerator GCC_E2E_EnumToggleGroupStaysInsideValueArea()
        {
            var w = EditorWindow.GetWindow<GameCharacterConfigWindow>();
            w.Repaint();
            yield return null;
            var root = w.rootVisualElement;

            yield return SwitchTab(root, 2); // 🧠 AI

            // AI 人格 EnumToggleButtons：普通 Button 网格（非下拉、非工具栏控件）
            var balanced = root.Query<UnityEngine.UIElements.Button>().Where(b => b.text == "Balanced").ToList();
            Assert.That(balanced.Count, Is.EqualTo(1), "AI 人格枚举按钮组未渲染");

            // 防溢出：网格可换行；每枚举值一格
            var grid = balanced[0].parent;
            Assert.That(grid.style.flexDirection.value, Is.EqualTo(FlexDirection.Row));
            Assert.That(grid.style.flexWrap.value, Is.EqualTo(Wrap.Wrap), "枚举按钮组容器应允许换行");
            Assert.That(grid.childCount, Is.EqualTo(5), "人格应为 5 个独立按钮");
            Assert.That(grid.Query<ToolbarToggle>().ToList().Count, Is.EqualTo(0), "枚举不应再渲染为 Toolbar 系控件");

            // 伤害类型枚举组在装备页，6 项
            yield return SwitchTab(root, 1);
            Assert.That(root.Query<UnityEngine.UIElements.Button>().Where(b => b.text == "Physical").ToList().Count,
                Is.EqualTo(1), "伤害类型枚举按钮组未渲染");

            yield break;
        }

        [UnityTest]
        public IEnumerator GCC_E2E_DebugTabHasValidationAndButtons()
        {
            var w = EditorWindow.GetWindow<GameCharacterConfigWindow>();
            w.Repaint();
            yield return null;
            var root = w.rootVisualElement;

            yield return SwitchTab(root, 4); // 🔧 调试

            // Validate 装饰器构建即执行并渲染为只读信息框（UnityEngine.UIElements.HelpBox）
            var validated = root.Query<HelpBox>()
                .Where(h => (h.text ?? "").Contains("配置验证通过")).ToList();
            Assert.That(validated.Count, Is.EqualTo(1), "校验结果未显示为信息框");

            var buttons = root.Query<UnityEngine.UIElements.Button>().ToList().Select(b => b.text).ToList();
            Assert.That(buttons, Does.Contain("清空背包"));
            Assert.That(buttons, Does.Contain("重置所有装备"));

            yield break;
        }

        // ------------------------------------------------------------------
        // PropertyGrid Feature Showcase 窗口（枚举溢出缺陷的原始报告处）
        // ------------------------------------------------------------------

        [UnityTest]
        public IEnumerator FeatureShowcase_E2E_RendersWithOverflowGuard()
        {
            var w = EditorWindow.GetWindow<PropertyGridExample>("PropertyGrid Feature Showcase");
            w.Repaint();
            yield return null;
            var root = w.rootVisualElement;
            Assert.That(root, Is.Not.Null);

            var labels = root.Query<Label>().ToList().Select(l => l.text).ToList();
            Assert.That(labels.Any(t => t.Contains("完整特性用例")), Is.True, "窗口头部未渲染");

            // DamageProfile 枚举按钮组（Burst 为默认值）——普通 Button 网格
            var burst = root.Query<UnityEngine.UIElements.Button>().Where(b => b.text == "Burst").ToList();
            Assert.That(burst.Count, Is.GreaterThan(0), "伤害模式枚举按钮组未渲染");

            // 防溢出样式：网格可换行
            Assert.That(burst[0].parent.style.flexWrap.value, Is.EqualTo(Wrap.Wrap));

            w.Close();
            yield break;
        }

        [TearDown]
        public void TearDown()
        {
            // 窗口复用是断言的一部分，测试后关闭避免跨用例串扰
            if (EditorWindow.HasOpenInstances<GameCharacterConfigWindow>())
                EditorWindow.GetWindow<GameCharacterConfigWindow>().Close();
            if (EditorWindow.HasOpenInstances<PropertyGridExample>())
                EditorWindow.GetWindow<PropertyGridExample>().Close();
        }
    }
}
