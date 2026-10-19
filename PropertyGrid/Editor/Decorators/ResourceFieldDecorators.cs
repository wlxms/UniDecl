using System;
using System.Linq;
using System.Reflection;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.BuiltIn.Runtime.Widgets;
using UniDecl.PropertyGrid.Runtime;
using UniDecl.UIToolKit.Runtime;
using UnityEditor;
using UnityEngine;
using PropertyField = UniDecl.PropertyGrid.Editor.Elements.PropertyField;

namespace UniDecl.PropertyGrid.Editor.Decorators
{
    // =========================================================================
    // 资源字段装饰器：[PreviewField] 对象预览、[ColorPalette] 预定义调色板。
    // =========================================================================

    /// <summary>
    /// [PreviewField(Height)]——对象选择器下方追加方形资源预览。
    /// 预览经 IMGUIContainer 每帧重绘，对象切换后自动跟随。
    /// </summary>
    public class PreviewFieldDecorator : ReplacementDecorator
    {
        public override bool Applies(in DecoratorContext ctx)
            => ctx.GetAttribute<PreviewFieldAttribute>() != null
               && typeof(UnityEngine.Object).IsAssignableFrom(ctx.Accessor.PropertyType);

        public override IElement Process(IElement input, DecoratorContext ctx)
        {
            if (!(input is PropertyField pf)) return input;
            var attr = ctx.GetAttribute<PreviewFieldAttribute>();
            var height = Mathf.Max(16, attr.Height);
            const string wrapKeySuffix = "_preview_col";

            // 预览只在有值时包装出现；空对象不占位——否则 128px 空白块把行撑开、
            // 标签与值错位。对象切换后由 FieldChanged 驱动加/拆包装并 Rebuild。
            AttachPreview();
            ctx.BuildContext.Root.FieldChanged += (_, __) =>
            {
                var before = pf.Editor;
                AttachPreview();
                if (!ReferenceEquals(before, pf.Editor) || ctx.Accessor.GetValue() != null)
                    pf.NotifyChanged(); // 空安全的重渲染请求（元素可能尚未挂载宿主）
            };
            return pf;

            bool HasValue()
            {
                // Unity 假空：占位对象需用重载的 == null 判定
                return ctx.Accessor.GetValue() is UnityEngine.Object o && o != null;
            }

            void AttachPreview()
            {
                if (HasValue())
                {
                    if (IsWrapped()) return; // 已带预览，IMGUI 闭包按帧读取最新值
                    var preview = new IMGUIContainer(() =>
                    {
                        if (!(ctx.Accessor.GetValue() is UnityEngine.Object current) || current == null) return;
                        var texture = current is Sprite sprite ? sprite.texture : AssetPreview.GetAssetPreview(current);
                        var rect = GUILayoutUtility.GetRect(height, height, GUILayout.ExpandWidth(true));
                        if (texture != null) GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
                        else GUI.Label(rect, "预览生成中…", GUI.skin.box);
                    });
                    preview.WithKey($"{pf.Editor?.Key ?? "preview"}_preview");
                    preview.With(new UITKStyle { Height = height, MinHeight = height, MarginTop = 2 });

                    var column = new VerticalLayout();
                    column.WithKey($"{pf.Editor?.Key ?? "preview"}{wrapKeySuffix}");
                    if (pf.Editor != null) column.Add(pf.Editor);
                    column.Add(preview);
                    pf.Editor = column;
                }
                else if (IsWrapped())
                {
                    pf.Editor = ((VerticalLayout)pf.Editor).Children.First();
                }
            }

            bool IsWrapped() => pf.Editor is VerticalLayout col && (col.Key?.EndsWith(wrapKeySuffix) ?? false);
        }
    }

    /// <summary>
    /// [ColorPalette(name)]——颜色字段旁追加预定义色板，点击色块即写入。
    /// 色板来源：Renderer 的 Get{PaletteName}Colors() 方法优先，其次内置色板注册表。
    /// </summary>
    public class ColorPaletteDecorator : ReplacementDecorator
    {
        public override bool Applies(in DecoratorContext ctx)
            => ctx.GetAttribute<ColorPaletteAttribute>() != null && ctx.Accessor.PropertyType == typeof(Color);

