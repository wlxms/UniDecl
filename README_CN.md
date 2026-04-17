# UniDecl

面向 Unity 的 React 风格声明式 GUI 引擎，同时支持 IMGUI 和 UI Toolkit 后端。

## 设计理念

### 诚实胜于表面

早期迭代中，UI 元素被定义为实现 `IElement` 接口的 `struct`——表面上追求"零分配"的纯粹，实际上每次经过 `IEnumerable<IElement>` 参数时都会发生隐式装箱。struct 在声明式框架的语境下撒了一个谎：声称自己轻量，却在每帧重建元素树时静默地将自身拷贝到堆上。对于一个每帧都在重建的声明式 UI 来说，这比坦诚的 class 产生了更多的 GC 压力。

UniDecl 选择 `class Element` 作为基类。不是因为它更快，而是因为它**诚实**——分配成本是显式、可预测的，不会以"零分配"的名义掩耳盗铃。诚实的抽象还带来了真正的工程收益：

- 引用标识使 Key-based Diff 成为可能（`ReferenceEquals`）
- 消除了值类型拷贝语义带来的心智负担
- `virtual` 派发取代了接口派发 + 装箱的双重代价

> 一个声称零分配却在暗中装箱的 struct，比一个坦诚分配的 class 更昂贵。

### 理解先于行动

单遍"边遍历边渲染"的直觉很自然——走到哪里画到哪里。但直觉往往是短视的。当需要增量更新时，你发现自己没有旧树可比较；当局部重建需要重新注入上下文时，你发现自己丢失了祖先信息。

UniDecl 将渲染拆分为两个阶段，就像先读图纸再盖房子：

- **BuildDOM** —— 理解：将声明式元素树展开为 `DOMNode` 树，解析 Context、State、Consumer 的语义关系
- **Render** —— 行动：遍历已理解的结构，调用渲染器将其具象化为像素

这一分离不是一个优化技巧，而是一个**架构立场**：理解与行动是两件不同的事，混为一谈只会让你两者都做不好。正是这一分离，使增量重建和 Diff 系统有了存在的根基。

### 不为不确定性下注

Unity 正从 IMGUI 向 UI Toolkit 迁移，没有人知道这个过渡期会持续多久。一个与单一后端绑定的框架，是在替开发者做选择——而开发者自己还没有答案。

UniDecl 的核心（`Runtime/Core`）不持有任何后端立场。它管理的是一棵逻辑上的元素树：构建它、Diff 它、重建它。至于这棵树最终以什么形式呈现——`GUILayout` 调用、`VisualElement` 实例、还是尚未出现的第三个后端——那是渲染器的责任，不是框架的。

> 框架应该为后端的多样性提供空间，而不是为后端的单一性做出承诺。

### 尊重平台已有的答案

UI Toolkit 已有 USS，IMGUI 已有 GUIStyle。它们是平台对自己的 UI 渲染模型给出的答案。再建一套并行的样式系统——CSS 解析器、主题资产、伪类状态机、过渡插值器——不是在解决问题，而是在否定平台的判断力。

UniDecl 提供的样式层薄到几乎透明：`InlineStyle` 携带 class name 和尺寸提示，`UITKStyle` 直接映射到 `StyleLength` 等原生类型。框架的野心不在于取代后端的样式系统，而在于**在正确的边界处让位**。

> 最好的抽象不是覆盖一切，而是知道自己该在哪里停下来。

### 身份的清晰性

`IContextProvider`、`IContextConsumer`、`IContainerElement` 是三种结构性角色。一个元素可以是其中之一，但不应同时是两个。这不仅是工程约束，更是对**身份清晰性**的要求——如果一个元素既是容器又是上下文提供者，它在 DOM 树中的语义就是暧昧的：它的子元素应该被展开，还是作为唯一包裹的子节点？

UniDecl 在构建阶段强制这一互斥约束，违反即抛出异常。这不是僵化，而是让歧义在最早的时刻被终结，而非在渲染结果的微妙偏差中消耗开发者的时间。

> 模棱两可的身份是 Bug 最喜欢的藏身之处。

### 结构即布局

在传统的 CSS/USS 思维中，布局是一种样式属性——你写 `display: flex`、`flex-direction: column`，布局信息被埋在样式表中，与颜色、字号、圆角混为一谈。读取代码时，你必须同时盯着元素结构和样式文件才能在脑海中拼出最终的布局形态。布局被**隐藏**了。

