using System;

namespace UniDecl.BuiltIn.Runtime.Core
{
    /// <summary>
    /// 标记一个类为 RenderHost Plugin。ElementRenderHost 实例化后会反射发现所有带此特性的类，
    /// 实例化并调用 OnPluginSetup(host)。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class RenderHostPluginAttribute : Attribute
    {
        public string Name { get; }
        public RenderHostPluginAttribute(string name) { Name = name; }
    }

    /// <summary>
    /// RenderHost Plugin 接口——用户实现 OnPluginSetup 在此注册渲染器、样式表等。
    /// host 参数是具体的 RenderHost 实例（如 UIToolkitRenderManager），
    /// 用户需自行类型判断（如 `if (host is IElementRenderHost&lt;VisualElement&gt; veHost)`）。
    /// </summary>
    public interface IElementRenderHostPlugin
    {
        string Name { get; }
        void OnPluginSetup(IElementRenderHostBase host);
    }
}
