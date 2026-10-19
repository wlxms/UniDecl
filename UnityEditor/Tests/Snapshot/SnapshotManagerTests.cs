using System;
using System.Collections.Generic;
using NUnit.Framework;
using UniDecl.Snapshot;

namespace UniDecl.Snapshot.Tests
{
    [TestFixture]
    public class SnapshotManagerTests
    {
        private SnapshotManager _mgr;
        private int _scopeId;

        [SetUp]
        public void SetUp()
        {
            _mgr = new SnapshotManager();
            _scopeId = _mgr.CreateScope();
        }

        private class SimpleObj
        {
            public int A;
            public string B;
        }

        private class ParentObj
        {
            public SimpleObj Child;
        }

        private class Node
        {
            public Node Next;
            public int Value;
        }

        // ═══════════════════════════════════════
        // 值叶子（SnapshotBinding 叶子形态）
        // ═══════════════════════════════════════

        private SnapshotBinding CreateLeaf(Func<float> getter, Action<float> setter, string path = "x")
            => new SnapshotBinding(_mgr, _scopeId, path,
                () => getter(),
                (restore, current, changes) => setter((float)restore));

        [Test]
        public void Test_ValueLeaf_RecordUndoRedo()
        {
            float current = 0f;
            var binding = CreateLeaf(() => current, v => current = v);

            current = 10f;
            binding.Commit();

            Assert.AreEqual(1, _mgr.UndoCount);
            Assert.IsTrue(_mgr.Undo());
            Assert.AreEqual(0f, current, 0.001f);

            Assert.IsTrue(_mgr.Redo());
            Assert.AreEqual(10f, current, 0.001f);
        }

        [Test]
        public void Test_ValueLeaf_NoChangeNoStep()
        {
            float current = 5f;
            var binding = CreateLeaf(() => current, v => current = v);

            binding.Commit(); // 基线 5 == 当前 5
            Assert.AreEqual(0, _mgr.UndoCount);
        }

        [Test]
        public void Test_ValueLeaf_IndependentCommitsNotMerged()
        {
            // 独立提交点（如两次 Blur/两次选择）由 BreakMerge 显式隔离——各自成 step、可分别 undo；
            // 未打断的连续记录才按时间窗合并（拖动等连续事件流的设计，见 MergeWithinGroup/StackLimit）
            float current = 0f;
            var binding = CreateLeaf(() => current, v => current = v);

            current = 10f;
            binding.Commit();
            binding.BreakMerge(); // 离散提交点：打断与上一步的合并
            current = 20f;
            binding.Commit();

            Assert.AreEqual(2, _mgr.UndoCount);
            _mgr.Undo();
            Assert.AreEqual(10f, current, 0.001f); // 只撤销第二次
            _mgr.Undo();
            Assert.AreEqual(0f, current, 0.001f); // 再撤销第一次
        }

        [Test]
        public void Test_ValueLeaf_MergeWithinGroup()
        {
            // merge 仅在当前 buffer 内生效（组内连续记录合并，保留最早旧值）
            float current = 0f;
            var binding = CreateLeaf(() => current, v => current = v);

            _mgr.BeginGroup("g");
            current = 10f;
            binding.Commit();
            current = 20f;
            binding.Commit(); // 组内同 binding 连续 → 合并
            _mgr.EndGroup();

            Assert.IsTrue(_mgr.CommitPending());
            Assert.AreEqual(1, _mgr.UndoCount);
            _mgr.Undo();
            Assert.AreEqual(0f, current, 0.001f);
        }

        [Test]
        public void Test_ValueLeaf_MergeDisabled()
        {
            float current = 0f;
            var binding = CreateLeaf(() => current, v => current = v);
            _mgr.EnableMerge = false;

            current = 10f;
            binding.Commit();
            current = 20f;
            binding.Commit();

            Assert.AreEqual(2, _mgr.UndoCount);
            _mgr.Undo();
            Assert.AreEqual(10f, current, 0.001f); // 只撤销最后一步
        }

        [Test]
        public void Test_ValueLeaf_UndoAfterUndoNoBaselineMismatch()
        {
            // undo 后基线同步为还原值：立即 commit 不应把还原当新变更
            float current = 0f;
            var binding = CreateLeaf(() => current, v => current = v);

            current = 10f;
            binding.Commit();
            _mgr.Undo();
            Assert.AreEqual(0f, current, 0.001f);

            binding.Commit(); // 基线已同步为 0，无变化
            Assert.AreEqual(0, _mgr.UndoCount);
        }

