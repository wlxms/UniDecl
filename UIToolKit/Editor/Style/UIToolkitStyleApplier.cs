using UnityEngine.UIElements;
using UniDecl.Runtime.Components;
using UniDecl.Runtime.Core;
using UniDecl.UIToolKit.Runtime;

namespace UniDecl.Editor.UIToolKit.Style
{
    public static class UIToolkitStyleApplier
    {
        public static void Apply(UITKStyle style, VisualElement ve)
        {
            if (style == null || ve == null) return;
            var s = ve.style;

            // Layout
            if (style.Width.HasValue) s.width = style.Width.Value;
            if (style.Height.HasValue) s.height = style.Height.Value;
            if (style.MinWidth.HasValue) s.minWidth = style.MinWidth.Value;
            if (style.MaxWidth.HasValue) s.maxWidth = style.MaxWidth.Value;
            if (style.MinHeight.HasValue) s.minHeight = style.MinHeight.Value;
            if (style.MaxHeight.HasValue) s.maxHeight = style.MaxHeight.Value;
            if (style.FlexGrow.HasValue) s.flexGrow = style.FlexGrow.Value;
            if (style.FlexShrink.HasValue) s.flexShrink = style.FlexShrink.Value;
            if (style.FlexBasis.HasValue) s.flexBasis = style.FlexBasis.Value;
            if (style.FlexWrap.HasValue) s.flexWrap = style.FlexWrap.Value;
            if (style.FlexDirection.HasValue) s.flexDirection = style.FlexDirection.Value;
            if (style.JustifyContent.HasValue) s.justifyContent = style.JustifyContent.Value;
            if (style.AlignItems.HasValue) s.alignItems = style.AlignItems.Value;
            if (style.AlignSelf.HasValue) s.alignSelf = style.AlignSelf.Value;
            if (style.AlignContent.HasValue) s.alignContent = style.AlignContent.Value;
            if (style.Display.HasValue) s.display = style.Display.Value;
            if (style.Visibility.HasValue) s.visibility = style.Visibility.Value;
            if (style.Opacity.HasValue) s.opacity = style.Opacity.Value;
            if (style.Overflow.HasValue) s.overflow = style.Overflow.Value;
            if (style.Position.HasValue) s.position = style.Position.Value;
            if (style.Left.HasValue) s.left = style.Left.Value;
            if (style.Right.HasValue) s.right = style.Right.Value;
            if (style.Top.HasValue) s.top = style.Top.Value;
            if (style.Bottom.HasValue) s.bottom = style.Bottom.Value;

            // Spacing
            if (style.MarginLeft.HasValue) s.marginLeft = style.MarginLeft.Value;
            if (style.MarginRight.HasValue) s.marginRight = style.MarginRight.Value;
            if (style.MarginTop.HasValue) s.marginTop = style.MarginTop.Value;
            if (style.MarginBottom.HasValue) s.marginBottom = style.MarginBottom.Value;
            if (style.PaddingLeft.HasValue) s.paddingLeft = style.PaddingLeft.Value;
            if (style.PaddingRight.HasValue) s.paddingRight = style.PaddingRight.Value;
            if (style.PaddingTop.HasValue) s.paddingTop = style.PaddingTop.Value;
            if (style.PaddingBottom.HasValue) s.paddingBottom = style.PaddingBottom.Value;

            // Border
            if (style.BorderTopWidth.HasValue) s.borderTopWidth = style.BorderTopWidth.Value;
            if (style.BorderRightWidth.HasValue) s.borderRightWidth = style.BorderRightWidth.Value;
            if (style.BorderBottomWidth.HasValue) s.borderBottomWidth = style.BorderBottomWidth.Value;
            if (style.BorderLeftWidth.HasValue) s.borderLeftWidth = style.BorderLeftWidth.Value;
            if (style.BorderTopColor.HasValue) s.borderTopColor = style.BorderTopColor.Value;
            if (style.BorderRightColor.HasValue) s.borderRightColor = style.BorderRightColor.Value;
            if (style.BorderBottomColor.HasValue) s.borderBottomColor = style.BorderBottomColor.Value;
            if (style.BorderLeftColor.HasValue) s.borderLeftColor = style.BorderLeftColor.Value;
            if (style.BorderTopLeftRadius.HasValue) s.borderTopLeftRadius = style.BorderTopLeftRadius.Value;
            if (style.BorderTopRightRadius.HasValue) s.borderTopRightRadius = style.BorderTopRightRadius.Value;
            if (style.BorderBottomLeftRadius.HasValue) s.borderBottomLeftRadius = style.BorderBottomLeftRadius.Value;
            if (style.BorderBottomRightRadius.HasValue) s.borderBottomRightRadius = style.BorderBottomRightRadius.Value;

            // Background
            if (style.BackgroundColor.HasValue) s.backgroundColor = style.BackgroundColor.Value;
            if (style.BackgroundImage.HasValue) s.backgroundImage = style.BackgroundImage.Value;
            if (style.BackgroundPositionX.HasValue) s.backgroundPositionX = style.BackgroundPositionX.Value;
            if (style.BackgroundPositionY.HasValue) s.backgroundPositionY = style.BackgroundPositionY.Value;
            if (style.BackgroundRepeat.HasValue) s.backgroundRepeat = style.BackgroundRepeat.Value;
            if (style.BackgroundSize.HasValue) s.backgroundSize = style.BackgroundSize.Value;
            if (style.BackgroundImageTintColor.HasValue) s.unityBackgroundImageTintColor = style.BackgroundImageTintColor.Value;

            // Text
            if (style.Color.HasValue) s.color = style.Color.Value;
            if (style.FontSize.HasValue) s.fontSize = style.FontSize.Value;
            if (style.UnityFont.HasValue) s.unityFont = style.UnityFont.Value;
            if (style.UnityFontStyleAndWeight.HasValue) s.unityFontStyleAndWeight = style.UnityFontStyleAndWeight.Value;
            if (style.UnityTextAlign.HasValue) s.unityTextAlign = style.UnityTextAlign.Value;
            if (style.LetterSpacing.HasValue) s.letterSpacing = style.LetterSpacing.Value;
            if (style.WordSpacing.HasValue) s.wordSpacing = style.WordSpacing.Value;
            if (style.WhiteSpace.HasValue) s.whiteSpace = style.WhiteSpace.Value;
            if (style.TextOverflow.HasValue) s.textOverflow = style.TextOverflow.Value;
            if (style.TextShadow.HasValue) s.textShadow = style.TextShadow.Value;
            if (style.TextOutlineColor.HasValue) s.unityTextOutlineColor = style.TextOutlineColor.Value;
            if (style.TextOutlineWidth.HasValue) s.unityTextOutlineWidth = style.TextOutlineWidth.Value;
            if (style.ParagraphSpacing.HasValue) s.unityParagraphSpacing = style.ParagraphSpacing.Value;
            if (style.TextOverflowPosition.HasValue) s.unityTextOverflowPosition = style.TextOverflowPosition.Value;

            // Transform
            if (style.Translate.HasValue) s.translate = style.Translate.Value;
            if (style.Rotate.HasValue) s.rotate = style.Rotate.Value;
            if (style.Scale.HasValue) s.scale = style.Scale.Value;
            if (style.TransformOrigin.HasValue) s.transformOrigin = style.TransformOrigin.Value;

            // Transition
            if (style.TransitionProperty.HasValue) s.transitionProperty = style.TransitionProperty.Value;
            if (style.TransitionDuration.HasValue) s.transitionDuration = style.TransitionDuration.Value;
            if (style.TransitionDelay.HasValue) s.transitionDelay = style.TransitionDelay.Value;
            if (style.TransitionTimingFunction.HasValue) s.transitionTimingFunction = style.TransitionTimingFunction.Value;

            // Misc
            if (style.Cursor.HasValue) s.cursor = style.Cursor.Value;
            if (style.SliceTop.HasValue) s.unitySliceTop = style.SliceTop.Value;
            if (style.SliceRight.HasValue) s.unitySliceRight = style.SliceRight.Value;
            if (style.SliceBottom.HasValue) s.unitySliceBottom = style.SliceBottom.Value;
            if (style.SliceLeft.HasValue) s.unitySliceLeft = style.SliceLeft.Value;
            if (style.SliceScale.HasValue) s.unitySliceScale = style.SliceScale.Value;

            // Tooltip (not in IStyle, set directly on VisualElement)
            if (!string.IsNullOrEmpty(style.Tooltip)) ve.tooltip = style.Tooltip;
        }

