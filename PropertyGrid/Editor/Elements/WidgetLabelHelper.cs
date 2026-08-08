using System.Reflection;
using UniDecl.BuiltIn.Runtime.Core;

namespace UniDecl.PropertyGrid.Editor
{
    /// <summary>
    /// 设置 Widget 的 Label 字段——让 Unity BaseField 原生渲染 label + input 水平排列。
    /// </summary>
    internal static class WidgetLabelHelper
    {
        /// <summary>设置 Widget 的 Label 属性（如果存在）。</summary>
        public static void SetLabel(IElement element, string label)
        {
            if (element == null) return;
            var prop = element.GetType().GetProperty("Label",
                BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
                prop.SetValue(element, label);
        }

        /// <summary>清空 Widget 的 Label（兼容旧调用）。</summary>
        public static void ClearLabel(IElement element) => SetLabel(element, null);
    }
}
