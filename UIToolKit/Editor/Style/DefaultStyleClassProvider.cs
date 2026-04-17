using System;
using System.Collections.Generic;
using UniDecl.Runtime.Contexts;
using UniDecl.Runtime.Core;
using UniDecl.Runtime.Widgets;
using UniDecl.Runtime.Widgets.MD;
using UniDecl.Runtime.Widgets.UE;

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
    }
}
