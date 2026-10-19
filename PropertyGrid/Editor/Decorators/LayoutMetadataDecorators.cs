using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.PropertyGrid.Runtime;
using PropertyField = UniDecl.PropertyGrid.Editor.Elements.PropertyField;

namespace UniDecl.PropertyGrid.Editor.Decorators
{
    // =========================================================================
    // 行级布局元数据装饰器：把 [Space] / [FlexGrow] / [Width] / [Align*] 写入
    // PropertyField 的布局属性，由其 Render() 统一落到行容器 UITKStyle。
    // =========================================================================

    /// <summary>[Space(Before, After)]——字段行前后间距（px）。</summary>
    public class SpaceDecorator : MetadataDecorator
    {
        public override bool Applies(in DecoratorContext ctx) => ctx.GetAttribute<SpaceAttribute>() != null;

        public override IElement Process(IElement input, DecoratorContext ctx)
        {
            if (input is PropertyField pf)
            {
                var attr = ctx.GetAttribute<SpaceAttribute>();
                pf.SpaceBefore = attr.Before;
                pf.SpaceAfter = attr.After;
            }
            return input;
        }
    }

    /// <summary>[FlexGrow]/[Width]/[AlignRight]/[AlignCenter]——水平行内占比、定宽与对齐。</summary>
    public class FlexSizeDecorator : MetadataDecorator
    {
        public override bool Applies(in DecoratorContext ctx)
            => ctx.GetAttribute<FlexGrowAttribute>() != null
               || ctx.GetAttribute<WidthAttribute>() != null
               || ctx.GetAttribute<AlignRightAttribute>() != null
               || ctx.GetAttribute<AlignCenterAttribute>() != null;

        public override IElement Process(IElement input, DecoratorContext ctx)
        {
            if (!(input is PropertyField pf)) return input;

            var grow = ctx.GetAttribute<FlexGrowAttribute>();
            if (grow != null) pf.FlexGrow = grow.Value;

            var width = ctx.GetAttribute<WidthAttribute>();
            if (width != null) pf.Width = width.Value;

            if (ctx.GetAttribute<AlignRightAttribute>() != null)
                pf.AlignSelf = UnityEngine.UIElements.Align.FlexEnd;
            else if (ctx.GetAttribute<AlignCenterAttribute>() != null)
                pf.AlignSelf = UnityEngine.UIElements.Align.Center;

            return input;
        }
    }
}
