using System;
using System.Diagnostics;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.BuiltIn.Runtime.Widgets;
using UniDecl.Editor.UIToolKit;
using UniDecl.PropertyGrid.Editor;
using UniDecl.PropertyGrid.Runtime;
using UnityEditor;
using UnityEngine;
using IAttr = UniDecl.PropertyGrid.Runtime;

namespace UniDecl.PropertyGrid.Editor.Examples
{
    // =========================================================================
    // 外源扩展范式：自定义 [CardGroup] 组类型
    //
    // 演示如何不改 UniDecl 源码，通过 Plugin 体系新增一个组类型：
    //   1. 自定义 Attribute（CardGroupAttribute）
    //   2. 自定义 ILayoutNode 子类（CardLayoutNode，持特有字段 AccentColor）
    //   3. 自定义 ILayoutHandler<CardLayoutNode>（解析 + 渲染合一）
    //   4. 自定义 Plugin（[UniPropertyGridPlugin] 标记，OnInit 注册 Handler）
    //
    // PluginDiscovery 会自动扫描并加载，无需手动注册。
    // =========================================================================

    // ---- 1. 自定义 Attribute ----
    /// <summary>卡片组——带强调色的可折叠容器</summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class CardGroupAttribute : LayoutGroupAttribute
    {
        /// <summary>强调色（CSS 风格字符串，如 "#FF5722"）</summary>
        public string AccentColor;
        public CardGroupAttribute(string path) : base(path) { }
    }

    // ---- 2. 自定义 ILayoutNode 子类 ----
    /// <summary>卡片组节点——携带 AccentColor 特有字段</summary>
    public class CardLayoutNode : GroupLayoutNode
    {
        public string AccentColor;
    }

    // ---- 3. 自定义 ILayoutHandler<CardLayoutNode> ----
    /// <summary>
    /// 卡片组处理器——负责：
    ///   - 识别 CardGroupAttribute（AttributeType）
    ///   - 构造 CardLayoutNode（CreateNode）
    ///   - 合并 AccentColor / Title（MergeAttribute）
    ///   - 渲染为 Foldout（Build，视觉暂用 Foldout，AccentColor 可通过 StyleClass 表达）
    /// </summary>
    public class CardLayoutHandler : ILayoutHandler<CardLayoutNode>
    {
        public Type LayoutNodeType => typeof(CardLayoutNode);
        public Type AttributeType => typeof(CardGroupAttribute);

        public CardLayoutNode CreateNode(string path) => new CardLayoutNode { DisplayName = path };

        public void MergeAttribute(CardLayoutNode node, LayoutGroupAttribute attr)
        {
            if (attr == null) return;
            if (attr.Title != null) node.DisplayName = attr.Title;
            if (attr is CardGroupAttribute card)
                node.AccentColor ??= card.AccentColor;
        }

        public IElement Build(CardLayoutNode node, BuildContext ctx)
        {
            var foldout = new Foldout(node.DisplayName ?? "Card");
            foldout.WithKey($"group_{node.DisplayName}");
            return foldout;
        }

        Type ILayoutHandler.LayoutNodeType => LayoutNodeType;
        Type ILayoutHandler.AttributeType => AttributeType;
        ILayoutNode ILayoutHandler.CreateNode(string path) => CreateNode(path);
        void ILayoutHandler.MergeAttribute(ILayoutNode node, LayoutGroupAttribute attr)
            => MergeAttribute((CardLayoutNode)node, attr);
        IElement ILayoutHandler.Build(ILayoutNode node, BuildContext ctx)
            => Build((CardLayoutNode)node, ctx);
    }

    // ---- 4. 自定义 Plugin ----
    /// <summary>卡片组插件——由 PluginDiscovery 自动发现</summary>
    [UniPropertyGridPlugin("Card")]
    public class CardGroupPlugin : IUniPropertyGridPlugin
    {
        public string Name => "Card";

        public void OnInit(IPluginRegistry registry)
        {
            registry.RegisterLayoutHandler<CardGroupAttribute, CardLayoutNode>(new CardLayoutHandler());
        }
    }

    // =========================================================================
    // 使用示例数据 + 窗口
    // =========================================================================

    [Serializable]
    public class CardExampleData
    {
        [CardGroup("Profile", Title = "角色档案", AccentColor = "#FF5722")]
        [IAttr.LabelText("角色名")]
        public string characterName = "艾尔莎";

        [CardGroup("Profile")]
        [IAttr.LabelText("等级")]
        [IAttr.Range(1, 99)]
        public int level = 42;

        [CardGroup("Stats", Title = "属性面板", AccentColor = "#2196F3")]
        [IAttr.LabelText("生命值")]
        public int hp = 850;

        [CardGroup("Stats")]
        [IAttr.LabelText("攻击力")]
        public int atk = 120;
    }

    /// <summary>Card 扩展示例窗口——菜单 Window > UniDecl > Card Group Example</summary>
    public class CardGroupExampleWindow : EditorWindow
    {
        [MenuItem("Window/UniDecl/Card Group Example")]
        public static void Open()
        {
            var w = GetWindow<CardGroupExampleWindow>("Card Group Example");
            w.Show();
        }

        private CardExampleData _data = new CardExampleData();
        private UIToolkitRenderManager _manager;

        private void CreateGUI()
        {
            _manager = new UIToolkitRenderManager();
            _manager.RegisterStyleSheet(Resources.Load<UnityEngine.UIElements.StyleSheet>("Themes/DefaultStyle"));
            Rebuild();
        }

        private void Rebuild()
        {
            rootVisualElement.Clear();
            var propGrid = new PropertyGridElement(_data);
            var root = _manager.RenderRoot(propGrid);
            if (root != null)
                rootVisualElement.Add(root);
        }

        private void OnDestroy()
        {
            // UIToolkitRenderManager 无需显式 Dispose
        }
    }
}
