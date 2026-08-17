# 单轨快照系统重构设计（Unified Snapshot）

> 状态：**构建完成（代码层）**，待 Unity 导入验证（2026-08-17）
> 目标：双轨（ValueStep + ObjectDiffStep）→ 单轨（统一值快照 + SnapshotBinding 树）

## 一、核心思想

**一切快照都是"值快照"**：`Commit()` 时对值做快照，值类型装箱即快照；object 的快照是递归展开的嵌套值快照（按字段/索引/key 展开为绑定树）。Undo/Redo 统一通过 `setter(restore, current, changes)` 写回并通知业务层。

**Object = 多个 Value 的集合 + Object 的嵌套**。绑定树由框架递归构建，叶子（值类型/string/Unity 对象/截断节点）持有用户 getter/setter；容器（Object/List/Dict）持有用户 getter + 通知型 setter，子字段由框架自动生成反射读写。

## 二、核心类型

### 2.1 ChangeSet（变更清单）

```csharp
struct FieldChange
{
    string Path;        // "config.Child.X"
    object OldValue;    // undo：恢复前值；redo：恢复后值（方向自动对调）
    object NewValue;    // undo：恢复后值
}

class ChangeSet
{
    List<FieldChange> Changes;   // 本次 undo/redo 实际变更的字段
}
```

来源：undo/redo 时由执行过的 step 聚合，非 diff。渲染层按 `Path` 局部刷新。

### 2.2 ISnapshotBinding（接口）

```csharp
public interface ISnapshotBinding : IDisposable
{
    Guid Id { get; }         // 自动生成，用户不传 key
    string Path { get; }     // 展示/changeSet 用
    void Commit();           // 叶子：基线对比；容器：递归子节点 + 自动打包组
}
```

### 2.3 SnapshotSetter（统一写回入口）

```csharp
delegate void SnapshotSetter(object restore, object current, ChangeSet changes);
```

| 节点 | restore | current | changes | 职责 |
|---|---|---|---|---|
| 叶子 | 还原的值 | 当前值 | 单条（自身） | 写回字段 + 业务层刷新 |
| 容器（Object/List/Dict） | 整体替换值（仅截断/引用替换场景，否则 null） | 当前对象引用 | 子树变更聚合清单 | 业务层刷新（子树字段已由框架写回） |

### 2.4 SnapshotBinding（抽象基类，叶子 + 对象展开）

```csharp
public abstract class SnapshotBinding : ISnapshotBinding
{
    protected ISnapshotManager Manager;   // 强引用 manager（manager 弱引用 binding，单向不循环）
    protected int ScopeId;
    protected object Baseline;            // 上次已提交基线（叶子用）

    // 自定义拓展点：如何把当前值展开为子绑定
    protected abstract IEnumerable<ISnapshotBinding> BuildChildren(object value, string path);

    // 叶子判定：当前值是否需要展开（默认：值类型/string/Unity 对象 → 叶子）
    protected virtual bool IsLeaf(object value);
    protected virtual bool IsCollection(object value);   // 默认 false；List/Dict 子类覆写

    public virtual void Commit();
    // 防重入：Manager 恢复期间 Commit → 抛 InvalidOperationException
}
```

### 2.5 类层次

| 类 | 展开方式（BuildChildren） | 构造 |
|---|---|---|
| `SnapshotBinding`（用户直接实例化） | 叶子：getter/setter 委托；对象：字段反射 `path.field` | 叶子/对象两种形态 |
| `ListSnapshotBinding` | 按索引 `path[i]`；数量变化 → 容器整体一个 step（旧值深拷贝） | 传 List getter |
| `DictSnapshotBinding` | 按 key `path[key]`；key 天然稳定 | 传 Dict getter |
| 用户自定义 | 继承基类覆写 `BuildChildren` / `IsLeaf` | 自定义 |

## 三、生命周期与注册

- 构造时自动向 Manager 注册（`Dictionary<Guid, WeakReference<ISnapshotBinding>>`）
- **弱引用**：binding 被 GC 后，Manager 下次操作时惰性清理（移除注册 + 过滤其历史 steps）——binding 生命周期 = 视图生命周期
- `Dispose()`：显式反注册（可选，不调则等 GC）
- Step 只存 `(Guid, Value, Path, ScopeId)`，不强引用 binding

## 四、Key 链路（Guid 化）

