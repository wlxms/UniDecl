using System.Collections.Generic;
using UnityEditor;
using VisualElement = UnityEngine.UIElements.VisualElement;
using UniDecl.Runtime.Contexts;
using UniDecl.Runtime.Core;
using UniDecl.Runtime.Widgets;
using UniDecl.Runtime.Widgets.MD;
using UniDecl.Runtime.Widgets.UE;
using UniDecl.Editor.UIToolKit.Renderers;
using UniDecl.Editor.UIToolKit.Renderers.UE;
using UniDecl.Editor.UIToolKit.Renderers.MD;

namespace UniDecl.Editor.UIToolKit
{
    public class UIToolkitRenderManager : ElementRenderHost<VisualElement>
    {
        private readonly List<UnityEngine.UIElements.StyleSheet> _styleSheets = new List<UnityEngine.UIElements.StyleSheet>();

        public void RegisterStyleSheet(UnityEngine.UIElements.StyleSheet sheet)
        {
            if (sheet != null) _styleSheets.Add(sheet);
        }

        public void LoadStyleSheet(string path)
        {
            var sheet = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.StyleSheet>(path);
            if (sheet != null) _styleSheets.Add(sheet);
        }

        public void LoadStyleSheetFromResources(string resourcePath)
        {
            var sheet = UnityEngine.Resources.Load<UnityEngine.UIElements.StyleSheet>(resourcePath);
            if (sheet != null) _styleSheets.Add(sheet);
        }

