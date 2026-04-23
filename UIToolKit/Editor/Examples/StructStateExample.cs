using UnityEditor;
using UnityEngine;
using UniDecl.Runtime.Core;
using UniDecl.Runtime.Widgets;
using UniDecl.Editor.UIToolKit;

namespace UniDecl.Editor.UIToolKit.Examples
{
    /// <summary>
    /// Struct-based State 示例
    /// 演示使用 StructStateElement 和 SetState 模式进行状态管理
    /// </summary>
    public class StructStateExample : EditorWindow
    {
        private UIToolkitRenderManager _manager;

        [MenuItem("Window/UniDecl/Struct State Example")]
        public static void ShowWindow()
        {
            GetWindow<StructStateExample>("Struct State Example");
        }

        public void CreateGUI()
        {
            _manager = new UIToolkitRenderManager();

            var root = new Panel
            {
                new VerticalLayout
                {
                    new Label("Struct State + SetState 模式示例")
                        .With(new UIToolKit.Runtime.UITKStyle { FontSize = 16, UnityFontStyleAndWeight = FontStyle.Bold }),
                    new Label(""),
                    new CounterExample(),
                    new Label(""),
                    new TodoListExample(),
                },
            };

            var ve = _manager.RenderRoot(root);
            if (ve != null)
                rootVisualElement.Add(ve);
        }

        /// <summary>
        /// 简单计数器示例
        /// </summary>
        private class CounterExample : StructStateElement<CounterExample.CounterState>
        {
            public struct CounterState
            {
                public int Count;
                public string Title;
            }

            public override CounterState BuildInitialState() => new CounterState
            {
                Count = 0,
                Title = "计数器"
            };

            public override IElement Render(CounterState state)
            {
                return new VerticalLayout
                {
                    new Label($"--- {state.Title} ---"),
                    new Label($"当前计数: {state.Count}"),
                    new HorizontalLayout
                    {
                        new Button("增加", () =>
                        {
                            SetState(s => new CounterState
                            {
                                Count = s.Count + 1,
                                Title = s.Title
                            });
                        }),
                        new Button("减少", () =>
                        {
                            SetState(s => new CounterState
                            {
                                Count = s.Count - 1,
                                Title = s.Title
                            });
                        }),
                        new Button("重置", () =>
                        {
                            SetState(s => new CounterState
                            {
                                Count = 0,
                                Title = s.Title
                            });
                        }),
                    },
                    new Button("更改标题", () =>
                    {
                        SetState(s => new CounterState
                        {
                            Count = s.Count,
                            Title = s.Title == "计数器" ? "Counter" : "计数器"
                        });
                    }),
                };
            }
        }

        /// <summary>
        /// TodoList 示例（使用 struct + array）
        /// </summary>
        private class TodoListExample : StructStateElement<TodoListExample.TodoState>
        {
            public struct TodoState
            {
                public string[] Items;
                public string Input;
            }

            public override TodoState BuildInitialState() => new TodoState
            {
                Items = new[] { "学习 UniDecl", "编写示例代码" },
                Input = ""
            };

            public override IElement Render(TodoState state)
            {
                var layout = new VerticalLayout
                {
                    new Label("--- Todo List (Struct) ---"),
                    new HorizontalLayout
                    {
                        new TextField(state.Input, "新任务")
                        {
                            OnValueChange = (newValue, _) =>
                            {
                                SetState(s => new TodoState
                                {
                                    Items = s.Items,
                                    Input = newValue
                                });
                            }
                        },
                        new Button("添加", () =>
                        {
                            if (!string.IsNullOrWhiteSpace(state.Input))
                            {
                                SetState(s =>
                                {
                                    var newItems = new string[s.Items.Length + 1];
                                    s.Items.CopyTo(newItems, 0);
                                    newItems[s.Items.Length] = s.Input;
                                    return new TodoState
                                    {
                                        Items = newItems,
                                        Input = ""
                                    };
                                });
                            }
                        }),
                    },
                };

                for (int i = 0; i < state.Items.Length; i++)
                {
                    var item = state.Items[i];
                    var index = i;
                    layout.Add(new HorizontalLayout
                    {
                        new Label($"{i + 1}. {item}"),
                        new Button("删除", () =>
                        {
                            SetState(s =>
                            {
                                var newItems = new string[s.Items.Length - 1];
                                int newIndex = 0;
                                for (int j = 0; j < s.Items.Length; j++)
                                {
                                    if (j != index)
                                        newItems[newIndex++] = s.Items[j];
                                }
                                return new TodoState
                                {
                                    Items = newItems,
                                    Input = s.Input
                                };
                            });
                        }),
                    });
                }

                if (state.Items.Length == 0)
                {
                    layout.Add(new Label("暂无任务"));
                }

                return layout;
            }
        }
    }
}
