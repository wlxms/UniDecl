using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitH1Renderer : IElementRenderer<W.H1, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.H1, VisualElement>
    {
        public VisualElement Render(W.H1 element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;
            var label = new Label(element.Text) { enableRichText = element.EnableRichText };
            UIToolkitStyleApplier.ApplyElementStyles(element, label);
            return label;
        }

        public bool TryUpdate(W.H1 element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is Label ve)
            {
                ve.text = element.Text;
                ve.enableRichText = element.EnableRichText;
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.H1 h && TryUpdate(h, existing, manager, state);
    }

    public class UIToolkitH2Renderer : IElementRenderer<W.H2, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.H2, VisualElement>
    {
        public VisualElement Render(W.H2 element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;
            var label = new Label(element.Text) { enableRichText = element.EnableRichText };
            UIToolkitStyleApplier.ApplyElementStyles(element, label);
            return label;
        }

        public bool TryUpdate(W.H2 element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is Label ve)
            {
                ve.text = element.Text;
                ve.enableRichText = element.EnableRichText;
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.H2 h && TryUpdate(h, existing, manager, state);
    }

    public class UIToolkitH3Renderer : IElementRenderer<W.H3, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.H3, VisualElement>
    {
        public VisualElement Render(W.H3 element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;
            var label = new Label(element.Text) { enableRichText = element.EnableRichText };
            UIToolkitStyleApplier.ApplyElementStyles(element, label);
            return label;
        }

        public bool TryUpdate(W.H3 element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is Label ve)
            {
                ve.text = element.Text;
                ve.enableRichText = element.EnableRichText;
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.H3 h && TryUpdate(h, existing, manager, state);
    }

    public class UIToolkitH4Renderer : IElementRenderer<W.H4, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.H4, VisualElement>
    {
        public VisualElement Render(W.H4 element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;
            var label = new Label(element.Text) { enableRichText = element.EnableRichText };
            UIToolkitStyleApplier.ApplyElementStyles(element, label);
            return label;
        }

        public bool TryUpdate(W.H4 element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is Label ve)
            {
                ve.text = element.Text;
                ve.enableRichText = element.EnableRichText;
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.H4 h && TryUpdate(h, existing, manager, state);
    }

    public class UIToolkitH5Renderer : IElementRenderer<W.H5, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.H5, VisualElement>
    {
        public VisualElement Render(W.H5 element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;
            var label = new Label(element.Text) { enableRichText = element.EnableRichText };
            UIToolkitStyleApplier.ApplyElementStyles(element, label);
            return label;
        }

        public bool TryUpdate(W.H5 element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is Label ve)
            {
                ve.text = element.Text;
                ve.enableRichText = element.EnableRichText;
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.H5 h && TryUpdate(h, existing, manager, state);
    }

    public class UIToolkitH6Renderer : IElementRenderer<W.H6, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.H6, VisualElement>
    {
        public VisualElement Render(W.H6 element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;
            var label = new Label(element.Text) { enableRichText = element.EnableRichText };
            UIToolkitStyleApplier.ApplyElementStyles(element, label);
            return label;
        }

        public bool TryUpdate(W.H6 element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is Label ve)
            {
                ve.text = element.Text;
                ve.enableRichText = element.EnableRichText;
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.H6 h && TryUpdate(h, existing, manager, state);
    }
}