        [Test]
        public void Test_CommitDuringRestore_Throws()
        {
            float current = 0f;
            SnapshotBinding binding = null;
            binding = new SnapshotBinding(_mgr, _scopeId, "x",
                () => current,
                (restore, cur, changes) =>
                {
                    current = (float)restore;
                    Assert.Throws<InvalidOperationException>(() => binding.Commit()); // 防重入
                });

            current = 10f;
            binding.Commit();
            _mgr.Undo();
        }

        // ═══════════════════════════════════════
        // 对象展开（SnapshotBinding 对象形态）
        // ═══════════════════════════════════════

        [Test]
        public void Test_Object_FieldLevelUndo()
        {
            var obj = new SimpleObj { A = 1, B = "hello" };
            var binding = new SnapshotBinding(_mgr, _scopeId, "obj", () => obj);

            obj.A = 99;
            binding.Commit();

            Assert.AreEqual(1, _mgr.UndoCount);
            Assert.IsTrue(_mgr.Undo());
            Assert.AreEqual(1, obj.A);
            Assert.AreEqual("hello", obj.B); // B 未变，不受影响
        }

        [Test]
        public void Test_Object_NestedFieldUndo()
        {
            var parent = new ParentObj { Child = new SimpleObj { A = 5, B = "inner" } };
            var binding = new SnapshotBinding(_mgr, _scopeId, "parent", () => parent);

            parent.Child.A = 100;
            binding.Commit();

            Assert.IsTrue(_mgr.Undo());
            Assert.AreEqual(5, parent.Child.A);
            Assert.AreEqual("inner", parent.Child.B);
        }

        [Test]
        public void Test_Object_NestedUndo_PreservesParentReference()
        {
            // 回归：容器 Notify 不得调用字段写回 setter 把父引用置 null
            var parent = new ParentObj { Child = new SimpleObj { A = 5, B = "inner" } };
            var binding = new SnapshotBinding(_mgr, _scopeId, "parent", () => parent);

            parent.Child.A = 100;
            binding.Commit();
            _mgr.Undo();

            Assert.IsNotNull(parent.Child);
            Assert.AreEqual(5, parent.Child.A);
        }

        [Test]
        public void Test_Object_ContainerSetterNotifiedWithChanges()
        {
            var obj = new SimpleObj { A = 1, B = "hello" };
            ChangeSet received = null;
            var binding = new SnapshotBinding(_mgr, _scopeId, "obj", () => obj,
                (restore, current, changes) => received = changes);

            obj.A = 99;
            binding.Commit();
            _mgr.Undo();

            Assert.IsNotNull(received);
            Assert.AreEqual(1, received.Changes.Count);
            Assert.AreEqual("obj.A", received.Changes[0].Path);
            Assert.AreEqual(99, received.Changes[0].OldValue);
            Assert.AreEqual(1, received.Changes[0].NewValue);
        }

        [Test]
        public void Test_Object_CircularRef_NoThrow()
        {
            var a = new Node { Value = 1 };
            var b = new Node { Value = 2 };
            a.Next = b;
            b.Next = a;

            Assert.DoesNotThrow(() => new SnapshotBinding(_mgr, _scopeId, "node", () => a));
        }

        [Test]
        public void Test_Object_CircularRef_TruncatedAsLeaf()
        {
            // 循环引用字段截断为整体引用叶子：undo 恢复引用，不深入内部
            var a = new Node { Value = 1 };
            var b = new Node { Value = 2 };
            a.Next = b;
            b.Next = a;

            var binding = new SnapshotBinding(_mgr, _scopeId, "a", () => a);
            var b2 = new Node { Value = 3 };
            a.Next = b2;
            binding.Commit();

            Assert.IsTrue(_mgr.Undo());
            Assert.AreSame(b, a.Next);
        }

        // ═══════════════════════════════════════
        // List / Dict
        // ═══════════════════════════════════════

        [Test]
        public void Test_List_CountChangeWholeStep()
        {
            var list = new List<int> { 1, 2 };
            var binding = new ListSnapshotBinding(_mgr, _scopeId, "list", () => list);

            list.Add(3);
            binding.Commit();

            Assert.AreEqual(1, _mgr.UndoCount);
            Assert.IsTrue(_mgr.Undo());
            CollectionAssert.AreEqual(new[] { 1, 2 }, list);
        }

