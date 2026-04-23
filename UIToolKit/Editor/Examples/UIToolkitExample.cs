using UnityEditor;
using UnityEngine;
using UniDecl.Runtime.Contexts;
using UniDecl.Runtime.Core;
using UniDecl.Runtime.Widgets;
using UniDecl.Runtime.Widgets.MD;
using UniDecl.Editor.UIToolKit;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;

namespace UniDecl.Editor.UIToolKit.Examples
{
    using W = UniDecl.Runtime.Widgets;
    using WM = UniDecl.Runtime.Widgets.MD;

    /// <summary>
    /// 状态化计数器组件 — 测试DOM重建能力
    /// </summary>
    public class CounterElement : Element<CounterElement.CounterState>
    {
        public struct CounterState
        {
            public int Count;
        }

        public override CounterState BuildState() => new CounterState();

        public override IElement Render(CounterState state)
        {
            var layout = new VerticalLayout
            {
                new HorizontalLayout
                {
                    new Button($"Add Element (Count: {state.Count})", () =>
                    {
                        SetState(s => new CounterState { Count = s.Count + 1 });
                    }),
                    new Button("Reset", () =>
                    {
                        SetState(s => new CounterState { Count = 0 });
                    }),
                }
            };

            for (int i = 0; i < state.Count; i++)
                layout.Add(new Label($"Dynamic Element #{i + 1}"));

            return layout;
        }
    }

    /// <summary>
    /// 分页状态化组件 — 演示 UniDecl 状态驱动 UI 切换
    /// </summary>
    public class TabContentElement : Element<TabContentElement.TabState>
    {
        public struct TabState
        {
            public int CurrentTab;
            public string InputText;
            public string PasswordText;
            public string MultilineText;
            public int IntegerValue;
            public float FloatValue;
            public int DropdownIndex;
            public int EnumValue;
        }

        private static readonly string[] TabNames =
        {
            "基础控件", "输入控件", "滑动与颜色", "布局容器", "进度与状态",
            "引用与资源", "向量与曲线", "Toolbar与工具", "MD 控件"
        };

        private static readonly string[] DropdownChoices = { "选项 A", "选项 B", "选项 C", "选项 D" };

        public override TabState BuildState() => new TabState
        {
            InputText = "默认文本",
            PasswordText = string.Empty,
            MultilineText = "多行\n输入",
            IntegerValue = 42,
            FloatValue = 3.14f
        };

        public override IElement Render(TabState state)
        {
            var tabButtons = new HorizontalLayout();
            for (int i = 0; i < TabNames.Length; i++)
            {
                var tabIndex = i;
                var name = tabIndex == state.CurrentTab ? $">> {TabNames[tabIndex]} <<" : TabNames[tabIndex];
                tabButtons.Add(new Button(name, () =>
                {
                    SetState(s => new TabState
                    {
                        CurrentTab = tabIndex,
                        InputText = s.InputText,
                        PasswordText = s.PasswordText,
                        MultilineText = s.MultilineText,
                        IntegerValue = s.IntegerValue,
                        FloatValue = s.FloatValue,
                        DropdownIndex = s.DropdownIndex,
                        EnumValue = s.EnumValue
                    });
                }));
            }

            IElement content = state.CurrentTab switch
            {
                0 => BuildBasicControlsTab(),
                1 => BuildInputControlsTab(state),
                2 => BuildSliderColorTab(),
                3 => BuildLayoutTab(),
                4 => BuildProgressStateTab(),
                5 => BuildResourceRefTab(),
                6 => BuildVectorCurveTab(),
                7 => BuildToolbarToolTab(),
                8 => BuildMarkdownTab(),
                _ => new Label("未知"),
            };

            return new VerticalLayout { tabButtons, new Label(""), content };
        }

