# UniDecl State Management Patterns

UniDecl 提供两种状态管理模式，适用于不同的场景和编程风格。

## 两种状态管理模式对比

| 特性 | Element&lt;TState&gt;<br/>(Struct) | Element&lt;TState&gt;<br/>(Class) | ReactiveStateElement&lt;TState&gt;<br/>(ReactiveValue) |
|------|------------------------|---------------------------|---------------------------|
| **状态类型** | `struct` | `class` | `class` |
| **更新方式** | `SetState(updater)` | 手动 `NotifyChanged()` | 自动（ReactiveValue） |
| **不可变性** | ✅ 强制不可变 | ❌ 可变 | ❌ 可变 |
| **代码简洁度** | 中等 | 简单 | 简洁 |
| **性能** | 优秀（栈分配） | 一般（GC压力） | 一般（反射初始化） |
| **适用场景** | 小型不可变状态 | 简单状态管理 | 复杂响应式状态 |

---

## 模式一：Element&lt;TState&gt; - 统一状态管理

### 概述
`Element<TState>` 是统一的状态化元素基类，根据 `TState` 是 `struct` 还是 `class` 自动采用不同的行为。

### 使用 Struct（推荐）

#### 特点
- State 是 `struct`，强制值语义和不可变性
- 通过 `SetState()` 更新状态，自动触发 UI 重建
- 类型安全，编译时保证
- 小型状态性能优秀（栈分配）

#### 示例

```csharp
public class Counter : Element<Counter.CounterState>
{
    public struct CounterState
    {
        public int Count;
        public string Title;
    }

    public override CounterState BuildState() => new CounterState
    {
        Count = 0,
        Title = "计数器"
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

#### 高级用法

**1. 直接设置新状态**

```csharp
SetState(new CounterState { Count = 100, Title = "Reset" });
```

**2. 读取当前状态**

```csharp
var currentCount = State.Count;
```

#### 优缺点

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

### 使用 Class

#### 特点
- State 是 `class`，引用语义
- 直接修改状态，需要手动调用 `NotifyChanged()` 触发 UI 更新
- 灵活但容易忘记调用通知
- 不能使用 `SetState()` 方法（会抛出异常）

#### 示例

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

#### 优缺点

**优点**：
- ✅ 简单直观
- ✅ 无额外学习成本
- ✅ 适合复杂对象

**缺点**：
- ❌ 容易忘记调用 `NotifyChanged()`
- ❌ State 可变，可能导致意外副作用
- ❌ 无法强制不可变性

**适用场景**：
- 快速原型开发
- 简单的状态管理
- 不需要严格不可变性的场景

---

## 模式二：ReactiveStateElement&lt;TState&gt; (ReactiveValue)

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
│   状态是否小型且简单？      │
│   (< 5 个字段，< 100 bytes) │
└──────────┬──────────────────┘
           │
    Yes ───┴─→ Element<TState> (struct)
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
┌─────────────────────────────┐
│     是否需要严格不可变？    │
└──────────┬──────────────────┘
           │
    Yes ───┴─→ Element<TState> (struct)
           │
    No     │
           ▼
    Element<TState> (class)
```

### 具体场景推荐

| 场景 | 推荐模式 | 原因 |
|------|---------|------|
| 计数器 | Element&lt;struct&gt; | 状态简单，struct 性能好 |
| TodoList | ReactiveStateElement | 需要动态列表 (ReactiveList) |
| 表单（< 5 字段） | Element&lt;struct&gt; | 不可变性保证数据一致性 |
| 表单（> 5 字段） | ReactiveStateElement | 更新代码更简洁 |
| 配置面板 | ReactiveStateElement | 字段多，频繁交互 |
| 快速原型 | Element&lt;class&gt; | 最简单快速 |

---

## 性能考虑

### 初始化开销

| 模式 | 开销 | 说明 |
|------|------|------|
| Element&lt;struct&gt; | 极低 | 栈分配 |
| Element&lt;class&gt; | 低 | 直接实例化 class |
| ReactiveStateElement | 中等 | 反射扫描绑定 ReactiveValue |

### 更新开销

| 模式 | 开销 | 说明 |
|------|------|------|
| Element&lt;struct&gt; | 中等 | 复制 struct（取决于大小） |
| Element&lt;class&gt; | 低 | 直接修改 + 手动通知 |
| ReactiveStateElement | 低 | 直接修改 + 自动通知 |

### 内存占用

| 模式 | 占用 | 说明 |
|------|------|------|
| Element&lt;struct&gt; | 低 | 栈分配（小型 struct） |
| Element&lt;class&gt; | 中等 | 堆分配 |
| ReactiveStateElement | 较高 | ReactiveValue 包装器开销 |

---

## 最佳实践

### Element&lt;struct&gt;

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

### 从旧的 Element&lt;TState&gt; (class only) 迁移到 struct

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
public class Counter : Element<Counter.CounterState>
{
    public struct CounterState // class → struct
    {
        public int Count;
    }

    public override CounterState BuildState() => new CounterState();

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

---

## 总结

| 需求 | 推荐模式 |
|------|---------|
| 简单状态 | `Element<struct>` |
| 快速原型 | `Element<class>` |
| 强制不可变 | `Element<struct>` |
| 自动更新 | `ReactiveStateElement<TState>` |
| 复杂状态 | `ReactiveStateElement<TState>` |
| 动态列表 | `ReactiveStateElement<TState>` + `ReactiveList<T>` |

**默认推荐**：
- **小型状态**：`Element<struct>` - 性能最佳，类型安全
- **复杂状态**：`ReactiveStateElement<TState>` - 开发体验最佳
