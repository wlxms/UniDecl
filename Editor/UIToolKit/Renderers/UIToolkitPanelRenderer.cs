using System.Collections.Generic;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    /// <summary>
    /// Panel容器组件（作为根容器）
    /// </summary>
    public class Panel : ContainerElement
    {
        private readonly List<IElement> _children = new List<IElement>();
        public override IEnumerable<IElement> Children => _children;
        public override void Add(IElement element) => _children.Add(element);
        public override IElement Render() => null;
    }

    public class UIToolkitPanelRenderer : IElementRenderer<Panel, VisualElement>
    {
        public VisualElement Render(Panel element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;

            foreach (var child in element.Children)
            {
                var childElement = manager.RenderElement(child);
                if (childElement != null)
                    container.Add(childElement);
            }

            UIToolkitStyleApplier.ApplyElementStyles(element, container);
            return container;
        }
    }
}
