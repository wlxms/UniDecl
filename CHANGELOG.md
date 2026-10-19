# Changelog

All notable changes to UniDecl will be documented in this file.

## [Unreleased]

### 2026-10-20 — Repository split by backend

**refactor: extract Unity/UI Toolkit backends into separate repositories**
- `UniDecl` core repository now contains only engine/backend-agnostic code:
  - `Core/` (`UniDecl.Core`, `noEngineReferences: true`)
  - `Snapshot/` (`UniDecl.Snapshot`, `noEngineReferences: true`)
  - `Widgets/` (`UniDecl.Widgets`, `noEngineReferences: true`)
  - `PropertyGrid/Runtime/` (`UniDecl.PropertyGrid`, `noEngineReferences: true`)
- New separate repositories:
  - `UniDecl.UIToolKit` — shared Unity adapter (`UniDecl.Unity`, `UniDecl.Unity.Editor`) + UI Toolkit backend + PropertyGrid editor
  - `UniDecl.IMGUI` — Unity IMGUI backend (placeholder)
  - `UniDecl.UMG` — Unreal UMG backend (placeholder)
- Core assemblies no longer reference any Unity assembly.
- `package.json` description updated to reflect the core-only package.

### 2026-10-19 — Assembly split & engine-agnostic core

**refactor: split into engine-agnostic and Unity-specific assemblies**
- Directory layout: `Core/`, `Snapshot/`, `Widgets/`, `Unity/`, `UnityEditor/`, `Backends/UIToolKit/`, `PropertyGrid/`
- New assemblies:
  - `UniDecl.Core` (`noEngineReferences: true`) — DOM, element tree, render pipeline, context/state/events/navigation
  - `UniDecl.Snapshot` (`noEngineReferences: true`) — Undo/Redo engine
  - `UniDecl.Widgets` (`noEngineReferences: true`) — backend-neutral widget definitions + Markdown
  - `UniDecl.PropertyGrid` (`noEngineReferences: true`) — PropertyGrid attributes
  - `UniDecl.Unity` — Unity value-type widgets, log bridge, Unity leaf policy
  - `UniDecl.Unity.Editor` — editor host + Unity Undo adapter
  - `UniDecl.Backend.UIToolKit` / `UniDecl.Backend.UIToolKit.Editor` — UI Toolkit backend
  - `UniDecl.PropertyGrid.Editor` — PropertyGrid editor
- Namespaces migrated to match the new assembly boundaries:
  - `UniDecl.BuiltIn.Runtime.Core` → `UniDecl.Core`
  - `UniDecl.BuiltIn.Runtime.Components` → `UniDecl.Core.Components`
  - `UniDecl.BuiltIn.Runtime.Contexts` → `UniDecl.Core.Contexts`
  - `UniDecl.BuiltIn.Runtime.Navigation` → `UniDecl.Core.Navigation`
  - `UniDecl.BuiltIn.Runtime.Snapshot` → `UniDecl.Snapshot`
  - `UniDecl.BuiltIn.Runtime.Widgets` → `UniDecl.Widgets`
  - `UniDecl.BuiltIn.Runtime.Widgets.MD` / `.MD` → `UniDecl.Widgets.MD`
  - Unity value-type widgets → `UniDecl.Unity.Widgets`
  - `UniDecl.BuiltIn.Editor*` → `UniDecl.Unity.Editor`
  - `UniDecl.UIToolKit.Runtime` → `UniDecl.Backend.UIToolKit`
  - `UniDecl.Editor.UIToolKit*` → `UniDecl.Backend.UIToolKit.Editor*`
  - `UniDecl.PropertyGrid.Runtime` → `UniDecl.PropertyGrid`
- `SnapshotManager` now takes an injected `ISnapshotLeafPolicy` (constructor injection) instead of a global registration:
  `new SnapshotManager(new UnitySnapshotLeafPolicy())`
- Added `UniDeclLog` in Core and `UnityLogBootstrap` in `UniDecl.Unity` to remove direct `UnityEngine.Debug` usage from pure assemblies.