UniDecl 认为布局不是装饰，而是**骨架**。骨架应该长在元素树上，而不是藏在样式表里：

```csharp
new VerticalLayout                    // 布局 = 结构
{
    new Label("标题"),
    new HorizontalLayout              // 嵌套 = 组合
    {
        new TextField(_name),
        new Button("提交", OnSubmit),
    },
}
```

你看到的就是你得到的。没有隐式的 `display: flex`，没有从样式表逆向推导的布局意图。元素树本身就是布局的可视化表达。样式只负责让这个骨架变得好看——颜色、间距、圆角——但绝不会试图重塑骨骼。

> 布局是结构的事，样式是皮囊的事。不要让皮囊替骨骼做决定。

### 封装而非暴露

很多框架暴露了渲染器的概念：你写一个 `Renderer`，手动调用 `GUILayout.Label()` 或 `new Label()`，然后注册到系统中。这意味着消费者必须理解渲染器的注册机制、生命周期和后端 API。

UniDecl 将这种复杂性折叠进了 Widget 内部。一个 `Button` 不是一个需要配套渲染器的接口——它是一个**完整的封装**：元素定义 + 渲染契约，由框架在幕后完成匹配。使用者只需要知道一件事：`new Button("OK", () => { })`。

渲染器依然存在，但它退到了框架的皮肤之下。就像你不应该需要知道神经信号如何传递才能抬起手臂一样，你不应该需要理解渲染器注册流程才能放一个按钮在屏幕上。

封装的边界划在"做什么"和"怎么做"之间。Widget 回答"做什么"，渲染器回答"怎么做"。使用者只需要和"做什么"对话。

> 好的封装不是隐藏实现，而是让你根本不需要知道实现的存在。

### 行为与结构同在

在传统的命令式 UI 中，结构和行为是分离的：你在 A 处构建按钮，在 B 处订阅事件，在 C 处处理回调。当 UI 变得复杂，这种分离会让代码像散落的拼图——你需要跳转多个位置才能理解一个按钮的完整生命周期。

UniDecl 的声明式事件系统让行为与结构**共生于一处**：

```csharp
new Button("删除", () =>
{
    _items.Remove(selected);
    NotifyChanged();
})
```

事件的意图、触发时机、响应逻辑——全部在一个表达式中完成。没有 `AddListener`，没有 `RegisterCallback`，没有散落在别处的 handler 方法。元素既是结构的描述，也是行为的载体。

更深层地看，这与声明式 UI 的核心主张一脉相承：**描述"是什么"，而非"怎么做"**。"是什么"不仅包含 UI 长什么样，也包含 UI 遇到交互时该怎么反应。将这两者拆开，就是在人为地割裂一个完整的意图。

> 一个需要你在三个地方才能读懂的按钮，不是一个声明式的按钮。

## 架构

```
UniDecl/
├── Runtime/
│   ├── Core/              # 后端无关的核心抽象
│   │   ├── IElement.cs            # 元素基础接口
│   │   ├── Element.cs             # 抽象基类（Element、ContainerElement、Element<TState>）
│   │   ├── DOMTree.cs             # 两阶段 DOM 构建 + Diff 重建
│   │   ├── DOMNode.cs             # DOM 树节点
│   │   ├── ElementRenderHost.cs   # 渲染管线：BuildDOM → Render、事件分发
│   │   ├── IElementRender.cs      # 渲染器 + 更新器接口
│   │   ├── Context/               # ContextStack、ContextProvider、ContextConsumer
│   │   ├── State/                 # StateManager、StateStack、ElementState
│   │   └── Event/                 # EventDispatcher（泛型 struct 事件）
│   │
│   ├── Components/         # 后端无关的元素定义
│   │   ├── InlineStyle.cs         # 轻量内联样式（class name + 尺寸属性）
│   │   └── IInlineStyle.cs        # 内联样式接口
│   │
│   ├── Widgets/            # 可复用的元素定义（class-based）
│   │   ├── Label, Button, TextField, Slider, Toggle, ...
│   │   ├── VerticalLayout, HorizontalLayout, ScrollView, Foldout, ...
│   │   ├── ListView, TreeView, MultiColumnListView
│   │   ├── Toolbar, ToolbarButton, ToolbarToggle, ...
│   │   ├── UE/                     # Unity Editor 专用（UeCard）
│   │   └── MD/                     # Markdown Widget（见下文）
│   │
│   ├── Contexts/           # 内置上下文提供者
│   │   └── DisableContext.cs
│   │
│   └── MD/                 # Markdown 解析器
│       ├── MdParser.cs             # 轻量、无外部依赖的解析器
│       ├── MdBlock.cs              # 块级 AST（Heading、Paragraph、CodeBlock 等）
│       └── MdInline.cs             # 行内级 AST（Bold、Italic、Code、Link 等）
│
├── UIToolKit/
│   ├── Runtime/
│   │   ├── UITKStyle.cs            # UI Toolkit 原生样式桥接
│   │   └── Widgets/                # UI Toolkit 专用 Widget（ListView、TreeView 等）
│   │
│   └── Editor/
│       ├── UIToolkitRenderManager.cs   # ElementRenderHost<VisualElement> 实现
│       ├── Renderers/                 # 55+ 个返回 VisualElement 的渲染器
│       │   ├── UE/                   # Unity Editor 字段渲染器
│       │   └── MD/                   # Markdown Widget 渲染器
│       ├── Style/                     # USS 样式表
│       └── Examples/                  # 示例 EditorWindow
│
└── Editor/
    └── UniDecl.Editor.asmdef
```