        public static void ApplyElementStyles(IElement element, VisualElement ve)
        {
            if (element == null || ve == null) return;

            // Apply framework default classes first so themes can style every widget consistently.
            DefaultStyleClassProvider.Apply(element, ve);

            // Apply IInlineStyle (class names + common properties)
            // With<T> stores by typeof(T), so InlineStyle is registered under typeof(InlineStyle),
            // not typeof(IInlineStyle). Check both to cover .With(new InlineStyle(...)) and
            // .With((IInlineStyle)new InlineStyle(...)) usage patterns.
            var inlineStyle = element.Get<InlineStyle>() ?? element.Get<IInlineStyle>();
            if (inlineStyle != null)
            {
                ApplyInlineStyle(inlineStyle, ve);
            }

            // Apply UITKStyle (UITK-specific inline styles, extends IInlineStyle)
            var uitkStyle = element.Get<UITKStyle>();
            if (uitkStyle != null)
            {
                Apply(uitkStyle, ve);
                if (uitkStyle.ClassNames != null)
                    foreach (var cls in uitkStyle.ClassNames)
                        if (!string.IsNullOrEmpty(cls))
                            ve.AddToClassList(cls);
            }

            // Apply event components (OnClick, OnPointerEnter, OnEvent<T>, etc.)
            ElementEventApplier.Apply(element, ve);
        }

