using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.BuiltIn.Runtime.Snapshot;

namespace UniDecl.PropertyGrid.Editor
{
    /// <summary>
    /// PropertyGrid 根元素——展开为字段编辑子树。
    /// 实现 IScopeProvider 标记：子树需要 Undo/Redo 作用域，
    /// 实际 Scope 由 Host（ElementRenderHostBase 注入 SnapshotManager 后）
    /// 写入 ElementState.Scope，本类无需关注快照细节。
    ///
    /// 直接通过 Render() 调用 PropertyGridModule.CreateElementTree 完成展开，
    /// 不再依赖外部的 ElementDomExpanderRegistry。
    /// </summary>
    public class PropertyGridElement : Element, IScopeProvider
    {
        public object Target { get; set; }

        public override IElement Render() => PropertyGridModule.CreateElementTree(this);

        public PropertyGridElement(object target)
        {
            Target = target;
        }
    }
}
