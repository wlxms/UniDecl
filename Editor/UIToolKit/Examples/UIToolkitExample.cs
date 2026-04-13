using UnityEditor;
using UnityEngine;
using UniDecl.Runtime.Contexts;
using UniDecl.Runtime.Core;
using UniDecl.Runtime.Widgets;
using UniDecl.Editor.UIToolkit;
using UniDecl.Editor.UIToolkit.Renderers;

namespace UniDecl.Editor.UIToolKit.Examples
{
    using W = UniDecl.Runtime.Widgets;

    /// <summary>
    /// 状态化计数器组件 — 测试DOM重建能力
    /// </summary>
    public class CounterElement : Element<CounterElement.CounterState>
    {
        public class CounterState
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
                        state.Count++;
                    }),
                    new Button("Reset", () =>
                    {
                        state.Count = 0;
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
        public class TabState
        {
            public int CurrentTab;
            public string InputText = "默认文本";
            public string PasswordText = string.Empty;
            public string MultilineText = "多行\n输入";
            public int IntegerValue = 42;
            public float FloatValue = 3.14f;
            public int DropdownIndex;
            public int EnumValue;
        }

        private static readonly string[] TabNames =
        {
            "基础控件", "输入控件", "滑动与颜色", "布局容器", "进度与状态"
        };

        private static readonly string[] DropdownChoices = { "选项 A", "选项 B", "选项 C", "选项 D" };

        public override TabState BuildState() => new TabState();

        public override IElement Render(TabState state)
        {
            var tabButtons = new HorizontalLayout();
            for (int i = 0; i < TabNames.Length; i++)
            {
                var tabIndex = i;
                var name = tabIndex == state.CurrentTab ? $">> {TabNames[tabIndex]} <<" : TabNames[tabIndex];
                tabButtons.Add(new Button(name, () =>
                {
                    state.CurrentTab = tabIndex;
                }));
            }

            IElement content = state.CurrentTab switch
            {
                0 => BuildBasicControlsTab(),
                1 => BuildInputControlsTab(state),
                2 => BuildSliderColorTab(),
                3 => BuildLayoutTab(),
                4 => BuildProgressStateTab(),
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
            };
        }

        // === Tab 2: 输入控件 ===
        private static Panel BuildInputControlsTab(TabState state)
        {
            var textField = new TextField(state.InputText, "请输入...")
            {
                OnValueChange = (next, _) => state.InputText = next,
            };

            var passwordField = new TextField(state.PasswordText, "密码框")
            {
                IsPassword = true,
                OnValueChange = (next, _) => state.PasswordText = next,
            };

            var multilineField = new TextField(state.MultilineText, "")
            {
                IsMultiline = true,
                OnValueChange = (next, _) => state.MultilineText = next,
            };

            var integerField = new IntegerField(state.IntegerValue)
            {
                OnValueChanged = (next, _) => state.IntegerValue = next,
            };

            var floatField = new FloatField(state.FloatValue)
            {
                OnValueChanged = (next, _) => state.FloatValue = next,
            };

            var dropdown = new W.Dropdown("选择项目", DropdownChoices, state.DropdownIndex)
            {
                OnSelectionChanged = idx => state.DropdownIndex = idx,
            };

            var enumField = new W.EnumField("日志级别", typeof(LogType), state.EnumValue)
            {
                OnValueChanged = value => state.EnumValue = value,
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

        [MenuItem("Window/UniDecl UIToolkit Example")]
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
