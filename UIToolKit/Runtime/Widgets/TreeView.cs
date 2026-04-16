using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;

namespace UniDecl.Editor.UIToolKit
{
    public class TreeView : Element
    {
        public IList ItemsSource { get; set; }
        public Func<VisualElement> MakeItem { get; set; }
        public Action<VisualElement, int> BindItem { get; set; }
        public int? SelectionType { get; set; }
        public bool ShowBorder { get; set; } = true;
        public string HeaderTitle { get; set; }
        public Action<IEnumerable<object>> OnSelectionChanged { get; set; }

        public override IElement Render() => null;

        public TreeView() { }
    }
}
