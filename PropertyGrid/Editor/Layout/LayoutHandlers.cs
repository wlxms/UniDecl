using System;
using System.Collections.Generic;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.BuiltIn.Runtime.Widgets;
using UniDecl.PropertyGrid.Runtime;

namespace UniDecl.PropertyGrid.Editor
{
    public interface ILayoutHandler { Type LayoutNodeType { get; } Type AttributeType { get; } ILayoutNode CreateNode(string path); void MergeAttribute(ILayoutNode node, LayoutGroupAttribute attr); IElement Build(ILayoutNode node, BuildContext ctx); }
    public interface ILayoutHandler<TNode> : ILayoutHandler where TNode : ILayoutNode { new TNode CreateNode(string path); void MergeAttribute(TNode node, LayoutGroupAttribute attr); IElement Build(TNode node, BuildContext ctx); }

    public abstract class BuiltInLayoutHandlerBase<TNode> : ILayoutHandler<TNode> where TNode : GroupLayoutNode, new()
    {
        public abstract Type AttributeType { get; }
        public Type LayoutNodeType => typeof(TNode);
        public TNode CreateNode(string path) => new TNode { Path = path, DisplayName = path };
        public virtual void MergeAttribute(TNode node, LayoutGroupAttribute attr) { if (attr == null) return; if (attr.Title != null) node.DisplayName = attr.Title; node.Order = attr.Order; if (!string.IsNullOrEmpty(attr.StyleClass)) node.StyleClass = string.IsNullOrEmpty(node.StyleClass) ? attr.StyleClass : node.StyleClass + " " + attr.StyleClass; }
        public abstract IElement Build(TNode node, BuildContext ctx);
        Type ILayoutHandler.LayoutNodeType => LayoutNodeType; Type ILayoutHandler.AttributeType => AttributeType;
        ILayoutNode ILayoutHandler.CreateNode(string p) => CreateNode(p);
        void ILayoutHandler.MergeAttribute(ILayoutNode n, LayoutGroupAttribute a) => MergeAttribute((TNode)n, a);
        IElement ILayoutHandler.Build(ILayoutNode n, BuildContext c) => Build((TNode)n, c);
    }

    public class HGroupLayoutHandler : BuiltInLayoutHandlerBase<HGroupLayoutNode> { public override Type AttributeType => typeof(HGroupAttribute); public override IElement Build(HGroupLayoutNode n, BuildContext c) { var h = new HorizontalLayout(); h.WithKey($"group_{n.Path}"); return h; } }
    public class VGroupLayoutHandler : BuiltInLayoutHandlerBase<VGroupLayoutNode> { public override Type AttributeType => typeof(VGroupAttribute); public override IElement Build(VGroupLayoutNode n, BuildContext c) { var v = new VerticalLayout(); v.WithKey($"group_{n.Path}"); return v; } }
    public class BoxGroupLayoutHandler : BuiltInLayoutHandlerBase<BoxGroupLayoutNode> { public override Type AttributeType => typeof(BoxGroupAttribute); public override IElement Build(BoxGroupLayoutNode n, BuildContext c) { var f = new Foldout(n.DisplayName ?? n.Path); f.WithKey($"group_{n.Path}"); return f; } }
    public class FoldoutLayoutHandler : BuiltInLayoutHandlerBase<FoldoutLayoutNode> { public override Type AttributeType => typeof(FoldoutGroupAttribute); public override void MergeAttribute(FoldoutLayoutNode n, LayoutGroupAttribute a) { base.MergeAttribute(n, a); if (a is FoldoutGroupAttribute fa) n.Expanded = fa.Expanded; } public override IElement Build(FoldoutLayoutNode n, BuildContext c) { var f = new Foldout(n.DisplayName ?? n.Path); f.WithKey($"group_{n.Path}"); f.Value = n.Expanded; return f; } }
    public class HeaderLayoutHandler : BuiltInLayoutHandlerBase<HeaderLayoutNode> { public override Type AttributeType => typeof(HeaderGroupAttribute); public override IElement Build(HeaderLayoutNode n, BuildContext c) { var f = new Foldout(n.DisplayName ?? n.Path); f.WithKey($"group_{n.Path}"); return f; } }
    public class TabLayoutHandler : BuiltInLayoutHandlerBase<TabLayoutNode> { public override Type AttributeType => typeof(TabGroupAttribute); public override IElement Build(TabLayoutNode n, BuildContext c) { var f = new Foldout(n.DisplayName ?? n.Path); f.WithKey($"group_{n.Path}"); return f; } }

    public class ObjectLayoutHandler : ILayoutHandler<ObjectLayoutNode>
    {
        public Type LayoutNodeType => typeof(ObjectLayoutNode); public Type AttributeType => null;
        public ObjectLayoutNode CreateNode(string p) => new ObjectLayoutNode { Path = p, DisplayName = p };
        public void MergeAttribute(ObjectLayoutNode n, LayoutGroupAttribute a) { }
        public IElement Build(ObjectLayoutNode n, BuildContext c) { var v = n.Parent == null ? new VerticalLayout() : (ContainerElement)(n.Direction == InlineDirection.Horizontal ? new HorizontalLayout() : new VerticalLayout()); v.WithKey(n.Parent == null ? "root" : $"obj_{n.DisplayName}"); return v; }
        Type ILayoutHandler.LayoutNodeType => LayoutNodeType; Type ILayoutHandler.AttributeType => AttributeType;
        ILayoutNode ILayoutHandler.CreateNode(string p) => CreateNode(p);
        void ILayoutHandler.MergeAttribute(ILayoutNode n, LayoutGroupAttribute a) => MergeAttribute((ObjectLayoutNode)n, a);
        IElement ILayoutHandler.Build(ILayoutNode n, BuildContext c) => Build((ObjectLayoutNode)n, c);
    }
}
