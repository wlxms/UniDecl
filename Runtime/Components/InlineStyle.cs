using System;
using System.Linq;

namespace UniDecl.Runtime.Components
{
    /// <summary>
    /// 轻量级运行时内联样式组件，携带 CSS class name 和通用样式属性。
    /// 实现了 <see cref="IInlineStyle"/> 接口，可在任何渲染后端使用。
    /// <para>
    /// 对于 UI Toolkit 后端，推荐直接使用 <see cref="UITKStyle"/> 以获得完整样式支持。
    /// 此类适合仅需 class name 的场景（如 Widget 内部默认样式）。
    /// </para>
    /// </summary>
    public sealed class InlineStyle : IInlineStyle
    {
        private string[] _classNames;
        public string[] ClassNames => _classNames;
        public string Tooltip { get; set; }
        public float? Width { get; set; }
        public float? Height { get; set; }
        public float? MinWidth { get; set; }
        public float? MaxWidth { get; set; }
        public float? MinHeight { get; set; }
        public float? MaxHeight { get; set; }
        public float? MarginLeft { get; set; }
        public float? MarginRight { get; set; }
        public float? MarginTop { get; set; }
        public float? MarginBottom { get; set; }
        public float? PaddingLeft { get; set; }
        public float? PaddingRight { get; set; }
        public float? PaddingTop { get; set; }
        public float? PaddingBottom { get; set; }
        public bool? Visible { get; set; }

        public InlineStyle() { }

        public InlineStyle(params string[] classNames)
        {
            _classNames = classNames ?? Array.Empty<string>();
        }

        public InlineStyle AddClass(params string[] classes)
        {
            if (classes == null || classes.Length == 0) return this;
            if (_classNames == null || _classNames.Length == 0)
                _classNames = classes;
            else
                _classNames = new System.Collections.Generic.List<string>(_classNames).Concat(classes).ToArray();
            return this;
        }
    }

}
