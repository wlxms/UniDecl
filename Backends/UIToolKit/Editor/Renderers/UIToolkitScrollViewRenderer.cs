using UnityEngine.UIElements;
using UniDecl.Core;
using UniDecl.Core.Navigation;
using W = UniDecl.Widgets;using UniDecl.Backend.UIToolKit.Editor.Style;
namespace UniDecl.Backend.UIToolKit.Editor.Renderers
{
    public class UIToolkitScrollViewRenderer : IElementRenderer<W.ScrollView, VisualElement>,
        IRendererEventListener<VisualElement, NavigationEvent>
    {
        public VisualElement Render(W.ScrollView element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var scrollView = new UnityEngine.UIElements.ScrollView();
            // flex-grow / shrink / min-height 见 .ud-scrollview（DefaultStyle.uss）
            scrollView.horizontalScrollerVisibility = ToVisibility(element.HorizontalScrollerVisibility);
            scrollView.verticalScrollerVisibility = ToVisibility(element.VerticalScrollerVisibility);

            foreach (var child in element.Children)
            {
                var childElement = manager.RenderElement(child);
                if (childElement != null)
                    scrollView.Add(childElement);
            }

            UIToolkitStyleApplier.ApplyElementStyles(element, scrollView);
            return scrollView;
        }

        static UnityEngine.UIElements.ScrollerVisibility ToVisibility(W.ScrollerVisibility v)
            => v == W.ScrollerVisibility.AlwaysVisible ? UnityEngine.UIElements.ScrollerVisibility.AlwaysVisible
             : v == W.ScrollerVisibility.Hidden ? UnityEngine.UIElements.ScrollerVisibility.Hidden
             : UnityEngine.UIElements.ScrollerVisibility.Auto;

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