        protected override void DiscoverRenderers()
        {
            RegisterRenderer<Label>(new UIToolkitLabelRenderer());
            RegisterRenderer<Button>(new UIToolkitButtonRenderer());
            RegisterRenderer<TextField>(new UIToolkitTextFieldRenderer());
            RegisterRenderer<VerticalLayout>(new UIToolkitVerticalLayoutRenderer());
            RegisterRenderer<HorizontalLayout>(new UIToolkitHorizontalLayoutRenderer());
            RegisterRenderer<Panel>(new UIToolkitPanelRenderer());
            RegisterRenderer<DisableContext>(new UIToolkitDisableContextRenderer());
            RegisterRenderer<Toggle>(new UIToolkitToggleRenderer());
            RegisterRenderer<IntegerField>(new UIToolkitIntegerFieldRenderer());
            RegisterRenderer<FloatField>(new UIToolkitFloatFieldRenderer());
            RegisterRenderer<Dropdown>(new UIToolkitDropdownRenderer());
            RegisterRenderer<EnumField>(new UIToolkitEnumFieldRenderer());
            RegisterRenderer<ColorField>(new UIToolkitColorFieldRenderer());
            RegisterRenderer<Slider>(new UIToolkitSliderRenderer());
            RegisterRenderer<MinMaxSlider>(new UIToolkitMinMaxSliderRenderer());
            RegisterRenderer<ScrollView>(new UIToolkitScrollViewRenderer());
            RegisterRenderer<Foldout>(new UIToolkitFoldoutRenderer());
            RegisterRenderer<HelpBox>(new UIToolkitHelpBoxRenderer());
            RegisterRenderer<ProgressBar>(new UIToolkitProgressBarRenderer());
            // P0-A: 引用/资源字段
            RegisterRenderer<ObjectField>(new UIToolkitObjectFieldRenderer());
            RegisterRenderer<PropertyField>(new UIToolkitPropertyFieldRenderer());
            RegisterRenderer<InspectorElement>(new UIToolkitInspectorElementRenderer());
            RegisterRenderer<TagField>(new UIToolkitTagFieldRenderer());
            RegisterRenderer<LayerField>(new UIToolkitLayerFieldRenderer());
            RegisterRenderer<MaskField>(new UIToolkitMaskFieldRenderer());
            RegisterRenderer<EnumFlagsField>(new UIToolkitEnumFlagsFieldRenderer());
            // P0-B: 数值/向量/曲线字段
            RegisterRenderer<Vector2Field>(new UIToolkitVector2FieldRenderer());
            RegisterRenderer<Vector3Field>(new UIToolkitVector3FieldRenderer());
            RegisterRenderer<Vector4Field>(new UIToolkitVector4FieldRenderer());
            RegisterRenderer<Vector2IntField>(new UIToolkitVector2IntFieldRenderer());
            RegisterRenderer<Vector3IntField>(new UIToolkitVector3IntFieldRenderer());
            RegisterRenderer<RectField>(new UIToolkitRectFieldRenderer());
            RegisterRenderer<RectIntField>(new UIToolkitRectIntFieldRenderer());
            RegisterRenderer<BoundsField>(new UIToolkitBoundsFieldRenderer());
            RegisterRenderer<BoundsIntField>(new UIToolkitBoundsIntFieldRenderer());
            RegisterRenderer<CurveField>(new UIToolkitCurveFieldRenderer());
            RegisterRenderer<GradientField>(new UIToolkitGradientFieldRenderer());
            // P1-A: Toolbar 系列
            RegisterRenderer<Toolbar>(new UIToolkitToolbarRenderer());
            RegisterRenderer<ToolbarButton>(new UIToolkitToolbarButtonRenderer());
            RegisterRenderer<ToolbarToggle>(new UIToolkitToolbarToggleRenderer());
            RegisterRenderer<ToolbarSearchField>(new UIToolkitToolbarSearchFieldRenderer());
            RegisterRenderer<ToolbarMenu>(new UIToolkitToolbarMenuRenderer());
            // P1-B: 数据视图
            RegisterRenderer<ListView>(new UIToolkitListViewRenderer());
            RegisterRenderer<TreeView>(new UIToolkitTreeViewRenderer());
            RegisterRenderer<MultiColumnListView>(new UIToolkitMultiColumnListViewRenderer());
            // P1-C: 布局/工具控件
            RegisterRenderer<PaneSplitView>(new UIToolkitPaneSplitViewRenderer());
            RegisterRenderer<VisualSplitter>(new UIToolkitVisualSplitterRenderer());
            RegisterRenderer<IMGUIContainer>(new UIToolkitIMGUIContainerRenderer());
            RegisterRenderer<UniDecl.Runtime.Widgets.PopupWindow>(new UIToolkitPopupWindowRenderer());
            RegisterRenderer<ResizableTextArea>(new UIToolkitResizableTextAreaRenderer());
            RegisterRenderer<UeCard>(new UIToolkitUeCardRenderer());
            // P1-D: 数值字段扩展
            RegisterRenderer<SliderInt>(new UIToolkitSliderIntRenderer());
            RegisterRenderer<DoubleField>(new UIToolkitDoubleFieldRenderer());
            RegisterRenderer<LongField>(new UIToolkitLongFieldRenderer());
            // MD: 目录导航栏 TocView（H1–H6 通过封装穿透到 Label 渲染，无需专属渲染器）
            RegisterRenderer<TocView>(new UIToolkitTocViewRenderer());
            // MD: inline widgets — RichText, Blockquote, Divider, MdTable, CodeBlock
            // MarkdownView composes via Render() and needs no dedicated renderer.
            RegisterRenderer<RichText>(new UIToolkitRichTextRenderer());
            RegisterRenderer<Blockquote>(new UIToolkitBlockquoteRenderer());
            RegisterRenderer<Divider>(new UIToolkitDividerRenderer());
            RegisterRenderer<MdTable>(new UIToolkitMdTableRenderer());
            RegisterRenderer<CodeBlock>(new UIToolkitCodeBlockRenderer());
        }

        public VisualElement RenderRoot(IElement rootElement)
        {
            if (rootElement == null) return null;
            BuildDOM(rootElement);
            var root = RenderElement(rootElement);
            if (root != null)
                foreach (var ss in _styleSheets)
                    root.styleSheets.Add(ss);
            return root;
        }

        protected override void ScheduleFlush()
        {
            EditorApplication.delayCall += FlushPendingRebuilds;
        }

        protected override void OnRenderResultChanged(IElement element, VisualElement oldVE, VisualElement newVE)
        {
            if (oldVE == null || newVE == null) return;
            if (ReferenceEquals(oldVE, newVE)) return;

            var parentVE = oldVE.parent;
            if (parentVE == null) return;

            int index = parentVE.IndexOf(oldVE);
            parentVE.Remove(oldVE);
            if (index >= 0 && index < parentVE.childCount)
                parentVE.Insert(index, newVE);
            else
                parentVE.Add(newVE);
        }
    }
}
