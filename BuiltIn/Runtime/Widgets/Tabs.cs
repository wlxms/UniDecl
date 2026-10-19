using System;
using System.Collections.Generic;
using System.Linq;
using UniDecl.BuiltIn.Runtime.Core;

namespace UniDecl.BuiltIn.Runtime.Widgets
{
    /// <summary>
    /// 页签容器——页签条 + 当前页内容。
    /// 页在构建期静态声明（PropertyGrid TabGroup 布局的直接产物），非动态增删。
    /// 实现 ContainerElement：页内容作为 Children 参与 DOM 树构建，
    /// 渲染器据此对当前选中页调用 RenderElement（与 Foldout 的子级渲染模式一致）。
    /// </summary>
    public class Tabs : ContainerElement
    {
        /// <summary>单页：标题 + 内容元素</summary>
        public sealed class TabPage
        {
            public string Title;
            public IElement Content;
        }

        private readonly List<TabPage> _pages = new List<TabPage>();
        public IReadOnlyList<TabPage> Pages => _pages;

        public int SelectedIndex { get; set; }
        public Action<int> OnSelectionChanged { get; set; }

        /// <summary>
        /// 切页是否纳入 Undo/Redo（默认纳入）。切页属视图状态，但编辑器里用户期望 Ctrl+Z
        /// 能退回上一个页签；纯展示型页签可置 false 保持不进撤销栈。
        /// </summary>
        public bool SelectionUndoable { get; set; } = true;

        public override IElement Render() => null;

        /// <summary>DOM 子级 = 各页内容（顺序与页一致）。</summary>
        public override IEnumerable<IElement> Children => _pages.Select(p => p.Content).Where(c => c != null);

        /// <summary>ContainerElement 接口实现：追加无标题页（一般走 AddPage）。</summary>
        public override void Add(IElement element) => AddPage(string.Empty, element);

        public void AddPage(string title, IElement content)
        {
            _pages.Add(new TabPage { Title = title, Content = content });
        }
    }
}
