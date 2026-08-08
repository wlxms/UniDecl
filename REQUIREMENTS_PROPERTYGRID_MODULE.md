# UniDecl 自声明 PropertyGrid 系统 — 需求文档

版本: 1.0
日期: 2026-05-09

---

## 1. 目标

让用户**只用属性标记（Attribute）**就能为任意数据结构生成一套完整的 PropertyGrid 渲染界面，布局灵活、代码简洁、运行时代码零污染。

---

## 2. 使用体验概览

### 2.1 最低成本体验

不加任何 Attribute，任何 `[Serializable]` 类就能传入 PropertyGrid 查看和编辑：

```csharp
// 数据类：零 Attribute
[Serializable]
public class PlayerConfig {
    public string playerName;
    public int health;
    public float speed;
}
```

一句代码打开 PropertyGrid——字段类型自动匹配编辑器控件：

```csharp
// 传入任意可序列化对象
var window = PropertyGridWindow.Open(playerConfig);
// 或挂载到 EditorWindow 中
GetHost().BuildDOM(new PropertyGridElement { Target = playerConfig });
```
- `int` → 整数输入框
- `float` → 浮点数输入框
- `string` → 文本框
- `bool` → 开关
- `enum` → 下拉
- `Vector3` → 向量字段
- `Color` → 颜色拾取器
- `UnityEngine.Object` → 对象选择器
- `List<T>` → 可增删列表
- 嵌套 `[Serializable]` 类 → 折叠展开递归

### 2.2 日常体验——字段级扩散 + 可选类级预定义

最常见的需求——先声明几个分组（折叠组、边框组），然后字段各归各位。**组由字段级属性创建并扩散**，类级声明只提供元数据（标题、起始顺序、展开状态）供快速感知布局：

```csharp
[BoxGroup("Stats", Title = "数值")]
[FoldoutGroup("Advanced", Title = "高级设置")]
[Serializable]
public class EquipConfig
{
    // 没有布局属性 → 根组，默认垂直排列
    [LabelText("装备名")]
    public string itemName;

    [LabelText("描述")]
    public string description;

    // [BoxGroup("Stats")] 创建 Box 组并进入，其后字段扩散进入该组
    // [HorizontalGroup("Stats/Row")] 在 Stats 内创建 Row 水平子组
    [BoxGroup("Stats")]
    [HorizontalGroup("Stats/Row")]
    [Range(1, 999)]
    public int atk;
    public int def;                     // 扩散到 Stats/Row
    public int spd;                     // 扩散到 Stats/Row

    // [VerticalGroup("Stats/V")] 在 Stats 内创建 V 垂直子组
    [VerticalGroup("Stats/V")]
    [LabelText("魔力")]
    public int mp;

    // [FoldoutGroup("Advanced")] 创建 Foldout 组并进入
    [FoldoutGroup("Advanced")]
    [Dropdown("GetSpecialTypes")]
    [LabelText("特殊属性")]
    public string special;

    // [HGroup] 无参：当前位置创建自动命名的水平组
    [HorizontalGroup]
    public float testA;
    public float testB;
}
```

渲染效果：

```
装备名 [Sword of Destiny_______________]
描述 [A legendary sword________________]
                               ← 无标题，根级字段直接垂直排列

╔═ 数值 ═════════════════════════════════╗ ← BoxGroup("Stats")
║ 攻击 [===50===] 防御 [===30===] 速度 [===20===]│ ← Stats/Row 水平
║ 魔力 [===80===]                   │ ← Stats/V 垂直
╚══════════════════════════════════════╝

▸ 高级设置                                  ← FoldoutGroup 折叠块
    特殊属性 [Fire ▾]

testA [1.0]  testB [2.0]                 ← 自动命名水平组
```

**类级属性的作用**：`[BoxGroup("Stats", Title = "数值")]` 声明了 Stats 组的标题、排序等默认值。`[BoxGroup("Stats")]` 在字段级第一次出现时看到 Stats 已存在类级声明，复用其元数据，将 atk 分配进 Stats 组并开始扩散。

如果去掉类级声明，直接在字段级写 `[BoxGroup("Stats")]`，同样创建 Stats 组——行为和效果完全一样。区别只在可读性上：类级声明让开发者扫一眼类名就能预见布局。

