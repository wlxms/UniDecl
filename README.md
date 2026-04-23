# UniDecl

A React-style declarative GUI engine for Unity, supporting both IMGUI and UI Toolkit backends.

## Design Philosophy

### Why class-based elements instead of struct?

In early iterations, UI elements were defined as `struct` implementing interfaces like `IElement`. This appeared to give "zero allocation" benefits on paper, but in practice every `params IElement[] children` or `IEnumerable<IElement>` parameter caused **implicit boxing** on every frame — the struct was boxed to a heap-allocated reference just to pass it into a container. For a declarative framework that rebuilds the element tree every frame (especially under IMGUI), this created **more GC pressure than plain classes**.

UniDecl uses `class Element` as the base. Not because it's faster, but because it's **honest** — the allocation cost is explicit and predictable, rather than masquerading as zero-cost while silently boxing on every frame. This honesty also unlocks real engineering benefits:

- Reference identity for key-based diffing (`ReferenceEquals`)
- No surprises from value-type copy semantics
- Natural use of `virtual` dispatch instead of interface dispatch + boxing

> A struct that claims zero allocation but silently boxes is more expensive than a class that honestly allocates.

### Why a two-phase pipeline (BuildDOM → Render)?

A single-pass "render as you traverse" approach is the natural intuition — draw what you see. But intuition is often shortsighted. When you need incremental updates, you find you have no old tree to compare against. When a partial rebuild must re-inject ancestor contexts, you find you've lost that information.

UniDecl splits rendering into two phases, like reading blueprints before laying bricks:

- **BuildDOM** — *understand*: expands the declarative element tree into a `DOMNode` tree, resolving the semantic relationships of Context, State, and Consumer
- **Render** — *act*: traverses the understood structure and invokes renderers to materialize it into pixels

This separation is not an optimization trick — it's an **architectural stance**: understanding and acting are fundamentally different things, and conflating them compromises both. It is this separation that makes incremental rebuild and diff possible in the first place.

### Why support multiple backends?

Unity is transitioning from IMGUI to UI Toolkit, and no one knows how long this transition will take. A framework bound to a single backend is making a choice on behalf of developers — who haven't made that choice yet.

UniDecl's core (`Runtime/Core`) holds no backend opinion. It manages a logical element tree: builds it, diffs it, rebuilds it. How that tree is ultimately rendered — as `GUILayout` calls, `VisualElement` instances, or a third backend that doesn't exist yet — is the renderer's responsibility, not the framework's.

> A framework should make room for backend diversity, not promises about backend singularity.

### Why not build a custom style/theme system?

UI Toolkit has USS. IMGUI has GUIStyle. These are the platform's own answers to the problem of styling UI. Building a parallel style system — a CSS parser, theme assets, pseudo-class state machines, transition interpolators — isn't solving a problem; it's dismissing the platform's judgment.

UniDecl's style layer is thin to the point of translucency: `InlineStyle` carries class names and dimension hints; `UITKStyle` maps directly to native types like `StyleLength`. The framework's ambition is not to replace the backend's style system, but to **step aside at the right boundary**.

> The best abstraction isn't one that covers everything — it's one that knows where to stop.

### Why enforce mutual exclusion on element interfaces?

`IContextProvider`, `IContextConsumer`, and `IContainerElement` are structural roles. An element can be one, but not two. This is not merely an engineering constraint — it's a requirement for **identity clarity**. If an element is both a container and a context provider, its semantics in the DOM tree become ambiguous: should its children be expanded, or treated as a single wrapped child?

UniDecl enforces this mutual exclusion at build time, throwing on violation. This isn't rigidity — it's ending ambiguity at the earliest possible moment, rather than letting it hide in subtle rendering discrepancies and consume developer time.

> An ambiguous identity is a bug's favorite hiding place.

### Structure is layout

In the traditional CSS/USS mindset, layout is a style property — you write `display: flex`, `flex-direction: column`, and layout information gets buried in a stylesheet alongside colors, font sizes, and border radii. Reading the code, you must hold both the element structure and the style file in your head to reconstruct the final layout. Layout is **hidden**.

