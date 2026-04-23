using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UniDecl.Runtime.Core;
using UniDecl.Runtime.Navigation;
using UniDecl.Runtime.Widgets;
using UniDecl.Runtime.Widgets.MD;
using UniDecl.Editor.UIToolKit;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;

namespace UniDecl.Editor.UIToolKit.Examples
{
    /// <summary>
    /// 导航系统演示窗口 — 展示 Anchor + NavigateTo + HostManager + URL 路由。
    /// 通过 Window → UniDecl Navigation 打开。
    /// 继承 UIToolkitHostEditorWindow 自动获得 host 注册、ready replay、跨窗口导航能力。
    /// </summary>
    [DeclHostWindow("nav-example")]
    public class NavigationExample : UIToolkitHostEditorWindow<UIToolkitRenderManager>
    {
        private UIToolkitRenderManager _manager;

        [MenuItem("Window/UniDecl/Navigation")]
        public static void ShowWindow()
        {
            GetWindow<NavigationExample>("UniDecl Navigation");
        }

        protected override string HostName => "nav-example";

        protected override void LoadStyles(UIToolkitRenderManager manager)
        {
            manager.LoadStyleSheetFromResources("Themes/DefaultStyle");
        }

        protected override IElement BuildContent()
        {
            return new HorizontalLayout
            {
                BuildSidebar(),
                BuildMainContent(),
            }.With(new UITKStyle { FlexGrow = 1, MinHeight = 0 });
        }

        private IElement BuildSidebar()
        {
            var toc = new TocView
            {
                Items = new List<TocEntry>
                {
                    new TocEntry("getting-started", "Getting Started", 1),
                    new TocEntry("installation", "Installation", 2),
                    new TocEntry("configuration", "Configuration", 2),
                    new TocEntry("api-reference", "API Reference", 1),
                    new TocEntry("core-types", "Core Types", 2),
                    new TocEntry("renderers", "Renderers", 2),
                    new TocEntry("advanced", "Advanced Topics", 1),
                },
                OnSelectionChanged = id =>
                {
                    Manager.NavigateTo(id);
                },
            };

            return new Panel
            {
                new Label("Navigation")
                    .With(new UITKStyle().AddClass("ud-label--heading")),
                new ScrollView { toc }
                    .With(new UITKStyle { FlexGrow = 1, MinHeight = 0 }),
            }
            .With(new UITKStyle
            {
                Width = 180,
                MinWidth = 160,
                MaxWidth = 220,
                FlexGrow = 1,
                FlexShrink = 0,
                MinHeight = 0,
                BorderRightWidth = 1,
                BorderRightColor = new Color(1, 1, 1, 0.08f),
            }.FlexColumn().AddClass("ud-panel").Padding(4));
        }

        private IElement BuildMainContent()
        {
            return new ScrollView
            {
                new Panel
                {
                    new H1("Getting Started").With(new Anchor("getting-started")),
                    new Label("Welcome to UniDecl Navigation System."),

                    new H2("Installation").With(new Anchor("installation")),
                    new Label("Install via Package Manager or Git URL."),

                    new H2("Configuration").With(new Anchor("configuration")),
                    new Label("Basic and advanced configuration options."),

                    new H1("API Reference").With(new Anchor("api-reference")),
                    new Label("Core types and renderer interfaces."),

                    new H2("Core Types").With(new Anchor("core-types")),
                    new Label("Element, DOMTree, EventDispatcher, etc."),

                    new H2("Renderers").With(new Anchor("renderers")),
                    new Label("UIToolkitRenderManager and element renderers."),

                    new H1("Advanced Topics").With(new Anchor("advanced")),
                    new Label("Deep dive into navigation and event system."),
                },
            };
        }
    }
}
