# UniDecl State Management Patterns

UniDecl 提供三种状态管理模式，适用于不同的场景和编程风格。

## 三种状态管理模式对比

| 特性 | Element&lt;TState&gt;<br/>(Class-based) | StructStateElement&lt;TState&gt;<br/>(Struct + SetState) | ReactiveStateElement&lt;TState&gt;<br/>(ReactiveValue) |
|------|------------------------|---------------------------|---------------------------|
| **状态类型** | `class` | `struct` | `class` |
| **更新方式** | 手动 `NotifyChanged()` | `SetState(updater)` | 自动（ReactiveValue） |
| **不可变性** | ❌ 可变 | ✅ 强制不可变 | ❌ 可变 |
| **代码简洁度** | 中等 | 较繁琐 | 简洁 |
| **性能** | 一般（GC压力） | 优秀（栈分配） | 一般（反射初始化） |
| **适用场景** | 向后兼容 | 小型不可变状态 | 复杂响应式状态 |

---

## 模式一：Element&lt;TState&gt; (Class-based)

### 概述
传统的 class-based 状态模式，适用于需要向后兼容的场景。

### 特点
- State 必须是 `class`
- 状态变更后需要手动调用 `NotifyChanged()` 触发 UI 更新
- 灵活但容易忘记调用通知

### 示例

```csharp
public class Counter : Element<Counter.CounterState>
{
    public class CounterState
    {
        public int Count;
    }

    public override CounterState BuildState() => new CounterState();

    public override IElement Render(CounterState state)
    {
        return new VerticalLayout
        {
            new Label($"Count: {state.Count}"),
            new Button("Increment", () =>
            {
                state.Count++;
                NotifyChanged(); // 必须手动调用
            })
        };
    }
}
```

### 优缺点

**优点**：
- 简单直观
- 向后兼容现有代码
- 无额外学习成本

**缺点**：
- 容易忘记调用 `NotifyChanged()`
- State 可变，可能导致意外副作用
- 无法强制不可变性

---

## 模式二：StructStateElement&lt;TState&gt; (Struct + SetState)

### 概述
基于 `struct` 的不可变状态模式，通过 `SetState` 方法强制状态更新。

### 特点
- State 必须是 `struct`
- 通过 `SetState(updater)` 方法更新状态
- **强制不可变性** - 每次更新都创建新的 state 实例
- 自动触发 UI 更新

### 示例

```csharp
public class Counter : StructStateElement<Counter.CounterState>
{
    public struct CounterState
    {
        public int Count;
        public string Title;
    }

    public override CounterState BuildInitialState() => new CounterState
    {
        Count = 0,
        Title = "Counter"
    };

    public override IElement Render(CounterState state)
    {
        return new VerticalLayout
        {
            new Label(state.Title),
            new Label($"Count: {state.Count}"),
            new Button("Increment", () =>
            {
                SetState(s => new CounterState
                {
                    Count = s.Count + 1,
                    Title = s.Title
                });
                // 自动触发 NotifyChanged()
            })
        };
    }
}
```

### 高级用法

#### 1. 直接设置新状态

```csharp
SetState(new CounterState { Count = 100, Title = "Reset" });
```

#### 2. 读取当前状态

```csharp
var currentCount = State.Count;
```

### 优缺点

**优点**：
- ✅ 强制不可变性，避免意外修改
- ✅ 类型安全，编译时保证
- ✅ 小型状态性能优秀（栈分配）
- ✅ 明确的更新路径
- ✅ 自动触发更新

**缺点**：
- ❌ 更新代码较繁琐（需要重建整个 struct）
- ❌ 嵌套结构更新复杂
- ❌ 大型状态结构复制开销大

**适用场景**：
- 小型状态对象（< 100 bytes）
- 需要强制不可变性的场景
- 简单的表单、配置等

---

## 模式三：ReactiveStateElement&lt;TState&gt; (ReactiveValue)

### 概述
响应式状态模式，使用 `ReactiveValue<T>` 和 `ReactiveList<T>` 包装器，自动检测变更并触发 UI 更新。

### 特点
- State 是 `class`，包含 `ReactiveValue<T>` 字段
- **自动变更检测** - 任何 ReactiveValue 修改自动触发更新
- 支持批量更新（`BatchUpdate`）
- 支持响应式集合（`ReactiveList<T>`）

### 基础示例

