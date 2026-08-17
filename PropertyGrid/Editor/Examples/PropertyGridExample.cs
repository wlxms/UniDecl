using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UniDecl.BuiltIn.Editor.Snapshot;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.BuiltIn.Runtime.Snapshot;
using UniDecl.BuiltIn.Runtime.Widgets;
using UniDecl.BuiltIn.Runtime.Widgets.MD;
using UniDecl.Editor.UIToolKit;
using UniDecl.PropertyGrid.Editor;
using IAttr = UniDecl.PropertyGrid.Runtime;

namespace UniDecl.PropertyGrid.Editor.Examples
{
    public enum ShowcaseRole
    {
        Warrior,
        Ranger,
        Support,
    }

    public enum DamageProfile
    {
        Burst,
        Sustain,
        Precision,
    }

    // =========================================================================
    // 嵌套对象：验证嵌套 PropertyGrid + 独立 Renderer
    // =========================================================================

    [IAttr.PropertyGridLabel("@NestedSummary", Order = -10)]
    [IAttr.BoxGroup("Core", Title = "嵌套调优")]
    [Serializable]
    public class CombatTuning
    {
        public string NestedSummary => $"当前连击倍率: {comboMultiplier:F1}";

        [IAttr.BoxGroup("Core")]
        [IAttr.LabelText("连击倍率")]
        [IAttr.Range(1f, 4f)]
        public float comboMultiplier = 1.5f;

        [IAttr.BoxGroup("Core")]
        [IAttr.LabelText("允许暴击")]
        public bool allowCritical = true;

        [IAttr.BoxGroup("Core")]
        [IAttr.Dropdown("GetAimModeChoices")]
        [IAttr.LabelText("锁定策略")]
        public string aimMode = "Boss";

        [IAttr.BoxGroup("Core")]
        [IAttr.EnumToggleButtons]
        [IAttr.LabelText("伤害模式")]
        public DamageProfile damageProfile = DamageProfile.Burst;

        [IAttr.BoxGroup("Core")]
        [IAttr.TextArea(2, 4)]
        [IAttr.LabelText("调优备注")]
        public string notes = "嵌套对象用于验证 PropertyGridElement 递归展开。";
    }

    [IAttr.PropertyGridRenderer(typeof(CombatTuning))]
    public class CombatTuningRenderer
    {
        public string[] GetAimModeChoices() => new[] { "Boss", "Nearest", "LowestHP", "Manual" };
    }

    // =========================================================================
    // 主示例数据：覆盖已接通的 PropertyGrid 特性
    // =========================================================================

    /// <summary>
    /// 完整 PropertyGrid 用例——覆盖当前已接通的布局、条件、回调、嵌套与渲染能力
    /// </summary>
    [IAttr.PropertyGridLabel("@Summary", Order = -300)]
    [IAttr.PropertyGridInfoBox("@PropertyGridHint", Order = -290)]
    [IAttr.Button("应用传说预设", "ApplyLegendaryPreset", Order = -280)]
    [IAttr.Button("应用支援预设", "ApplySupportPreset", Order = -270)]
    [IAttr.Button("切换调试模式", "ToggleDebug", Order = -260)]
    [IAttr.BoxGroup("Identity", Title = "基础信息")]
    [IAttr.BoxGroup("Combat", Title = "交互链路验证")]
    [IAttr.FoldoutGroup("Advanced", Title = "条件与动作")]
    [IAttr.FoldoutGroup("Nested", Title = "嵌套对象")]
    [IAttr.FoldoutGroup("RenderCoverage", Title = "渲染覆盖（当前以展示为主）")]
    [Serializable]
    public class PropertyGridShowcase
    {
        public string Summary => $"当前装备: {itemName} / 稀有度: {rarity} / 职业: {role}";

        public string PropertyGridHint => showDebug
            ? "调试链路已开启：可见调试标签，禁用原因字段会被隐藏。"
            : "调试链路已关闭：可点击顶部按钮切换预设，并验证 ShowIf / HideIf。";

        public string SpeedLabel => showDebug ? "速度（调试）" : "速度";

