using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UniDecl.Runtime.Core;
using UniDecl.Runtime.Widgets;
using UniDecl.Runtime.Widgets.MD;
using UniDecl.Editor.UIToolKit;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;

namespace UniDecl.Editor.UIToolKit.Examples
{
    using WM = UniDecl.Runtime.Widgets.MD;

    /// <summary>
    /// Markdown 渲染能力全量测试用例 — 模拟真实 MD 文档浏览器布局。
    /// 左侧单一导航侧边栏，右侧内容区按选中项切换显示。
    /// 通过 Window → UniDecl MD Test Case 打开。
    /// </summary>
    public class MarkdownTestCaseExample : EditorWindow
    {
        private const string DefaultThemePath = "Themes/DefaultStyle";
        private const string Ue5ThemePath = "Themes/UE5Style";
        private const string HybridThemePath = "Themes/UnityUEHybridStyle";

        private static string _activeThemePath = DefaultThemePath;
        private UIToolkitRenderManager _manager;
        private static string _selectedNavId = "block-heading";

        [MenuItem("Window/UniDecl MD Test Case")]
        public static void ShowWindow()
        {
            GetWindow<MarkdownTestCaseExample>("UniDecl MD Test Case");
        }

        private static bool IsTheme(string path)
            => string.Equals(_activeThemePath, path, StringComparison.OrdinalIgnoreCase);

        private static void SwitchTheme(string themePath)
        {
            if (IsTheme(themePath)) return;
            _activeThemePath = themePath;
            var win = GetWindow<MarkdownTestCaseExample>();
            win.Repaint();
            win.CreateGUI();
        }

        public void CreateGUI()
        {
            rootVisualElement.Clear();

            _manager = new UIToolkitRenderManager();
            _manager.LoadStyleSheetFromResources(_activeThemePath);

            var root = new Panel
            {
                new HorizontalLayout
                {
                    BuildSidebar(),
                    BuildMainContent(),
                }.With(new UITKStyle { FlexGrow = 1, MinHeight = 0 }),
            }
            .With(new UITKStyle().AddClass("ud-root"));

            var ve = _manager.RenderRoot(root);
            if (ve != null)
                rootVisualElement.Add(ve);
        }

        // =================================================================
        // 左侧导航栏 — 单一 TocView + 主题切换
        // =================================================================

        private IElement BuildSidebar()
        {
            var toc = new WM.TocView
            {
                Items = BuildNavTree(),
                SelectedId = _selectedNavId,
                OnSelectionChanged = id =>
                {
                    _selectedNavId = id;
                    CreateGUI();
                },
            };

            return new Panel
            {
                new HorizontalLayout
                {
                    new Button("Default", () => SwitchTheme(DefaultThemePath))
                        .With(new UITKStyle().AddClass(IsTheme(DefaultThemePath) ? "ud-button" : "ud-button--secondary")),
                    new Button("UE5", () => SwitchTheme(Ue5ThemePath))
                        .With(new UITKStyle().AddClass(IsTheme(Ue5ThemePath) ? "ud-button" : "ud-button--secondary")),
                    new Button("Hybrid", () => SwitchTheme(HybridThemePath))
                        .With(new UITKStyle().AddClass(IsTheme(HybridThemePath) ? "ud-button" : "ud-button--secondary")),
                }
                .With(new UITKStyle().AddClass("ud-button-row", "ud-sidebar-theme-row").Margin(0, 0, 6, 0)),

                new ScrollView { toc }.With(new UITKStyle { FlexGrow = 1, MinHeight = 0 }.AddClass("ud-sidebar-scroll")),
            }
            .With(new UITKStyle
            {
                Width = 200,
                MinWidth = 180,
                MaxWidth = 240,
                FlexGrow = 1,
                FlexShrink = 0,
                MinHeight = 0,
                BorderRightWidth = 1,
                BorderRightColor = new Color(1, 1, 1, 0.08f),
            }.FlexColumn().AddClass("ud-panel", "ud-sidebar").Margin(0).Padding(4, 4, 4, 4));
        }

        // =================================================================
        // 导航树结构
        // =================================================================