        // === Tab 1: 基础控件 ===
        private static Panel BuildBasicControlsTab()
        {
            return new Panel
            {
                new Label("--- Toggle 开关示例 ---"),
                new W.Toggle("启用功能 A", true),
                new W.Toggle("启用功能 B", false),
                new Label(""),
                new Label("--- Button 按钮示例 ---"),
                new Button("普通按钮", () => Debug.Log("普通按钮被点击")),
                new Button("禁用按钮", () => { }) { Enabled = false },
                new Label(""),
                new Label("--- Label 文本示例 ---"),
                new Label("普通文本"),
                new Label("富文本 <b>粗体</b> <i>斜体</i>") { EnableRichText = true },
                new Label(""),
                new Label("--- HelpBox 提示框示例 ---"),
                new W.HelpBox("这是一条信息提示", HelpBoxMessageType.Info),
                new W.HelpBox("这是一条警告提示", HelpBoxMessageType.Warning),
                new W.HelpBox("这是一条错误提示", HelpBoxMessageType.Error),
                new Label(""),
                new Label("--- UITKStyle 样式示例 ---"),
                new Label("红色背景白色文字")
                    .With(new UITKStyle
                    {
                        BackgroundColor = new Color(0.8f, 0.2f, 0.2f),
                        Color = Color.white,
                        FontSize = 14,
                    }),
                new Label("Flex 布局居中 - 使用 Fluent API")
                    .With(new UITKStyle().FlexRow().JustifyCenter().AlignCenter().Padding(10)),
                new Button("蓝色边框按钮", () => Debug.Log("Styled!"))
                    .With(new UITKStyle
                    {
                        BorderTopColor = new Color(0.2f, 0.4f, 0.8f),
                        BorderBottomColor = new Color(0.2f, 0.4f, 0.8f),
                        BorderLeftColor = new Color(0.2f, 0.4f, 0.8f),
                        BorderRightColor = new Color(0.2f, 0.4f, 0.8f),
                    }
                    .BorderRadius(4)
                    .Margin(0, 0, 5, 0)),
            };
        }

        // === Tab 2: 输入控件 ===
        private Panel BuildInputControlsTab(TabState state)
        {
            var textField = new TextField(state.InputText, "请输入...")
            {
                OnValueChange = (next, _) =>
                {
                    SetState(s => new TabState
                    {
                        CurrentTab = s.CurrentTab,
                        InputText = next,
                        PasswordText = s.PasswordText,
                        MultilineText = s.MultilineText,
                        IntegerValue = s.IntegerValue,
                        FloatValue = s.FloatValue,
                        DropdownIndex = s.DropdownIndex,
                        EnumValue = s.EnumValue
                    });
                },
            };

            var passwordField = new TextField(state.PasswordText, "密码框")
            {
                IsPassword = true,
                OnValueChange = (next, _) =>
                {
                    SetState(s => new TabState
                    {
                        CurrentTab = s.CurrentTab,
                        InputText = s.InputText,
                        PasswordText = next,
                        MultilineText = s.MultilineText,
                        IntegerValue = s.IntegerValue,
                        FloatValue = s.FloatValue,
                        DropdownIndex = s.DropdownIndex,
                        EnumValue = s.EnumValue
                    });
                },
            };

            var multilineField = new TextField(state.MultilineText, "")
            {
                IsMultiline = true,
                OnValueChange = (next, _) =>
                {
                    SetState(s => new TabState
                    {
                        CurrentTab = s.CurrentTab,
                        InputText = s.InputText,
                        PasswordText = s.PasswordText,
                        MultilineText = next,
                        IntegerValue = s.IntegerValue,
                        FloatValue = s.FloatValue,
                        DropdownIndex = s.DropdownIndex,
                        EnumValue = s.EnumValue
                    });
                },
            };

            var integerField = new IntegerField(state.IntegerValue)
            {
                OnValueChanged = (next, _) =>
                {
                    SetState(s => new TabState
                    {
                        CurrentTab = s.CurrentTab,
                        InputText = s.InputText,
                        PasswordText = s.PasswordText,
                        MultilineText = s.MultilineText,
                        IntegerValue = next,
                        FloatValue = s.FloatValue,
                        DropdownIndex = s.DropdownIndex,
                        EnumValue = s.EnumValue
                    });
                },
            };

            var floatField = new FloatField(state.FloatValue)
            {
                OnValueChanged = (next, _) =>
                {
                    SetState(s => new TabState
                    {
                        CurrentTab = s.CurrentTab,
                        InputText = s.InputText,
                        PasswordText = s.PasswordText,
                        MultilineText = s.MultilineText,
                        IntegerValue = s.IntegerValue,
                        FloatValue = next,
                        DropdownIndex = s.DropdownIndex,
                        EnumValue = s.EnumValue
                    });
                },
            };

            var dropdown = new W.Dropdown("选择项目", DropdownChoices, state.DropdownIndex)
            {
                OnSelectionChanged = idx =>
                {
                    SetState(s => new TabState
                    {
                        CurrentTab = s.CurrentTab,
                        InputText = s.InputText,
                        PasswordText = s.PasswordText,
                        MultilineText = s.MultilineText,
                        IntegerValue = s.IntegerValue,
                        FloatValue = s.FloatValue,
                        DropdownIndex = idx,
                        EnumValue = s.EnumValue
                    });
                },
            };

            var enumField = new W.EnumField("日志级别", typeof(LogType), state.EnumValue)
            {
                OnValueChanged = value =>
                {
                    SetState(s => new TabState
                    {
                        CurrentTab = s.CurrentTab,
                        InputText = s.InputText,
                        PasswordText = s.PasswordText,
                        MultilineText = s.MultilineText,
                        IntegerValue = s.IntegerValue,
                        FloatValue = s.FloatValue,
                        DropdownIndex = s.DropdownIndex,
                        EnumValue = value
                    });
                },
            };

            return new Panel
            {
                new Label("--- TextField 文本输入 ---"),
                textField,
                passwordField,
                multilineField,
                new Label(""),
                new Label("--- IntegerField 整数输入 ---"),
                integerField,
                new Label(""),
                new Label("--- FloatField 浮点输入 ---"),
                floatField,
                new Label(""),
                new Label("--- Dropdown 下拉选择 ---"),
                dropdown,
                new Label(""),
                new Label("--- EnumField 枚举选择 ---"),
                enumField,
            };
        }

