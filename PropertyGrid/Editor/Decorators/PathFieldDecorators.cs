using System;
using UniDecl.Core;
using UniDecl.Widgets;
using UniDecl.PropertyGrid;
using UniDecl.Backend.UIToolKit;
using UnityEditor;
using PropertyField = UniDecl.PropertyGrid.Editor.Elements.PropertyField;
// UnityEditor.FilePathAttribute 与本框架特性同名，显式别名消歧
using FilePathAttribute = UniDecl.PropertyGrid.FilePathAttribute;

namespace UniDecl.PropertyGrid.Editor.Decorators
{
    // =========================================================================
    // 路径字段装饰器：[FilePath]/[FolderPath]——在文本框旁追加浏览按钮，
    // 打开系统对话框并把结果写回字段（经 accessor 提交，走统一的刷新链路）。
    // =========================================================================

    /// <summary>[FilePath(Extensions, Parent)]——文件路径选择。</summary>
    public class FilePathDecorator : ReplacementDecorator
    {
        public override bool Applies(in DecoratorContext ctx)
            => ctx.GetAttribute<FilePathAttribute>() != null && ctx.Accessor.PropertyType == typeof(string);

        public override IElement Process(IElement input, DecoratorContext ctx)
        {
            if (!(input is PropertyField pf)) return input;
            var attr = ctx.GetAttribute<FilePathAttribute>();
            return PathFieldDecoratorImpl.WrapWithBrowseButton(pf, ctx, "选择文件", parent =>
            {
                var firstExt = attr.Extensions?.Split(',', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                return EditorUtility.OpenFilePanel("选择文件", parent ?? "", firstExt ?? "");
            }, attr.Parent ?? "");
        }
    }

    /// <summary>[FolderPath(Parent)]——文件夹路径选择。</summary>
    public class FolderPathDecorator : ReplacementDecorator
    {
        public override bool Applies(in DecoratorContext ctx)
            => ctx.GetAttribute<FolderPathAttribute>() != null && ctx.Accessor.PropertyType == typeof(string);

        public override IElement Process(IElement input, DecoratorContext ctx)
        {
            if (!(input is PropertyField pf)) return input;
            var attr = ctx.GetAttribute<FolderPathAttribute>();
            return PathFieldDecoratorImpl.WrapWithBrowseButton(pf, ctx, "选择文件夹", parent =>
                EditorUtility.OpenFolderPanel("选择文件夹", parent ?? "", ""));
        }
    }

    /// <summary>路径字段共用实现：文本编辑器 + 浏览按钮的水平行。</summary>
    internal static class PathFieldDecoratorImpl
    {
        public delegate string BrowseDialog(string parent);

        public static IElement WrapWithBrowseButton(PropertyField pf, DecoratorContext ctx, string tooltip, BrowseDialog dialog, string parent = "")
        {
            // 原文本编辑器——浏览确认后除写数据外直接驱动其显示刷新：
            // 编辑器被包进行容器后，通用的 EditorValueSync 按 Value 属性同步会失效
            var textEditor = pf.Editor;
            if (pf.Editor != null)
                pf.Editor.With(new UITKStyle { FlexGrow = 1, MinWidth = 0 });

            var browse = new Button("…") { OnClick = () =>
            {
                var path = dialog(parent);
                if (!string.IsNullOrEmpty(path))
                {
                    ctx.Accessor.SetValue(path);
                    if (textEditor is UniDecl.Widgets.TextField tf)
                    {
                        tf.Value = path;
                        tf.NotifyChanged();
                    }
                }
            } };
            browse.WithKey($"{pf.Editor?.Key ?? "path"}_browse");
            browse.With(new UITKStyle { Width = 26, Tooltip = tooltip });

            var row = new HorizontalLayout();
            row.WithKey($"{pf.Editor?.Key ?? "path"}_row");
            if (pf.Editor != null) row.Add(pf.Editor);
            row.Add(browse);

            pf.Editor = row;
            return pf;
        }
    }
}
