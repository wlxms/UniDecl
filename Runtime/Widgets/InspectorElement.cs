using UnityEngine;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class InspectorElement : Element
    {
        public object Target { get; set; }

        /// <summary>
        /// 宿主 Unity 对象——用于 Undo 系统注册撤销点
        /// 通常是持有此 InspectorElement 的 EditorWindow 或 Editor
        /// </summary>
        public UnityEngine.Object HostObject { get; set; }

        public override IElement Render() => null;

        public InspectorElement(object target, UnityEngine.Object hostObject = null)
        {
            Target = target;
            HostObject = hostObject;
        }
    }
}