        [Test]
        public void Test_List_ElementChange()
        {
            var list = new List<int> { 1, 2 };
            var binding = new ListSnapshotBinding(_mgr, _scopeId, "list", () => list);

            list[0] = 100;
            binding.Commit();

            Assert.AreEqual(1, _mgr.UndoCount);
            Assert.IsTrue(_mgr.Undo());
            CollectionAssert.AreEqual(new[] { 1, 2 }, list);
        }

        [Test]
        public void Test_List_ObjectElementsRecursive()
        {
            var list = new List<SimpleObj> { new SimpleObj { A = 1, B = "x" } };
            var binding = new ListSnapshotBinding(_mgr, _scopeId, "list", () => list);

            list[0].A = 99;
            binding.Commit();

            Assert.IsTrue(_mgr.Undo());
            Assert.AreEqual(1, list[0].A);
        }

        [Test]
        public void Test_Dict_KeyChangeWholeStep()
        {
            var dict = new Dictionary<string, int> { ["a"] = 1 };
            var binding = new DictSnapshotBinding(_mgr, _scopeId, "dict", () => dict);

            dict["b"] = 2;
            binding.Commit();

            Assert.AreEqual(1, _mgr.UndoCount);
            Assert.IsTrue(_mgr.Undo());
            CollectionAssert.AreEqual(new[] { "a" }, new List<string>(dict.Keys));
            Assert.AreEqual(1, dict["a"]);
        }

        [Test]
        public void Test_Dict_ValueChange()
        {
            var dict = new Dictionary<string, int> { ["a"] = 1 };
            var binding = new DictSnapshotBinding(_mgr, _scopeId, "dict", () => dict);

            dict["a"] = 42;
            binding.Commit();

            Assert.IsTrue(_mgr.Undo());
            Assert.AreEqual(1, dict["a"]);
        }

        // ═══════════════════════════════════════
        // Group
        // ═══════════════════════════════════════

        [Test]
        public void Test_Group_ManualWrapsMultipleCommits()
        {
            float x = 0f;
            var binding = CreateLeaf(() => x, v => x = v);

            _mgr.BeginGroup("manual");
            x = 10f;
            binding.Commit(); // step 进手动组 buffer
            x = 20f;
            binding.Commit(); // 同 binding 合并，保留最早旧值 0
            _mgr.EndGroup();

            Assert.IsTrue(_mgr.CommitPending());
            Assert.AreEqual(1, _mgr.UndoCount);

            _mgr.Undo();
            Assert.AreEqual(0f, x, 0.001f); // 一步撤销整组
        }

        [Test]
        public void Test_Group_AutoGroupNestedInManual()
        {
            var obj = new SimpleObj { A = 1, B = "hello" };
            var binding = new SnapshotBinding(_mgr, _scopeId, "obj", () => obj);

            _mgr.BeginGroup("manual");
            obj.A = 99;
            binding.Commit(); // 自动组{obj.A} 嵌套进手动组
            _mgr.EndGroup();
            _mgr.CommitPending();

            Assert.AreEqual(1, _mgr.UndoCount);
            _mgr.Undo();
            Assert.AreEqual(1, obj.A);
        }

        [Test]
        public void Test_Group_EmptyGroupDiscarded()
        {
            float x = 0f;
            var binding = CreateLeaf(() => x, v => x = v);

            _mgr.BeginGroup("empty");
            _mgr.EndGroup(); // 无 step，丢弃
            binding.Commit(); // 无变化，不记录

            Assert.AreEqual(0, _mgr.UndoCount);
        }

        // ═══════════════════════════════════════
        // changeSet / 事件
        // ═══════════════════════════════════════

        [Test]
        public void Test_ChangeSet_UndoEventCarriesPaths()
        {
            var obj = new SimpleObj { A = 1, B = "hello" };
            var binding = new SnapshotBinding(_mgr, _scopeId, "obj", () => obj);

            ChangeSet last = null;
            _mgr.OnUndoRedoPerformed += cs => last = cs;

            obj.A = 99;
            binding.Commit();
            _mgr.Undo();

            Assert.IsNotNull(last);
            Assert.AreEqual(1, last.Changes.Count);
            Assert.AreEqual("obj.A", last.Changes[0].Path);
        }