### 2.3 完全自定义

下拉选项来自 AssetDatabase、需要自定义验证、需要按钮回调——写一个纯 Editor 侧的工具类，零运行时污染：

```csharp
// Editor 侧，随意使用 UnityEditor API
public class EquipConfig_Renderer {
    // 用于 2.2 示例中 [Dropdown("GetWeaponList")] 的下拉选项
    string[] GetWeaponList() => AssetDatabase.FindAssets("t:Weapon")
        .Select(AssetDatabase.GUIDToAssetPath)
        .Select(Path.GetFileNameWithoutExtension).ToArray();

    // 用于 2.4 示例中 [Validate("CheckSpeed")] 的验证逻辑
    string CheckSpeed(float val) => val > 0 ? null : "速度必须 > 0";

    // 用于 2.4 示例中 [Button("重置默认", "ResetConfig")] 的回调
    void ResetConfig(EquipConfig t) {
        t.level = 1;
        EditorUtility.SetDirty(t as UnityEngine.Object);
    }
}
```

### 2.4 完整示例——多组嵌套

```csharp
[HeaderGroup("Basic", Title = "装备编辑器")]
[BoxGroup("Stats", Title = "属性")]
[FoldoutGroup("Advanced", Title = "高级")]
[Serializable]
public class EquipConfig
{
    // Basic 组内：Row 水平子组
    [HeaderGroup("Basic")]
    [HorizontalGroup("Basic/Row")]
    [FlexGrow(1)] [LabelText("名称")] public string itemName;
    [Width(50)]    [ReadOnly]         public int itemId;

    // Basic 组内：V 垂直子组
    [VerticalGroup("Basic/V")]
    [LabelText("备注")] public string remark;

    // 进入 Stats 组，创建 V 垂直子组
    [BoxGroup("Stats")]
    [VerticalGroup("Stats/V")]
    [Range(1, 999)] [LabelText("等级")] public int level;
    [MinMaxSlider(0, 100)]              public Vector2 attackRange;

    // Stats 组内：Row 水平子组
    [HorizontalGroup("Stats/Row")]
    [Dropdown("GetWeaponList")]        [LabelText("武器")] public string weapon;
    [Validate("CheckSpeed")]        [LabelText("速度")] public float speed;
    [LabelText("暴击")]                  public float critRate;

    // 回到根级：分割线 + 条件字段
    [Divider]
    [ShowIf("isDebug")]
    [InfoBox("仅调试可见")]
    public string debugData;

    public bool isDebug;

    [Button("重置默认", "ResetConfig")]
    public int __reset;
}
```

渲染：

```
> 装备编辑器                                     ← HeaderGroup
  名称 [Sword of Destiny_______________]  [101]   ← Basic/Row 水平
  备注 [A legendary weapon___________]            ← Basic/V 垂直

╔═ 属性 ══════════════════════════════════════╗
║ 等级 [===========50===========]            ║  ← Stats/V 垂直
║ 攻击范围 [╫══20══╪════80════]               ║
║ 武器 [Sword of Flames ▾]  速度 [15]  暴击 [5]║  ← Stats/Row 水平
╚═══════════════════════════════════════════╝
────────────────────────────────────
☑ isDebug
ℹ️ 仅调试可见 [______________]

            [重置默认]
```

---

## 3. 核心能力（用户可见）

### 3.1 布局扩散（传染式 Group）

`[HGroup]` / `[VGroup]` 标记会影响**后续所有字段的排列方向**，直到遇到下一个布局标记改变方向：

```
[HGroup] public float a;    // a, b, c 水平排列在同一行
         public float b;
         public float c;
[VGroup] public int d;      // d, e 垂直排列
         public int e;
```

- `[HGroup("path")]` 启动水平排列，后续字段自动加入同一行。path 为空则自动命名组。
- `[VGroup("path")]` 启动垂直排列，后续字段自动垂直排序。path 为空则自动命名组。
- `[HGroup]`（无参）自动产生组名字，与前一字段水平同行。
- `[VGroup]`（无参）自动产生组名字，后续字段垂直排列。
- `[PropertyOrder(n)]` 在组内调整前后顺序。
- 组路径支持 Odin 风格嵌套：`"Stats/Row"` 表示 Stats 组内的 Row 子组。

