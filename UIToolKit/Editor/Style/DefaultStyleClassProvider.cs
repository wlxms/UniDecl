using System;
using System.Collections.Generic;
using UniDecl.BuiltIn.Runtime.Contexts;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.BuiltIn.Runtime.Widgets;
using UniDecl.BuiltIn.Runtime.Widgets.MD;
using UniDecl.BuiltIn.Runtime.Widgets.UE;

namespace UniDecl.Editor.UIToolKit.Style
{
    public static class DefaultStyleClassProvider
    {
        public static IEnumerable<string> Resolve(IElement element)
        {
            if (element == null)
                yield break;

            yield return "ud-element";

            if (element is ContainerElement)
                yield return "ud-container";

            // 所有"字段型"原子控件统一加 ud-field class。
            // USS 中通过 .ud-field 集中定义字段共通样式（如 flex-grow=1 / flex-shrink=1），
            // 让字段在 Hor{label, value} 布局中默认占满剩余空间且允许压缩。
            // 用户通过 UITKStyle 内联设置 flexGrow/flexShrink 优先级高于 USS，可覆盖。
            if (IsFieldWidget(element))
                yield return "ud-field";

            switch (element)
            {
                case Label:
                    yield return "ud-label";
                    break;
                case Button:
                    yield return "ud-button";
                    break;
                case TextField:
                    yield return "ud-textfield";
                    break;
                case VerticalLayout:
                    yield return "ud-vertical-layout";
                    break;
                case HorizontalLayout:
                    yield return "ud-horizontal-layout";
                    break;
                case Panel:
                    yield return "ud-panel";
                    break;
                case Toggle:
                    yield return "ud-toggle";
                    break;
                case IntegerField:
                    yield return "ud-numfield";
                    yield return "ud-integer-field";
                    break;
                case FloatField:
                    yield return "ud-numfield";
                    yield return "ud-float-field";
                    break;
                case DoubleField:
                    yield return "ud-numfield";
                    yield return "ud-double-field";
                    break;
                case LongField:
                    yield return "ud-numfield";
                    yield return "ud-long-field";
                    break;
                case Dropdown:
                    yield return "ud-dropdown";
                    break;
                case EnumField:
                    yield return "ud-enumfield";
                    break;
                case ColorField:
                    yield return "ud-colorfield";
                    break;
                case Slider:
                    yield return "ud-slider";
                    break;
                case SliderInt:
                    yield return "ud-slider";
                    yield return "ud-slider-int";
                    break;
                case MinMaxSlider:
                    yield return "ud-slider";
                    yield return "ud-minmax-slider";
                    break;
                case Foldout:
                    yield return "ud-foldout";
                    break;
                case HelpBox hb:
                    yield return "ud-helpbox";
                    switch (hb.MessageType)
                    {
                        case HelpBoxMessageType.Info:
                            yield return "ud-helpbox-info";
                            break;
                        case HelpBoxMessageType.Warning:
                            yield return "ud-helpbox-warning";
                            break;
                        case HelpBoxMessageType.Error:
                            yield return "ud-helpbox-error";
                            break;
                    }
                    break;
                case ProgressBar:
                    yield return "ud-progress-bar";
                    break;
                case Toolbar:
                    yield return "ud-toolbar";
                    break;
                case ToolbarButton:
                    yield return "ud-toolbar-button";
                    break;
                case ToolbarToggle:
                    yield return "ud-toolbar-toggle";
                    break;
                case ToolbarSearchField:
                    yield return "ud-toolbar-search-field";
                    break;
                case ToolbarMenu:
                    yield return "ud-toolbar-menu";
                    break;
                case UeCard:
                    yield return "ud-card";
                    break;
                case DisableContext:
                    yield return "ud-disable-context";
                    break;
                case TocView:
                    yield return "ud-toc-view";
                    break;
                case MarkdownView:
                    yield return "ud-markdown";
                    break;
            }
        }

        public static void Apply(IElement element, UnityEngine.UIElements.VisualElement ve)
        {
            if (element == null || ve == null)
                return;

            foreach (var cls in Resolve(element))
            {
                if (!string.IsNullOrEmpty(cls) && !ve.ClassListContains(cls))
                    ve.AddToClassList(cls);
            }
        }

        /// <summary>
        /// 判断 Widget 是否为"字段型"原子控件——即渲染为 Unity BaseField&lt;T&gt; 派生类的控件。
        /// 这些控件在 PropertyGrid 的 Hor{label, value} 布局中需要默认占满 value 槽。
        /// 包含：数值字段、文本、枚举、颜色、向量、Rect/Bounds、Curve/Gradient、Layer/Mask/Tag、Object、Slider 系列、Dropdown、Toggle。
        /// </summary>
        static bool IsFieldWidget(IElement element)
        {
            switch (element)
            {
                case IntegerField:
                case FloatField:
                case DoubleField:
                case LongField:
                case TextField:
                case Toggle:
                case EnumField:
                case EnumFlagsField:
                case ColorField:
                case Vector2Field:
                case Vector3Field:
                case Vector4Field:
                case Vector2IntField:
                case Vector3IntField:
                case RectField:
                case RectIntField:
                case BoundsField:
                case BoundsIntField:
                case CurveField:
                case GradientField:
                case LayerField:
                case MaskField:
                case TagField:
                case ObjectField:
                case Slider:
                case SliderInt:
                case MinMaxSlider:
                case Dropdown:
                case ResizableTextArea:
                    return true;
                default:
                    return false;
            }
        }
    }
}
