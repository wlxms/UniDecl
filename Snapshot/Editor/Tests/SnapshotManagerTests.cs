using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UniDecl.Snapshot;

namespace UniDecl.Snapshot.Editor.Tests
{
    [TestFixture]
    public class SnapshotManagerTests
    {
        private SnapshotManager _mgr;

        [SetUp]
        public void SetUp()
        {
            _mgr = new SnapshotManager();
        }

        // ═══════════════════════════════════════
        // ValueStep Tests
        // ═══════════════════════════════════════

        [Test]
        public void Test_ValueStep_RecordUndoRedo()
        {
            float current = 0f;
            _mgr.Register<float>("x", v => { var prev = current; current = v; return prev; });

            _mgr.Record(0f, "x");
            current = 10f;

            Assert.IsTrue(_mgr.Undo());
            Assert.AreEqual(0f, current, 0.001f);

            Assert.IsTrue(_mgr.Redo());
            Assert.AreEqual(10f, current, 0.001f);
        }

        [Test]
        public void Test_ValueStep_UnregisteredKeyThrows()
        {
            Assert.Throws<InvalidOperationException>(() => _mgr.Record(1.0f, "x"));
        }

        [Test]
        public void Test_ValueStep_MultipleKeys()
        {
            float x = 0f, y = 0f;
            _mgr.Register<float>("x", v => { var prev = x; x = v; return prev; });
            _mgr.Register<float>("y", v => { var prev = y; y = v; return prev; });

            _mgr.Record(0f, "x");
            x = 10f;
            _mgr.Record(0f, "y");
            y = 20f;

            _mgr.Undo(); // y → 0
            Assert.AreEqual(0f, y, 0.001f);
            Assert.AreEqual(10f, x, 0.001f);

            _mgr.Undo(); // x → 0
            Assert.AreEqual(0f, x, 0.001f);
        }

        [Test]
        public void Test_ValueStep_StackLimit()
        {
            float val = 0f;
            _mgr.Register<float>("x", v => { var prev = val; val = v; return prev; });
            _mgr.MaxSteps = 3;

            for (int i = 0; i < 4; i++)
            {
                _mgr.Record(i, "x");
                val = i + 1;
            }

            Assert.AreEqual(3, _mgr.UndoCount);
        }

        // ═══════════════════════════════════════
        // ObjectDiffStep Tests
        // ═══════════════════════════════════════

        private class SimpleObj
        {
            public int A;
            public string B;
        }

        [Test]
        public void Test_ObjectDiffStep_RecordUndoRedo()
        {
            var obj = new SimpleObj { A = 1, B = "hello" };
            _mgr.RecordObject(obj, "obj");

            obj.A = 99;
            obj.B = "world";

            Assert.IsTrue(_mgr.Undo());
            Assert.AreEqual(1, obj.A);
            Assert.AreEqual("hello", obj.B);

            Assert.IsTrue(_mgr.Redo());
            Assert.AreEqual(99, obj.A);
            Assert.AreEqual("world", obj.B);
        }

        private class ParentObj
        {
            public SimpleObj Child;
        }

        [Test]
        public void Test_ObjectDiffStep_NestedObject()
        {
            var parent = new ParentObj { Child = new SimpleObj { A = 5, B = "inner" } };
            _mgr.RecordObject(parent, "parent");

            parent.Child.A = 100;

            Assert.IsTrue(_mgr.Undo());
            Assert.AreEqual(5, parent.Child.A);
        }

        private class Node
        {
            public Node Next;
            public int Value;
        }

        [Test]
        public void Test_ObjectDiffStep_CircularRef()
        {
            var a = new Node { Value = 1 };
            var b = new Node { Value = 2 };
            a.Next = b;
            b.Next = a;

            Assert.DoesNotThrow(() => _mgr.RecordObject(a, "node"));
        }

        [Test]
        public void Test_ObjectDiffStep_SharedReferenceCloned()
        {
            var shared = new SimpleObj { A = 42, B = "shared" };
            var container = new Container { X = shared, Y = shared };

            _mgr.RecordObject(container, "c");
            container.X.A = 99;

            _mgr.Undo();

            // X and Y are independent copies after deep copy
            Assert.AreEqual(42, container.X.A);
            Assert.AreEqual(42, container.Y.A);
            Assert.AreNotSame(container.X, container.Y);
        }

