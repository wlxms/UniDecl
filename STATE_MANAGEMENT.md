# UniDecl State Management Pattern

UniDecl 使用 **Element<TState>** 基于 struct 的不可变状态管理模式。

## 概述

`Element<TState>` 是统一的状态化元素基类，使用 struct 作为状态类型，强制值语义和不可变性。

### 特点
- State 是 `struct`，强制值语义和不可变性
- 通过 `SetState()` 更新状态，自动触发 UI 重建
- 类型安全，编译时保证
- 小型状态性能优秀（栈分配）

### 示例

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

### 高级用法

**1. 直接设置新状态**

```csharp
SetState(new CounterState { Count = 100, Title = "Reset" });
```

**2. 读取当前状态**

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

## 性能考虑

### 初始化开销

| 模式 | 开销 | 说明 |
|------|------|------|
| Element&lt;struct&gt; | 极低 | 栈分配 |

### 更新开销

| 模式 | 开销 | 说明 |
|------|------|------|
| Element&lt;struct&gt; | 中等 | 复制 struct（取决于大小） |

### 内存占用

| 模式 | 占用 | 说明 |
|------|------|------|
| Element&lt;struct&gt; | 低 | 栈分配（小型 struct） |

---

## 最佳实践

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

---

## 总结

**默认推荐**：
- **小型状态**：`Element<struct>` - 性能最佳，类型安全
