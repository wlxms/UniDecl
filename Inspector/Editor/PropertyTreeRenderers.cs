using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using UniDecl.Inspector.Editor.Elements;

namespace UniDecl.Inspector.Editor
{
    /// <summary>
    /// InspectorPropertyField 的 UIToolkit 渲染器
    /// </summary>
    public class UIToolkitInspectorPropertyFieldRenderer : IElementRenderer<InspectorPropertyField, VisualElement>
    {
        public VisualElement Render(InspectorPropertyField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var container = new VisualElement();
            container.AddToClassList("inspector-property-field");

            // 渲染子元素（FieldWidget）
            if (element.FieldWidget != null)
            {
                var result = manager.RenderElement(element.FieldWidget);
                if (result != null)
                    container.Add(result);
            }

            foreach (var child in element.Children)
            {
                var childResult = manager.RenderElement(child);
                if (childResult != null)
                    container.Add(childResult);
            }

            UIToolkitStyleApplier.ApplyElementStyles(element, container);
            return container;
        }
    }

    /// <summary>
    /// InspectorGroupBox 的 UIToolkit 渲染器
    /// </summary>
    public class UIToolkitInspectorGroupBoxRenderer : IElementRenderer<InspectorGroupBox, VisualElement>
    {
        public VisualElement Render(InspectorGroupBox element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            VisualElement container;

            switch (element.Type)
            {
                case GroupType.Foldout:
                case GroupType.Header:
                    var foldout = new UnityEngine.UIElements.Foldout
                    {
                        text = element.Title ?? element.GroupPath,
                        value = element.Expanded,
                    };
                    container = foldout;
                    break;

                case GroupType.Box:
                    var box = new VisualElement();
                    box.AddToClassList("inspector-box-group");
                    if (!string.IsNullOrEmpty(element.Title))
                    {
                        var label = new UnityEngine.UIElements.Label(element.Title);
                        label.AddToClassList("inspector-box-title");
                        box.Add(label);
                    }
                    container = box;
                    break;

                case GroupType.Horizontal:
                    var hbox = new VisualElement();
                    hbox.style.flexDirection = FlexDirection.Row;
                    hbox.AddToClassList("inspector-horizontal-group");
                    container = hbox;
                    break;

                default:
                    container = new VisualElement();
                    break;
            }

            container.AddToClassList("inspector-group");

            foreach (var child in element.Children)
            {
                var childResult = manager.RenderElement(child);
                if (childResult != null)
                    container.Add(childResult);
            }

            UIToolkitStyleApplier.ApplyElementStyles(element, container);
            return container;
        }
    }

    /// <summary>
    /// InspectorConditionalElement 的 UIToolkit 渲染器
    /// </summary>
    public class UIToolkitInspectorConditionalElementRenderer : IElementRenderer<InspectorConditionalElement, VisualElement>
    {
        public VisualElement Render(InspectorConditionalElement element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var container = new VisualElement();

            if (!element.IsVisible)
            {
                container.style.display = DisplayStyle.None;
                return container;
            }

            foreach (var child in element.Children)
            {
                var childResult = manager.RenderElement(child);
                if (childResult != null)
                    container.Add(childResult);
            }

            UIToolkitStyleApplier.ApplyElementStyles(element, container);
            return container;
        }
    }

    /// <summary>
    /// InspectorButtonElement 的 UIToolkit 渲染器
    /// </summary>
    public class UIToolkitInspectorButtonElementRenderer : IElementRenderer<InspectorButtonElement, VisualElement>
    {
        public VisualElement Render(InspectorButtonElement element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var button = new UnityEngine.UIElements.Button(() => element.OnClick?.Invoke())
            {
                text = element.Label,
            };
            button.AddToClassList("inspector-button");

            UIToolkitStyleApplier.ApplyElementStyles(element, button);
            return button;
        }
    }
}