        [IAttr.BoxGroup("Identity")]
        [IAttr.LabelText("装备名")]
        public string itemName = "Excalibur";

        [IAttr.BoxGroup("Identity")]
        [IAttr.LabelText("描述")]
        [IAttr.TextArea(2, 5)]
        public string description = "一把传说中的圣剑";

        [IAttr.BoxGroup("Identity")]
        [IAttr.Dropdown("GetRarityList")]
        [IAttr.LabelText("稀有度")]
        public string rarity = "Legendary";

        [IAttr.BoxGroup("Identity")]
        [IAttr.Dropdown("GetDifficultyList")]
        [IAttr.LabelText("难度索引")]
        public int difficultyIndex = 2;

        [IAttr.BoxGroup("Identity")]
        [IAttr.LabelText("主职业")]
        public ShowcaseRole role = ShowcaseRole.Warrior;

        [IAttr.BoxGroup("Combat")]
        [IAttr.HGroup("Combat/Row")]
        [IAttr.LabelText("攻击")]
        [IAttr.Range(0f, 100f)]
        public float attack = 80f;

        [IAttr.HGroup("Combat/Row")]
        [IAttr.LabelText("防御")]
        [IAttr.Range(0f, 100f)]
        public float defense = 55f;

        [IAttr.HGroup("Combat/Row")]
        [IAttr.LabelText("@SpeedLabel")]
        [IAttr.Range(0f, 100f)]
        public float speed = 40f;

        [IAttr.BoxGroup("Combat")]
        [IAttr.LabelText("暴击区间")]
        [IAttr.MinMaxSlider(0f, 100f)]
        public Vector2 critRange = new Vector2(10f, 35f);

        [IAttr.BoxGroup("Combat")]
        [IAttr.EnumToggleButtons]
        [IAttr.LabelText("输出模式")]
        public DamageProfile profile = DamageProfile.Burst;

        [IAttr.BoxGroup("Combat")]
        [IAttr.LabelText("显示调试字段")]
        public bool showDebug = false;

        [IAttr.FoldoutGroup("Advanced")]
        [IAttr.ShowIf("showDebug")]
        [IAttr.LabelText("调试ID")]
        public string debugId = "DEBUG_001";

        [IAttr.FoldoutGroup("Advanced")]
        [IAttr.HideIf("showDebug")]
        [IAttr.TextArea(2, 4)]
        [IAttr.LabelText("禁用原因")]
        public string disabledReason = "默认关闭调试模式时显示该字段。";

        [IAttr.FoldoutGroup("Advanced")]
        [IAttr.Button("生成调试标签", "GenerateDebugTag")]
        public int _generateDebugTagButton;

        [IAttr.FoldoutGroup("Nested")]
        [IAttr.LabelText("战斗调优")]
        public CombatTuning tuning = new CombatTuning();

        [IAttr.FoldoutGroup("RenderCoverage")]
        [IAttr.LabelText("主题色")]
        public Color themeColor = new Color(0.25f, 0.65f, 0.95f, 1f);

        [IAttr.FoldoutGroup("RenderCoverage")]
        [IAttr.LabelText("出生偏移")]
        public Vector3 spawnOffset = new Vector3(2f, 1f, -3f);

        [IAttr.FoldoutGroup("RenderCoverage")]
        [IAttr.LabelText("伤害曲线")]
        public AnimationCurve damageCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [IAttr.FoldoutGroup("RenderCoverage")]
        [IAttr.LabelText("预览材质")]
        public Material previewMaterial;

        [IAttr.FoldoutGroup("RenderCoverage")]
        [IAttr.LabelText("关键词数组")]
        public string[] keywords = { "boss", "holy", "quest" };

        [IAttr.FoldoutGroup("RenderCoverage")]
        [IAttr.LabelText("关键词列表")]
        public List<string> keywordList = new List<string> { "support", "burst", "melee" };
    }

    // =========================================================================
    // 示例 Renderer：提供 Dropdown 选项和 Button 回调
    // =========================================================================

