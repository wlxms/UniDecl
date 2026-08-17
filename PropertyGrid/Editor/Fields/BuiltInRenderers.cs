using System;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.BuiltIn.Runtime.Widgets;
using UniDecl.PropertyGrid.Runtime;
using PropertyField = UniDecl.PropertyGrid.Editor.Elements.PropertyField;
using RangeAttribute = UniDecl.PropertyGrid.Runtime.RangeAttribute;
using TextAreaAttribute = UniDecl.PropertyGrid.Runtime.TextAreaAttribute;
using TooltipAttribute = UniDecl.PropertyGrid.Runtime.TooltipAttribute;
using UnityEngine;
using Button = UniDecl.BuiltIn.Runtime.Widgets.Button;

namespace UniDecl.PropertyGrid.Editor
{
    // =========================================================================
    // TypeRenderer（11 个 + Fallback 已在 PropertyAccessor.cs）
    // =========================================================================

    public class IntTypeRenderer : IFieldTypeRenderer
    {
        public Type FieldType => typeof(int);
        public IElement CreateWidget(PropertyAccessor a, BuildContext c)
        {
            var f = new IntegerField(Convert.ToInt32(a.GetValue()));
            f.OnValueChanged = (n, _) => a.SetValue(n);
            return f;
        }
    }

    public class FloatTypeRenderer : IFieldTypeRenderer
    {
        public Type FieldType => typeof(float);
        public IElement CreateWidget(PropertyAccessor a, BuildContext c)
        {
            var f = new FloatField(Convert.ToSingle(a.GetValue()));
            f.OnValueChanged = (n, _) => a.SetValue(n);
            return f;
        }
    }

    public class DoubleTypeRenderer : IFieldTypeRenderer
    {
        public Type FieldType => typeof(double);
        public IElement CreateWidget(PropertyAccessor a, BuildContext c)
        {
            var f = new DoubleField(value: Convert.ToDouble(a.GetValue()));
            f.OnValueChanged = (n, _) => a.SetValue(n);
            return f;
        }
    }

    public class LongTypeRenderer : IFieldTypeRenderer
    {
        public Type FieldType => typeof(long);
        public IElement CreateWidget(PropertyAccessor a, BuildContext c)
        {
            var f = new LongField(value: Convert.ToInt64(a.GetValue()));
            f.OnValueChanged = (n, _) => a.SetValue(n);
            return f;
        }
    }

    public class StringTypeRenderer : IFieldTypeRenderer
    {
        public Type FieldType => typeof(string);
        public IElement CreateWidget(PropertyAccessor a, BuildContext c)
        {
            var f = new TextField(a.GetValue() as string ?? "");
            f.OnValueChange = (n, _) => a.SetValue(n);
            return f;
        }
    }

    public class BoolTypeRenderer : IFieldTypeRenderer
    {
        public Type FieldType => typeof(bool);
        public IElement CreateWidget(PropertyAccessor a, BuildContext c)
        {
            var f = new Toggle(null, a.GetValue() is bool b && b);
            f.OnValueChanged = (n) => a.SetValue(n);
            return f;
        }
    }

    public class EnumTypeRenderer : IFieldTypeRenderer
    {
        public Type FieldType => typeof(Enum);
        public IElement CreateWidget(PropertyAccessor a, BuildContext c)
        {
            var t = a.PropertyType;
            var v = a.GetValue();
            var f = new EnumField(null, t, v != null ? Convert.ToInt32(v) : 0);
            f.OnValueChanged = (n) => { if (t.IsEnum) a.SetValue(Enum.ToObject(t, n)); else a.SetValue(n); };
            return f;
        }
    }

    public class ColorTypeRenderer : IFieldTypeRenderer
    {
        public Type FieldType => typeof(Color);
        public IElement CreateWidget(PropertyAccessor a, BuildContext c)
        {
            var f = new ColorField(null, a.GetValue() is Color col ? col : Color.white);
            f.OnValueChanged = (n) => a.SetValue(n);
            return f;
        }
    }

