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

    public class UE5StyleExample : EditorWindow
    {
        private const string UeThemePath = "Themes/UE5Style";
        private const string HybridThemePath = "Themes/UnityUEHybridStyle";
        private const string DefaultThemePath = "Themes/DefaultStyle";

        private static string _activeThemePath = UeThemePath;
        private UIToolkitRenderManager _manager;

        [MenuItem("Window/UniDecl/UE5 Style Example")]
        public static void ShowWindow()
        {
            ShowWindowWithTheme(UeThemePath, "UniDecl Style");
        }

        [MenuItem("Window/UniDecl/Style Example/UE Theme")]
        public static void ShowUeThemeWindow()
        {
            ShowWindowWithTheme(UeThemePath, "UniDecl Style (UE)");
        }

        [MenuItem("Window/UniDecl/Style Example/Default Theme")]
        public static void ShowDefaultThemeWindow()
        {
            ShowWindowWithTheme(DefaultThemePath, "UniDecl Style (Default)");
        }

        [MenuItem("Window/UniDecl/Style Example/Unity UE Hybrid Theme")]
        public static void ShowHybridThemeWindow()
        {
            ShowWindowWithTheme(HybridThemePath, "UniDecl Style (Hybrid)");
        }

        private static void ShowWindowWithTheme(string themePath, string title)
        {
            _activeThemePath = themePath;
            var window = GetWindow<UE5StyleExample>(title);
            window.titleContent = new GUIContent(title);
            window.Repaint();
        }

        public void CreateGUI()
        {
            rootVisualElement.Clear();

            _manager = new UIToolkitRenderManager();
            _manager.LoadStyleSheetFromResources(_activeThemePath);
            var isUeTheme = string.Equals(_activeThemePath, UeThemePath, System.StringComparison.OrdinalIgnoreCase);
            var isHybridTheme = string.Equals(_activeThemePath, HybridThemePath, System.StringComparison.OrdinalIgnoreCase);

            var content = new VerticalLayout
            {
                BuildThemeSwitcherSection(),
                new Label(isUeTheme ? "Unreal Engine 5 Style" : isHybridTheme ? "Unity UE Hybrid Style" : "Default Style")
                    .With(new UITKStyle().AddClass("ud-label--heading")),
                new Label("UniDecl UIToolkit Theme Demo")
                    .With(new UITKStyle { Color = new Color(0.6f, 0.6f, 0.6f) }),

                BuildButtonsSection(),
                BuildInputSection(),
                BuildSlidersAndColorSection(),
                BuildLayoutSection(),
                BuildHelpBoxSection(),
                BuildToolbarSection(),
                BuildMarkdownSection(),
            };

            var root = new Panel
            {
                new ScrollView { content }
                    .With(new UITKStyle().AddClass("ud-root-scrollview")),
            }
            .With(new UITKStyle().AddClass("ud-root"));

            var ve = _manager.RenderRoot(root);
            if (ve != null)
                rootVisualElement.Add(ve);
        }

        private IElement BuildThemeSwitcherSection()
        {
            var isUeTheme = string.Equals(_activeThemePath, UeThemePath, System.StringComparison.OrdinalIgnoreCase);
            var isHybridTheme = string.Equals(_activeThemePath, HybridThemePath, System.StringComparison.OrdinalIgnoreCase);
            var isDefaultTheme = string.Equals(_activeThemePath, DefaultThemePath, System.StringComparison.OrdinalIgnoreCase);
            return new Panel
            {
                new Label("Theme")
                    .With(new UITKStyle().AddClass("ud-label--divider")),
                new HorizontalLayout
                {
                    new Button("UE", () => SwitchTheme(UeThemePath))
                        .With(new UITKStyle().AddClass(isUeTheme ? "ud-button" : "ud-button--secondary")),
                    new Button("Hybrid", () => SwitchTheme(HybridThemePath))
                        .With(new UITKStyle().AddClass(isHybridTheme ? "ud-button" : "ud-button--secondary")),
                    new Button("Default", () => SwitchTheme(DefaultThemePath))
                        .With(new UITKStyle().AddClass(isDefaultTheme ? "ud-button" : "ud-button--secondary")),
                }.With(new UITKStyle().AddClass("ud-button-row")),
            }.With(new UITKStyle().AddClass("ud-panel"));
        }

        private void SwitchTheme(string themePath)
        {
            if (string.Equals(_activeThemePath, themePath, System.StringComparison.OrdinalIgnoreCase))
                return;

            _activeThemePath = themePath;
            CreateGUI();
        }

        private static IElement BuildButtonsSection()
        {
            return new Panel
            {
                new Label("Buttons")
                    .With(new UITKStyle().AddClass("ud-label--divider")),
                new HorizontalLayout
                {
                    new Button("Primary Action", () => Debug.Log("UE5 Primary"))
                        .With(new UITKStyle().AddClass("ud-button")),
                    new Button("Secondary Action", () => Debug.Log("UE5 Secondary"))
                        .With(new UITKStyle().AddClass("ud-button--secondary")),
                    new Button("Delete", () => Debug.Log("UE5 Danger"))
                        .With(new UITKStyle().AddClass("ud-button--danger")),
                    new Button("Disabled", () => { }) { Enabled = false }
                        .With(new UITKStyle().AddClass("ud-button--secondary")),
                }.With(new UITKStyle().AddClass("ud-button-row")),
            }.With(new UITKStyle().AddClass("ud-panel"));
        }

        private static IElement BuildInputSection()
        {
            return new Panel
            {
                new Label("Input Fields")
                    .With(new UITKStyle().AddClass("ud-label--divider")),
                new Label("Text Input")
                    .With(new UITKStyle().AddClass("ud-label--subheading")),
                new TextField("Enter text...", "Placeholder")
                    .With(new UITKStyle().AddClass("ud-textfield")),
                new TextField("password123", "Password") { IsPassword = true }
                    .With(new UITKStyle().AddClass("ud-textfield")),
                new TextField("Line 1\nLine 2\nLine 3", "") { IsMultiline = true }
                    .With(new UITKStyle().AddClass("ud-textfield")),
                new Label("Numeric Input")
                    .With(new UITKStyle().AddClass("ud-label--subheading")),
                new IntegerField(42)
                    .With(new UITKStyle().AddClass("ud-numfield")),
                new FloatField(3.14f)
                    .With(new UITKStyle().AddClass("ud-numfield")),
                new Label("Selection")
                    .With(new UITKStyle().AddClass("ud-label--subheading")),
                new W.Dropdown("Choose Option", new[] { "Option A", "Option B", "Option C" }, 0)
                    .With(new UITKStyle().AddClass("ud-dropdown")),
                new W.EnumField("Log Level", typeof(LogType), 0)
                    .With(new UITKStyle().AddClass("ud-enumfield")),
                new W.Toggle("Enable Feature", true)
                    .With(new UITKStyle().AddClass("ud-toggle")),
            }.With(new UITKStyle().AddClass("ud-panel"));
        }

        private static IElement BuildSlidersAndColorSection()
        {
            return new Panel
            {
                new Label("Sliders & Colors")
                    .With(new UITKStyle().AddClass("ud-label--divider")),
                new W.Slider("Volume", 75f, 0f, 100f)
                    .With(new UITKStyle().AddClass("ud-slider")),
                new W.Slider("Brightness", 0.5f, 0f, 1f)
                    .With(new UITKStyle().AddClass("ud-slider")),
                new W.MinMaxSlider("Distance Range", 10f, 90f, 0f, 100f)
                    .With(new UITKStyle().AddClass("ud-slider")),
                new Label("Progress Bars")
                    .With(new UITKStyle().AddClass("ud-label--subheading")),
                new ProgressBar(0.25f, "Compiling... 25%")
                    .With(new UITKStyle().AddClass("ud-progress-bar")),
                new ProgressBar(0.75f, "Building... 75%")
                    .With(new UITKStyle().AddClass("ud-progress-bar")),
                new ProgressBar(1.0f, "Complete")
                    .With(new UITKStyle().AddClass("ud-progress-bar")),
                new Label("Color Fields")
                    .With(new UITKStyle().AddClass("ud-label--subheading")),
                new W.ColorField("Accent", new Color(0.04f, 0.52f, 1.0f))
                    .With(new UITKStyle().AddClass("ud-colorfield")),
                new W.ColorField("Warning", new Color(0.91f, 0.65f, 0.19f))
                    .With(new UITKStyle().AddClass("ud-colorfield")),
            }.With(new UITKStyle().AddClass("ud-panel"));
        }

        private static IElement BuildLayoutSection()
        {
            var scrollContent = new VerticalLayout();
            for (int i = 1; i <= 8; i++)
                scrollContent.Add(new Label($"  Scroll Item #{i}")
                    .With(new UITKStyle().AddClass("ud-scroll-item")));

            return new Panel
            {
                new Label("Layout Containers")
                    .With(new UITKStyle().AddClass("ud-label--divider")),
                new Foldout("Details Panel (UE5 Foldout)")
                {
                    new W.Toggle("Cast Shadows", true)
                        .With(new UITKStyle().AddClass("ud-toggle")),
                    new W.Toggle("Receive Shadows", true)
                        .With(new UITKStyle().AddClass("ud-toggle")),
                    new W.Toggle("Affects World", false)
                        .With(new UITKStyle().AddClass("ud-toggle")),
                }.With(new UITKStyle().AddClass("ud-foldout")),
                new Label("Scroll View")
                    .With(new UITKStyle().AddClass("ud-label--subheading")),
                new ScrollView { scrollContent }
                    .With(new UITKStyle().AddClass("ud-scrollview", "ud-scrollview--compact")),
            }.With(new UITKStyle().AddClass("ud-panel"));
        }

        private static IElement BuildHelpBoxSection()
        {
            return new Panel
            {
                new Label("Notifications")
                    .With(new UITKStyle().AddClass("ud-label--divider")),
                new W.HelpBox("Shader compiled successfully.", HelpBoxMessageType.Info)
                    .With(new UITKStyle().AddClass("ud-helpbox-info")),
                new W.HelpBox("Texture resolution exceeds recommended size.", HelpBoxMessageType.Warning)
                    .With(new UITKStyle().AddClass("ud-helpbox-warning")),
                new W.HelpBox("Material shader compilation failed. Check output log.", HelpBoxMessageType.Error)
                    .With(new UITKStyle().AddClass("ud-helpbox-error")),
            }.With(new UITKStyle().AddClass("ud-panel"));
        }

        private static IElement BuildToolbarSection()
        {
            return new Panel
            {
                new Label("Toolbar")
                    .With(new UITKStyle().AddClass("ud-label--divider")),
                new W.Toolbar
                {
                    new W.ToolbarButton("Save", () => Debug.Log("Save")),
                    new W.ToolbarButton("Build", () => Debug.Log("Build")),
                    new W.ToolbarButton("Launch", () => Debug.Log("Launch")),
                    new W.ToolbarToggle("Live", false),
                    new W.ToolbarSearchField(),
                    new W.ToolbarMenu("Options", () => Debug.Log("Options clicked")),
                }.With(new UITKStyle().AddClass("ud-toolbar")),
                new Label("UE5 Style Theme Demo - End")
                    .With(new UITKStyle
                    {
                        Color = new Color(0.4f, 0.4f, 0.4f),
                        FontSize = 11,
                        UnityTextAlign = TextAnchor.MiddleCenter,
                    }.FlexRow().JustifyCenter()),
            }.With(new UITKStyle().AddClass("ud-panel"));
        }

        private static IElement BuildMarkdownSection()
        {
            var toc = new TocView(new[]
            {
                new TocEntry("Introduction", 1),
                new TocEntry("Getting Started", 2),
                new TocEntry("Installation", 3),
                new TocEntry("Configuration", 3),
                new TocEntry("Advanced Usage", 2),
                new TocEntry("Custom Styles", 3),
                new TocEntry("Event Callbacks", 3),
                new TocEntry("API Reference", 2),
            });

            return new Panel
            {
                new Label("MD Headings")
                    .With(new UITKStyle().AddClass("ud-label--divider")),
                new H1("H1 Document Title"),
                new H2("H2 Section Heading"),
                new H3("H3 Subsection"),
                new H4("H4 Detail"),
                new H5("H5 Supplementary"),
                new H6("H6 Footnote Level"),
                new Label("TocView (Left Navigation)")
                    .With(new UITKStyle().AddClass("ud-label--divider")),
                toc,
            }.With(new UITKStyle().AddClass("ud-panel"));
        }
    }
}