```csharp
public class Counter : ReactiveStateElement<Counter.CounterState>
{
    public class CounterState
    {
        public ReactiveValue<int> Count = new(0);
        public ReactiveValue<string> Title = new("Counter");
    }

    public override CounterState BuildState() => new CounterState();

    public override IElement Render(CounterState state)
    {
        return new VerticalLayout
        {
            new Label(state.Title), // 隐式转换为 string
            new Label($"Count: {state.Count.Value}"),
            new Button("Increment", () =>
            {
                state.Count.Value++; // 自动触发 NotifyChanged()
            }),
            new Button("Change Title", () =>
            {
                state.Title.Value = "New Title"; // 自动触发
            })
        };
    }
}
```

### 高级用法

#### 1. ReactiveList - 响应式列表

```csharp
public class TodoList : ReactiveStateElement<TodoList.TodoState>
{
    public class TodoState
    {
        public ReactiveList<string> Items = new(new[] { "Task 1", "Task 2" });
        public ReactiveValue<string> Input = new("");
    }

    public override TodoState BuildState() => new TodoState();

    public override IElement Render(TodoState state)
    {
        var layout = new VerticalLayout
        {
            new TextField(state.Input.Value, "New Task")
            {
                OnValueChange = (value, _) => state.Input.Value = value
            },
            new Button("Add", () =>
            {
                state.Items.Add(state.Input.Value); // 自动触发更新
                state.Input.Value = "";
            })
        };

        foreach (var item in state.Items)
        {
            layout.Add(new Label(item));
        }

        return layout;
    }
}
```

#### 2. BatchUpdate - 批量更新

多个属性修改只触发一次 UI 更新：

```csharp
public class Form : ReactiveStateElement<Form.FormState>
{
    public class FormState
    {
        public ReactiveValue<string> Name = new("");
        public ReactiveValue<int> Age = new(0);
        public ReactiveValue<string> Email = new("");
    }

    public override FormState BuildState() => new FormState();

    public override IElement Render(FormState state)
    {
        return new VerticalLayout
        {
            new Button("Reset All", () =>
            {
                // 三次修改只触发一次重建
                BatchUpdate(s =>
                {
                    s.Name.Value = "";
                    s.Age.Value = 0;
                    s.Email.Value = "";
                });
            })
        };
    }
}
```

#### 3. 隐式转换

`ReactiveValue<T>` 支持隐式转换为 `T`，便于读取：

```csharp
var state = new { Count = new ReactiveValue<int>(10) };

// 读取时可以省略 .Value
int value = state.Count; // 隐式转换

// 写入必须使用 .Value
state.Count.Value = 20;
```

### ReactiveList 常用方法

```csharp
var list = new ReactiveList<string>();

// 基础操作（自动触发更新）
list.Add("item");
list.Remove("item");
list.RemoveAt(0);
list.Clear();
list[0] = "new value";

// 批量操作（只触发一次更新）
list.AddRange(new[] { "a", "b", "c" });
list.RemoveAll(x => x.StartsWith("a"));
list.Sort();
```

### 优缺点

**优点**：
- ✅ 自动变更检测，无需手动通知
- ✅ 代码简洁直观
- ✅ 支持复杂嵌套状态
- ✅ 细粒度更新
- ✅ 支持批量更新优化
- ✅ 适合大型状态对象

**缺点**：
- ❌ 初始化时反射扫描有开销
- ❌ 需要 `.Value` 访问（写入）
- ❌ 调试时隐式行为不直观

**适用场景**：
- 复杂的表单、配置
- 动态列表、集合
- 大型状态对象
- 需要频繁更新的场景

---

## 如何选择？

### 推荐流程

```
┌─────────────────────────────┐
│  是否需要向后兼容现有代码？  │
└──────────┬──────────────────┘
           │
    Yes ───┴─→ Element<TState> (Class-based)
           │
    No     │
           ▼
┌─────────────────────────────┐
│   状态是否小型且简单？      │
│   (< 5 个字段，< 100 bytes) │
└──────────┬──────────────────┘
           │
    Yes ───┴─→ StructStateElement<TState>
           │
    No     │
           ▼
┌─────────────────────────────┐
│ 是否需要频繁更新或动态集合？│
└──────────┬──────────────────┘
           │
    Yes ───┴─→ ReactiveStateElement<TState>
           │
    No     │
           ▼
    StructStateElement 或 ReactiveStateElement
    （根据个人偏好）
```

### 具体场景推荐

| 场景 | 推荐模式 | 原因 |
|------|---------|------|
| 计数器 | StructStateElement | 状态简单，struct 性能好 |
| TodoList | ReactiveStateElement | 需要动态列表 (ReactiveList) |
| 表单（< 5 字段） | StructStateElement | 不可变性保证数据一致性 |
| 表单（> 5 字段） | ReactiveStateElement | 更新代码更简洁 |
| 配置面板 | ReactiveStateElement | 字段多，频繁交互 |
| 向后兼容 | Element&lt;TState&gt; | 保持现有代码不变 |