    /// <summary>
    /// 主示例 Renderer——纯 Editor 侧，不参与发布构建
    /// </summary>
    [IAttr.PropertyGridRenderer(typeof(PropertyGridShowcase))]
    public class PropertyGridShowcaseRenderer
    {
        public string[] GetRarityList() => new[] { "Common", "Uncommon", "Rare", "Epic", "Legendary" };

        public string[] GetDifficultyList() => new[] { "Story", "Normal", "Hard", "Nightmare" };

        public void ApplyLegendaryPreset(PropertyGridShowcase target)
        {
            target.itemName = "Excalibur";
            target.description = "传说中的圣剑，适合验证 PropertyGrid 的完整链路。";
            target.rarity = "Legendary";
            target.difficultyIndex = 3;
            target.role = ShowcaseRole.Warrior;
            target.attack = 90f;
            target.defense = 60f;
            target.speed = 45f;
            target.critRange = new Vector2(20f, 50f);
            target.profile = DamageProfile.Burst;
        }

        public void ApplySupportPreset(PropertyGridShowcase target)
        {
            target.itemName = "Mercy Staff";
            target.description = "偏支援型配置，用于验证按钮回调是否可一次性改动多字段。";
            target.rarity = "Epic";
            target.difficultyIndex = 1;
            target.role = ShowcaseRole.Support;
            target.attack = 35f;
            target.defense = 70f;
            target.speed = 55f;
            target.critRange = new Vector2(5f, 20f);
            target.profile = DamageProfile.Sustain;
        }

        public void ToggleDebug(PropertyGridShowcase target)
        {
            target.showDebug = !target.showDebug;
            if (target.showDebug && string.IsNullOrEmpty(target.debugId))
                target.debugId = "DEBUG_INIT";
        }

        public void GenerateDebugTag(PropertyGridShowcase target)
        {
            target.showDebug = true;
            target.debugId = $"DBG-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant()}";
        }
    }

    // =========================================================================
    // 示例窗口
    // =========================================================================

    /// <summary>
    /// PropertyGrid 完整特性示例窗口
    /// 菜单: Window > UniDecl > PropertyGrid Example / PropertyGrid Feature Showcase
    ///
    /// 演示：
    /// 1. 纯数据类 + 属性标记 → 自动生成 PropertyGrid UI
    /// 2. Renderer 提供下拉选项和按钮回调
    /// 3. 条件控制（ShowIf/HideIf）
    /// 4. 布局分组（BoxGroup/FoldoutGroup）
    /// 5. 数值约束（Range/MinMaxSlider）
    /// </summary>
    public class PropertyGridExample : EditorWindow
    {
        private UIToolkitRenderManager _manager;
        private PropertyGridShowcase _config;

        [MenuItem("Window/UniDecl/PropertyGrid Example")]
        [MenuItem("Window/UniDecl/PropertyGrid Feature Showcase")]
        public static void ShowWindow()
        {
            GetWindow<PropertyGridExample>("PropertyGrid Feature Showcase");
        }

        public void CreateGUI()
        {
            _config = new PropertyGridShowcase();

            // 注入 EditorSnapshotManager：PropertyGrid 的 Undo/Redo 由 Host 自动接线
            _manager = new UIToolkitRenderManager(new EditorSnapshotManager(new SnapshotManager()));
            var root = new Panel
            {
                new VerticalLayout
                {
                    new Label("PropertyGrid 完整特性用例"),
                    new Label("本用例覆盖：类级标签/信息框/按钮、布局组、Dropdown、按钮替换、ShowIf/HideIf、嵌套对象与常见字段类型。"),
                    new Label("说明：渲染覆盖区主要用于验证控件可见性，核心交互链路集中在“基础信息 / 交互链路验证 / 条件与动作 / 嵌套对象”。"),
                    new Divider(),
                    new ScrollView
                    {
                        new PropertyGridElement(_config).WithKey("feature_showcase_propertygrid"),
                    },
                },
            };

            var ve = _manager.RenderRoot(root);
            if (ve != null)
                rootVisualElement.Add(ve);
        }

        private void OnDestroy()
        {
            _manager = null;
        }
    }
}