        // === Tab 3: 滑动与颜色 ===
        private static Panel BuildSliderColorTab()
        {
            return new Panel
            {
                new Label("--- Slider 滑块 ---"),
                new W.Slider("音量", 75f, 0f, 100f),
                new W.Slider("亮度", 0.5f, 0f, 1f),
                new Label(""),
                new Label("--- MinMaxSlider 范围滑块 ---"),
                new W.MinMaxSlider("距离范围", 10f, 90f, 0f, 100f),
                new W.MinMaxSlider("角度范围", 45f, 135f, 0f, 360f),
                new Label(""),
                new Label("--- ColorField 颜色选择 ---"),
                new W.ColorField("主颜色", Color.red),
                new W.ColorField("背景颜色", Color.blue),
            };
        }

        // === Tab 4: 布局容器 ===
        private static Panel BuildLayoutTab()
        {
            var scrollContent = new VerticalLayout();
            for (int i = 1; i <= 20; i++)
                scrollContent.Add(new Label($"ScrollView 项目 #{i}"));

            return new Panel
            {
                new Label("--- ScrollView 滚动容器 ---"),
                new ScrollView { scrollContent },
                new Label(""),
                new Label("--- Foldout 折叠区域 ---"),
                new Foldout("基础设置")
                {
                    new W.Toggle("启用阴影", true),
                    new W.Toggle("启用抗锯齿", false),
                },
                new Foldout("高级设置")
                {
                    new IntegerField(8),
                    new FloatField(60f),
                },
                new Label(""),
                new Label("--- HorizontalLayout 水平布局 ---"),
                new HorizontalLayout
                {
                    new Label("左"),
                    new Label("中"),
                    new Label("右"),
                },
                new Label(""),
                new Label("--- VerticalLayout 垂直布局 ---"),
                new VerticalLayout
                {
                    new Label("第一行"),
                    new Label("第二行"),
                    new Label("第三行"),
                },
            };
        }

        // === Tab 5: 进度与状态 ===
        private static Panel BuildProgressStateTab()
        {
            return new Panel
            {
                new Label("--- ProgressBar 进度条 ---"),
                new ProgressBar(0.25f, "加载中... 25%"),
                new ProgressBar(0.5f, "加载中... 50%"),
                new ProgressBar(0.75f, "加载中... 75%"),
                new ProgressBar(1.0f, "加载完成"),
                new Label(""),
                new Label("--- DisableContext 禁用上下文 ---"),
                new DisableContext(true,
                    new VerticalLayout
                    {
                        new Label("以下控件已被禁用："),
                        new TextField("无法输入", ""),
                        new Button("无法点击", () => { }),
                        new W.Toggle("无法切换", true),
                    }
                ),
                new Label(""),
                new Label("--- CounterElement 状态化示例 ---"),
                new Foldout("DOM Rebuild 测试")
                {
                    new CounterElement(),
                },
            };
        }

