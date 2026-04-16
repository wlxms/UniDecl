using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using MCLV = UniDecl.Editor.UIToolKit.MultiColumnListView;
using UTKMCLV = UnityEngine.UIElements.MultiColumnListView;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitMultiColumnListViewRenderer : IElementRenderer<MCLV, VisualElement>
    {
        public VisualElement Render(MCLV element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var listView = new UTKMCLV();

            if (element.ItemsSource != null)
                listView.itemsSource = element.ItemsSource;
            if (element.SelectionType.HasValue)
                listView.selectionType = (SelectionType)element.SelectionType.Value;
            listView.showBorder = element.ShowBorder;
            if (!string.IsNullOrEmpty(element.HeaderTitle))
                listView.headerTitle = element.HeaderTitle;
            if (element.ItemHeight > 0)
                listView.itemHeight = (int)element.ItemHeight;

            if (element.OnSelectionChanged != null)
                listView.selectionChanged += objs => element.OnSelectionChanged(objs);
            if (element.OnItemClicked != null)
                listView.itemsChosen += objs => element.OnItemClicked(objs);

            UIToolkitStyleApplier.ApplyElementStyles(element, listView);
            return listView;
        }
    }
}