## 核心概念

### 元素（Element）

所有 UI 元素继承 `Element`（布局容器继承 `ContainerElement`）：

```csharp
// 叶子元素 —— 由注册的渲染器绘制
public class Label : Element
{
    public string Text { get; set; }
    public override IElement Render() => null;  // 无子树
    public Label(string text) { Text = text; }
}

// 容器元素 —— 持有子元素
public class VerticalLayout : ContainerElement
{
    public override IEnumerable<IElement> Children => ...;
    public override void Add(IElement element) => ...;
}
```

声明式组合：
```csharp
new VerticalLayout
{
    new Label("用户名:"),
    new TextField(_username, v => _username = v),
    new Button("提交", OnSubmit),
}
```

### 有状态元素（Stateful Element）

对于需要跨重建保持状态的组件，使用 `Element<TState>`：

```csharp
public class Counter : Element<Counter.CounterState>
{
    public class CounterState { public int Count; }

    public override CounterState BuildState() => new CounterState();

    public override IElement Render(CounterState state)
    {
        return new VerticalLayout
        {
            new Button($"计数: {state.Count}", () => state.Count++),
            new Button("重置", () => state.Count = 0),
        };
    }
}
```

框架在首次遇到该元素时通过 `BuildState()` 创建状态，并在后续重建中保持引用。当元素触发 `Rebuild()` 或 `NotifyChanged()` 时，同一状态实例会被复用。

### 上下文系统（Context）

向元素树注入值，并在任意深度消费：

```csharp
// 提供上下文
new DisableContext(true)
{
    new ContextConsumer(reader =>
    {
        var ctx = reader.Get<DisableContext>();
        return new Label(ctx?.Value == true ? "已禁用" : "已启用");
    })
}
```

上下文使用栈式作用域：同类型的嵌套提供者会遮蔽外层的。构建阶段自动处理上下文的入栈/出栈；渲染阶段在局部重建时使用预计算的上下文链。

### 内联样式（Inline Style）

附加布局提示和 class name，不耦合到特定后端：

```csharp
new Label("带样式的标签")
    .With(new InlineStyle("title-label") { Width = 200, MarginBottom = 8 })
```

对于 UI Toolkit，`UITKStyle` 直接映射到 `StyleLength`、`StyleEnum<Visibility>` 等 USS 原生类型：

```csharp
new Label("UITK 样式标签")
    .With(new UITKStyle { Width = new StyleLength(200), MarginBottom = 8 })
```

## 增量重建与 Diff

这是 UniDecl 性能模型的核心。

### 重建粒度

当元素调用 `Rebuild()` 时，只有该元素的子树被重建——而非整棵 DOM：

```
完整树:             重建目标:          仅子树被重建:
  A                    A                   A
  ├── B                ├── B (目标)        ├── B ← 重建
  │   ├── C            │   ├── C           │   ├── C' (新)
  │   └── D            │   └── D           │   └── D' (新)
  └── E                └── E               └── E (不受影响)
```

### 基于 Key 的 Diff

在子树重建过程中，UniDecl 使用**Key 优先、位置次之**的策略对比新旧子节点：

1. **Key 匹配** —— 如果新子节点的 `Key` 与旧子节点匹配，复用该 DOMNode
2. **位置回退** —— 如果没有 Key 匹配，尝试同位置复用
3. **类型检查** —— 如果复用节点的类型一致，递归 Diff；否则整体替换

