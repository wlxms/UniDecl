using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Runtime.Navigation;
using W = UniDecl.Runtime.Widgets;using UniDecl.Editor.UIToolKit.Style;
namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitScrollViewRenderer : IElementRenderer<W.ScrollView, VisualElement>,
        IRendererEventListener<VisualElement, NavigationEvent>
    {
        public VisualElement Render(W.ScrollView element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var scrollView = new UnityEngine.UIElements.ScrollView();
            scrollView.style.flexGrow = 1;
            scrollView.style.flexShrink = 1;

            foreach (var child in element.Children)
            {
                var childElement = manager.RenderElement(child);
                if (childElement != null)
                    scrollView.Add(childElement);
            }

            UIToolkitStyleApplier.ApplyElementStyles(element, scrollView);
            return scrollView;
        }

        public void OnEvent(NavigationEvent @event, DOMNode<VisualElement> node, DOMTree<VisualElement> tree)
        {
            if (@event.IsTarget) return;
            if (!(node.RenderResult is UnityEngine.UIElements.ScrollView scrollView)) return;

            // 通过 anchorId 在 DOMTree 中找到目标节点，取其 RenderResult 作为滚动目标
            var targetNode = tree.GetNodeByAnchor(@event.AnchorId);
            if (targetNode == null) return;

            // 穿透到第一个有 RenderResult 的后代
            var renderable = targetNode;
            while (renderable != null && renderable.RenderResult == null && renderable.Children != null && renderable.Children.Count > 0)
                renderable = renderable.Children[0] as DOMNode<VisualElement>;

            var targetVE = renderable?.RenderResult;
            if (targetVE == null) return;

            // 延迟一帧等待 Foldout 展开和布局生效后再滚动
            targetVE.schedule.Execute(() =>
            {
                if (targetVE.parent != null)
                    scrollView.ScrollTo(targetVE);
            });
        }
    }
}