        // === Tab 6: 引用与资源 ===
        private static Panel BuildResourceRefTab()
        {
            return new Panel
            {
                new Label("--- TagField 标签选择 ---"),
                new W.TagField("标签", "Untagged"),
                new Label(""),
                new Label("--- LayerField 层级选择 ---"),
                new W.LayerField("层级", 0),
                new Label(""),
                new Label("--- MaskField Layer Mask ---"),
                new W.MaskField("层掩码", 0, new[] { "Layer 0", "Layer 1", "Layer 2", "Layer 3", "Layer 4", "Layer 5", "Layer 6", "Layer 7" }),
                new Label(""),
                new Label("--- EnumFlagsField 枚举标志 ---"),
                new W.EnumFlagsField("光照模式", typeof(UnityEngine.LightType), 0),
                new Label(""),
                new Label("--- ObjectField 对象引用 ---"),
                new W.ObjectField("纹理", typeof(Texture2D)),
                new W.ObjectField("材质", typeof(Material)),
                new W.ObjectField("预制体", typeof(GameObject)),
                new Label(""),
                new Label("--- PropertyField 属性绑定 ---"),
                new W.PropertyField("m_Name"),
                new Label(""),
                new Label("--- SliderInt / DoubleField / LongField ---"),
                new W.SliderInt("整数滑块", 5, 0, 10),
                new W.DoubleField("双精度", 3.14159265358979),
                new W.LongField("长整数", 1234567890123),
                new Label(""),
                new Label("--- ResizableTextArea ---"),
                new W.ResizableTextArea("可调整大小\n的多行文本", "备注"),
                new Label(""),
                new Label("--- Tooltip 支持 ---"),
                new Label("鼠标悬停查看提示").With(new UITKStyle { Tooltip = "这是一条 tooltip 提示" }),
            };
        }

        // === Tab 7: 向量与曲线 ===
        private static Panel BuildVectorCurveTab()
        {
            return new Panel
            {
                new Label("--- Vector2Field ---"),
                new W.Vector2Field("UV 偏移", new Vector2(0.5f, 0.5f)),
                new Label(""),
                new Label("--- Vector3Field ---"),
                new W.Vector3Field("位置", new Vector3(1f, 2f, 3f)),
                new Label(""),
                new Label("--- Vector4Field ---"),
                new W.Vector4Field("裁剪平面", new Vector4(0f, 1f, 0f, 100f)),
                new Label(""),
                new Label("--- Vector2IntField ---"),
                new W.Vector2IntField("网格坐标", new Vector2Int(10, 20)),
                new Label(""),
                new Label("--- Vector3IntField ---"),
                new W.Vector3IntField("体素坐标", new Vector3Int(1, 2, 3)),
                new Label(""),
                new Label("--- RectField ---"),
                new W.RectField("矩形", new Rect(10f, 20f, 100f, 200f)),
                new Label(""),
                new Label("--- RectIntField ---"),
                new W.RectIntField("整数矩形", new RectInt(0, 0, 256, 256)),
                new Label(""),
                new Label("--- BoundsField ---"),
                new W.BoundsField("包围盒", new Bounds(Vector3.zero, Vector3.one * 10f)),
                new Label(""),
                new Label("--- BoundsIntField ---"),
                new W.BoundsIntField("整数包围盒", new BoundsInt(Vector3Int.zero, new Vector3Int(16, 16, 16))),
                new Label(""),
                new Label("--- CurveField ---"),
                new W.CurveField("动画曲线", AnimationCurve.Linear(0f, 0f, 1f, 1f)),
                new Label(""),
                new Label("--- GradientField ---"),
                new W.GradientField("渐变"),
            };
        }

        // === Tab 8: Toolbar 与工具 ===
        private static Panel BuildToolbarToolTab()
        {
            return new Panel
            {
                new Label("--- Toolbar ---"),
                new W.Toolbar
                {
                    new W.ToolbarButton("新建", () => Debug.Log("New")),
                    new W.ToolbarButton("打开", () => Debug.Log("Open")),
                    new W.ToolbarToggle("自动保存", false),
                    new W.ToolbarSearchField(),
                    new W.ToolbarMenu("更多选项"),
                },
                new Label(""),
                new Label("--- PaneSplitView (replaces TwoPaneSplitView) ---"),
                new W.HorizontalPaneSplitView()
                {
                    new VerticalLayout
                    {
                        new Label("左侧面板"),
                        new Button("按钮 A", () => Debug.Log("A")),
                        new Button("按钮 B", () => Debug.Log("B")),
                    },
                    new VerticalLayout
                    {
                        new Label("右侧面板"),
                        new W.Slider("参数", 50f, 0f, 100f),
                    },
                },
                new Label(""),
                new Label("--- IMGUIContainer ---"),
                new W.IMGUIContainer(() =>
                {
                    GUILayout.Label("这是 IMGUI 内容");
                    GUILayout.Space(10);
                    if (GUILayout.Button("IMGUI 按钮"))
                        Debug.Log("IMGUI Button clicked");
                }),
            };
        }

