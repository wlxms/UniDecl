using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UniDecl.Core;
using UniDecl.PropertyGrid.Editor.Examples;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using W = UniDecl.Widgets;

namespace UniDecl.PropertyGrid.Editor.Tests
{
    /// <summary>
    /// GameCharacterConfig 全交互链路 E2E——对窗口内真实 VisualElement 执行操作
    /// （切页、折叠、勾选、滑条、文本、按钮、枚举按钮），延迟多帧后校验：
    /// 数据状态、UI 状态、页签选择稳定性（任何交互都可能触发根级整树重建）。
    /// </summary>
    public class GameCharacterConfigInteractionTests
    {
        static readonly string[] ExpectedTabs = { "📋 基础", "⚔ 装备", "🧠 AI", "🎨 外观", "🔧 调试" };

        GameCharacterConfigWindow _w;

        [SetUp]
        public void SetUp()
        {
            Elements.TabSelectionMemory.Clear();
            _w = EditorWindow.GetWindow<GameCharacterConfigWindow>();
            _w.Repaint();
        }

        [TearDown]
        public void TearDown()
        {
            _w?.Close();
            if (EditorWindow.HasOpenInstances<GameCharacterConfigWindow>())
                EditorWindow.GetWindow<GameCharacterConfigWindow>().Close();
        }

        // ------------------------------------------------------------------
        // 辅助
        // ------------------------------------------------------------------

        static GameCharacterConfig ConfigOf(GameCharacterConfigWindow w) =>
            (GameCharacterConfig)typeof(GameCharacterConfigWindow)
                .GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(w);

        static VisualElement Root(GameCharacterConfigWindow w) => w.rootVisualElement;

        static System.Collections.Generic.List<ToolbarToggle> TabToggles(VisualElement root) =>
            root.Query<ToolbarToggle>().Where(t => t.parent is Toolbar).ToList();

        static IEnumerator SwitchTab(VisualElement root, int index)
        {
            TabToggles(root)[index].value = true;
            yield return null;
            yield return null;
        }

        /// <summary>label 所在字段行（label.parent）内第一个指定类型控件。</summary>
        static T EditorInLabelRow<T>(VisualElement root, string labelText) where T : VisualElement
        {
            var label = root.Query<Label>().Where(l => l.text == labelText).ToList().FirstOrDefault();
            if (label?.parent == null) return null;
            return label.parent.Query<T>().ToList().FirstOrDefault();
        }

        /// <summary>
        /// 触发窗口 DOM 中同名声明式按钮的 OnClick——覆盖 widget→Renderer 回调→数据→
        /// FieldChanged 刷新的完整链路（Unity 内部的指针命中分发是引擎自身机制，不在此重放）。
        /// </summary>
        static void ClickWidgetButton(GameCharacterConfigWindow w, string text)
        {
            var btn = FindWidget<W.Button>(w, b => b.Text == text);
            btn.OnClick();
        }

        /// <summary>触发声明式枚举按钮组选择指定枚举名。</summary>
        static void SelectEnumToggle(GameCharacterConfigWindow w, string enumTypeName, string enumName)
        {
            var group = FindWidget<W.EnumToggleGroup>(w, g => g.EnumType.Name == enumTypeName);
            var index = System.Array.IndexOf(group.GetChoiceNames(), enumName);
            group.OnValueChanged(group.ValueAtIndex(index));
        }

        /// <summary>在窗口活跃 DOM 树中查找声明式元素（typed 树为宿主实际使用的树）。</summary>
        static T FindWidget<T>(GameCharacterConfigWindow w, System.Func<T, bool> match) where T : class, IElement
        {
            var mgr = GetField(w, "_manager");
            var tree = (DOMTree)FindFieldDeep(mgr.GetType(), "_typedDomTree", "_domTree").GetValue(mgr);
            return tree.AllNodes.Select(n => n.Element).OfType<T>().First(match);
        }

        static object GetField(object obj, string name) =>
            FindFieldDeep(obj.GetType(), name).GetValue(obj);

        static FieldInfo FindFieldDeep(Type type, params string[] names)
        {
            for (var t = type; t != null; t = t.BaseType)
                foreach (var name in names)
                {
                    var f = t.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    if (f != null) return f;
                }
            throw new System.MissingFieldException(type.Name, string.Join("/", names));
        }

