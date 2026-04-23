using UnityEditor;
using UnityEngine;
using UniDecl.Runtime.Core;
using UniDecl.Runtime.Widgets;
using UniDecl.Editor.UIToolKit;

namespace UniDecl.Editor.UIToolKit.Examples
{
    /// <summary>
    /// ReactiveValue 示例
    /// 演示使用 ReactiveStateElement 和 ReactiveValue/ReactiveList 进行状态管理
    /// </summary>
    public class ReactiveStateExample : EditorWindow
    {
        private UIToolkitRenderManager _manager;

        [MenuItem("Window/UniDecl/Reactive State Example")]
        public static void ShowWindow()
        {
            GetWindow<ReactiveStateExample>("Reactive State Example");
        }

        public void CreateGUI()
        {
            _manager = new UIToolkitRenderManager();

            var root = new Panel
            {
                new VerticalLayout
                {
                    new Label("ReactiveValue 模式示例")
                        .With(new UIToolKit.Runtime.UITKStyle { FontSize = 16, UnityFontStyleAndWeight = FontStyle.Bold }),
                    new Label(""),
                    new CounterExample(),
                    new Label(""),
                    new TodoListExample(),
                    new Label(""),
                    new BatchUpdateExample(),
                },
            };

            var ve = _manager.RenderRoot(root);
            if (ve != null)
                rootVisualElement.Add(ve);
        }

        /// <summary>
        /// 简单计数器示例
        /// </summary>
        private class CounterExample : ReactiveStateElement<CounterExample.CounterState>
        {
            public class CounterState
            {
                public ReactiveValue<int> Count = new(0);
                public ReactiveValue<string> Title = new("计数器");
            }

            public override CounterState BuildState() => new CounterState();

            public override IElement Render(CounterState state)
            {
                return new VerticalLayout
                {
                    new Label($"--- {state.Title} ---"), // 隐式转换
                    new Label($"当前计数: {state.Count.Value}"),
                    new HorizontalLayout
                    {
                        new Button("增加", () =>
                        {
                            state.Count.Value++; // 自动触发重建
                        }),
                        new Button("减少", () =>
                        {
                            state.Count.Value--;
                        }),
                        new Button("重置", () =>
                        {
                            state.Count.Value = 0;
                        }),
                    },
                    new Button("更改标题", () =>
                    {
                        state.Title.Value = state.Title.Value == "计数器" ? "Counter" : "计数器";
                    }),
                };
            }
        }

        /// <summary>
        /// TodoList 示例（使用 ReactiveList）
        /// </summary>
        private class TodoListExample : ReactiveStateElement<TodoListExample.TodoState>
        {
            public class TodoState
            {
                public ReactiveList<string> Items = new(new[] { "学习 UniDecl", "编写示例代码" });
                public ReactiveValue<string> Input = new("");
            }

            public override TodoState BuildState() => new TodoState();

            public override IElement Render(TodoState state)
            {
                var layout = new VerticalLayout
                {
                    new Label("--- Todo List (ReactiveList) ---"),
                    new HorizontalLayout
                    {
                        new TextField(state.Input.Value, "新任务")
                        {
                            OnValueChange = (newValue, _) =>
                            {
                                state.Input.Value = newValue; // 自动触发重建
                            }
                        },
                        new Button("添加", () =>
                        {
                            if (!string.IsNullOrWhiteSpace(state.Input.Value))
                            {
                                state.Items.Add(state.Input.Value); // 自动触发重建
                                state.Input.Value = "";
                            }
                        }),
                    },
                };

                for (int i = 0; i < state.Items.Count; i++)
                {
                    var item = state.Items[i];
                    var index = i;
                    layout.Add(new HorizontalLayout
                    {
                        new Label($"{i + 1}. {item}"),
                        new Button("删除", () =>
                        {
                            state.Items.RemoveAt(index); // 自动触发重建
                        }),
                    });
                }

                if (state.Items.Count == 0)
                {
                    layout.Add(new Label("暂无任务"));
                }

                layout.Add(new Label(""));
                layout.Add(new Button("清空全部", () =>
                {
                    state.Items.Clear(); // 自动触发重建
                }));

                return layout;
            }
        }

        /// <summary>
        /// 批量更新示例
        /// </summary>
        private class BatchUpdateExample : ReactiveStateElement<BatchUpdateExample.FormState>
        {
            public class FormState
            {
                public ReactiveValue<string> Name = new("张三");
                public ReactiveValue<int> Age = new(25);
                public ReactiveValue<string> Email = new("example@email.com");
            }

            public override FormState BuildState() => new FormState();

            public override IElement Render(FormState state)
            {
                return new VerticalLayout
                {
                    new Label("--- 批量更新示例 ---"),
                    new Label($"姓名: {state.Name.Value}"),
                    new Label($"年龄: {state.Age.Value}"),
                    new Label($"邮箱: {state.Email.Value}"),
                    new Label(""),
                    new HorizontalLayout
                    {
                        new Button("单独更新姓名", () =>
                        {
                            state.Name.Value = "李四"; // 触发一次重建
                        }),
                        new Button("批量更新全部", () =>
                        {
                            // 使用 BatchUpdate，三次修改只触发一次重建
                            BatchUpdate(s =>
                            {
                                s.Name.Value = "王五";
                                s.Age.Value = 30;
                                s.Email.Value = "wangwu@email.com";
                            });
                        }),
                    },
                    new Button("重置", () =>
                    {
                        BatchUpdate(s =>
                        {
                            s.Name.Value = "张三";
                            s.Age.Value = 25;
                            s.Email.Value = "example@email.com";
                        });
                    }),
                };
            }
        }
    }
}
