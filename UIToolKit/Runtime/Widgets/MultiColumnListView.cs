using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;

namespace UniDecl.Editor.UIToolKit
{
    public class MultiColumnListView : Element
    {
        public IList ItemsSource { get; set; }
        public int? SelectionType { get; set; }
        public bool ShowBorder { get; set; } = true;
        public string HeaderTitle { get; set; }
        public float ItemHeight { get; set; } = 20f;
        public Action<IEnumerable<object>> OnSelectionChanged { get; set; }
        public Action<IEnumerable<object>> OnItemClicked { get; set; }

        public override IElement Render() => null;

        public MultiColumnListView() { }
    }
}