### 更新器接口（UI Toolkit）

在 UI Toolkit 下，渲染器可以同时实现 `IElementUpdater<TRenderResult>` 和 `IElementRenderer`：

```csharp
public class MyLabelRenderer : IElementRenderer<Label, VisualElement>,
                               IElementUpdater<Label, VisualElement>
{
    public VisualElement Render(Label element, ...) => ...;

    // 当同一 Label 元素因属性变化被重新渲染时调用。
    // 返回 true 可跳过完整的 Render()，复用已有的 VisualElement。
    public bool TryUpdate(Label element, VisualElement existing,
                          IElementRenderHost<VisualElement> manager, ElementState state)
    {
        existing.text = element.Text;
        return true;
    }
}
```

如果 `TryUpdate` 返回 false 或未实现，框架会回退到完整的 `Render()`。

### 自动重建与合并

`NotifyChanged()`（或状态变更触发的自动重建）会收集待处理的重建请求，在帧尾通过 `EditorApplication.delayCall` 统一执行。对同一元素的多次变更会被合并为一次重建。

### 性能监控

在渲染主机上启用 `EnableRebuildPerformanceMonitoring` 即可接收 `RebuildPerformanceEvent`，获取各阶段的耗时分解：

```csharp
renderHost.EnableRebuildPerformanceMonitoring = true;
renderHost.Subscribe(new MyPerformanceListener());
```

## Markdown 支持

UniDecl 内置了轻量、无外部依赖的 Markdown 解析器（`MdParser`），提供块级和行内级 AST：

**块级类型**：ATX 标题（H1-H6）、Setext 标题、段落、围栏代码块、有序/无序列表、引用块、水平线。

**行内类型**：粗体、斜体、粗斜体、行内代码、链接、图片、硬换行。

### Markdown Widget

14 个专为在 UI Toolkit 中渲染 Markdown 内容而构建的 Widget：

| Widget | 说明 |
|--------|------|
| `H1`–`H6` | 可配置样式的标题元素 |
| `RichText` | 支持行内格式的富文本（粗体、斜体、链接） |
| `CodeBlock` | 带语言感知样式的围栏代码块 |
| `CodeHighlighter` | 语法高亮代码展示 |
| `Blockquote` | 带左边框样式的引用块 |
| `Divider` | 水平线 |
| `MdTable` | Markdown 表格渲染 |
| `TocView` | 从标题中提取的目录导航 |
| `MarkdownView` | 一站式 Markdown 文档查看器 |

```csharp
var md = "# 你好\n\n这是**粗体**文本。\n\n```csharp\nDebug.Log(42);\n```";

new MarkdownView(md);
```

## 内置 Widget

### 基础控件
Label, Button, TextField, Toggle, Slider, SliderInt, IntegerField, FloatField, DoubleField, LongField, Dropdown, EnumField, EnumFlagsField, ColorField, ObjectField, PropertyField, LayerField, TagField, MaskField, HelpBox, ProgressBar, CurveField, GradientField, ResizableTextArea

### 布局
VerticalLayout, HorizontalLayout, Panel, ScrollView, Foldout, TwoPaneSplitView, VisualSplitter, PopupWindow, IMGUIContainer

### 数据视图
ListView, TreeView, MultiColumnListView

### 工具栏
Toolbar, ToolbarButton, ToolbarToggle, ToolbarSearchField, ToolbarMenu

### 向量 / 几何
Vector2Field, Vector3Field, Vector4Field, Vector2IntField, Vector3IntField, RectField, RectIntField, BoundsField, BoundsIntField

### 特殊
InspectorElement, UeCard

## 快速开始（UI Toolkit）

```csharp
using UnityEditor;
using UnityEngine;
using UniDecl.Editor.UIToolKit;
using UniDecl.Runtime.Widgets;

public class MyWindow : EditorWindow
{
    private UIToolkitRenderManager _renderManager;

    [MenuItem("Tools/My Window")]
    static void Show() => GetWindow<MyWindow>();

    void CreateGUI()
    {
        _renderManager = new UIToolkitRenderManager();
        _renderManager.LoadStyleSheetFromResources("UniDecl/Style/default");
        rootVisualElement.Add(_renderManager.RenderRoot(BuildUI()));
    }

    IElement BuildUI()
    {
        return new VerticalLayout
        {
            new Label("Hello UniDecl!"),
            new Button("Click Me", () => Debug.Log("Clicked")),
        };
    }
}
```

## 许可证

MIT