| 概念 | 用途 | 生成方式 |
|---|---|---|
| `Guid Id` | 注册表索引、step 定位、合并判断 | binding 构造时 `Guid.NewGuid()` |
| `string Path` | changeSet 展示、UI 局部刷新 | 构建时自动拼接（`Child.X`、`Items[0]`、`Map[k]`） |

- Scope 反向索引：`Dictionary<int, HashSet<Guid>>`，Dispose 时 O(1) 取 guid 集清理
- 合并：同 `Id` + 同 `ScopeId` + 时间窗（同 binding 连续输入合并，保留最早旧值）
- Group：自动组（一次根 Commit）嵌套手动组（BeginGroup/EndGroup，保留），组栈任意深度；组不合并

## 五、提交 / 撤销流程

```
Commit()（叶子）:
    if Manager.IsRestoring → throw
    current = Getter()
    if !Equals(current, Baseline):   // 只有变更才生效
        Manager.Record(Baseline, Id, Path, ScopeId)
        Baseline = current

Commit()（容器）:
    BeginGroup(自动)                 // 与手动组嵌套共存
    foreach child: child.Commit()
    EndGroup()                       // 空组自动丢弃

Manager.Undo():
    _restoring = true
    foreach step in group(逆序):
        binding = 注册表[step.Id]    // 弱引用取；已死则跳过
        current = binding.Getter()
        binding.Setter(step.Value, current, changeSet单条)   // 叶子写回
        binding.Baseline = step.Value                        // 基线同步，防误判
        聚合 changeSet
    容器级 setter(null, 对象引用, 子树聚合清单)               // 自底向上通知
    _restoring = false
    广播 OnUndoRedoPerformed(ChangeSet)
```

## 六、边界规则

1. **循环引用**：`visited` 截断，该字段作为"整体引用叶子"（setter 整体替换引用，可 undo 粒度粗）
2. **Unity 对象**：叶子白名单，保持引用不展开（`GetUninitializedObject` 不可用于 Unity 对象）
3. **集合数量变化**：List/Dict 容器整体一个 step，旧值深拷贝（map 模式保持元素共享）
4. **防重入**：`_restoring` 期间任何 `Commit()` 抛 `InvalidOperationException`；VE 写回用 `SetValueWithoutNotify` 双保险
5. **readonly/init-only 字段**：反射写回时跳过（避免 FieldAccessException）

## 七、Manager 公开面（收敛后）

```csharp
public interface ISnapshotManager
{
    int CreateScope(int parent = 0);
    void DisposeScope(int scopeId);
    void BeginGroup(string name); void EndGroup();
    bool Undo(); bool Redo(); void Clear();
    event Action<ChangeSet> OnUndoRedoPerformed;
    bool IsRestoring { get; }
    // 内部（binding 专用，不暴露给用户）：
    void RegisterBinding(ISnapshotBinding b);      // internal
    void UnregisterBinding(Guid id);               // internal
    void RecordValue(object oldValue, Guid id, string path, int scopeId);  // internal
}
```

## 八、迁移映射

| 现有 | 新 |
|---|---|
| `SnapshotBinding<T>`（UIToolKit/Editor） | 叶子形态 `SnapshotBinding`（基类） |
| `_mgr.Register<T>(key, setter)` + `Record(old, key)` | binding 构造自动注册，`Commit()` |
| `_mgr.RecordObject(target, key)` | `new SnapshotBinding(mgr, scopeId, path, () => target)` |
| `UndoScope.Register/Record` | 移除；binding 直接持 ScopeId |
| `ValueStep` / `ObjectDiffStep` | 统一 `SnapshotStep(Guid, Value, Path, ScopeId)` |
| 用户 key / `ScopedKey` | Guid + ScopeId 双键 |
| `Unregister(key)` | 弱引用惰性清理 + `Dispose()` |
| Renderer 手写 `binding.Commit()` + onExternalChange | 叶子 setter 合并通知；容器递归提交 |

## 九、落地顺序

1. 核心类型：`ChangeSet` / `ISnapshotBinding` / `SnapshotStep` / `SnapshotSetter`
2. `SnapshotBinding` 基类（叶子 + 对象展开 + 递归 Commit）
3. `List/DictSnapshotBinding`
4. `SnapshotManager` 重构（Guid 注册表 + 弱引用 + 防重入 + 惰性清理 + changeSet）
5. `UndoScope` / `EditorSnapshotManager` / `ISnapshotManager` 适配
6. UIToolKit Renderer 迁移（38 处）
7. 测试重写与验证