        private static List<WM.TocEntry> BuildNavTree()
        {
            return new List<WM.TocEntry>
            {
                new WM.TocEntry("block", "Block 级语法", 1)
                {
                    Children =
                    {
                        new WM.TocEntry("block-heading", "标题 (H1–H6)", 2),
                        new WM.TocEntry("block-code", "代码块", 2),
                        new WM.TocEntry("block-list", "列表", 2),
                        new WM.TocEntry("block-blockquote", "引用块", 2),
                        new WM.TocEntry("block-hr", "分隔线", 2),
                        new WM.TocEntry("block-paragraph", "段落", 2),
                    }
                },
                new WM.TocEntry("inline", "Inline 级语法", 1)
                {
                    Children =
                    {
                        new WM.TocEntry("inline-bold", "粗体 / 斜体 / 粗斜体", 2),
                        new WM.TocEntry("inline-code", "内联代码", 2),
                        new WM.TocEntry("inline-link", "链接与图片", 2),
                        new WM.TocEntry("inline-mixed", "混合格式", 2),
                        new WM.TocEntry("inline-escape", "转义与硬换行", 2),
                    }
                },
                new WM.TocEntry("widget", "Widget 控件", 1)
                {
                    Children =
                    {
                        new WM.TocEntry("widget-headings", "标题控件", 2),
                        new WM.TocEntry("widget-codeblock", "CodeBlock", 2),
                        new WM.TocEntry("widget-richquote", "RichText & Blockquote", 2),
                        new WM.TocEntry("widget-toc", "TocView (自身嵌套)", 2),
                    }
                },
                new WM.TocEntry("doc", "完整文档", 1),
                new WM.TocEntry("edge", "边界场景", 1),
            };
        }

        // =================================================================
        // 右侧内容区 — 按选中 Id 路由
        // =================================================================

        private IElement BuildMainContent()
        {
            var content = _selectedNavId switch
            {
                "block-heading" => BuildBlockHeadingContent(),
                "block-code" => BuildBlockCodeContent(),
                "block-list" => BuildBlockListContent(),
                "block-blockquote" => BuildBlockBlockquoteContent(),
                "block-hr" => BuildBlockHrContent(),
                "block-paragraph" => BuildBlockParagraphContent(),
                "inline-bold" => BuildInlineBoldContent(),
                "inline-code" => BuildInlineCodeContent(),
                "inline-link" => BuildInlineLinkContent(),
                "inline-mixed" => BuildInlineMixedContent(),
                "inline-escape" => BuildInlineEscapeContent(),
                "widget-headings" => BuildWidgetHeadingsContent(),
                "widget-codeblock" => BuildWidgetCodeBlockContent(),
                "widget-richquote" => BuildWidgetRichQuoteContent(),
                "widget-toc" => BuildWidgetTocContent(),
                "doc" => BuildFullDocumentContent(),
                "edge" => BuildEdgeCaseContent(),
                _ => BuildWelcomeContent(),
            };

            return new ScrollView
            {
                new Panel
                {
                    new Label("MD Test Case")
                        .With(new UITKStyle().AddClass("ud-label--heading")),
                    content,
                }.With(new UITKStyle().AddClass("ud-panel").Padding(10)),
            }
            .With(new UITKStyle { FlexGrow = 1, MinHeight = 0 });
        }

        // =================================================================
        // Welcome
        // =================================================================