        IEnumerator WaitForRebuild(int frames = 3)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return null;
                FlushPendingRebuilds();
            }
        }

        /// <summary>
        /// EditMode 测试的 yield 不驱动 EditorApplication.delayCall，
        /// 手动冲刷渲染宿主的待重建队列（真实编辑器中该队列每帧自动执行）。
        /// </summary>
        void FlushPendingRebuilds()
        {
            var mgr = GetField(_w, "_manager");
            for (var t = mgr.GetType(); t != null; t = t.BaseType)
            {
                var m = t.GetMethod("FlushPendingRebuilds",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (m != null) { m.Invoke(mgr, null); return; }
            }
        }

        // ------------------------------------------------------------------
        // P0：页签选择跨整树重建稳定（用户报告：折叠后跳回第一页）
        // ------------------------------------------------------------------

        [UnityTest]
        public IEnumerator Interaction_FoldoutToggle_DoesNotResetTabSelection()
        {
            var root = Root(_w);
            var cfg = ConfigOf(_w);

            yield return SwitchTab(root, 4); // 🔧 调试
            Assert.That(TabToggles(root)[4].value, Is.True);

            // 折叠“运行时统计”——该交互触发根级重建（修复前页签会跳回基础页）
            var stats = root.Query<UnityEngine.UIElements.Foldout>().Where(f => f.text == "运行时统计").ToList().First();
            stats.value = false;
            yield return WaitForRebuild(4);

            Assert.That(TabToggles(root)[4].value, Is.True, "折叠后页签跳回第一页");
            Assert.That(root.Query<Label>().Where(l => l.text == "校验面板").ToList().Count, Is.EqualTo(1),
                "调试页内容丢失");
            Assert.That(stats.value, Is.False, "折叠状态未保持");
        }

        [UnityTest]
        public IEnumerator Interaction_ToggleAndDropdown_KeepTabAndState()
        {
            var root = Root(_w);
            var cfg = ConfigOf(_w);

            yield return SwitchTab(root, 4); // 🔧 调试
            var debugToggle = EditorInLabelRow<Toggle>(root, "启用调试日志");
            Assert.That(debugToggle, Is.Not.Null, "调试开关未找到");
            Assert.That(cfg.enableDebugLog, Is.False);

            debugToggle.value = true;
            yield return WaitForRebuild(4);

            // 数据 + UI 双侧状态保持；ShowIf 字段出现
            Assert.That(cfg.enableDebugLog, Is.True, "开关数据未写入");
            Assert.That(debugToggle.value, Is.True);
            Assert.That(root.Query<Label>().Where(l => l.text == "日志级别").ToList().Count, Is.EqualTo(1),
                "ShowIf 依赖字段未出现");
            Assert.That(TabToggles(root)[4].value, Is.True, "开关操作后页签跳回");

            // 关闭后 ShowIf 字段隐藏
            debugToggle.value = false;
            yield return WaitForRebuild(3);
            Assert.That(root.Query<Label>().Where(l => l.text == "日志级别").ToList().Count, Is.EqualTo(0));
        }

        // ------------------------------------------------------------------
        // 校验面板：字段级按钮触发导出（一次性动作用 Button 而非 Toggle+复位）
        // ------------------------------------------------------------------

        [UnityTest]
        public IEnumerator Interaction_ExportButton_TriggersExport()
        {
            var root = Root(_w);

            yield return SwitchTab(root, 4); // 🔧 调试

            // 按钮以字段级 Button 形态渲染
            var exportBtn = root.Query<UnityEngine.UIElements.Button>()
                .Where(b => b.text == "📤 导出配置").ToList().FirstOrDefault();
            Assert.That(exportBtn, Is.Not.Null, "导出按钮未渲染");

            LogAssert.Expect(LogType.Log, new Regex(@"配置已导出"));
            ClickWidgetButton(_w, "📤 导出配置");
            yield return WaitForRebuild(4);

            Assert.That(TabToggles(root)[4].value, Is.True, "按钮点击后页签跳回");

            // 清理导出的 JSON 副产物
            foreach (var f in Directory.GetFiles(Application.dataPath, "CharacterConfig_*.json"))
                File.Delete(f);
        }

        // ------------------------------------------------------------------
        // 类级按钮：数据变更 → @ 标题/编辑器刷新
        // ------------------------------------------------------------------

        [UnityTest]
        public IEnumerator Interaction_RandomizeButton_RefreshesHeaderAndEditors()
        {
            var root = Root(_w);
            var cfg = ConfigOf(_w);
            var oldTitle = root.Query<Label>().Where(l => l.text.StartsWith("【")).ToList().First().text;

            var randomBtn = root.Query<UnityEngine.UIElements.Button>()
                .Where(b => b.text == "🎲 随机生成").ToList().First();
            Assert.That(randomBtn, Is.Not.Null);
            ClickWidgetButton(_w, "🎲 随机生成");
            yield return WaitForRebuild(5);

            var newTitle = root.Query<Label>().Where(l => l.text.StartsWith("【")).ToList().First().text;
            Assert.That(newTitle, Is.Not.EqualTo(oldTitle), "随机生成后标题未刷新");
            Assert.That(newTitle, Is.EqualTo($"【{cfg.charClass}】{cfg.characterName} - Lv.{cfg.level}"),
                "标题与数据不一致");

            // 编辑器值同步：重建后重新查询等级滑条，与数据一致
            yield return WaitForRebuild(3);
            var levelSlider = EditorInLabelRow<UnityEngine.UIElements.Slider>(root, "等级");
            Assert.That(levelSlider, Is.Not.Null);
            Assert.That((int)levelSlider.value, Is.EqualTo(cfg.level), "滑条未随数据刷新");
        }

        // ------------------------------------------------------------------
        // 枚举按钮组：点击 → 数据 + 高亮保持，且不丢页签
        // ------------------------------------------------------------------

        [UnityTest]
        public IEnumerator Interaction_EnumButton_ClickUpdatesDataAndHighlight()
        {
            var root = Root(_w);
            var cfg = ConfigOf(_w);

            yield return SwitchTab(root, 2); // 🧠 AI
            var aggressive = root.Query<UnityEngine.UIElements.Button>()
                .Where(b => b.text == "Aggressive").ToList().First();
            Assert.That(aggressive, Is.Not.Null);
            SelectEnumToggle(_w, nameof(AIPersonality), "Aggressive");
            yield return WaitForRebuild(4);

            Assert.That(cfg.ai.personality, Is.EqualTo(AIPersonality.Aggressive), "枚举按钮点击未写入数据");
            Assert.That(TabToggles(root)[2].value, Is.True, "枚举点击后页签跳回");

            // 选中高亮保持（重建后新按钮仍标记选中态，与渲染器选中色一致）
            var sel = root.Query<UnityEngine.UIElements.Button>()
                .Where(b => b.text == "Aggressive").ToList().First();
            var expected = new Color(0.24f, 0.44f, 0.9f);
            var bg = sel.style.backgroundColor.value;
            Assert.That(Mathf.Abs(bg.r - expected.r) < 0.02f && Mathf.Abs(bg.g - expected.g) < 0.02f && Mathf.Abs(bg.b - expected.b) < 0.02f,
                Is.True, $"选中按钮缺少高亮，实际色 {bg}");
        }

        // ------------------------------------------------------------------
        // 滑条 / 文本：编辑 → 数据回写，UI 稳定
        // ------------------------------------------------------------------

        [UnityTest]
        public IEnumerator Interaction_SliderAndTextField_WriteBackToData()
        {
            var root = Root(_w);
            var cfg = ConfigOf(_w);

            yield return SwitchTab(root, 0); // 📋 基础
            var hpSlider = EditorInLabelRow<UnityEngine.UIElements.Slider>(root, "生命值");
            Assert.That(hpSlider, Is.Not.Null);
            // 经 widget 层触发滑条变更（等价拖动的完整下游：吸附→回写→刷新）
            FindWidget<W.Slider>(_w, x => x.Key == "insp_maxHP").OnValueChanged(2000f);
            yield return WaitForRebuild(5);
            // maxHP 带 [Step(10)]、min=1：2000 吸附到 1+200×10=2001——吸附语义包含在断言内
            Assert.That(cfg.maxHP, Is.EqualTo(2001f).Within(0.5f), "滑条未回写数据（或步进吸附失效）");
            // 重建后重新查询当前滑条实例，验证显示与数据一致
            var hpAfter = EditorInLabelRow<UnityEngine.UIElements.Slider>(root, "生命值");
            Assert.That(hpAfter.value, Is.EqualTo(2001f), "滑条显示未与吸附后数据对齐");

            var nameField = EditorInLabelRow<TextField>(root, "角色名称");
            Assert.That(nameField, Is.Not.Null);
            FindWidget<W.TextField>(_w, x => x.Key == "insp_characterName").OnValueChange("测试英雄", "");
            yield return WaitForRebuild(5);
            Assert.That(cfg.characterName, Is.EqualTo("测试英雄"), "文本未回写数据");
            Assert.That(TabToggles(root)[0].value, Is.True);
        }

        // ------------------------------------------------------------------
        // 列表增删（背包收藏物品）
        // ------------------------------------------------------------------

        [UnityTest]
        public IEnumerator Interaction_ListRemoveButton_DecrementsItems()
        {
            var root = Root(_w);
            var cfg = ConfigOf(_w);

            yield return SwitchTab(root, 1); // ⚔ 装备
            var before = cfg.favoriteItems.Count;
            Assert.That(before, Is.GreaterThan(0));

            // 收藏物品区第一个删除按钮（文本 ✕）
            var removeBtn = root.Query<UnityEngine.UIElements.Button>()
                .Where(b => b.text == "✕").ToList().FirstOrDefault();
            Assert.That(removeBtn, Is.Not.Null, "列表删除按钮未渲染");
            ClickWidgetButton(_w, "✕");
            yield return WaitForRebuild(5);

            Assert.That(cfg.favoriteItems.Count, Is.EqualTo(before - 1), "列表删除未生效");
            Assert.That(TabToggles(root)[1].value, Is.True);
        }

        // ------------------------------------------------------------------
        // 色板：点击色块 → 数据与颜色控件显示同步刷新
        // ------------------------------------------------------------------

        [UnityTest]
        public IEnumerator Interaction_ColorPaletteSwatch_UpdatesDataAndField()
        {
            var root = Root(_w);
            var cfg = ConfigOf(_w);

            yield return SwitchTab(root, 3); // 🎨 外观
            var expected = new Color(0.38f, 0.26f, 0.18f); // CharacterSkin 色板第 5 色（与默认主色调不同）
            Assert.That(cfg.appearance.primaryColor, Is.Not.EqualTo(expected), "前置：主色调非目标色");

            FindWidget<W.Button>(_w, b => b.Key == "insp_primaryColor_palette_4").OnClick();
            yield return WaitForRebuild(4);

            Assert.That(cfg.appearance.primaryColor, Is.EqualTo(expected).Within(0.002f), "色块点击未写入数据");
            var colorField = EditorInLabelRow<ColorField>(root, "主色调");
            Assert.That(colorField, Is.Not.Null);
            Assert.That(colorField.value, Is.EqualTo(expected).Within(0.002f), "颜色控件显示未随色块刷新");
            Assert.That(TabToggles(root)[3].value, Is.True);
        }

        // ------------------------------------------------------------------
        // 横向滚动：内容不得宽于视口（批处理下布局可能未计算，结构断言兜底在 dump 校验）
        // ------------------------------------------------------------------

        [UnityTest]
        public IEnumerator Interaction_NoHorizontalOverflow()
        {
            var root = Root(_w);
            foreach (var i in new[] { 0, 1, 2, 3, 4 })
            {
                yield return SwitchTab(root, i);
                var sv = root.Query<ScrollView>().First();
                var cw = sv.contentContainer.resolvedStyle.width;
                var vw = sv.contentViewport.resolvedStyle.width;
                if (cw > 0 && vw > 0)
                    Assert.That(cw, Is.LessThanOrEqualTo(vw + 1f),
                        $"页 {ExpectedTabs[i]} 内容宽于视口（无内容横向滚动条）");
                Assert.That(sv.horizontalScroller.resolvedStyle.display, Is.EqualTo(DisplayStyle.None),
                    $"页 {ExpectedTabs[i]} 横向滚动条应隐藏（无内容可横向滚动）");
            }
        }
    }
}