        private class Container
        {
            public SimpleObj X;
            public SimpleObj Y;
        }

        // ═══════════════════════════════════════
        // GroupStep Tests
        // ═══════════════════════════════════════

        [Test]
        public void Test_GroupStep_BasicTransaction()
        {
            float x = 0f, y = 0f;
            _mgr.Register<float>("x", v => { var prev = x; x = v; return prev; });
            _mgr.Register<float>("y", v => { var prev = y; y = v; return prev; });

            _mgr.BeginGroup("move");
            _mgr.Record(0f, "x");
            x = 10f;
            _mgr.Record(0f, "y");
            y = 20f;
            _mgr.EndGroup();

            _mgr.Undo();
            Assert.AreEqual(0f, x, 0.001f);
            Assert.AreEqual(0f, y, 0.001f);
        }

        [Test]
        public void Test_GroupStep_NestedGroups()
        {
            float x = 0f, y = 0f, z = 0f;
            _mgr.Register<float>("x", v => { var prev = x; x = v; return prev; });
            _mgr.Register<float>("y", v => { var prev = y; y = v; return prev; });
            _mgr.Register<float>("z", v => { var prev = z; z = v; return prev; });

            _mgr.BeginGroup("outer");
            _mgr.Record(0f, "x");
            x = 10f;
            _mgr.BeginGroup("inner");
            _mgr.Record(0f, "y");
            y = 20f;
            _mgr.EndGroup();
            _mgr.Record(0f, "z");
            z = 30f;
            _mgr.EndGroup();

            _mgr.Undo();
            Assert.AreEqual(0f, x, 0.001f);
            Assert.AreEqual(0f, y, 0.001f);
            Assert.AreEqual(0f, z, 0.001f);
        }

        [Test]
        public void Test_GroupStep_MixedSteps()
        {
            float val = 0f;
            var obj = new SimpleObj { A = 1, B = "hello" };
            _mgr.Register<float>("val", v => { var prev = val; val = v; return prev; });

            _mgr.BeginGroup("mixed");
            _mgr.Record(0f, "val");
            val = 99f;
            _mgr.RecordObject(obj, "obj");
            obj.A = 100;
            _mgr.EndGroup();

            _mgr.Undo();
            Assert.AreEqual(0f, val, 0.001f);
            Assert.AreEqual(1, obj.A);
        }

        [Test]
        public void Test_GroupStep_PendingBufferAutoCommit()
        {
            float x = 0f, y = 0f;
            _mgr.Register<float>("x", v => { var prev = x; x = v; return prev; });
            _mgr.Register<float>("y", v => { var prev = y; y = v; return prev; });

            _mgr.Record(0f, "x");
            x = 10f;
            _mgr.Record(0f, "y");
            y = 20f;

            _mgr.Undo(); // auto-commit pending, then undo the group
            Assert.AreEqual(0f, x, 0.001f);
            Assert.AreEqual(0f, y, 0.001f);
        }

        [Test]
        public void Test_GroupStep_UnclosedGroupAutoCommit()
        {
            float x = 0f;
            _mgr.Register<float>("x", v => { var prev = x; x = v; return prev; });

            _mgr.BeginGroup("unclosed");
            _mgr.Record(0f, "x");
            x = 10f;
            // No EndGroup

            _mgr.Undo();
            Assert.AreEqual(0f, x, 0.001f);
        }

        // ═══════════════════════════════════════
        // Merge Tests
        // ═══════════════════════════════════════

        [Test]
        public void Test_Merge_SameKeyConsecutive()
        {
            float val = 0f;
            _mgr.Register<float>("x", v => { var prev = val; val = v; return prev; });
            _mgr.EnableMerge = true;

            _mgr.Record(1.0f, "x"); val = 1.1f;
            _mgr.Record(1.1f, "x"); val = 1.2f;
            _mgr.Record(1.2f, "x"); val = 1.3f;

            _mgr.Undo();
            Assert.AreEqual(1.0f, val, 0.001f); // 恢复到最早值
        }