    public class Vector2TypeRenderer : IFieldTypeRenderer
    {
        public Type FieldType => typeof(Vector2);
        public IElement CreateWidget(PropertyAccessor a, BuildContext c)
        {
            var f = new Vector2Field(null, a.GetValue() is Vector2 v ? v : Vector2.zero);
            f.OnValueChanged = (n) => a.SetValue(n);
            return f;
        }
    }

    public class Vector3TypeRenderer : IFieldTypeRenderer
    {
        public Type FieldType => typeof(Vector3);
        public IElement CreateWidget(PropertyAccessor a, BuildContext c)
        {
            var f = new Vector3Field(null, a.GetValue() is Vector3 v ? v : Vector3.zero);
            f.OnValueChanged = (n) => a.SetValue(n);
            return f;
        }
    }

    public class ObjectTypeRenderer : IFieldTypeRenderer
    {
        public Type FieldType => typeof(UnityEngine.Object);
        public IElement CreateWidget(PropertyAccessor a, BuildContext c)
        {
            var f = new ObjectField(null, a.PropertyType, a.GetValue() as UnityEngine.Object);
            f.OnValueChanged = (n) => a.SetValue(n);
            return f;
        }
    }

    // =========================================================================
    // Decorator（Replacement + Metadata）
    // =========================================================================

    public class RangeDecorator : ReplacementDecorator
    {
        public override bool Applies(in DecoratorContext ctx) => ctx.GetAttribute<RangeAttribute>() != null;
        public override IElement Process(IElement input, DecoratorContext ctx)
        {
            if (!(input is PropertyField pf)) return input;
            var attr = ctx.GetAttribute<RangeAttribute>();
            var slider = new Slider(value: Convert.ToSingle(ctx.Accessor.GetValue()), low: (float)attr.Min, high: (float)attr.Max);
            slider.WithKey(pf.Editor?.Key); // 继承 insp_字段名：跨 rebuild 稳定（undo 合并按 Path 匹配）
            slider.OnValueChanged = (n) => ctx.Accessor.SetValue(ctx.Accessor.PropertyType == typeof(int) ? Mathf.RoundToInt(n) : (object)n);
            pf.Editor = slider;
            return pf;
        }
    }

    public class LabelTextDecorator : MetadataDecorator
    {
        public override bool Applies(in DecoratorContext ctx) => ctx.GetAttribute<LabelTextAttribute>() != null;
        public override IElement Process(IElement input, DecoratorContext ctx)
        {
            if (input is PropertyField pf)
                pf.LabelText = FieldBinder.ResolveReference(ctx.GetAttribute<LabelTextAttribute>().Text, ctx.BuildContext.Renderer, ctx.BuildContext.Target);
            return input;
        }
    }

    public class HideLabelDecorator : MetadataDecorator
    {
        public override bool Applies(in DecoratorContext ctx) => ctx.GetAttribute<HideLabelAttribute>() != null;
        public override IElement Process(IElement input, DecoratorContext ctx)
        {
            if (input is PropertyField pf) pf.ShowLabel = false;
            return input;
        }
    }

    public class ReadOnlyDecorator : MetadataDecorator
    {
        public override bool Applies(in DecoratorContext ctx) => ctx.GetAttribute<ReadOnlyAttribute>() != null;
        public override IElement Process(IElement input, DecoratorContext ctx)
        {
            if (input is PropertyField pf) pf.IsReadOnly = ctx.GetAttribute<ReadOnlyAttribute>().IsReadOnly;
            return input;
        }
    }

    public class ConditionDecorator : MetadataDecorator
    {
        public override bool Applies(in DecoratorContext ctx)
            => ctx.GetAttribute<ShowIfAttribute>() != null || ctx.GetAttribute<HideIfAttribute>() != null;