---

## 性能考虑

### 初始化开销

| 模式 | 开销 | 说明 |
|------|------|------|
| Element&lt;TState&gt; | 低 | 直接实例化 class |
| StructStateElement | 极低 | 栈分配 |
| ReactiveStateElement | 中等 | 反射扫描绑定 ReactiveValue |

### 更新开销

| 模式 | 开销 | 说明 |
|------|------|------|
| Element&lt;TState&gt; | 低 | 直接修改 + 手动通知 |
| StructStateElement | 中等 | 复制 struct（取决于大小） |
| ReactiveStateElement | 低 | 直接修改 + 自动通知 |

### 内存占用

| 模式 | 占用 | 说明 |
|------|------|------|
| Element&lt;TState&gt; | 中等 | 堆分配 |
| StructStateElement | 低 | 栈分配（小型 struct） |
| ReactiveStateElement | 较高 | ReactiveValue 包装器开销 |

---

## 最佳实践

### StructStateElement

✅ **推荐**

```csharp
// 状态精简，字段少
public struct AppState
{
    public int Counter;
    public bool IsEnabled;
}

// 使用 C# 10+ 的 with 表达式（如果可用）
SetState(s => s with { Counter = s.Counter + 1 });
```

❌ **不推荐**

```csharp
// 状态过大，复制开销高
public struct LargeState
{
    public string[] Items; // 引用类型在 struct 中
    public Dictionary<string, int> Map;
    // ... 10+ 个字段
}
```

### ReactiveStateElement

✅ **推荐**

```csharp
// 使用批量更新
BatchUpdate(state =>
{
    state.Name.Value = "New Name";
    state.Age.Value = 30;
    state.Email.Value = "new@email.com";
});

// ReactiveList 用于动态集合
public ReactiveList<TodoItem> Items = new();
```

❌ **不推荐**

```csharp
// 频繁单独更新（触发多次重建）
state.Name.Value = "A";
state.Name.Value = "B";
state.Name.Value = "C";

// 应该用 BatchUpdate 或一次性赋值
BatchUpdate(s => s.Name.Value = "C");
```

---

## 迁移指南

### 从 Element&lt;TState&gt; 迁移到 StructStateElement

**之前：**

```csharp
public class Counter : Element<Counter.CounterState>
{
    public class CounterState
    {
        public int Count;
    }

    public override CounterState BuildState() => new CounterState();

    public override IElement Render(CounterState state)
    {
        return new Button("Increment", () =>
        {
            state.Count++;
            NotifyChanged(); // 手动
        });
    }
}
```

**之后：**

```csharp
public class Counter : StructStateElement<Counter.CounterState>
{
    public struct CounterState // class → struct
    {
        public int Count;
    }

    public override CounterState BuildInitialState() => new CounterState();

    public override IElement Render(CounterState state)
    {
        return new Button("Increment", () =>
        {
            SetState(s => new CounterState { Count = s.Count + 1 });
            // 自动触发
        });
    }
}
```

### 从 Element&lt;TState&gt; 迁移到 ReactiveStateElement

**之前：**

```csharp
public class Counter : Element<Counter.CounterState>
{
    public class CounterState
    {
        public int Count;
        public List<string> Items = new();
    }

    public override CounterState BuildState() => new CounterState();

    public override IElement Render(CounterState state)
    {
        return new Button("Add Item", () =>
        {
            state.Items.Add("Item");
            NotifyChanged(); // 手动
        });
    }
}
```

**之后：**

```csharp
public class Counter : ReactiveStateElement<Counter.CounterState>
{
    public class CounterState
    {
        public ReactiveValue<int> Count = new(0); // 包装
        public ReactiveList<string> Items = new(); // ReactiveList
    }

    public override CounterState BuildState() => new CounterState();

    public override IElement Render(CounterState state)
    {
        return new Button("Add Item", () =>
        {
            state.Items.Add("Item"); // 自动触发
        });
    }
}
```

---

## 总结

| 需求 | 推荐模式 |
|------|---------|
| 向后兼容 | `Element<TState>` |
| 强制不可变 | `StructStateElement<TState>` |
| 自动更新 | `ReactiveStateElement<TState>` |
| 简单状态 | `StructStateElement<TState>` |
| 复杂状态 | `ReactiveStateElement<TState>` |
| 动态列表 | `ReactiveStateElement<TState>` + `ReactiveList<T>` |

**默认推荐**：对于新项目，优先考虑 `ReactiveStateElement<TState>`，它提供了最佳的开发体验和可维护性。