无参 `[HGroup]` 和 `[VGroup]` 各自**创建一个自动命名的新组并使用该方向扩散**——不是在同组内改变方向。`[HGroup]` 创建水平组并扩散，`[VGroup]` 创建垂直组并扩散。两者独立，不冲突。

**组类型唯一性**：一个组名全局唯一，且只对应一种组类型。首次出现在类级 `[BoxGroup("Stats")]` → "Stats" 锁死为 Box；如果首次出现在字段级 `[BoxGroup("Stats")]` → 同样锁死为 Box。进入已存在的组只能用同类型属性，不能用 `[HGroup("Stats")]` 进入一个 Box 组。

**类级声明 vs 字段级创建**：

类级 `[BoxGroup("Stats", Title = "数值")]` 只是元数据声明（标题、排序等），**不产生任何渲染效果**——没有字段被分配进 Stats。字段级 `[BoxGroup("Stats")]` 则**创建组的实例**、将当前字段分配进该组、并开始扩散。如果字段级写一个已在类级声明过的组名，则复用类级元数据。两者分工明确：

- 类级：提供**可覆盖的默认值**（标题、排序等），方便扫一眼类名就感知布局
- 字段级：**创建并进入**组，实际影响渲染结构

### 3.2 双重定向引用（@）

适用于文本框内容的属性（如 `[LabelText]`、`[Tooltip]`、`[SuffixLabel]`），字符串参数加 `@` 前缀会在渲染时自动解析——`[LabelText("@displayName")]` 从 Renderer 取显示标签，`[SuffixLabel("@unit")]` 从 Renderer 取单位后缀。

条件属性（`[ShowIf]`、`[HideIf]`、`[EnableIf]`）不使用 `@` 解析——它们的参数始终指向**数据类的成员名**，读取其运行时的值作为条件判定。

解析顺序（仅用于 `@` 前缀的属性）：
1. 在关联的 Renderer 类的字段/属性/方法中查找
2. 在数据类自身查找
3. 都没找到 → 去掉 `@` 前缀后的字符串作字面量

用户可以把 Editor 侧的数据（标签文字、选项列表、验证错误消息等）放在 Renderer 类中，运行时数据类保持清洁。

### 3.3 外源性变更自动检测

系统自动感知 PropertyGrid 之外的字段修改（动画驱动、网络回调、其他工具窗口等），并自动刷新显示：

| 方式 | 适用对象 | 延迟 |
|------|---------|------|
| 事件通知 | 实现了 `INotifyPropertyChanged` 的类 | 即时 |
| DirtyFlag | `MonoBehaviour` / `ScriptableObject` | <1帧 |
| 快照检测 | 普通 C# 对象 | <1帧 |

系统自动为不同目标类型选择最优检测方式，用户无需配置。

### 3.4 撤销/重做

| 对象类型 | 撤销方式 |
|---------|---------|
| `MonoBehaviour` / `ScriptableObject` | Unity 原生 `Ctrl+Z` |
| 普通 C# 对象 | 系统内置快照栈（最多 50 步） |

### 3.5 模块化即插即用

PropertyGrid 系统是 UniDecl 的**可选插件**，加载后自动启用。不加载时 UniDecl 核心正常工作，不受影响。

### 3.6 PlayMode 属性的作用域

`[DisableInPlayMode]` 和 `[HideInPlayMode]` 控制字段在 **Editor 的 Play Mode（运行模式）** 下的行为。它们不影响 Runtime 发布构建——发布构建中所有 `[Conditional("UNITY_EDITOR")]` 标记已被擦除，这些属性不存在。

### 3.7 绑定型属性的 Renderer 方法签名

`[Dropdown("method")]`、`[Validate("method")]`、`[Button("label", "method")]`、`[OnValueChanged("method")]` 的字符串参数指向 Renderer 类中的同名方法。约定的方法签名：