        private static IElement BuildWelcomeContent()
        {
            return new WM.MarkdownView(@"
# UniDecl Markdown 渲染测试

欢迎使用 Markdown 渲染测试面板。

请从左侧导航栏选择一个测试章节：

- **Block 级语法** — 标题、代码块、列表、引用、分隔线、段落
- **Inline 级语法** — 粗体、斜体、代码、链接、图片、转义
- **Widget 控件** — 独立 Widget 控件展示
- **完整文档** — 真实文档风格的全量渲染
- **边界场景** — 空输入、特殊字符等边界用例

---
");
        }

        // =================================================================
        // Block 级语法
        // =================================================================

        private static IElement BuildBlockHeadingContent()
        {
            return new WM.MarkdownView(@"
# ATX 标题语法

## 二级标题
### 三级标题
#### 四级标题
##### 五级标题
###### 六级标题

---

# Setext 标题语法

Setext H1 标题
===

Setext H2 标题
---
");
        }

        private static IElement BuildBlockCodeContent()
        {
            return new WM.MarkdownView(@"
# 围栏代码块

使用三个反引号围栏：

```csharp
public class Example
{
    public void Hello() => Debug.Log(""Hello"");
}
```

使用波浪号围栏：

~~~python
def hello():
    print(""Hello"")
~~~

无语言标注的代码块：

```
plain text block
no language specified
```
");
        }

        private static IElement BuildBlockListContent()
        {
            return new WM.MarkdownView(@"
# 列表

## 无序列表

- 使用减号的列表项 A
- 使用减号的列表项 B

* 使用星号的列表项 C
* 使用星号的列表项 D

+ 使用加号的列表项 E
+ 使用加号的列表项 F

## 有序列表

1. 第一步：打开项目
2. 第二步：导入资源
3. 第三步：配置场景
4. 第四步：构建运行
");
        }

        private static IElement BuildBlockBlockquoteContent()
        {
            return new WM.MarkdownView(@"
# 引用块

> 这是一段引用文字。
> 可以包含多行内容。

> **注意**：引用块中可以包含行内格式化。
> 例如 `代码`、**粗体**、*斜体*。

> # 引用中的标题
>
> 这是一段包含 `内联代码` 的引用文本。
>
> - 引用中的列表项
> - 另一个列表项
");
        }

        private static IElement BuildBlockHrContent()
        {
            return new WM.MarkdownView(@"
# 水平分割线

上方文本。

---

使用三个减号。

***

使用三个星号。

___

使用三个下划线。

---

下方文本。
");
        }

        private static IElement BuildBlockParagraphContent()
        {
            return new WM.MarkdownView(@"
# 段落

这是第一段普通文本。段落由连续的非空行组成，遇到空行结束。

这是第二段普通文本，段落之间用空行分隔。段落中可以包含 `内联代码`、**粗体**、*斜体* 等行内格式。

这是一段较长的文本，用于测试段落渲染的换行和显示效果。Markdown 段落由连续的非空行组成，直到遇到空行或其他块级语法标记才会结束。当文本很长时，渲染控件应正确处理换行和滚动。
");
        }

        // =================================================================
        // Inline 级语法
        // =================================================================

        private static IElement BuildInlineBoldContent()
        {
            return new WM.MarkdownView(@"
# 粗体 / 斜体 / 粗斜体

## 粗体

**双星号粗体** 文本。

__双下划线粗体__ 文本。

## 斜体

*单星号斜体* 文本。

_单下划线斜体_ 文本。

## 粗斜体

***三星号粗斜体*** 文本。

___三下划线粗斜体___ 文本。

## 嵌套

**粗体中嵌套 `代码` 和 *斜体*。**
");
        }

        private static IElement BuildInlineCodeContent()
        {
            return new WM.MarkdownView(@"
# 内联代码

在文本中使用 `int x = 42;` 表示内联代码。

支持多个内联代码：`MdParser`、`MdBlock`、`MdInline`。

`代码中**不会**触发格式化`。
");
        }

        private static IElement BuildInlineLinkContent()
        {
            return new WM.MarkdownView(@"
# 链接与图片

## 链接

访问 [UniDecl GitHub 仓库](https://github.com/wlxms/UniDecl) 获取源码。

查看 [Unity 官方文档](https://docs.unity3d.com/) 了解更多。

## 图片

![UniDecl Logo](https://example.com/logo.png)

![截图示例](https://example.com/screenshot.png)

> 点击链接或图片会触发 OnUrlClick 回调。
");
        }

        private static IElement BuildInlineMixedContent()
        {
            return new WM.MarkdownView(@"
# 混合行内格式

这是 **粗体** 和 *斜体* 混合的文本。

段落中包含 `代码`、[链接](https://example.com) 和 ![图片](https://example.com/img.png)。

***粗斜体*** 和 `代码` 可以自由组合。

1. **粗体**列表项
2. *斜体*列表项
3. `代码`列表项
4. [链接](https://example.com)列表项
5. 混合：**粗体** + *斜体* + `代码`
");
        }

        private static IElement BuildInlineEscapeContent()
        {
            return new WM.MarkdownView(@"
# 转义与硬换行

## 反斜杠转义

使用 \*转义星号\* 不会变为斜体。

使用 \[转义方括号\] 不会变为链接。

## 硬换行

第一行文本。
第二行文本（硬换行）。

第三行文本。
第四行文本。

## 特殊字符

文件路径：C:\Users\test\file.txt

数学表达式：x = a * b + c - d

URL：https://github.com/wlxms/UniDecl

邮箱：user@example.com
");
        }

        // =================================================================
        // Widget 控件
        // =================================================================

        private static IElement BuildWidgetHeadingsContent()
        {
            return new Panel
            {
                new Label("H1–H6 标题控件"),
                new H1("H1 文档标题"),
                new H2("H2 章节标题"),
                new H3("H3 小节标题"),
                new H4("H4 细节标题"),
                new H5("H5 补充说明"),
                new H6("H6 脚注级标题"),
            };
        }

        private static IElement BuildWidgetCodeBlockContent()
        {
            return new Panel
            {
                new Label("CodeBlock 代码块控件"),
                new WM.CodeBlock("csharp", "public void Hello()\n{\n    Debug.Log(\"Hello\");\n}"),
                new Label(""),
                new WM.CodeBlock(null, "无语言标注的代码块"),
            };
        }

        private static IElement BuildWidgetRichQuoteContent()
        {
            var inlines = new List<UniDecl.Runtime.MD.MdInline>
            {
                UniDecl.Runtime.MD.MdInline.PlainText("普通文本 "),
                UniDecl.Runtime.MD.MdInline.Bold("粗体文本 "),
                UniDecl.Runtime.MD.MdInline.Italic("斜体文本 "),
                UniDecl.Runtime.MD.MdInline.BoldItalic("粗斜体文本 "),
                UniDecl.Runtime.MD.MdInline.Code("inline code"),
                UniDecl.Runtime.MD.MdInline.LineBreak(),
                UniDecl.Runtime.MD.MdInline.Link("可点击链接", "https://github.com/wlxms/UniDecl"),
            };

            return new Panel
            {
                new Label("RichText 富文本控件"),
                new WM.RichText(inlines, url => Debug.Log($"[RichText] {url}")),
                new Label(""),
                new Label("Blockquote 引用控件"),
                new WM.Blockquote
                {
                    new Label("这是一段引用内容。"),
                    new Label("可包含多个子元素。"),
                },
                new Label(""),
                new Label("Divider 分隔线控件"),
                new WM.Divider(),
                new Label("分隔线上方和下方"),
            };
        }

        private static IElement BuildWidgetTocContent()
        {
            return new Panel
            {
                new Label("TocView 目录导航控件（自身嵌套展示）"),
                new WM.TocView(new[]
                {
                    new WM.TocEntry("root", "文档标题", 1)
                    {
                        Children =
                        {
                            new WM.TocEntry("s1", "第一章", 2)
                            {
                                Children =
                                {
                                    new WM.TocEntry("s1-1", "1.1 概述", 3),
                                    new WM.TocEntry("s1-2", "1.2 详解", 3),
                                }
                            },
                            new WM.TocEntry("s2", "第二章", 2)
                            {
                                Children =
                                {
                                    new WM.TocEntry("s2-1", "2.1 概述", 3),
                                    new WM.TocEntry("s2-2", "2.2 详解", 3),
                                    new WM.TocEntry("s2-3", "2.3 补充", 4),
                                }
                            },
                            new WM.TocEntry("appendix", "附录", 2),
                        }
                    },
                })
                {
                    SelectedId = "s1",
                    OnSelectionChanged = id => Debug.Log($"[Nested TocView] Selected: {id}"),
                },
            };
        }

        // =================================================================
        // 完整文档
        // =================================================================

        private static IElement BuildFullDocumentContent()
        {
            const string fullDocument = @"
# UniDecl 使用手册

## 简介

**UniDecl** 是一个仿 React 风格的 Unity GUI 框架，支持声明式 UI 构建。

它提供了 `MarkdownView` 控件，可将 Markdown 字符串直接渲染为 Unity Editor 界面。

## 功能特性

- **标题**：H1 ~ H6 均支持（ATX 和 Setext 两种语法）
- **行内格式**：粗体 `**text**`、斜体 `*text*`、粗斜体 `***text***`
- **内联代码**：使用反引号 `` `code` ``
- **链接**：`[text](url)` 支持 `OnUrlClick` 回调
- **图片**：`![alt](url)` 支持 `OnUrlClick` 回调
- 有序列表与无序列表
- 围栏代码块（``` 和 ~~~ 两种围栏符号）
- 引用块
- 水平分割线

---

## 快速开始

### 安装

在 Unity Package Manager 中通过 Git URL 添加：

```
https://github.com/wlxms/UniDecl.git
```

### 配置

创建一个继承自 `EditorWindow` 的类：

```csharp
public class MyWindow : EditorWindow
{
    private UIToolkitRenderManager _manager;

    public void CreateGUI()
    {
        _manager = new UIToolkitRenderManager();
        var root = new Panel { /* ... */ };
        var ve = _manager.RenderRoot(root);
        rootVisualElement.Add(ve);
    }
}
```

### 使用 MarkdownView

```csharp
var mdView = new MarkdownView(markdownText)
{
    OnUrlClick = url => Debug.Log(""Clicked: "" + url),
};
```

## 链接与图片

访问 [UniDecl GitHub 仓库](https://github.com/wlxms/UniDecl) 获取最新源码。

查看 [Unity 官方文档](https://docs.unity3d.com/) 了解更多。

![UniDecl Logo](https://example.com/logo.png)

> **注意**：点击链接或图片会触发 `OnUrlClick` 回调,
> 而不是直接打开浏览器，从而支持自定义跳转逻辑。

## API 参考

1. `MarkdownView` — 完整 Markdown 渲染控件
2. `RichText` — 富文本渲染控件
3. `CodeBlock` — 代码块控件
4. `Blockquote` — 引用块容器
5. `Divider` — 分隔线控件
6. `TocView` — 目录导航控件
7. `H1` ~ `H6` — 标题控件

---

### 样式自定义

所有 MD 控件均使用 CSS class 名称进行样式控制：

* `ud-markdown` — 根容器
* `ud-md-paragraph` — 段落
* `ud-md-codeblock` — 代码块容器
* `ud-md-blockquote` — 引用块
* `ud-heading` / `ud-h1` ~ `ud-h6` — 标题
* `ud-toc-item` — 目录条目

### 注意事项

* 不支持嵌套引用和嵌套列表
* 不支持表格语法
* 不支持任务列表（checkbox）
* 行内格式不支持跨行匹配
";

            return new WM.MarkdownView(fullDocument,
                url => Debug.Log($"[FullDoc] URL clicked: {url}"));
        }

        // =================================================================
        // 边界场景
        // =================================================================

        private static IElement BuildEdgeCaseContent()
        {
            return new WM.MarkdownView(@"
# 边界与特殊场景

## 空字符串输入

（上方为空字符串输入，无可见输出）

## 仅空行

（上方为仅空行输入，无可见输出）

## 仅标题

# Only Heading

## 连续分割线

---
***
___
---

## 特殊字符

包含特殊字符的文本：< > & "" ' 

文件路径：C:\Users\test\file.txt

URL：https://github.com/wlxms/UniDecl

邮箱：user@example.com

## 长文本段落

这是一段很长的文本，用于测试段落渲染的换行和显示效果。Markdown 段落由连续的非空行组成，直到遇到空行或其他块级语法标记才会结束。当文本很长时，渲染控件应正确处理换行和滚动。在 Unity Editor 中，段落通常通过 Label 或 RichText 控件显示，支持富文本格式。

## 无关闭围栏的代码块

```csharp
public void Unclosed()
{
    // This fence is never closed

## 列表项内含行内格式

1. **粗体**列表项
2. *斜体*列表项
3. `代码`列表项
4. [链接](https://example.com)列表项
5. ***粗斜体***列表项
");
        }
    }
}
