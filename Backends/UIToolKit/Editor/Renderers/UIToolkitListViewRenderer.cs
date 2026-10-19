using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UniDecl.Core;
using UniDecl.Backend.UIToolKit.Editor.Style;
using LV = UniDecl.Backend.UIToolKit.ListView;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers
{
    public class UIToolkitListViewRenderer : IElementRenderer<LV, VisualElement>
    {
        public VisualElement Render(LV element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var listView = new UnityEngine.UIElements.ListView();

            if (element.ItemsSource != null)
                listView.itemsSource = element.ItemsSource;
            if (element.MakeItem != null)
                listView.makeItem = element.MakeItem;
            if (element.BindItem != null)
                listView.bindItem = element.BindItem;
            if (element.SelectionType.HasValue)
                listView.selectionType = (SelectionType)element.SelectionType.Value;
            listView.showBorder = element.ShowBorder;
            if (!string.IsNullOrEmpty(element.HeaderTitle))
                listView.headerTitle = element.HeaderTitle;
            if (element.ItemHeight > 0)
                listView.itemHeight = (int)element.ItemHeight;

            if (element.OnSelectionChanged != null)
                listView.selectionChanged += objs => element.OnSelectionChanged(objs);
            if (element.OnItemsChosen != null)
                listView.itemsChosen += objs => element.OnItemsChosen(objs);

            UIToolkitStyleApplier.ApplyElementStyles(element, listView);
            return listView;
        }
    }
}