        public override IElement Process(IElement input, DecoratorContext ctx)
        {
            if (!(input is PropertyField pf)) return input;
            var showIf = ctx.GetAttribute<ShowIfAttribute>();
            var hideIf = ctx.GetAttribute<HideIfAttribute>();
            string member = showIf?.Member ?? hideIf?.Member;
            bool isShow = showIf != null;
            var expected = isShow ? showIf.Value : hideIf?.Value;

            var val = ctx.BuildContext.ResolveMember(member);
            pf.Visible = isShow ? IsTrue(val, expected) : !IsTrue(val, expected);

            ctx.BuildContext.FieldChanged += (name, _) =>
            {
                if (name == member)
                {
                    var nv = ctx.BuildContext.ResolveMember(member);
                    pf.Visible = isShow ? IsTrue(nv, expected) : !IsTrue(nv, expected);
                }
            };
            return pf;
        }

        static bool IsTrue(object mv, object ev)
        {
            if (ev == null) return mv is bool b ? b : mv != null;
            if (mv == null) return false;
            return ev.Equals(mv);
        }
    }

    // =========================================================================
    // 补充 Replacement Decorator（5 个）
    // =========================================================================

    public class MinMaxSliderDecorator : ReplacementDecorator
    {
        public override bool Applies(in DecoratorContext ctx)
            => ctx.GetAttribute<MinMaxSliderAttribute>() != null && ctx.Accessor.PropertyType == typeof(Vector2);
        public override IElement Process(IElement input, DecoratorContext ctx)
        {
            if (!(input is PropertyField pf)) return input;
            var attr = ctx.GetAttribute<MinMaxSliderAttribute>();
            var v = (Vector2)ctx.Accessor.GetValue();
            var slider = new MinMaxSlider(min: v.x, max: v.y, lowLimit: (float)attr.Min, highLimit: (float)attr.Max);
            slider.WithKey(pf.Editor?.Key); // 继承 insp_字段名：跨 rebuild 稳定（undo 合并按 Path 匹配）
            slider.OnValueChanged = (nmin, nmax) => ctx.Accessor.SetValue(new Vector2(nmin, nmax));
            pf.Editor = slider;
            return pf;
        }
    }

    public class TextAreaDecorator : ReplacementDecorator
    {
        public override bool Applies(in DecoratorContext ctx)
            => ctx.GetAttribute<TextAreaAttribute>() != null && ctx.Accessor.PropertyType == typeof(string);
        public override IElement Process(IElement input, DecoratorContext ctx)
        {
            if (!(input is PropertyField pf)) return input;
            var val = ctx.Accessor.GetValue() as string ?? "";
            var ta = new ResizableTextArea(val);
            ta.OnValueChanged = (n, _) => ctx.Accessor.SetValue(n);
            pf.Editor = ta;
            return pf;
        }
    }

    public class EnumToggleButtonsDecorator : ReplacementDecorator
    {
        public override bool Applies(in DecoratorContext ctx)
            => ctx.GetAttribute<EnumToggleButtonsAttribute>() != null && ctx.Accessor.PropertyType.IsEnum;
        public override IElement Process(IElement input, DecoratorContext ctx)
        {
            if (!(input is PropertyField pf)) return input;
            var t = ctx.Accessor.PropertyType;
            var v = ctx.Accessor.GetValue();
            var ef = new EnumField(null, t, v != null ? Convert.ToInt32(v) : 0);
            ef.OnValueChanged = (n) => ctx.Accessor.SetValue(Enum.ToObject(t, n));
            pf.Editor = ef;
            return pf;
        }
    }

    public class ButtonFieldDecorator : ReplacementDecorator
    {
        public override bool Applies(in DecoratorContext ctx) => ctx.GetAttribute<ButtonAttribute>() != null;
        public override IElement Process(IElement input, DecoratorContext ctx)
        {
            if (!(input is PropertyField pf)) return input;
            var attr = ctx.GetAttribute<ButtonAttribute>();
            var btn = new Button(attr.Label);
            btn.OnClick = () =>
            {
                if (ctx.BuildContext.Renderer == null) return;
                var m = FieldBinder.FindMethod(ctx.BuildContext.Renderer.GetType(), attr.Method, ctx.BuildContext.Target?.GetType());
                if (m == null) return;
                var ps = m.GetParameters();
                if (ps.Length == 0) m.Invoke(ctx.BuildContext.Renderer, null);
                else if (ps.Length == 1 && ctx.BuildContext.Target != null) m.Invoke(ctx.BuildContext.Renderer, new[] { ctx.BuildContext.Target });
            };
            pf.Editor = btn;
            pf.ShowLabel = false;
            return pf;
        }
    }