        public override IElement Process(IElement input, DecoratorContext ctx)
        {
            if (!(input is PropertyField pf)) return input;
            var attr = ctx.GetAttribute<ColorPaletteAttribute>();
            var palette = ColorPaletteRegistry.Resolve(attr.PaletteName, ctx.BuildContext.Renderer);
            if (palette == null || palette.Length == 0) return pf;

            // 原色值编辑器（ColorField widget）——点击色块后除写数据外还要驱动它显示刷新：
            // 编辑器被包进行容器后，通用的 EditorValueSync 按 Value 属性同步会失效
            var colorEditor = pf.Editor;
            if (pf.Editor != null)
                pf.Editor.With(new UITKStyle { FlexGrow = 1, MinWidth = 0 });

            var swatches = new HorizontalLayout();
            swatches.WithKey($"{pf.Editor?.Key ?? "color"}_palette");
            for (int i = 0; i < palette.Length; i++)
            {
                var color = palette[i];
                var swatch = new Button("")
                {
                    OnClick = () =>
                    {
                        ctx.Accessor.SetValue(color);
                        if (colorEditor is UniDecl.BuiltIn.Runtime.Widgets.ColorField cf)
                        {
                            cf.Value = color;
                            cf.NotifyChanged();
                        }
                    },
                };
                swatch.WithKey($"{swatches.Key}_{i}");
                swatch.With(new UITKStyle
                {
                    Width = 16,
                    Height = 16,
                    MarginLeft = 1,
                    BackgroundColor = color,
                    Tooltip = ColorUtility.ToHtmlStringRGB(color),
                });
                swatches.Add(swatch);
            }

            var row = new HorizontalLayout();
            row.WithKey($"{pf.Editor?.Key ?? "color"}_row");
            if (pf.Editor != null) row.Add(pf.Editor);
            row.Add(swatches);

            pf.Editor = row;
            return pf;
        }
    }

    /// <summary>预定义色板注册表；Renderer 方法（Get{PaletteName}Colors）优先于内置项。</summary>
    public static class ColorPaletteRegistry
    {
        static Color[] SkinTones()
        {
            return new[]
            {
                new Color(0.95f, 0.85f, 0.75f), new Color(0.87f, 0.70f, 0.56f), new Color(0.72f, 0.52f, 0.38f),
                new Color(0.55f, 0.38f, 0.26f), new Color(0.38f, 0.26f, 0.18f), new Color(0.22f, 0.15f, 0.11f),
            };
        }

        static Color[] BasicTones()
        {
            return new[]
            {
                Color.white, Color.black, Color.gray, Color.red, new Color(1f, 0.5f, 0f), Color.yellow,
                Color.green, Color.cyan, Color.blue, new Color(0.5f, 0f, 0.5f), Color.magenta, Color.clear,
            };
        }

        /// <summary>内置色板（名称 → 工厂）；用户可 Register 覆盖或追加。</summary>
        static readonly System.Collections.Generic.Dictionary<string, Func<Color[]>> _builtIn =
            new System.Collections.Generic.Dictionary<string, Func<Color[]>>
            {
                ["CharacterSkin"] = SkinTones,
                ["Basic"] = BasicTones,
            };

        public static void Register(string name, Func<Color[]> factory) => _builtIn[name] = factory;

        public static Color[] Resolve(string paletteName, object renderer)
        {
            // 1. Renderer 方法：Get{PaletteName}Colors()
            if (renderer != null && !string.IsNullOrEmpty(paletteName))
            {
                var method = renderer.GetType().GetMethod($"Get{paletteName}Colors",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
                if (method != null)
                {
                    try { if (method.Invoke(renderer, null) is Color[] arr) return arr; }
                    catch { }
                }
            }

            // 2. 内置/注册色板
            return _builtIn.TryGetValue(paletteName, out var factory) ? factory() : null;
        }
    }
}
