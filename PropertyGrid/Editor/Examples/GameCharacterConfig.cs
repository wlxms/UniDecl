using System;
using System.Collections.Generic;
using System.Linq;
using UniDecl.BuiltIn.Editor.Snapshot;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.BuiltIn.Runtime.Snapshot;
using UniDecl.BuiltIn.Runtime.Widgets;
using UniDecl.Editor.UIToolKit;
using UniDecl.PropertyGrid.Editor;
using UniDecl.PropertyGrid.Runtime;
using UnityEditor;
using UnityEngine;
using IAttr = UniDecl.PropertyGrid.Runtime;

namespace UniDecl.PropertyGrid.Editor.Examples
{
    // =========================================================================
    // 枚举定义
    // =========================================================================

    public enum CharClass
    {
        Warrior,
        Mage,
        Ranger,
        Rogue,
        Paladin,
    }

    public enum WeaponType
    {
        Sword,
        Dagger,
        Bow,
        Staff,
        Shield,
        Greatsword,
    }

    public enum ArmorMaterial
    {
        Cloth,
        Leather,
        Chainmail,
        Plate,
        Magical,
    }

    public enum ModifierTarget
    {
        Attack,
        Defense,
        Speed,
        CritRate,
        CritDamage,
        Health,
        Mana,
    }

    public enum DamageType
    {
        Physical,
        Fire,
        Ice,
        Lightning,
        Holy,
        Dark,
    }

    public enum AIPersonality
    {
        Aggressive,
        Defensive,
        Balanced,
        Support,
        Coward,
    }