    public class DropdownDecorator : ReplacementDecorator
    {
        public override bool Applies(in DecoratorContext ctx) => ctx.GetAttribute<DropdownAttribute>() != null;
        public override IElement Process(IElement input, DecoratorContext ctx)
        {
            if (!(input is PropertyField pf)) return input;
            var attr = ctx.GetAttribute<DropdownAttribute>();
            var choices = ResolveChoices(attr, ctx.BuildContext.Renderer, ctx.BuildContext.Target);
            var idx = ResolveIndex(ctx.Accessor.PropertyType, ctx.Accessor.GetValue(), choices);
            var dd = new UniDecl.BuiltIn.Runtime.Widgets.Dropdown(null, choices, idx);
            dd.OnSelectionChanged = (ni) =>
            {
                if (ctx.Accessor.PropertyType == typeof(string))
                    ctx.Accessor.SetValue(ni >= 0 && ni < choices.Length ? choices[ni] : "");
                else if (ctx.Accessor.PropertyType == typeof(int))
                    ctx.Accessor.SetValue(ni);
            };
            pf.Editor = dd;
            return pf;
        }

        static string[] ResolveChoices(DropdownAttribute attr, object renderer, object target)
        {
            if (attr == null || renderer == null) return Array.Empty<string>();
            var m = FieldBinder.FindMethod(renderer.GetType(), attr.Method, target?.GetType());
            if (m == null) return Array.Empty<string>();
            try
            {
                object result = null;
                var ps = m.GetParameters();
                if (ps.Length == 0) result = m.Invoke(renderer, null);
                else if (ps.Length == 1 && target != null) result = m.Invoke(renderer, new[] { target });
                if (result is string[] arr) return arr;
                if (result is System.Collections.Generic.IEnumerable<string> en) return new System.Collections.Generic.List<string>(en).ToArray();
            }
            catch { }
            return Array.Empty<string>();
        }

        static int ResolveIndex(Type ft, object val, string[] choices)
        {
            if (choices == null || choices.Length == 0) return 0;
            if (ft == typeof(string)) { var i = Array.IndexOf(choices, val as string); return i >= 0 ? i : 0; }
            if (ft == typeof(int) && val is int ii) return Mathf.Clamp(ii, 0, choices.Length - 1);
            return 0;
        }
    }

    // =========================================================================
    // 补充 Metadata Decorator（4 个）
    // =========================================================================

    public class TooltipDecorator : MetadataDecorator
    {
        public override bool Applies(in DecoratorContext ctx) => ctx.GetAttribute<TooltipAttribute>() != null;
        public override IElement Process(IElement input, DecoratorContext ctx)
        {
            if (input is PropertyField pf) pf.Tooltip = ctx.GetAttribute<TooltipAttribute>().Text;
            return input;
        }
    }

    public class IndentDecorator : MetadataDecorator
    {
        public override bool Applies(in DecoratorContext ctx) => ctx.GetAttribute<IndentAttribute>() != null;
        public override IElement Process(IElement input, DecoratorContext ctx)
        {
            if (input is PropertyField pf) pf.IndentLevel = ctx.GetAttribute<IndentAttribute>().Level;
            return input;
        }
    }

