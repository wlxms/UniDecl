using UnityEngine;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.BuiltIn.Runtime.Snapshot;

namespace UniDecl.PropertyGrid.Editor
{
    /// <summary>
    /// PropertyGrid 根元素——展开为字段编辑子树。
    /// 实现 IScopeProvider：展开子树时为所有子 Widget 提供 UndoScope，
    /// 让 Renderer 通过 ElementState.Scope 接入 Undo/Redo。
    ///
    /// 直接通过 Render() 调用 PropertyGridModule.CreateElementTree 完成展开，
    /// 不再依赖外部的 ElementDomExpanderRegistry。
    /// </summary>
    public class PropertyGridElement : Element, IScopeProvider
    {
        public object Target { get; set; }

        /// <summary>
        /// 宿主 Unity 对象——用于 Undo 系统注册撤销点
        /// 通常是持有此 PropertyGridElement 的 EditorWindow 或 Editor
        /// </summary>
        public UnityEngine.Object HostObject { get; set; }

        /// <summary>
        /// 子 Widget 共享的 Undo/Redo 作用域。
        /// 由 PropertyGridModule.CreateElementTree 在展开时注入。
        /// </summary>
        public UndoScope Scope { get; set; }

        public override IElement Render() => PropertyGridModule.CreateElementTree(this);

        public PropertyGridElement(object target, UnityEngine.Object hostObject = null)
        {
            Target = target;
            HostObject = hostObject;
        }
    }
}