UniDecl treats layout not as decoration, but as **skeleton**. And a skeleton should grow on the element tree, not hide in a stylesheet:

```csharp
new VerticalLayout                    // layout = structure
{
    new Label("Title"),
    new HorizontalLayout              // nesting = composition
    {
        new TextField(_name),
        new Button("Submit", OnSubmit),
    },
}
```

What you see is what you get. No implicit `display: flex`, no reverse-engineering layout intent from a stylesheet. The element tree is the visual expression of layout. Style makes the skeleton look good — color, spacing, border radius — but it never reshapes the bones.

> Layout is a structural concern, not a stylistic one. Don't let skin dictate bone.

### Encapsulation over exposure

Many frameworks expose the renderer as a first-class concept: you write a `Renderer`, manually call `GUILayout.Label()` or `new Label()`, then register it into the system. This means consumers must understand the renderer registration mechanism, its lifecycle, and the backend API.

UniDecl folds this complexity into the Widget itself. A `Button` is not an interface that needs a companion renderer — it's a **complete capsule**: element definition + rendering contract, matched by the framework behind the scenes. The user only needs to know one thing: `new Button("OK", () => { })`.

Renderers still exist, but they retreat beneath the framework's skin. Just as you don't need to understand neural signaling to lift your arm, you don't need to understand renderer registration to place a button on screen.

The boundary of encapsulation falls between "what" and "how". Widgets answer "what", renderers answer "how". Users only converse with "what".

> Good encapsulation doesn't hide implementation — it makes you never need to know it exists.

### Behavior lives with structure

In imperative UI, structure and behavior are separated: you construct a button in one place, subscribe to events in another, and handle callbacks in a third. As UI grows complex, this separation turns code into scattered puzzle pieces — you must jump across multiple locations to understand a single button's full lifecycle.

UniDecl's declarative event system lets behavior and structure **coexist**:

```csharp
new Button("Delete", () =>
{
    _items.Remove(selected);
    NotifyChanged();
})
```

The event's intent, trigger, and response — all in a single expression. No `AddListener`, no `RegisterCallback`, no handler method scattered elsewhere. An element is both a description of structure and a carrier of behavior.

At a deeper level, this follows the core thesis of declarative UI: **describe "what", not "how"**. "What" doesn't stop at what the UI looks like — it extends to what the UI does when interacted with. Tearing these apart is an artificial separation of a single, complete intention.

> A button that requires you to visit three places to fully understand is not a declarative button.

## Architecture

```
UniDecl/
├── Runtime/
│   ├── Core/              # Backend-agnostic abstractions
│   │   ├── IElement.cs            # Element base interface
│   │   ├── Element.cs             # Abstract base (Element, ContainerElement, Element<TState>)
│   │   ├── DOMTree.cs             # Two-phase DOM construction + diff rebuild
│   │   ├── DOMNode.cs             # Node in the DOM tree
│   │   ├── ElementRenderHost.cs   # Render pipeline: BuildDOM → Render, event dispatch
│   │   ├── IElementRender.cs      # Renderer + Updater interfaces
│   │   ├── Context/               # ContextStack, ContextProvider, ContextConsumer
│   │   ├── State/                 # StateManager, StateStack, ElementState
│   │   └── Event/                 # EventDispatcher (generic struct events)
│   │
│   ├── Components/         # Backend-agnostic element definitions
│   │   ├── InlineStyle.cs         # Lightweight inline style (class names + dimensions)
│   │   └── IInlineStyle.cs        # Inline style interface
│   │
│   ├── Widgets/            # Reusable element definitions (class-based)
│   │   ├── Label, Button, TextField, Slider, Toggle, ...
│   │   ├── VerticalLayout, HorizontalLayout, ScrollView, Foldout, ...
│   │   ├── ListView, TreeView, MultiColumnListView
│   │   ├── Toolbar, ToolbarButton, ToolbarToggle, ...
│   │   ├── UE/                     # Unity Editor specific (UeCard)
│   │   └── MD/                     # Markdown widgets (see below)
│   │
│   ├── Contexts/           # Built-in context providers
│   │   └── DisableContext.cs
│   │
│   └── MD/                 # Markdown parser
│       ├── MdParser.cs             # Lightweight, dependency-free parser
│       ├── MdBlock.cs              # Block-level AST (Heading, Paragraph, CodeBlock, ...)
│       └── MdInline.cs             # Inline-level AST (Bold, Italic, Code, Link, ...)
│
├── UIToolKit/
│   ├── Runtime/
│   │   ├── UITKStyle.cs            # UI Toolkit native style bridge
│   │   └── Widgets/                # UITK-specific widgets (ListView, TreeView, ...)
│   │
│   └── Editor/
│       ├── UIToolkitRenderManager.cs   # ElementRenderHost<VisualElement> implementation
│       ├── Renderers/                 # 55+ renderers returning VisualElement
│       │   ├── UE/                   # Unity Editor field renderers
│       │   └── MD/                   # Markdown widget renderers
│       ├── Style/                     # USS style sheets
│       └── Examples/                  # Example EditorWindows
│
└── Editor/
    └── UniDecl.Editor.asmdef
```