    public class OnValueChangedDecorator : MetadataDecorator
    {
        public override bool Applies(in DecoratorContext ctx) => ctx.GetAttribute<OnValueChangedAttribute>() != null;
        public override IElement Process(IElement input, DecoratorContext ctx)
        {
            if (!(input is PropertyField pf) || pf.Accessor == null) return input;
            var attr = ctx.GetAttribute<OnValueChangedAttribute>();
            pf.Accessor.ValueChanged += (newVal, oldVal) =>
            {
                if (ctx.BuildContext.Renderer == null) return;
                var m = FieldBinder.FindMethod(ctx.BuildContext.Renderer.GetType(), attr.Method, ctx.BuildContext.Target?.GetType());
                if (m == null) return;
                var ps = m.GetParameters();
                if (ps.Length == 0) m.Invoke(ctx.BuildContext.Renderer, null);
                else if (ps.Length == 2) m.Invoke(ctx.BuildContext.Renderer, new[] { oldVal, newVal });
                else if (ps.Length == 3) m.Invoke(ctx.BuildContext.Renderer, new[] { oldVal, newVal, ctx.BuildContext.Target });
            };
            return input;
        }
    }

    // =========================================================================
    // 补充 TypeRenderer（8 个）
    // =========================================================================

    public class Vector4TypeRenderer : IFieldTypeRenderer
    {
        public Type FieldType => typeof(Vector4);
        public IElement CreateWidget(PropertyAccessor a, BuildContext c)
        {
            var f = new Vector4Field(null, a.GetValue() is Vector4 v ? v : Vector4.zero);
            f.OnValueChanged = (n) => a.SetValue(n);
            return f;
        }
    }

    public class Vector2IntTypeRenderer : IFieldTypeRenderer
    {
        public Type FieldType => typeof(Vector2Int);
        public IElement CreateWidget(PropertyAccessor a, BuildContext c)
        {
            var f = new Vector2IntField(null, a.GetValue() is Vector2Int v ? v : Vector2Int.zero);
            f.OnValueChanged = (n) => a.SetValue(n);
            return f;
        }
    }

    public class Vector3IntTypeRenderer : IFieldTypeRenderer
    {
        public Type FieldType => typeof(Vector3Int);
        public IElement CreateWidget(PropertyAccessor a, BuildContext c)
        {
            var f = new Vector3IntField(null, a.GetValue() is Vector3Int v ? v : Vector3Int.zero);
            f.OnValueChanged = (n) => a.SetValue(n);
            return f;
        }
    }

    public class RectTypeRenderer : IFieldTypeRenderer
    {
        public Type FieldType => typeof(Rect);
        public IElement CreateWidget(PropertyAccessor a, BuildContext c)
        {
            var f = new RectField(null, a.GetValue() is Rect v ? v : Rect.zero);
            f.OnValueChanged = (n) => a.SetValue(n);
            return f;
        }
    }

    public class BoundsTypeRenderer : IFieldTypeRenderer
    {
        public Type FieldType => typeof(Bounds);
        public IElement CreateWidget(PropertyAccessor a, BuildContext c)
        {
            var f = new BoundsField(null, a.GetValue() is Bounds v ? v : new Bounds());
            f.OnValueChanged = (n) => a.SetValue(n);
            return f;
        }
    }

    public class CurveTypeRenderer : IFieldTypeRenderer
    {
        public Type FieldType => typeof(AnimationCurve);
        public IElement CreateWidget(PropertyAccessor a, BuildContext c)
        {
            var f = new CurveField(null, a.GetValue() as AnimationCurve ?? new AnimationCurve());
            f.OnValueChanged = (n) => a.SetValue(n);
            return f;
        }
    }

    public class GradientTypeRenderer : IFieldTypeRenderer
    {
        public Type FieldType => typeof(Gradient);
        public IElement CreateWidget(PropertyAccessor a, BuildContext c)
        {
            var f = new GradientField(null, a.GetValue() as Gradient ?? new Gradient());
            f.OnValueChanged = (n) => a.SetValue(n);
            return f;
        }
    }

    public class LayerMaskTypeRenderer : IFieldTypeRenderer
    {
        public Type FieldType => typeof(LayerMask);
        public IElement CreateWidget(PropertyAccessor a, BuildContext c)
        {
            var val = a.GetValue();
            var f = new LayerField(null, val is int i ? i : 0);
            f.OnValueChanged = (n) => a.SetValue((LayerMask)n);
            return f;
        }
    }
}