        [Test]
        public void Test_ChangeSet_RedoReversesValues()
        {
            var obj = new SimpleObj { A = 1 };
            var binding = new SnapshotBinding(_mgr, _scopeId, "obj", () => obj);

            ChangeSet last = null;
            _mgr.OnUndoRedoPerformed += cs => last = cs;

            obj.A = 99;
            binding.Commit();
            _mgr.Undo();  // 事件：99 → 1
            _mgr.Redo();  // 事件：1 → 99（本次操作前的值=撤销后值，恢复到 99）

            Assert.IsNotNull(last);
            Assert.AreEqual(1, last.Changes[0].OldValue);  // redo：旧=撤销后的值
            Assert.AreEqual(99, last.Changes[0].NewValue); // 新=恢复值
        }

        // ═══════════════════════════════════════
        // Scope / 生命周期
        // ═══════════════════════════════════════

        [Test]
        public void Test_Scope_DisposeKeepsHistory_UndoNoOp()
        {
            // 设计：DisposeScope 反注册 binding 但保留历史——跨结构 rebuild 的 undo 靠 Path 兜底。
            // 已无当代 binding 的 step 成为 no-op（弹出但不产生写回，redo 不产生）。
            float x = 0f;
            var binding = CreateLeaf(() => x, v => x = v);

            x = 10f;
            binding.Commit();
            Assert.AreEqual(1, _mgr.UndoCount);

            _mgr.DisposeScope(_scopeId);

            Assert.AreEqual(1, _mgr.UndoCount, "历史按设计保留（Path 兜底）");
            Assert.IsTrue(_mgr.Undo()); // step 弹出成功
            Assert.AreEqual(10f, x, 0.001f, "无当代 binding：不写回");
            Assert.IsFalse(_mgr.Redo(), "no-op 不产生 redo step");
        }

        [Test]
        public void Test_Scope_DisposeBinding_PathFallbackRestores()
        {
            // 设计动机：结构 rebuild（条件显隐等）会 dispose 旧 binding 并注册同 Path 新 binding，
            // 历史不清理——undo 经 Path 兜底落到当代 binding 上照常恢复。
            float x = 0f;
            var old = CreateLeaf(() => x, v => x = v);
            x = 10f;
            old.Commit();

            var fresh = CreateLeaf(() => x, v => x = v); // 模拟 rebuild 后的同 Path 新代
            old.Dispose();                                // 旧代反注册

            Assert.AreEqual(1, _mgr.UndoCount, "换代不丢历史");
            Assert.IsTrue(_mgr.Undo());
            Assert.AreEqual(0f, x, 0.001f, "Path 兜底到当代 binding 恢复");
        }

        [Test]
        public void Test_Scope_DisposeBinding_KeepsHistory()
        {
            float x = 0f;
            var binding = CreateLeaf(() => x, v => x = v);
            x = 10f;
            binding.Commit();

            binding.Dispose(); // 显式反注册；历史保留（Path 兜底设计）

            Assert.AreEqual(1, _mgr.UndoCount);
            Assert.IsTrue(_mgr.Undo());
            Assert.AreEqual(10f, x, 0.001f, "已反注册：不写回");
        }

        [Test]
        public void Test_Scope_ParentDisposesChildren()
        {
            float x = 0f;
            var childScope = _mgr.CreateScope(_scopeId);
            var binding = new SnapshotBinding(_mgr, childScope, "x",
                () => x, (restore, cur, ch) => x = (float)restore);

            x = 10f;
            binding.Commit();
            Assert.AreEqual(1, _mgr.UndoCount);

            _mgr.DisposeScope(_scopeId); // 父级联清理子 scope 的 binding；历史保留
            Assert.AreEqual(1, _mgr.UndoCount, "级联反注册后历史仍保留（Path 兜底）");
            Assert.IsTrue(_mgr.Undo());
            Assert.AreEqual(10f, x, 0.001f, "子 binding 已随 scope 反注册：不写回");
        }

        [Test]
        public void Test_Scope_DoubleDisposeSafe()
        {
            var scope = new UndoScope(_mgr);
            scope.Dispose();
            Assert.DoesNotThrow(() => scope.Dispose());
        }

        [Test]
        public void Test_MaxSteps_StackLimit()
        {
            float x = 0f;
            var binding = CreateLeaf(() => x, v => x = v);
            _mgr.MaxSteps = 3;
            _mgr.EnableMerge = false; // 避免连续 commit 被合并

            for (int i = 0; i < 4; i++)
            {
                x = i + 1;
                binding.Commit();
            }

            Assert.AreEqual(3, _mgr.UndoCount);
        }
    }
}