| 属性 | Renderer 方法签名 | 返回值 |
|------|-------------------|--------|
| `[Dropdown("GetOpt")]` | `string[] GetOpt()` / `string[] GetOpt(T target)` | 下拉选项列表 |
| `[Validate("Check")]` | `string Check(TField value)` / `string Check(TField val, T target)` | `null`=通过, 字符串=错误消息 |
| `[Button("label", "Act")]` | `void Act()` / `void Act(T target)` | 无 |
| `[OnValueChanged("Log")]` | `void Log(TField oldVal, TField newVal, T target)` | 无 |

## 4. 属性标记清单 (45 个)

### 4.1 类级：预定义分组

| 属性 | 效果 | 频率 |
|------|------|------|
| `[HeaderGroup("path", Title?)` | 声明一个带标题的折叠组 | 中 |
| `[BoxGroup("path", Title?)` | 声明一个带边框的区块 | 高 |
| `[FoldoutGroup("path", Title?)` | 声明一个可折叠的组 | 高 |
| `[TabGroup("path", Title?)` | 声明一个标签页容器 | 低 |
| `[Button("label", "method")]` | 声明一个按钮，点击回调同 Renderer 的 method 方法 | 中 |

类级属性提供组的默认元数据（标题、排序、展开/折叠状态），**不产生渲染效果**——它只声明，不分配字段。字段级属性引用同名字段级时复用这些元数据。类级声明是可选的，唯一目的是让开发者扫一眼类名就预见布局。

### 4.2 字段级：布局（传染式）

| 属性 | 传染 | 效果 | 频率 |
|------|------|------|------|
| `[HGroup]` / `[HGroup("path")]` | ✅ | 启动水平排列，后续字段自动加入同一行；空 path 自动命名组 | 高 |
| `[VGroup]` / `[VGroup("path")]` | ✅ | 启动垂直排列，后续字段自动垂直排序；空 path 自动命名组 | 中 |
| `[PropertyOrder(n)]` | ❌ | 在组内指定排序位置，越小越靠前 | 中 |

扩散规则：标记 `[HGroup]` 后所有后续字段自动水平排列，直到遇到下一个布局标记。标记 `[VGroup]` 后后续字段自动垂直排列。空 path 与有 path 的行为一致——都创建一个新组并开始在该组内的排列扩散。

### 4.3 字段级：显示

| 属性 | 效果 | 频率 |
|------|------|------|
| `[LabelText("@key" / "text")]` | 自定义标签；`@` 前缀运行时查 Renderer/数据类 | **高频** |
| `[HideLabel]` | 隐藏标签 | 中 |
| `[Tooltip("@key" / "text")]` | 悬停提示 | 中 |
| `[SuffixLabel("@key" / "text")]` | 编辑控件后追加文字（如 "m/s"） | 中 |
| `[Title("text")]` | 块级标题 | 低 |
| `[Divider]` | 分割线 | 中 |
| `[Space(before?, after?)]` | 字段上下间距（px），before=上方间距，after=下方间距 | 低 |
| `[InfoBox("text", type?)]` | 提示信息框（信息/警告/错误） | 中 |
| `[ReadOnly]` | 只读 | 高 |
| `[GUIColor(r,g,b,a)]` | 字段颜色 | 低 |
| `[Indent(level)]` | 缩进 | 低 |
| `[HideInPlayMode]` | 运行时隐藏 | 低 |
| `[DisableInPlayMode]` | 运行时禁用 | 低 |

### 4.4 字段级：Flex 对齐

| 属性 | 效果 | 频率 |
|------|------|------|
| `[FlexGrow(n)]` | 水平行中占用剩余空间的 n 份比例 | 中 |
| `[Width(n)]` | 固定像素宽度 | 中 |
| `[AlignRight]` | 水平行中靠右对齐 | 低 |
| `[AlignCenter]` | 水平行中居中对齐 | 低 |

### 4.5 字段级：数值约束

| 属性 | 效果 | 频率 |
|------|------|------|
| `[Range(min, max)]` | 显示为滑块，值限制在范围内 | **高频** |
| `[MinValue(n)]` | 最小值 | 中 |
| `[MaxValue(n)]` | 最大值 | 中 |
| `[MinMaxSlider(min, max)]` | 双端滑块（用于 Vector2） | 中 |
| `[Step(n)]` | 步进值 | 低 |
| `[Wrap(min, max)]` | 越界回绕 | 低 |