### 2026-04-18 — MD widgets namespace migration & enhancements

**refactor: MD widgets to dedicated namespace, add syntax highlighting, enhance TocView/RichText, improve themes** `527b62f`
- Migrate all MD widgets (H1–H6, Blockquote, Divider, MarkdownView, MdTable, RichText, TocView, CodeBlock) into `UniDecl.Runtime.Widgets.MD` namespace
- Add `CodeHighlighter` with syntax highlighting support
- Enhance TocView with new `UIToolkitTocViewRenderer`
- Enhance RichText renderer
- Add Chinese font resources (msyh.ttc family)
- Improve Default/UE5/UnityUEHybrid themes
- Rewrite README with design philosophy, architecture overview, and widget list
- Add README_CN.md (Chinese localization)

### 2026-04-17 — MarkdownView decomposition & style modernization

**feat: decompose MarkdownView into composable widgets, drop UIToolkitMarkdownViewRenderer** `64180f7`
- Split monolithic MarkdownView into composable widgets: Blockquote, CodeBlock, Divider, MdTable, RichText
- Remove `UIToolkitMarkdownViewRenderer` in favor of individual renderers
- Add renderers for each new widget

**refactor: restructure UIToolKit directory and modernize style system** `a2d5803`
- Move `Editor/UIToolKit/` to `UIToolKit/Editor/` for proper assembly layout
- Move Panel, ListView, TreeView, MultiColumnListView from Editor to Runtime
- Move assembly definition to `Runtime/UniDecl.Runtime.asmdef`
- Replace `StyleClasses` with `IInlineStyle`/`InlineStyle` for backend-agnostic styling
- Migrate H1–H6 heading widgets to use `InlineStyle`
- Add `UIToolKit.Runtime` and `UIToolKit.Editor` assembly definitions

### 2026-04-16 — MarkdownView, style encapsulation, code review

**feat: add complete MarkdownView widget with parser, renderer, and URL forwarding** `d3e382e`
- Add `MdParser` with block/inline parsing (headings, code blocks, bold, italic, links, etc.)
- Add `MarkdownView` widget and `UIToolkitMarkdownViewRenderer`
- Support URL forwarding in parsed links
- Add USS theme styles for Markdown elements

**refactor: use static compiled Regex and named color constants per code review** `cdca974`
- Optimize `MdParser` with static compiled `Regex` instances
- Replace magic color strings with named constants

**refactor: replace custom H1-H6 renderers with encapsulation penetration via StyleClasses** `0ece73b`
- Remove `UIToolkitHeadingRenderer`
- Add `StyleClasses` system for encapsulation-based styling
- H1–H6 widgets now use `StyleClasses` for style penetration through DOM hierarchy

### 2026-04-15 — Heading controls & TocView

**feat: add MD heading controls (H1-H6) and TocView navigation sidebar** `ea0a03e`
- Add H1–H6 heading widgets with `UIToolkitHeadingRenderer`
- Add `TocView` navigation sidebar widget with `UIToolkitTocViewRenderer`
- Add heading styles to all three themes (Default, UE5, UnityUEHybrid)

**refactor: address code review feedback on TocView and TocViewRenderer** `aff1bf1`
- Refine TocView and renderer based on code review

### 2026-04-14 — Render cache refactoring

**refactor: 重构渲染缓存机制，将渲染结果存储到 DOMNode 内部** `a448f94`
- Move render cache storage into `DOMNode` for lifecycle correctness

### 2026-04-13 — UIToolKit style system & extended widgets

**feat(uitoolkit): add style system and extended widget support** `88e50a8`
- Add `UITKStyle` applier and `DefaultStyleClassProvider`
- Add UE5/Default/Hybrid theme USS files and style demo window
- Add runtime widgets and editor renderers for advanced fields, toolbar, split/layout, and data views (40+ renderers)
- Wire new renderers and stylesheet registration in render manager

### 2026-04-12 — Project initialization

**Initial commit** `5b34166`
- UniDecl framework foundation

**Framework for UniDecl** `88487f4`
- Core framework: DOM tree, widgets, renderers, and diff system
