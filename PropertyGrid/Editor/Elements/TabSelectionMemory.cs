using System.Collections.Generic;

namespace UniDecl.PropertyGrid.Editor.Elements
{
    /// <summary>
    /// 页签选择记忆——按 Tabs 视图 Key 跨整树重建保存当前页索引。
    /// 任何控件交互（折叠、勾选、按钮）都可能触发根级重建（DOMTree.FindRebuildTarget
    /// 上溯到根元素），布局节点只携带初始索引；没有记忆则每次重建都跳回第一页。
    /// 生命周期为编辑器域会话，与其它静态 UI 状态一致。
    /// </summary>
    public static class TabSelectionMemory
    {
        static readonly Dictionary<string, int> Selection = new Dictionary<string, int>();

        public static int Get(string viewKey, int fallback)
        {
            return Selection.TryGetValue(viewKey, out var v) ? v : fallback;
        }

        public static void Set(string viewKey, int index)
        {
            Selection[viewKey] = index;
        }

        /// <summary>测试用：清空记忆。</summary>
        public static void Clear() => Selection.Clear();
    }
}
