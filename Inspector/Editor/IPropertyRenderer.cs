using System.Reflection;
using UniDecl.Runtime.Core;

namespace UniDecl.Inspector.Editor
{
    /// <summary>
    /// 属性渲染器接口——用户可自定义特定字段的渲染方式
    /// V1 预留，暂不实现
    /// </summary>
    public interface IPropertyRenderer
    {
        /// <summary>
        /// 是否能渲染指定字段
        /// </summary>
        bool CanRender(FieldInfo field);

        /// <summary>
        /// 渲染字段为 Element
        /// </summary>
        IElement Render(string label, object value, object target);
    }
}