### 4.6 字段级：条件控制

| 属性 | 效果 | 频率 |
|------|------|------|
| `[ShowIf("member")]` / `[ShowIf("member", value)]` | 条件为真时显示 | 高 |
| `[HideIf("member")]` / `[HideIf("member", value)]` | 条件为真时隐藏 | 中 |
| `[EnableIf("member")]` | 条件为真时可编辑 | 低 |

### 4.7 字段级：Editor 绑定（方法引用）

| 属性 | 效果 | 频率 |
|------|------|------|
| `[Dropdown("method")]` | 下拉选项来自 Renderer 的同名方法 | 高 |
| `[Validate]` / `[Validate("method")]` | 验证器来自 Renderer 的同名方法 | 中 |
| `[Button("label", "method")]` | 按钮，点击调用 Renderer 的同名方法（也可置于类级） | 中 |
| `[OnValueChanged("method")]` | 值变更时回调 Renderer 的同名方法 | 低 |

所有绑定型属性保持 Odin 风格的字符串参数风格。

### 4.8 字段级：资源

| 属性 | 效果 | 频率 |
|------|------|------|
| `[PreviewField(height?)]` | 对象选择器带方形预览图 | 中 |
| `[FilePath(extensions?, parent?)]` | 文件路径选择对话框 | 低 |
| `[FolderPath(parent?)]` | 文件夹路径选择对话框 | 低 |
| `[AssetsOnly]` | 限选项目资源 | 低 |
| `[SceneObjectsOnly]` | 限选场景对象 | 低 |

### 4.9 字段级：枚举/颜色/多行

| 属性 | 效果 | 频率 |
|------|------|------|
| `[EnumToggleButtons]` | 枚举选项显示为按钮组 | 中 |
| `[ColorPalette("paletteName")]` | 为颜色字段提供预定义调色板 | 中 |
| `[TextArea(min, max)]` | 字符串显示为可拉伸多行文本框 | 中 |

---

## 5. 自定义渲染器（用户视角）

### 5.1 渲染器是什么

渲染器是一个纯 Editor 侧的类，与某个数据类型关联。系统**自动发现并匹配**，不需要用户手动注册。

### 5.2 渲染器能做什么

用户可以**按需提供**以下任意组合，全部可选：

| 能力 | 做法 | 示例 |
|------|------|------|
| 提供 @ 引用值 | 写一个公共字段或属性 | `public string displayName = "敌人";` |
| 提供下拉选项 | 写一个返回 `string[]` 的方法 | `string[] GetTypes() => ...;` |
| 提供验证逻辑 | 写一个返回错误消息的方法 | `string CheckHp(int v, Config t) => ...;` |
| 提供按钮回调 | 写一个 void 方法 | `void Reset(Config t) { ... }` |
| 让属性引用这些方法 | 在字段上用 `[Dropdown("GetTypes")]` | `[Dropdown("GetTypes")] public string type;` |
| 覆盖 UI | 写 Build 方法，包装默认 Widget | 在系统默认 UI 外追加其他内容 |

### 5.3 渲染器的解析规则

Renderer 中的方法通过**方法名**与属性中的字符串参数匹配。方法签名约定见 [3.7 绑定型属性的 Renderer 方法签名](#37-绑定型属性的-renderer-方法签名)。

---

## 6. 系统约束

| 约束 | 说明 |
|------|------|
| PropertyGrid 是可选插件 | 不加载模块时 UniDecl 核心正常工作 |
| 属性标记发布时自动消除 | 所有 Attribute 标记 `[Conditional("UNITY_EDITOR")]`，发布构建零残留 |
| 渲染器是纯 Editor 代码 | 可使用 AssetDatabase 等 Editor API，不参与发布构建 |
| 不需要继承自定义类 | 适用于任何 `[Serializable]` 类，不需要继承 Odin 式的 `SerializedMonoBehaviour` |
| 不需要手动注册 | 渲染器自动发现，属性自动读取 |
| 运行时数据类零污染 | @ 引用和 [Dropdown("method")] 等方法引用目标都在 Editor 侧 Renderer 上 |