        private static void ApplyInlineStyle(IInlineStyle style, VisualElement ve)
        {
            if (style.ClassNames != null)
                foreach (var cls in style.ClassNames)
                    if (!string.IsNullOrEmpty(cls))
                        ve.AddToClassList(cls);

            if (!string.IsNullOrEmpty(style.Tooltip)) ve.tooltip = style.Tooltip;

            var s = ve.style;
            if (style.Width.HasValue) s.width = style.Width.Value;
            if (style.Height.HasValue) s.height = style.Height.Value;
            if (style.MinWidth.HasValue) s.minWidth = style.MinWidth.Value;
            if (style.MaxWidth.HasValue) s.maxWidth = style.MaxWidth.Value;
            if (style.MinHeight.HasValue) s.minHeight = style.MinHeight.Value;
            if (style.MaxHeight.HasValue) s.maxHeight = style.MaxHeight.Value;
            if (style.MarginLeft.HasValue) s.marginLeft = style.MarginLeft.Value;
            if (style.MarginRight.HasValue) s.marginRight = style.MarginRight.Value;
            if (style.MarginTop.HasValue) s.marginTop = style.MarginTop.Value;
            if (style.MarginBottom.HasValue) s.marginBottom = style.MarginBottom.Value;
            if (style.PaddingLeft.HasValue) s.paddingLeft = style.PaddingLeft.Value;
            if (style.PaddingRight.HasValue) s.paddingRight = style.PaddingRight.Value;
            if (style.PaddingTop.HasValue) s.paddingTop = style.PaddingTop.Value;
            if (style.PaddingBottom.HasValue) s.paddingBottom = style.PaddingBottom.Value;
            if (style.Visible.HasValue)
                ve.style.visibility = style.Visible.Value
                    ? UnityEngine.UIElements.Visibility.Visible
                    : UnityEngine.UIElements.Visibility.Hidden;
        }
    }
}
