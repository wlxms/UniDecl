using System.Collections.Generic;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Components
{
    /// <summary>
    /// 渲染后端无关的内联样式接口。
    /// 定义跨后端通用的样式属性子集（class name、tooltip、尺寸、间距、可见性），
    /// 供 Runtime 层 Widget 使用，避免依赖特定渲染后端的类型系统。
    /// <para>
    /// 后端特化实现（如 UITKStyle）可扩展此接口，添加后端专属属性。
    /// </para>
    /// </summary>
    public interface IInlineStyle : IElementComponent
    {
        /// <summary>CSS class name 列表，由渲染后端映射为对应样式。</summary>
        string[] ClassNames { get; }

        /// <summary>鼠标悬停提示文本。</summary>
        string Tooltip { get; set; }

        // === Sizing ===
        float? Width { get; set; }
        float? Height { get; set; }
        float? MinWidth { get; set; }
        float? MaxWidth { get; set; }
        float? MinHeight { get; set; }
        float? MaxHeight { get; set; }

        // === Spacing (px) ===
        float? MarginLeft { get; set; }
        float? MarginRight { get; set; }
        float? MarginTop { get; set; }
        float? MarginBottom { get; set; }
        float? PaddingLeft { get; set; }
        float? PaddingRight { get; set; }
        float? PaddingTop { get; set; }
        float? PaddingBottom { get; set; }

        // === Visibility ===
        bool? Visible { get; set; }
    }
}