## Core Concepts

### Elements

All UI elements extend `Element` (or `ContainerElement` for layout containers):

```csharp
// Leaf element — rendered by a registered renderer
public class Label : Element
{
    public string Text { get; set; }
    public override IElement Render() => null;  // no sub-tree
    public Label(string text) { Text = text; }
}

// Container element — holds children
public class VerticalLayout : ContainerElement
{
    public override IEnumerable<IElement> Children => ...;
    public override void Add(IElement element) => ...;
}
```

Declarative composition:
```csharp
new VerticalLayout
{
    new Label("Username:"),
    new TextField(_username, v => _username = v),
    new Button("Submit", OnSubmit),
}
```

### Stateful Elements

UniDecl provides struct-based immutable state management through `Element<TState>`:

```csharp
public class Counter : Element<Counter.CounterState>
{
    public struct CounterState { public int Count; }

    public override CounterState BuildState() => new CounterState();

    public override IElement Render(CounterState state)
    {
        return new Button($"Count: {state.Count}", () =>
        {
            SetState(s => new CounterState { Count = s.Count + 1 });
            // SetState automatically triggers UI rebuild
        });
    }
}
```

**Struct-based** state enforces immutability and guarantees every update creates a new state instance. Use `SetState()` to update, which automatically triggers UI rebuild.

For detailed state management patterns and best practices, see [STATE_MANAGEMENT.md](STATE_MANAGEMENT.md).

### Context System

Inject values into the element tree and consume them at any depth:

```csharp
// Provide
new DisableContext(true)
{
    new ContextConsumer(reader =>
    {
        var ctx = reader.Get<DisableContext>();
        return new Label(ctx?.Value == true ? "Disabled" : "Enabled");
    })
}
```

Contexts use a stack-based scope: nested providers of the same type shadow outer ones. The build phase pushes/pops contexts automatically; the render phase uses pre-computed context chains for partial rebuilds.

### Inline Style

Attach layout hints and class names without coupling to a specific backend:

```csharp
new Label("Styled")
    .With(new InlineStyle("title-label") { Width = 200, MarginBottom = 8 })
```

For UI Toolkit, `UITKStyle` maps directly to `StyleLength`, `StyleEnum<Visibility>`, and other USS-native types:

```csharp
new Label("UITK Styled")
    .With(new UITKStyle { Width = new StyleLength(200), MarginBottom = 8 })
```

## Incremental Rebuild & Diff

This is the core of UniDecl's performance model.

### Rebuild Granularity

When an element calls `Rebuild()`, only that element's sub-tree is rebuilt — not the entire DOM:

```
Full tree:           Rebuild Target:     Only subtree rebuilt:
  A                     A                    A
  ├── B                 ├── B (target)       ├── B ← rebuilt
  │   ├── C             │   ├── C             │   ├── C' (new)
  │   └── D             │   └── D             │   └── D' (new)
  └── E                 └── E                 └── E (untouched)
```