        // === Tab 9: MD 控件 ===
        private static Panel BuildMarkdownTab()
        {
            var toc = new WM.TocView(new[]
            {
                new WM.TocEntry("文档标题", 1),
                new WM.TocEntry("简介", 2),
                new WM.TocEntry("快速开始", 2),
                new WM.TocEntry("安装", 3),
                new WM.TocEntry("配置", 3),
                new WM.TocEntry("进阶用法", 2),
                new WM.TocEntry("自定义样式", 3),
                new WM.TocEntry("事件回调", 3),
                new WM.TocEntry("注意事项", 4),
                new WM.TocEntry("API 参考", 2),
            });

            const string sampleMarkdown = @"# UniDecl MarkdownView

## 简介

**UniDecl** 是一个仿 React 风格的 Unity GUI 框架，支持声明式 UI 构建。

本控件 `MarkdownView` 可直接传入 Markdown 字符串并渲染为完整的富文本界面。

---

## 功能特性

- **标题**：H1 ~ H6 均支持
- **行内格式**：粗体 `**text**`、斜体 `*text*`、粗斜体 `***text***`
- **内联代码**：使用反引号 `` `code` ``
- 有序列表与无序列表
- 代码块、引用、水平分割线

## 快速开始

### 安装

在 Unity Package Manager 中通过 Git URL 添加：

```
https://github.com/wlxms/UniDecl.git
```

### 配置

```csharp
var mdView = new MarkdownView(markdownText)
{
    OnUrlClick = url => Debug.Log(""Clicked: "" + url),
};
```

## 链接示例

访问 [UniDecl GitHub 仓库](https://github.com/wlxms/UniDecl) 获取最新源码。

> **注意**：点击上方链接会触发 `OnUrlClick` 回调，
> 而不是直接打开浏览器，从而支持自定义跳转逻辑。

---

## API 参考

| 属性 | 类型 | 说明 |
|------|------|------|
| `Markdown` | string | Markdown 源文本 |
| `OnUrlClick` | Action<string> | 链接点击回调 |
";

            var markdownView = new WM.MarkdownView(sampleMarkdown,
                url => Debug.Log($"[MarkdownView] URL clicked: {url}"));

            return new Panel
            {
                new Label("--- 标题控件 H1–H6 ---"),
                new H1("H1 文档标题"),
                new H2("H2 章节标题"),
                new H3("H3 小节标题"),
                new H4("H4 细节标题"),
                new H5("H5 补充说明"),
                new H6("H6 脚注级标题"),
                new Label(""),
                new Label("--- TocView 目录导航栏 ---"),
                toc,
                new Label(""),
                new Label("--- MarkdownView 完整 MD 渲染控件 ---"),
                markdownView,
            };
        }
    }

    public class UIToolkitExample : EditorWindow, IEventListener<RebuildPerformanceEvent>
    {
        private UIToolkitRenderManager _manager;

        public void OnEvent(RebuildPerformanceEvent @event)
        {
            var elementName = @event.Element?.GetType().Name ?? "<null>";
            Debug.Log(
                $"[UniDecl][RebuildPerf] Trigger={@event.Trigger}, Element={elementName}, " +
                $"Before={@event.BeforeRebuildMs:F3}ms, DOM={@event.DomRebuildMs:F3}ms, " +
                $"After={@event.AfterRebuildMs:F3}ms, Total={@event.TotalMs:F3}ms");
        }

        [MenuItem("Window/UniDecl/UIToolkit Example")]
        public static void ShowWindow()
        {
            GetWindow<UIToolkitExample>("UniDecl UIToolkit");
        }

        public void CreateGUI()
        {
            _manager = new UIToolkitRenderManager();
            _manager.EnableRebuildPerformanceMonitoring = true;
            _manager.Subscribe(this);

            var root = new Panel { new TabContentElement() };
            var ve = _manager.RenderRoot(root);
            if (ve != null)
                rootVisualElement.Add(ve);
        }
    }
}