    public enum RarityGrade
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythic,
    }

    public enum ElementReaction
    {
        None,
        Vaporize,
        Melt,
        Overload,
        Superconduct,
        Freeze,
    }

    // =========================================================================
    // 嵌套对象链 #1: 装备修饰器（三级深度）
    // =========================================================================

    /// <summary>
    /// 单条修饰器——三级嵌套的最深层
    /// </summary>
    [Serializable]
    public class ModifierEntry
    {
        [IAttr.LabelText("目标属性")]
        public ModifierTarget target = ModifierTarget.Attack;

        [IAttr.LabelText("数值")]
        [IAttr.Range(-100f, 500f)]
        public float value = 10f;

        [IAttr.LabelText("百分比模式")]
        [IAttr.Tooltip("勾选后数值视为百分比加成")]
        public bool isPercent = false;

        [IAttr.LabelText("生效条件")]
        [IAttr.HideIf("isAlwaysActive")]
        [IAttr.Dropdown("GetConditionOptions")]
        public string condition = "Always";

        [IAttr.ShowIf("isAlwaysActive")]
        [IAttr.HideLabel]
        [IAttr.ReadOnly]
        [IAttr.LabelText(" ")]
        public string conditionHint = "✓ 始终生效";

        [IAttr.HideLabel]
        public bool isAlwaysActive = true;

        [IAttr.LabelText("持续时间")]
        [IAttr.Range(0f, 60f)]
        public float duration = 0f;

        [IAttr.ShowIf("duration", 0f)]
        [IAttr.HideLabel]
        [IAttr.ReadOnly]
        public string durationHint = "∞ 永久生效";

        [IAttr.LabelText("堆叠层数上限")]
        [IAttr.Range(1, 99)]
        [IAttr.Step(1)]
        public int maxStacks = 1;

        [IAttr.LabelText("标签")]
        public string[] tags = Array.Empty<string>();
    }

    [Serializable]
    public class FloatRangeConfig
    {
        [IAttr.LabelText("最小值")]
        [IAttr.Range(-1000f, 1000f)]
        public float min = 0f;

        [IAttr.LabelText("最大值")]
        [IAttr.Range(-1000f, 1000f)]
        public float max = 100f;
    }

    /// <summary>
    /// 装备修饰器列表——二级嵌套
    [Serializable]
    public class EquipmentModifiers
    {
        [IAttr.LabelText("主修饰器")]
        [IAttr.Tooltip("装备的主要属性加成")]
        public ModifierEntry primary = new ModifierEntry
        {
            target = ModifierTarget.Attack,
            value = 50f,
            isAlwaysActive = true,
        };

        [IAttr.LabelText("副修饰器")]
        [IAttr.Tooltip("装备的次要属性加成")]
        public ModifierEntry secondary = new ModifierEntry
        {
            target = ModifierTarget.Defense,
            value = 20f,
            isAlwaysActive = true,
        };

        [IAttr.LabelText("额外修饰器列表")]
        [IAttr.Space(Before = 4)]
        public List<ModifierEntry> extras = new List<ModifierEntry>();

        [IAttr.HideLabel]
        [IAttr.ReadOnly]
        [IAttr.Space(After = 4)]
        public string totalTip;
    }

    // =========================================================================
    // 嵌套对象链 #2: 装备（二级深度）
    // =========================================================================

    /// <summary>
    /// 装备基类——被 CharacterEquipment 引用
    /// </summary>
    [Serializable]
    public class WeaponConfig
    {
        [IAttr.LabelText("武器名称")]
        public string weaponName = "铁剑";

        [IAttr.LabelText("武器类型")]
        public WeaponType type = WeaponType.Sword;

        [IAttr.LabelText("稀有度")]
        public RarityGrade rarity = RarityGrade.Common;

        [IAttr.HGroup("WeaponStats")]
        [IAttr.LabelText("基础攻击")]
        [IAttr.Range(1f, 999f)]
        public float baseAttack = 10f;

        [IAttr.HGroup("WeaponStats")]
        [IAttr.LabelText("攻击速度")]
        [IAttr.Range(0.1f, 5f)]
        [IAttr.Step(0.1)]
        public float attackSpeed = 1.0f;

        [IAttr.HGroup("WeaponStats")]
        [IAttr.LabelText("射程")]
        [IAttr.Range(0f, 50f)]
        public float range = 2f;

        [IAttr.LabelText("伤害类型")]
        [IAttr.EnumToggleButtons]
        public DamageType damageType = DamageType.Physical;

        [IAttr.LabelText("元素反应")]
        [IAttr.HideIf("damageType", DamageType.Physical)]
        public ElementReaction reaction = ElementReaction.None;

        [IAttr.LabelText("装备修饰器")]
        public EquipmentModifiers modifiers = new EquipmentModifiers();
    }

    [Serializable]
    public class ArmorConfig
    {
        [IAttr.LabelText("护甲名称")]
        public string armorName = "布甲";

        [IAttr.LabelText("护甲材质")]
        public ArmorMaterial material = ArmorMaterial.Cloth;

        [IAttr.HGroup("ArmorStats")]
        [IAttr.LabelText("基础防御")]
        [IAttr.Range(1f, 500f)]
        public float baseDefense = 5f;

        [IAttr.HGroup("ArmorStats")]
        [IAttr.LabelText("重量")]
        [IAttr.Range(0f, 100f)]
        [IAttr.Tooltip("越重移动速度越慢")]
        public float weight = 2f;

        [IAttr.HGroup("ArmorStats")]
        [IAttr.LabelText("耐久度")]
        [IAttr.Range(0, 1000)]
        public int durability = 100;

        [IAttr.LabelText("可维修")]
        public bool repairable = true;

        [IAttr.LabelText("装备修饰器")]
        public EquipmentModifiers modifiers = new EquipmentModifiers();
    }

    [Serializable]
    public class AccessoryConfig
    {
        [IAttr.LabelText("饰品名称")]
        public string accessoryName = "普通戒指";

        [IAttr.LabelText("稀有度")]
        public RarityGrade rarity = RarityGrade.Common;

        [IAttr.LabelText("特效类型")]
        [IAttr.Dropdown("GetAccessoryEffectTypes")]
        public string effectType = "None";

        [IAttr.LabelText("特效数值")]
        [IAttr.Range(0f, 100f)]
        [IAttr.HideIf("effectType", "None")]
        public float effectValue = 0f;

        [IAttr.LabelText("冷却缩减")]
        [IAttr.Range(0f, 0.4f)]
        public float cooldownReduction = 0f;

        [IAttr.LabelText("唯一被动")]
        public bool uniquePassive = false;

        [IAttr.LabelText("被动描述")]
        [IAttr.ShowIf("uniquePassive")]
        [IAttr.TextArea(2, 4)]
        public string passiveDescription = "";
    }

    // =========================================================================
    // 嵌套对象链 #3: 角色装备（一级嵌套容器）
    // =========================================================================

    [Serializable]
    public class CharacterEquipment
    {
        [IAttr.FoldoutGroup("Weapon", Title = "武器配置")]
        public WeaponConfig weapon = new WeaponConfig();

        [IAttr.FoldoutGroup("Armor", Title = "护甲配置")]
        public ArmorConfig armor = new ArmorConfig();

        [IAttr.FoldoutGroup("Accessories", Title = "饰品配置")]
        public AccessoryConfig accessory1 = new AccessoryConfig();

        [IAttr.FoldoutGroup("Accessories")]
        public AccessoryConfig accessory2 = new AccessoryConfig();

        [IAttr.FoldoutGroup("Accessories")]
        [IAttr.LabelText("额外饰品")]
        [IAttr.Space(Before = 4)]
        public List<AccessoryConfig> extraAccessories = new List<AccessoryConfig>();
    }

    // =========================================================================
    // AI 行为配置（独立分支，演示 TabGroup）
    // =========================================================================

    [Serializable]
    public class AIConfig
    {
        [IAttr.LabelText("AI 人格")]
        [IAttr.EnumToggleButtons]
        public AIPersonality personality = AIPersonality.Balanced;

        [IAttr.LabelText("警戒范围")]
        [IAttr.MinMaxSlider(0f, 50f)]
        public Vector2 detectRange = new Vector2(5f, 20f);

        [IAttr.HGroup("AICombat")]
        [IAttr.LabelText("攻击偏好距离")]
        [IAttr.Range(1f, 30f)]
        public float preferredDistance = 5f;

        [IAttr.HGroup("AICombat")]
        [IAttr.LabelText("走位频率")]
        [IAttr.Range(0f, 10f)]
        [IAttr.Step(0.5)]
        public float repositionRate = 3f;

        [IAttr.HGroup("AITarget")]
        [IAttr.LabelText("优先目标类型")]
        [IAttr.Dropdown("GetTargetPriorityOptions")]
        public string targetPriority = "Nearest";

        [IAttr.HGroup("AITarget")]
        [IAttr.LabelText("仇恨衰减")]
        [IAttr.Range(0f, 1f)]
        public float threatDecay = 0.1f;

        [IAttr.LabelText("技能循环")]
        [IAttr.TextArea(3, 6)]
        [IAttr.Tooltip("每行一个技能名，按顺序释放")]
        public string skillRotation = "重击\n横扫\n防御姿态";

        [IAttr.LabelText("智能闪避")]
        [IAttr.EnableIf("hasDodgeSkill")]
        public bool enableDodge = true;

        [IAttr.LabelText("闪避冷却")]
        [IAttr.Range(0.5f, 10f)]
        [IAttr.ShowIf("enableDodge")]
        public float dodgeCooldown = 2f;

        [IAttr.ShowIf("enableDodge")]
        public float minDodgeCooldown = 0.5f;

        [IAttr.HideLabel]
        [IAttr.ReadOnly]
        public bool hasDodgeSkill = true;
    }

    // =========================================================================
    // 外观配置
    // =========================================================================

    [Serializable]
    public class AppearanceConfig
    {
        [IAttr.TabGroup("Colors")]
        [IAttr.LabelText("主色调")]
        [IAttr.ColorPalette("CharacterSkin")]
        public Color primaryColor = new Color(0.95f, 0.85f, 0.75f);

        [IAttr.TabGroup("Colors")]
        [IAttr.LabelText("副色调")]
        [IAttr.ColorPalette("CharacterSkin")]
        public Color secondaryColor = new Color(0.4f, 0.2f, 0.1f);

        [IAttr.TabGroup("Colors")]
        [IAttr.LabelText("发光色")]
        public Color glowColor = new Color(0.2f, 0.6f, 1.0f);

        [IAttr.TabGroup("Sizes")]
        [IAttr.LabelText("身高")]
        [IAttr.Range(0.5f, 2.5f)]
        [IAttr.Step(0.05)]
        public float height = 1.8f;

        [IAttr.TabGroup("Sizes")]
        [IAttr.LabelText("体型")]
        [IAttr.Range(0.3f, 2f)]
        public float bodyScale = 1f;

        [IAttr.TabGroup("Sizes")]
        [IAttr.LabelText("头占比")]
        [IAttr.Range(0.1f, 0.5f)]
        public float headRatio = 0.2f;

        [IAttr.TabGroup("Assets")]
        [IAttr.LabelText("模型预制体")]
        [IAttr.PreviewField(Height = 128)]
        public GameObject modelPrefab;

        [IAttr.TabGroup("Assets")]
        [IAttr.LabelText("头像图标")]
        [IAttr.PreviewField(Height = 64)]
        public Sprite portraitIcon;

        [IAttr.TabGroup("Assets")]
        [IAttr.LabelText("语音包路径")]
        [IAttr.FilePath(Extensions = "ogg,wav,mp3")]
        public string voicePackPath = "";

        [IAttr.TabGroup("Assets")]
        [IAttr.LabelText("特效资源文件夹")]
        [IAttr.FolderPath(Parent = "Assets/Effects")]
        public string effectFolder = "Assets/Effects/Common";
    }

    // =========================================================================
    // 主数据类 — 完整生产级角色配置
    // =========================================================================

    /// <summary>
    /// 生产级角色配置——覆盖 PropertyGrid 全部已接通特性：
    /// • 三级嵌套：Character → Equipment → ModifierEntry
    /// • TabGroup / FoldoutGroup / BoxGroup / HGroup / VGroup
    /// • InlineProperty 展开子对象字段
    /// • ShowIf / HideIf / EnableIf 条件控制
    /// • Dropdown / Validate / OnValueChanged
    /// • Range / MinMaxSlider / Step / Wrap 数值约束
    /// • PreviewField / FilePath / FolderPath 资源选择
    /// • EnumToggleButtons / ColorPalette / TextArea
    /// • FlexGrow / Width / AlignRight 布局对齐
    /// • 类级 Button / PropertyGridLabel / PropertyGridInfoBox
    /// • 字段级 Button（替换字段为按钮）
    /// • @ 表达式引用 Renderer 属性
    /// • List 字段
    /// • Custom Plugin（CardGroup）
    /// </summary>
    [IAttr.PropertyGridLabel("@CharacterTitle", Order = -1000)]
    [IAttr.PropertyGridInfoBox("@StatusHint", Type = InfoBoxType.Info, Order = -990)]
    [IAttr.Button("🎲 随机生成", "RandomizeCharacter", Order = -980)]
    [IAttr.Button("📋 导出配置", "ExportConfig", Order = -970)]
    [IAttr.Button("⚠ 恢复默认", "ResetToDefault", Order = -960)]
    [IAttr.TabGroup("Basic", Title = "📋 基础")]
    [IAttr.TabGroup("Equipment", Title = "⚔ 装备")]
    [IAttr.TabGroup("AI", Title = "🧠 AI")]
    [IAttr.TabGroup("Appearance", Title = "🎨 外观")]
    [IAttr.TabGroup("Debug", Title = "🔧 调试")]
    [Serializable]
    public class GameCharacterConfig
    {
        // ===================================================================
        // §1 基础标签页
        // ===================================================================

        [IAttr.TabGroup("Basic")]
        [IAttr.BoxGroup("Basic/Identity", Title = "身份信息")]
        [IAttr.LabelText("角色名称")]
        [IAttr.Tooltip("游戏内显示的角色名称")]
        public string characterName = "无名英雄";

        [IAttr.BoxGroup("Basic/Identity")]
        [IAttr.HideLabel]
        [IAttr.TextArea(2, 4)]
        [IAttr.LabelText("背景故事")]
        public string backstory = "一位勇敢的冒险者。";

        [IAttr.BoxGroup("Basic/Identity")]
        [IAttr.HGroup("Basic/Identity/ClassRow")]
        [IAttr.LabelText("职业")]
        [IAttr.FlexGrow(1)]
        public CharClass charClass = CharClass.Warrior;

        [IAttr.HGroup("Basic/Identity/ClassRow")]
        [IAttr.LabelText("等级")]
        [IAttr.Width(80)]
        [IAttr.Range(1, 99)]
        public int level = 1;

        [IAttr.BoxGroup("Basic/Attributes", Title = "基础属性")]
        [IAttr.HGroup("Basic/Attributes/Row1")]
        [IAttr.LabelText("生命值")]
        [IAttr.FlexGrow(2)]
        [IAttr.Range(1f, 99999f)]
        [IAttr.Step(10)]
        public float maxHP = 1000f;

        [IAttr.HGroup("Basic/Attributes/Row1")]
        [IAttr.LabelText("魔法值")]
        [IAttr.FlexGrow(2)]
        [IAttr.Range(0f, 99999f)]
        [IAttr.Step(5)]
        public float maxMP = 500f;

        [IAttr.BoxGroup("Basic/Attributes")]
        [IAttr.InlineProperty(StripOwnGroups = true)]
        public FloatRangeConfig validDamageRange = new FloatRangeConfig();

        [IAttr.BoxGroup("Basic/Attributes")]
        [IAttr.HGroup("Basic/Attributes/Row2")]
        [IAttr.LabelText("经验值")]
        [IAttr.FlexGrow(1)]
        [IAttr.Range(0f, 999999f)]
        [IAttr.Width(120)]
        public float experience = 0f;

        [IAttr.HGroup("Basic/Attributes/Row2")]
        [IAttr.LabelText("金币")]
        [IAttr.FlexGrow(1)]
        [IAttr.Range(0, 999999)]
        [IAttr.Width(100)]
        public int gold = 100;

        [IAttr.HGroup("Basic/Attributes/Row2")]
        [IAttr.LabelText("负重上限")]
        [IAttr.FlexGrow(1)]
        [IAttr.Range(0f, 9999f)]
        [IAttr.Width(100)]
        public float carryWeight = 500f;

        [IAttr.BoxGroup("Basic/Attributes")]
        [IAttr.VGroup("Basic/Attributes/VerticalStats")]
        [IAttr.LabelText("力量")]
        [IAttr.FlexGrow(1)]
        [IAttr.Range(1, 999)]
        public int strength = 10;

        [IAttr.VGroup("Basic/Attributes/VerticalStats")]
        [IAttr.LabelText("敏捷")]
        [IAttr.Range(1, 999)]
        public int dexterity = 10;

        [IAttr.VGroup("Basic/Attributes/VerticalStats")]
        [IAttr.LabelText("智力")]
        [IAttr.Range(1, 999)]
        public int intelligence = 10;

        [IAttr.VGroup("Basic/Attributes/VerticalStats")]
        [IAttr.LabelText("耐力")]
        [IAttr.Range(1, 999)]
        public int stamina = 10;

        // ===================================================================
        // §2 装备标签页（三级嵌套 + InlineProperty）
        // ===================================================================

        [IAttr.TabGroup("Equipment")]
        public CharacterEquipment equipment = new CharacterEquipment();

        [IAttr.TabGroup("Equipment")]
        [IAttr.BoxGroup("Equipment/Inventory", Title = "背包")]
        [IAttr.LabelText("快捷栏物品")]
        [IAttr.TextArea(2, 4)]
        [IAttr.Tooltip("每行一个物品 ID")]
        public string quickSlotItems = "HealthPotion\nManaPotion";

        [IAttr.BoxGroup("Equipment/Inventory")]
        [IAttr.LabelText("收藏物品")]
        public List<string> favoriteItems = new List<string> { "Sword_001", "Shield_003" };

        // ===================================================================
        // §3 AI 标签页
        // ===================================================================

        [IAttr.TabGroup("AI")]
        public AIConfig ai = new AIConfig();

        // ===================================================================
        // §4 外观标签页
        // ===================================================================

        [IAttr.TabGroup("Appearance")]
        public AppearanceConfig appearance = new AppearanceConfig();

        // ===================================================================
        // §5 调试标签页
        // ===================================================================

        [IAttr.TabGroup("Debug")]
        [IAttr.BoxGroup("Debug/General", Title = "开发选项")]
        [IAttr.LabelText("启用调试日志")]
        public bool enableDebugLog = false;

        [IAttr.TabGroup("Debug")]
        [IAttr.BoxGroup("Debug/General")]
        [IAttr.ShowIf("enableDebugLog")]
        [IAttr.Dropdown("GetLogLevelOptions")]
        [IAttr.LabelText("日志级别")]
        public string logLevel = "Info";

        [IAttr.TabGroup("Debug")]
        [IAttr.BoxGroup("Debug/General")]
        [IAttr.ShowIf("enableDebugLog")]
        [IAttr.TextArea(3, 8)]
        [IAttr.LabelText("作弊命令")]
        [IAttr.Tooltip("每行一个控制台命令")]
        public string cheatCommands = "";

        [IAttr.TabGroup("Debug")]
        [IAttr.BoxGroup("Debug/Validation", Title = "校验面板")]
        [IAttr.Validate("ValidateConfig")]
        [IAttr.HideLabel]
        [IAttr.LabelText("点击验证")]
        public string validationResult = "";

        [IAttr.TabGroup("Debug")]
        [IAttr.BoxGroup("Debug/Validation")]
        // 一次性动作用字段级 Button 表达（Toggle+复位的中转写法会让勾选"一闪而过"，交互形态错误）
        [IAttr.Button("📤 导出配置", "ExportConfig")]
        [IAttr.HideLabel]
        public int _exportButton;

        [IAttr.TabGroup("Debug")]
        [IAttr.BoxGroup("Debug/Stats", Title = "运行时统计")]
        [IAttr.LabelText("创建时间戳")]
        [IAttr.ReadOnly]
        public string createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        [IAttr.TabGroup("Debug")]
        [IAttr.BoxGroup("Debug/Stats")]
        [IAttr.LabelText("修改次数")]
        [IAttr.ReadOnly]
        public int editCount = 0;

        [IAttr.TabGroup("Debug")]
        [IAttr.BoxGroup("Debug/Stats")]
        [IAttr.LabelText("版本号")]
        [IAttr.ReadOnly]
        public string configVersion = "1.0.0";

        // ===================================================================
        // 字段级 Button（替换字段编辑器为按钮）
        // ===================================================================

        [IAttr.TabGroup("Debug")]
        [IAttr.BoxGroup("Debug/BulkOps", Title = "批量操作")]
        [IAttr.Button("清空背包", "ClearInventory")]
        [IAttr.HideLabel]
        public int _clearInventoryButton;

        [IAttr.TabGroup("Debug")]
        [IAttr.BoxGroup("Debug/BulkOps")]
        [IAttr.Button("重置所有装备", "ResetAllEquipment")]
        [IAttr.HideLabel]
        public int _resetEquipmentButton;

        [IAttr.TabGroup("Debug")]
        [IAttr.BoxGroup("Debug/BulkOps")]
        [IAttr.Button("重新计算属性", "RecalculateStats")]
        [IAttr.HideLabel]
        public int _recalcStatsButton;

        // ===================================================================
        // @ 表达式用属性（Renderer 会覆盖这些值）
        // ===================================================================
        public string CharacterTitle => $"【{charClass}】{characterName} - Lv.{level}";
        public string StatusHint => $"HP: {maxHP}  MP: {maxMP}  职业: {charClass}  装备评分: 待计算";
        public string ValidatedLabel => string.IsNullOrEmpty(validationResult)
            ? "点击校验按钮查看结果" : validationResult;
    }

    // =========================================================================
    // Renderer — 提供动态选项和回调
    // =========================================================================

    [IAttr.PropertyGridRenderer(typeof(GameCharacterConfig))]
    public class GameCharacterConfigRenderer
    {
        // ---- Dropdown 选项 ----
        public string[] GetConditionOptions() => new[]
        {
            "Always", "HealthBelow50", "HealthBelow30",
            "ManaAbove80", "EnemyCountAbove3", "BossBattle",
        };

        public string[] GetAccessoryEffectTypes() => new[]
        {
            "None", "LifeSteal", "ManaRegen", "Thorns",
            "SlowResist", "StunResist", "ExpBoost", "GoldBoost",
        };

        public string[] GetTargetPriorityOptions() => new[]
        {
            "Nearest", "LowestHP", "HighestATK", "Boss",
            "Healer", "Random", "Taunting",
        };

        public string[] GetLogLevelOptions() => new[]
        {
            "Verbose", "Debug", "Info", "Warning", "Error", "Fatal",
        };

        // ---- Button 回调 ----
        public void RandomizeCharacter(GameCharacterConfig target)
        {
            var names = new[] { "阿尔托莉雅", "卫宫士郎", "远坂凛", "间桐樱", "吉尔伽美什" };
            var classes = (CharClass[])Enum.GetValues(typeof(CharClass));
            target.characterName = names[UnityEngine.Random.Range(0, names.Length)];
            target.charClass = classes[UnityEngine.Random.Range(0, classes.Length)];
            target.level = UnityEngine.Random.Range(1, 100);
            target.maxHP = UnityEngine.Random.Range(500f, 99999f);
            target.maxMP = UnityEngine.Random.Range(100f, 9999f);
            target.strength = UnityEngine.Random.Range(1, 999);
            target.dexterity = UnityEngine.Random.Range(1, 999);
            target.intelligence = UnityEngine.Random.Range(1, 999);
            target.stamina = UnityEngine.Random.Range(1, 999);
            target.editCount++;
            Debug.Log($"[PropertyGrid] 随机生成角色: {target.characterName}");
        }

        public void ExportConfig(GameCharacterConfig target)
        {
            var json = JsonUtility.ToJson(target, prettyPrint: true);
            var path = $"CharacterConfig_{target.characterName}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, path), json);
            Debug.Log($"[PropertyGrid] 配置已导出: Assets/{path}");
        }

        public void ResetToDefault(GameCharacterConfig target)
        {
            // 此处仅重置顶层字段作为演示
            target.characterName = "无名英雄";
            target.charClass = CharClass.Warrior;
            target.level = 1;
            target.maxHP = 1000f;
            target.maxMP = 500f;
            target.strength = 10;
            target.dexterity = 10;
            target.intelligence = 10;
            target.stamina = 10;
            target.editCount++;
            Debug.Log("[PropertyGrid] 已恢复默认配置");
        }

        public void ClearInventory(GameCharacterConfig target)
        {
            target.quickSlotItems = "";
            target.favoriteItems.Clear();
            Debug.Log("[PropertyGrid] 背包已清空");
        }

        public void ResetAllEquipment(GameCharacterConfig target)
        {
            target.equipment = new CharacterEquipment();
            target.editCount++;
            Debug.Log("[PropertyGrid] 所有装备已重置");
        }

        public void RecalculateStats(GameCharacterConfig target)
        {
            // 模拟重新计算属性
            target.editCount++;
            Debug.Log($"[PropertyGrid] 属性已重新计算 (共 {target.editCount} 次修改)");
        }

        // ---- OnValueChanged / Validate ----
        public string ValidateConfig(GameCharacterConfig target)
        {
            var errors = new System.Collections.Generic.List<string>();
            if (string.IsNullOrEmpty(target.characterName))
                errors.Add("角色名不能为空");
            if (target.level < 1)
                errors.Add("等级不能小于 1");
            if (target.maxHP <= 0)
                errors.Add("生命值必须大于 0");
            if (target.strength + target.dexterity + target.intelligence + target.stamina <= 0)
                errors.Add("属性点总和必须大于 0");

            var result = errors.Count > 0
                ? $"⚠ 发现 {errors.Count} 个问题:\n{string.Join("\n", errors.Select(e => $"• {e}"))}"
                : "✅ 配置验证通过";
            Debug.Log($"[PropertyGrid] 验证结果:\n{result}");
            return result;
        }
    }

    // =========================================================================
    // 展示窗口
    // =========================================================================

    /// <summary>
    /// 生产级 PropertyGrid 用例展示窗口。
    /// 覆盖：TabGroup 分区 · 三级嵌套 · 条件控制 · Renderer 回调 · Plugin 扩展 · 资源选择。
    ///
    /// 菜单: Window > UniDecl > Game Character Config
    /// </summary>
    public class GameCharacterConfigWindow : EditorWindow
    {
        private GameCharacterConfig _config;
        private UIToolkitRenderManager _manager;

        [MenuItem("Window/UniDecl/Game Character Config")]
        public static void Open()
        {
            var w = GetWindow<GameCharacterConfigWindow>("Game Character Config");
            w.minSize = new Vector2(400, 500);
            w.Show();
        }

        private void CreateGUI()
        {
            _config = new GameCharacterConfig();
            _manager = new UIToolkitRenderManager(new EditorSnapshotManager(new SnapshotManager()));

            // 属性面板按纵向滚动；横向滚动条在无溢出内容时不应出现
            var scroll = new ScrollView { HorizontalScrollerVisibility = ScrollerVisibility.Hidden };
            scroll.Add(new PropertyGridElement(_config).WithKey("game_character_propertygrid"));

            var root = new VerticalLayout { scroll };

            var ve = _manager.RenderRoot(root);
            if (ve != null)
                rootVisualElement.Add(ve);
        }

        private void OnDestroy()
        {
            _manager = null;
            _config = null;
        }
    }
}