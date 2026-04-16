using System.Collections.Generic;
using System.Linq;
using UniDecl.Runtime.Components;
using UnityEngine;
using UnityEngine.UIElements;

namespace UniDecl.UIToolKit.Runtime
{
    public class UITKStyle : IInlineStyle
    {
        private string[] _classNames;
        public string[] ClassNames => _classNames;

        // === Tooltip (IInlineStyle) ===
        public string Tooltip { get; set; }

        // === IInlineStyle bridge: delegates to UITK-specific StyleLength properties ===
        float? IInlineStyle.Width { get => ToFloat(Width); set => Width = FromFloat(value); }
        float? IInlineStyle.Height { get => ToFloat(Height); set => Height = FromFloat(value); }
        float? IInlineStyle.MinWidth { get => ToFloat(MinWidth); set => MinWidth = FromFloat(value); }
        float? IInlineStyle.MaxWidth { get => ToFloat(MaxWidth); set => MaxWidth = FromFloat(value); }
        float? IInlineStyle.MinHeight { get => ToFloat(MinHeight); set => MinHeight = FromFloat(value); }
        float? IInlineStyle.MaxHeight { get => ToFloat(MaxHeight); set => MaxHeight = FromFloat(value); }
        float? IInlineStyle.MarginLeft { get => ToFloat(MarginLeft); set => MarginLeft = FromFloat(value); }
        float? IInlineStyle.MarginRight { get => ToFloat(MarginRight); set => MarginRight = FromFloat(value); }
        float? IInlineStyle.MarginTop { get => ToFloat(MarginTop); set => MarginTop = FromFloat(value); }
        float? IInlineStyle.MarginBottom { get => ToFloat(MarginBottom); set => MarginBottom = FromFloat(value); }
        float? IInlineStyle.PaddingLeft { get => ToFloat(PaddingLeft); set => PaddingLeft = FromFloat(value); }
        float? IInlineStyle.PaddingRight { get => ToFloat(PaddingRight); set => PaddingRight = FromFloat(value); }
        float? IInlineStyle.PaddingTop { get => ToFloat(PaddingTop); set => PaddingTop = FromFloat(value); }
        float? IInlineStyle.PaddingBottom { get => ToFloat(PaddingBottom); set => PaddingBottom = FromFloat(value); }
        bool? IInlineStyle.Visible
        {
            get => Visibility.HasValue ? Visibility.Value == UnityEngine.UIElements.Visibility.Visible : (bool?)null;
            set => Visibility = value.HasValue
                ? (value.Value ? UnityEngine.UIElements.Visibility.Visible : UnityEngine.UIElements.Visibility.Hidden)
                : (StyleEnum<Visibility>?)null;
        }

        private static float? ToFloat(StyleLength? v)
        {
            if (!v.HasValue) return null;
            return v.Value.value.value;
        }
        private static StyleLength? FromFloat(float? v) => v.HasValue ? new StyleLength(v.Value) : (StyleLength?)null;

        // === Layout (UITK-specific) ===
        public StyleLength? Width { get; set; }
        public StyleLength? Height { get; set; }
        public StyleLength? MinWidth { get; set; }
        public StyleLength? MaxWidth { get; set; }
        public StyleLength? MinHeight { get; set; }
        public StyleLength? MaxHeight { get; set; }
        public StyleFloat? FlexGrow { get; set; }
        public StyleFloat? FlexShrink { get; set; }
        public StyleLength? FlexBasis { get; set; }
        public StyleEnum<Wrap>? FlexWrap { get; set; }
        public StyleEnum<FlexDirection>? FlexDirection { get; set; }
        public StyleEnum<Justify>? JustifyContent { get; set; }
        public StyleEnum<Align>? AlignItems { get; set; }
        public StyleEnum<Align>? AlignSelf { get; set; }
        public StyleEnum<Align>? AlignContent { get; set; }
        public StyleEnum<DisplayStyle>? Display { get; set; }
        public StyleEnum<Visibility>? Visibility { get; set; }
        public StyleFloat? Opacity { get; set; }
        public StyleEnum<Overflow>? Overflow { get; set; }
        public StyleEnum<Position>? Position { get; set; }
        public StyleLength? Left { get; set; }
        public StyleLength? Right { get; set; }
        public StyleLength? Top { get; set; }
        public StyleLength? Bottom { get; set; }

        // === Spacing ===
        public StyleLength? MarginLeft { get; set; }
        public StyleLength? MarginRight { get; set; }
        public StyleLength? MarginTop { get; set; }
        public StyleLength? MarginBottom { get; set; }
        public StyleLength? PaddingLeft { get; set; }
        public StyleLength? PaddingRight { get; set; }
        public StyleLength? PaddingTop { get; set; }
        public StyleLength? PaddingBottom { get; set; }

        // === Border ===
        public StyleFloat? BorderTopWidth { get; set; }
        public StyleFloat? BorderRightWidth { get; set; }
        public StyleFloat? BorderBottomWidth { get; set; }
        public StyleFloat? BorderLeftWidth { get; set; }
        public StyleColor? BorderTopColor { get; set; }
        public StyleColor? BorderRightColor { get; set; }
        public StyleColor? BorderBottomColor { get; set; }
        public StyleColor? BorderLeftColor { get; set; }
        public StyleLength? BorderTopLeftRadius { get; set; }
        public StyleLength? BorderTopRightRadius { get; set; }
        public StyleLength? BorderBottomLeftRadius { get; set; }
        public StyleLength? BorderBottomRightRadius { get; set; }

