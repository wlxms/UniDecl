using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using TV = UniDecl.Editor.UIToolKit.TreeView;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitTreeViewRenderer : IElementRenderer<TV, VisualElement>
    {
        public VisualElement Render(TV element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var treeView = new UnityEngine.UIElements.TreeView();
            treeView.makeItem = element.MakeItem;
            treeView.bindItem = element.BindItem;
            if (element.SelectionType.HasValue)
                treeView.selectionType = (SelectionType)element.SelectionType.Value;
            treeView.showBorder = element.ShowBorder;
            if (!string.IsNullOrEmpty(element.HeaderTitle))
                treeView.viewDataKey = element.HeaderTitle;

            if (element.OnSelectionChanged != null)
                treeView.selectionChanged += objs => element.OnSelectionChanged(objs);

            UIToolkitStyleApplier.ApplyElementStyles(element, treeView);
            return treeView;
        }
    }
}