### Key-Based Diffing

During subtree rebuild, UniDecl diffs old and new children by **key first, position second**:

1. **Key match** — if a new child has a `Key` matching an old child, reuse that DOMNode
2. **Positional fallback** — if no key match, try same-index reuse
3. **Type check** — if reused node's type matches, recursively diff; otherwise replace entirely

### Updater Interface (UI Toolkit)

For UI Toolkit, renderers can implement `IElementUpdater<TRenderResult>` alongside `IElementRenderer`:

```csharp
public class MyLabelRenderer : IElementRenderer<Label, VisualElement>,
                               IElementUpdater<Label, VisualElement>
{
    public VisualElement Render(Label element, ...) => ...;

    // Called when the same Label element is re-rendered with new properties.
    // Return true to skip full Render(), keeping the existing VisualElement.
    public bool TryUpdate(Label element, VisualElement existing,
                          IElementRenderHost<VisualElement> manager, ElementState state)
    {
        existing.text = element.Text;
        return true;
    }
}
```

If `TryUpdate` returns false or is not implemented, the framework falls back to full `Render()`.

### Auto-Rebuild with Coalescing

`NotifyChanged()` (or state mutation triggering auto-rebuild) collects pending rebuilds and flushes them at the end of the frame via `EditorApplication.delayCall`. Multiple mutations to the same element are coalesced into a single rebuild.

### Performance Monitoring

Enable `EnableRebuildPerformanceMonitoring` on the render host to receive `RebuildPerformanceEvent` with timing breakdowns:

```csharp
renderHost.EnableRebuildPerformanceMonitoring = true;
renderHost.Subscribe(new MyPerformanceListener());
```

## Markdown Support

UniDecl includes a lightweight, dependency-free Markdown parser (`MdParser`) with block and inline AST:

**Block types**: ATX headings (H1-H6), setext headings, paragraphs, fenced code blocks, ordered/unordered lists, blockquotes, horizontal rules.

**Inline types**: bold, italic, bold-italic, inline code, links, images, line breaks.

### Markdown Widgets

14 purpose-built widgets for rendering Markdown content in UI Toolkit:

| Widget | Description |
|--------|-------------|
| `H1`–`H6` | Heading elements with configurable styles |
| `RichText` | Rich text with inline formatting (bold, italic, links) |
| `CodeBlock` | Fenced code block with language-aware styling |
| `CodeHighlighter` | Syntax-highlighted code display |
| `Blockquote` | Block quote with left border styling |
| `Divider` | Horizontal rule |
| `MdTable` | Markdown table rendering |
| `TocView` | Table of contents extracted from headings |
| `MarkdownView` | All-in-one Markdown document viewer |

```csharp
var md = "# Hello\n\nThis is **bold** text.\n\n```csharp\nDebug.Log(42);\n```";

new MarkdownView(md);
```

## Built-in Widgets

### Basic Controls
Label, Button, TextField, Toggle, Slider, SliderInt, IntegerField, FloatField, DoubleField, LongField, Dropdown, EnumField, EnumFlagsField, ColorField, ObjectField, PropertyField, LayerField, TagField, MaskField, HelpBox, ProgressBar, CurveField, GradientField, ResizableTextArea

### Layout
VerticalLayout, HorizontalLayout, Panel, ScrollView, Foldout, TwoPaneSplitView, VisualSplitter, PopupWindow, IMGUIContainer

### Data Views
ListView, TreeView, MultiColumnListView

### Toolbar
Toolbar, ToolbarButton, ToolbarToggle, ToolbarSearchField, ToolbarMenu

### Vector / Geometry
Vector2Field, Vector3Field, Vector4Field, Vector2IntField, Vector3IntField, RectField, RectIntField, BoundsField, BoundsIntField

### Special
InspectorElement, UeCard

## Quick Start (UI Toolkit)

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

## License

MIT
