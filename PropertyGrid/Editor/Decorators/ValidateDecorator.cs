using System;
using UniDecl.Core;
using UniDecl.Widgets;
using UniDecl.PropertyGrid;
using UnityEngine;
using PropertyField = UniDecl.PropertyGrid.Editor.Elements.PropertyField;

namespace UniDecl.PropertyGrid.Editor.Decorators
{
    /// <summary>
    /// [Validate("Method")]——构建时及任一字段变更后调用 Renderer 校验方法，
    /// 结果渲染为只读 HelpBox（校验结果是显示态，不是可编辑字段——
    /// 可编辑框会让用户输入被下一次校验覆盖）。返回字符串同时写回字段供外部读取。
    /// 方法签名约定与 Button 一致：Method() 或 Method(T target)。
    /// </summary>
    public class ValidateDecorator : ReplacementDecorator
    {
        public override bool Applies(in DecoratorContext ctx)
            => ctx.GetAttribute<ValidateAttribute>() != null
               && ctx.Accessor != null && ctx.Accessor.PropertyType == typeof(string);

        public override IElement Process(IElement input, DecoratorContext ctx)
        {
            if (!(input is PropertyField pf) || pf.Accessor == null) return input;

            var attr = ctx.GetAttribute<ValidateAttribute>();
            var method = FieldBinder.FindMethod(ctx.BuildContext.Renderer?.GetType(), attr.Method, ctx.BuildContext.Target?.GetType());
            if (method == null)
            {
                Debug.LogWarning($"[PropertyGrid] Validate method '{attr.Method}' not found on renderer.");
                return input;
            }

            var box = new HelpBox(RunValidation(), MessageTypeOf(pf.Accessor.GetValue() as string));
            box.WithKey($"{pf.Editor?.Key ?? pf.Accessor.FullPath}_validate");
            pf.Editor = box;

            ctx.BuildContext.Root.FieldChanged += (_, __) =>
            {
                var text = RunValidation();
                if (box.Text != text)
                {
                    box.Text = text;
                    box.MessageType = MessageTypeOf(text);
                    box.NotifyChanged(); // 空安全的重渲染请求（元素可能尚未挂载宿主）
                }
            };
            return pf;

            string RunValidation()
            {
                try
                {
                    var result = method.GetParameters().Length == 0
                        ? method.Invoke(ctx.BuildContext.Renderer, null)
                        : method.Invoke(ctx.BuildContext.Renderer, new[] { ctx.BuildContext.Target });
                    if (result is string text)
                    {
                        if (!string.Equals(pf.Accessor.GetValue() as string, text))
                            pf.Accessor.SetValue(text); // 数据侧同步，供外部属性读取
                        return text;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
                return string.Empty;
            }
        }

        /// <summary>按校验结果内容分级：❌ → Error，⚠ → Warning，其余 Info。</summary>
        static HelpBoxMessageType MessageTypeOf(string text)
        {
            if (string.IsNullOrEmpty(text)) return HelpBoxMessageType.None;
            if (text.Contains("❌")) return HelpBoxMessageType.Error;
            if (text.Contains("⚠")) return HelpBoxMessageType.Warning;
            return HelpBoxMessageType.Info;
        }
    }
}
