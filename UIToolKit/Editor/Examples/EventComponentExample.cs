using System;
using UnityEditor;
using UnityEngine;
using UniDecl.Runtime.Components;
using UniDecl.Runtime.Core;
using UniDecl.Runtime.Widgets;
using UniDecl.UIToolKit.Runtime;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;

namespace UniDecl.Editor.UIToolKit.Examples
{
    /// <summary>
    /// 事件组件系统测试用例 — 覆盖 OnClick、OnPointerEnter/Leave、OnEvent&lt;T&gt;、
    /// 构造函数注入与 .With() 附加两种方式，以及 Get&lt;T&gt; 读取。
    /// 通过 Window → UniDecl Event Test 打开。
    /// </summary>
    public class EventComponentExample : EditorWindow
    {
        private UIToolkitRenderManager _manager;

        [MenuItem("Window/UniDecl Event Test")]
        public static void ShowWindow() => GetWindow<EventComponentExample>("UniDecl Event Test");

        public void CreateGUI()
        {
            rootVisualElement.Clear();

            _manager = new UIToolkitRenderManager();

            var root = new Panel
            {
                new VerticalLayout
                {
                    new Label("Event Component 测试").With(new UITKStyle { FontSize = 20, UnityFontStyleAndWeight = FontStyle.Bold }),

                    // 1. 构造函数注入 OnClick
                    new SectionLabel("1. 构造函数注入 — OnClick"),
                    new Button("Click Me (ctor)", new OnClick(() => Debug.Log("[ctor] Button clicked!")))
                        .With(new UITKStyle { Width = 200 }),

                    new Label(""),

                    // 2. .With() 附加 OnClick
                    new SectionLabel("2. .With() 附加 — OnClick"),
                    new Button("Click Me (.With)")
                        .With(new OnClick(() => Debug.Log("[with] Button clicked!")))
                        .With(new UITKStyle { Width = 200 }),

                    new Label(""),

                    // 3. OnPointerEnter / OnPointerLeave
                    new SectionLabel("3. 指针事件 — OnPointerEnter / OnPointerLeave"),
                    new Panel
                    {
                        new Label("Hover over this panel")
                            .With(new OnPointerEnter(() => Debug.Log("[enter] Mouse entered")))
                            .With(new OnPointerLeave(() => Debug.Log("[leave] Mouse left"))),
                    }
                    .With(new UITKStyle { Width = 200, Height = 40, BackgroundColor = new UnityEngine.Color(0.2f, 0.2f, 0.2f, 1f) }),

                    new Label(""),

                    // 4. OnEvent<T> — MouseDownEvent 右键检测
                    new SectionLabel("4. OnEvent<T> — MouseDownEvent 右键检测"),
                    new Label("Right-click this label (logs to Console)")
                        .With(new OnEvent<UnityEngine.UIElements.MouseDownEvent>(e =>
                        {
                            if (e.button == 1) // 1 = right button
                                Debug.Log($"[OnEvent<MouseDownEvent>] Right-clicked at ({e.localMousePosition.x}, {e.localMousePosition.y})");
                        }))
                        .With(new UITKStyle { Width = 300 }),

                    new Label(""),

                    // 5. OnEvent<T> — MouseDownEvent
                    new SectionLabel("5. OnEvent<T> — MouseDownEvent 计数"),
                    BuildMouseDownCounter(),

                    new Label(""),

                    // 6. 多事件组合
                    new SectionLabel("6. 多事件组合 — 同一元素 OnClick + OnPointerEnter"),
                    new Panel
                    {
                        new Label("Hover & Click me"),
                    }
                    .With(new OnClick(() => Debug.Log("[combo] Click")))
                    .With(new OnPointerEnter(() => Debug.Log("[combo] Enter")))
                    .With(new OnPointerLeave(() => Debug.Log("[combo] Leave")))
                    .With(new UITKStyle { Width = 200, Height = 40, BackgroundColor = new UnityEngine.Color(0.15f, 0.25f, 0.4f, 1f) }),

                    new Label(""),

                    // 7. Get<T> 读取组件
                    new SectionLabel("7. Get<T> 读取 — 反序列化事件组件"),
                    new Button("Read My Components", new OnClick(() => Debug.Log("[get] Component read click")))
                        .With(new OnPointerEnter(() => { }))
                        .With(new UITKStyle { Width = 250 }),
                    new Button("Inspect Above Button", new OnClick(InspectAboveButton))
                        .With(new UITKStyle { Width = 250, MarginTop = 4 }),
                },
            };

            var ve = _manager.RenderRoot(root);
            if (ve != null)
                rootVisualElement.Add(ve);
        }

        private static IElement BuildMouseDownCounter()
        {
            int count = 0;
            return new Label("Click count: 0")
                .With(new OnEvent<UnityEngine.UIElements.MouseDownEvent>(_ =>
                {
                    count++;
                    Debug.Log($"[mousedown] Count = {count}");
                }))
                .With(new UITKStyle { Width = 200 });
        }

        private static void InspectAboveButton()
        {
            // 演示 Get<T> API：实际使用时需持有 Element 引用
            // 这里仅验证 OnClick 组件可以正确构造和读取
            var click = new OnClick(() => Debug.Log("hello"));
            var enter = new OnPointerEnter(() => { });

            Debug.Log($"[get] OnClick type: {click.GetType().Name}, HasHandler: {click.Handler != null}");
            Debug.Log($"[get] OnPointerEnter type: {enter.GetType().Name}, HasHandler: {enter.Handler != null}");

            // 验证 With 注册 / Get 读取
            var panel = new Panel { new Label("test") };
            panel.With(click);
            panel.With(enter);

            var retrieved = panel.Get<OnClick>();
            Debug.Log($"[get] panel.Get<OnClick> == click: {ReferenceEquals(retrieved, click)}");
            Debug.Log($"[get] panel.Get<OnPointerEnter> == enter: {ReferenceEquals(panel.Get<OnPointerEnter>(), enter)}");
        }

        /// <summary>
        /// 章节标题，带样式区分。
        /// </summary>
        private class SectionLabel : Element
        {
            private readonly string _text;

            public SectionLabel(string text)
            {
                _text = text;
            }

            public override IElement Render()
            {
                return new Label(_text)
                    .With(new UITKStyle
                    {
                        FontSize = 14,
                        UnityFontStyleAndWeight = FontStyle.Bold,
                        MarginTop = 8,
                        MarginBottom = 2,
                    });
            }
        }
    }
}