        [Test]
        public void Test_Merge_DifferentKeyNoMerge()
        {
            float x = 0f, y = 0f;
            _mgr.Register<float>("x", v => { var prev = x; x = v; return prev; });
            _mgr.Register<float>("y", v => { var prev = y; y = v; return prev; });

            _mgr.Record(0f, "x"); x = 10f;
            _mgr.Record(0f, "y"); y = 20f;

            _mgr.Undo(); // only y restored
            Assert.AreEqual(10f, x, 0.001f);
            Assert.AreEqual(0f, y, 0.001f);
        }

        [Test]
        public void Test_Merge_Disabled()
        {
            float val = 0f;
            _mgr.Register<float>("x", v => { var prev = val; val = v; return prev; });
            _mgr.EnableMerge = false;

            _mgr.Record(1.0f, "x"); val = 1.1f;
            _mgr.Record(1.1f, "x"); val = 1.2f;

            _mgr.Undo(); // only last record undone
            Assert.AreEqual(1.1f, val, 0.001f);
        }

        [Test]
        public void Test_Merge_MergeWindowExpired()
        {
            float val = 0f;
            _mgr.Register<float>("x", v => { var prev = val; val = v; return prev; });
            _mgr.MergeWindowMs = 10;

            _mgr.Record(1.0f, "x"); val = 1.1f;
            Thread.Sleep(50);
            _mgr.Record(1.1f, "x"); val = 1.2f;

            _mgr.Undo();
            Assert.AreEqual(1.1f, val, 0.001f); // window expired, not merged
        }

        // ═══════════════════════════════════════
        // UndoScope Tests
        // ═══════════════════════════════════════

        [Test]
        public void Test_Scope_BasicLifecycle()
        {
            float val = 0f;
            var scope = new UndoScope(_mgr);
            scope.Register<float>("x", v => { var prev = val; val = v; return prev; });

            scope.Record(0f, "x");
            val = 10f;

            _mgr.Undo();
            Assert.AreEqual(0f, val, 0.001f);

            scope.Dispose();
            Assert.AreEqual(0, _mgr.UndoCount);
        }

        [Test]
        public void Test_Scope_DisposeRemovesSetters()
        {
            var scope = new UndoScope(_mgr);
            scope.Register<float>("x", v => v);

            scope.Dispose();
            Assert.Throws<InvalidOperationException>(() => _mgr.Record(1.0f, "x"));
        }

        [Test]
        public void Test_Scope_ParentDisposesChildren()
        {
            float val = 0f;
            var parent = new UndoScope(_mgr);
            var child = new UndoScope(parent);
            child.Register<float>("x", v => { var prev = val; val = v; return prev; });

            child.Record(0f, "x");
            val = 10f;

            parent.Dispose();
            Assert.AreEqual(0, _mgr.UndoCount);
            Assert.Throws<InvalidOperationException>(() => _mgr.Record(1.0f, "x"));
        }

        [Test]
        public void Test_Scope_MultipleScopesIsolated()
        {
            float a = 0f, b = 0f;
            var scopeA = new UndoScope(_mgr);
            var scopeB = new UndoScope(_mgr);
            scopeA.Register<float>("x", v => { var prev = a; a = v; return prev; });
            scopeB.Register<float>("y", v => { var prev = b; b = v; return prev; });

            scopeA.Record(0f, "x"); a = 10f;
            scopeB.Record(0f, "y"); b = 20f;

            scopeA.Dispose();
            Assert.AreEqual(1, _mgr.UndoCount); // scopeB's step remains

            Assert.IsTrue(_mgr.Undo());
            Assert.AreEqual(0f, b, 0.001f);
        }

        [Test]
        public void Test_Scope_DoubleDisposeSafe()
        {
            var scope = new UndoScope(_mgr);
            scope.Register<float>("x", v => v);

            scope.Dispose();
            Assert.DoesNotThrow(() => scope.Dispose());
        }
    }
}