        // === Background ===
        public StyleColor? BackgroundColor { get; set; }
        public StyleBackground? BackgroundImage { get; set; }
        public StyleBackgroundPosition? BackgroundPositionX { get; set; }
        public StyleBackgroundPosition? BackgroundPositionY { get; set; }
        public StyleBackgroundRepeat? BackgroundRepeat { get; set; }
        public StyleBackgroundSize? BackgroundSize { get; set; }
        public StyleColor? BackgroundImageTintColor { get; set; }

        // === Text ===
        public StyleColor? Color { get; set; }
        public StyleLength? FontSize { get; set; }
        public StyleFont? UnityFont { get; set; }
        public StyleEnum<FontStyle>? UnityFontStyleAndWeight { get; set; }
        public StyleEnum<TextAnchor>? UnityTextAlign { get; set; }
        public StyleLength? LetterSpacing { get; set; }
        public StyleLength? WordSpacing { get; set; }
        public StyleEnum<WhiteSpace>? WhiteSpace { get; set; }
        public StyleEnum<TextOverflow>? TextOverflow { get; set; }
        public StyleTextShadow? TextShadow { get; set; }
        public StyleColor? TextOutlineColor { get; set; }
        public StyleFloat? TextOutlineWidth { get; set; }
        public StyleLength? ParagraphSpacing { get; set; }
        public StyleEnum<TextOverflowPosition>? TextOverflowPosition { get; set; }

        // === Transform ===
        public StyleTranslate? Translate { get; set; }
        public StyleRotate? Rotate { get; set; }
        public StyleScale? Scale { get; set; }
        public StyleTransformOrigin? TransformOrigin { get; set; }

        // === Transition ===
        public StyleList<StylePropertyName>? TransitionProperty { get; set; }
        public StyleList<TimeValue>? TransitionDuration { get; set; }
        public StyleList<TimeValue>? TransitionDelay { get; set; }
        public StyleList<EasingFunction>? TransitionTimingFunction { get; set; }

        // === Misc ===
        public StyleCursor? Cursor { get; set; }
        public StyleInt? SliceTop { get; set; }
        public StyleInt? SliceRight { get; set; }
        public StyleInt? SliceBottom { get; set; }
        public StyleInt? SliceLeft { get; set; }
        public StyleFloat? SliceScale { get; set; }

        // === Fluent API: Spacing Shortcuts ===
        public UITKStyle Margin(float all) { MarginLeft = MarginRight = MarginTop = MarginBottom = all; return this; }
        public UITKStyle Margin(float horiz, float vert) { MarginLeft = MarginRight = horiz; MarginTop = MarginBottom = vert; return this; }
        public UITKStyle Margin(float l, float r, float t, float b) { MarginLeft = l; MarginRight = r; MarginTop = t; MarginBottom = b; return this; }
        public UITKStyle Padding(float all) { PaddingLeft = PaddingRight = PaddingTop = PaddingBottom = all; return this; }
        public UITKStyle Padding(float horiz, float vert) { PaddingLeft = PaddingRight = horiz; PaddingTop = PaddingBottom = vert; return this; }
        public UITKStyle Padding(float l, float r, float t, float b) { PaddingLeft = l; PaddingRight = r; PaddingTop = t; PaddingBottom = b; return this; }

        // === Fluent API: Border Shortcuts ===
        public UITKStyle BorderWidth(float all) { BorderTopWidth = BorderRightWidth = BorderBottomWidth = BorderLeftWidth = all; return this; }
        public UITKStyle BorderRadius(float all) { BorderTopLeftRadius = BorderTopRightRadius = BorderBottomLeftRadius = BorderBottomRightRadius = all; return this; }
        public UITKStyle BorderColor(Color color) { BorderTopColor = BorderRightColor = BorderBottomColor = BorderLeftColor = color; return this; }

        // === Fluent API: Layout Shortcuts ===
        public UITKStyle FlexRow() { FlexDirection = UnityEngine.UIElements.FlexDirection.Row; return this; }
        public UITKStyle FlexColumn() { FlexDirection = UnityEngine.UIElements.FlexDirection.Column; return this; }
        public UITKStyle FlexNoWrap() { FlexWrap = Wrap.NoWrap; return this; }
        public UITKStyle AlignCenter() { AlignItems = Align.Center; return this; }
        public UITKStyle AlignStretch() { AlignItems = Align.Stretch; return this; }
        public UITKStyle AlignFlexStart() { AlignItems = Align.FlexStart; return this; }
        public UITKStyle AlignFlexEnd() { AlignItems = Align.FlexEnd; return this; }
        public UITKStyle JustifyCenter() { JustifyContent = Justify.Center; return this; }
        public UITKStyle JustifySpaceBetween() { JustifyContent = Justify.SpaceBetween; return this; }
        public UITKStyle DisplayNone() { Display = DisplayStyle.None; return this; }
        public UITKStyle DisplayFlex() { Display = DisplayStyle.Flex; return this; }

        // === Fluent API: USS class names (append semantics) ===
        public UITKStyle AddClass(params string[] classes)
        {
            if (classes == null || classes.Length == 0) return this;
            if (_classNames == null || _classNames.Length == 0)
                _classNames = classes;
            else
                _classNames = new List<string>(_classNames).Concat(classes).ToArray();
            return this;
        }
    }
}
