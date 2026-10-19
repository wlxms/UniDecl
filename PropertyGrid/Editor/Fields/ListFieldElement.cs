using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.BuiltIn.Runtime.Widgets;
using UniDecl.UIToolKit.Runtime;
using UnityEditor;

namespace UniDecl.PropertyGrid.Editor
{
    /// <summary>
    /// List/数组字段编辑器——每元素一行 + 尾部增行按钮，数组增删经重建提交。
    /// 值类型元素经 ListItemAccessor 写透（复用全部 TypeRenderer/Decorator）；
    /// Serializable 对象元素以元素实例为根递归 LayoutResolver+WidgetFactory 展开，
    /// 条件显隐、Range、Tooltip 等特性在元素内部照常生效。
    /// </summary>
    public sealed class ListFieldElement : Element
    {
        readonly PropertyAccessor _accessor;
        readonly BuildContext _ctx;

        public ListFieldElement(PropertyAccessor accessor, BuildContext ctx)
        {
            _accessor = accessor;
            _ctx = ctx;
            WithKey($"list_{accessor.FullPath}");
        }

        public override IElement Render()
        {
            var root = new VerticalLayout();
            var elemType = GetElementType(_accessor.PropertyType);
            if (elemType == null)
            {
                root.Add(new Label($"(不支持的集合类型: {_accessor.PropertyType.Name})"));
                return root;
            }

            var list = _accessor.GetValue() as IList;
            var expandable = LayoutResolver.IsExpandable(elemType);

            if (list != null)
                for (int i = 0; i < list.Count; i++)
                    root.Add(expandable ? BuildObjectRow(list, i, elemType) : BuildValueRow(list, i, elemType));

            var addBtn = new Button("+ 添加元素") { OnClick = () => AddItem(elemType) };
            addBtn.WithKey($"listadd_{_accessor.FullPath}");
            addBtn.With(new UITKStyle { Width = 110, AlignSelf = UnityEngine.UIElements.Align.FlexStart });
            root.Add(addBtn);
            return root;
        }

        // ------------------------------------------------------------------
        // 行构建
        // ------------------------------------------------------------------

        IElement BuildValueRow(IList list, int index, Type elemType)
        {
            var row = NewRow(index);
            var itemAccessor = new ListItemAccessor(list, index, $"{_accessor.FullPath}[{index}]") { Context = _ctx };

            IElement editor;
            if (FieldTypeRendererRegistry.TryResolve(elemType, out var tr))
                editor = tr.CreateWidget(itemAccessor, _ctx);
            else
                editor = new Label($"{list[index]}");

            if (editor is Element e)
            {
                e.WithKey($"li_{_accessor.FullPath}_{index}");
                e.With(new UITKStyle { FlexGrow = 1, MinWidth = 0 });
            }
            row.Add(editor);
            row.Add(NewRemoveButton(list, index));
            return row;
        }

        IElement BuildObjectRow(IList list, int index, Type elemType)
        {
            var item = list[index];
            var row = NewRow(index);

            if (item == null)
            {
                row.Add(new Label($"[{index}] (null)"));
                row.Add(NewRemoveButton(list, index));
                return row;
            }

            // 以元素实例为根递归解析——元素内的装饰器/条件/嵌套全部复用主流程
            var meta = ReflectionCache.GetOrCreateMeta(elemType);
            var itemRoot = new PropertyAccessor(item, Array.Empty<FieldInfo>(), $"[{index}]", $"{_accessor.FullPath}[{index}]") { Context = _ctx };
            var subtree = WidgetFactory.CreateTree(LayoutResolver.Resolve(meta, item, itemRoot), _ctx);

            var foldout = new Foldout(GetItemTitle(item, index)) { Value = true };
            foldout.WithKey($"lifold_{_accessor.FullPath}_{index}");
            foldout.With(new UITKStyle { FlexGrow = 1, MinWidth = 0 });
            if (subtree != null) foldout.Add(subtree);

            row.Add(foldout);
            row.Add(NewRemoveButton(list, index));
            return row;
        }

        HorizontalLayout NewRow(int index)
        {
            var row = new HorizontalLayout();
            row.WithKey($"lirow_{_accessor.FullPath}_{index}");
            row.With(new UITKStyle { AlignItems = UnityEngine.UIElements.Align.FlexStart });
            return row;
        }

        Button NewRemoveButton(IList list, int index)
        {
            var btn = new Button("✕") { OnClick = () => RemoveItem(list, index) };
            btn.WithKey($"lirm_{_accessor.FullPath}_{index}");
            btn.With(new UITKStyle { Width = 24 });
            return btn;
        }

        /// <summary>元素标题：首个 string 字段值（如名称）优先，否则用类型名。</summary>
        static string GetItemTitle(object item, int index)
        {
            var nameField = item.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(f => f.FieldType == typeof(string));
            if (nameField?.GetValue(item) is string name && !string.IsNullOrEmpty(name))
                return $"[{index}] {name}";
            return $"[{index}] {ObjectNames.NicifyVariableName(item.GetType().Name)}";
        }

        // ------------------------------------------------------------------
        // 增删
        // ------------------------------------------------------------------

        void AddItem(Type elemType)
        {
            if (_accessor.PropertyType.IsArray)
            {
                var old = _accessor.GetValue() as Array;
                var newArr = Array.CreateInstance(elemType, (old?.Length ?? 0) + 1);
                if (old != null) Array.Copy(old, newArr, old.Length);
                _accessor.SetValue(newArr);
            }
            else
            {
                var list = GetOrCreateList();
                if (list == null) return;
                list.Add(CreateDefault(elemType));
                CommitList(list);
            }
            Rebuild();
        }

        void RemoveItem(IList list, int index)
        {
            if (list == null || index < 0 || index >= list.Count) return;

            if (_accessor.PropertyType.IsArray)
            {
                var elemType = _accessor.PropertyType.GetElementType();
                var newArr = Array.CreateInstance(elemType, list.Count - 1);
                for (int i = 0, j = 0; i < list.Count; i++)
                    if (i != index) newArr.SetValue(list[i], j++);
                _accessor.SetValue(newArr);
            }
            else
            {
                list.RemoveAt(index);
                CommitList(list);
            }
            Rebuild();
        }

        IList GetOrCreateList()
        {
            var v = _accessor.GetValue() as IList;
            if (v == null && !_accessor.PropertyType.IsArray)
            {
                v = Activator.CreateInstance(_accessor.PropertyType) as IList;
                _accessor.SetValue(v);
            }
            return v;
        }

        /// <summary>列表内容变更后整字段提交（引用未变也提交）——触发 FieldChanged/Undo 链。</summary>
        void CommitList(IList list) => _accessor.SetValue(list);

        static object CreateDefault(Type t)
        {
            if (t == typeof(string)) return "";
            if (typeof(UnityEngine.Object).IsAssignableFrom(t)) return null;
            if (t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null)
                return Activator.CreateInstance(t);
            return null;
        }

        static Type GetElementType(Type collectionType)
        {
            if (collectionType == null) return null;
            if (collectionType.IsArray) return collectionType.GetElementType();
            if (collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof(List<>))
                return collectionType.GetGenericArguments()[0];
            return null;
        }
    }

    /// <summary>
    /// 列表元素访问器——按下标寻址，写透到 IList；
    /// 其余语义（ValueChanged/FieldChanged）与字段访问器一致。
    /// </summary>
    public sealed class ListItemAccessor : PropertyAccessor
    {
        readonly IList _list;
        readonly int _index;

        public ListItemAccessor(IList list, int index, string fullPath)
            : base(list, Array.Empty<FieldInfo>(), $"[{index}]", fullPath)
        {
            _list = list;
            _index = index;
        }

        public override object GetValue() => _list[_index];

        public override void SetValue(object value)
        {
            var old = _list[_index];
            _list[_index] = value;
            RaiseChanged(value, old);
        }
    }

    /// <summary>
    /// 集合字段渲染器——注册键为 typeof(IList) 标记类型，
    /// 由 FieldTypeRendererRegistry.TryResolve 的集合回退分支路由（数组/List 均命中）。
    /// </summary>
    public class ListTypeRenderer : IFieldTypeRenderer
    {
        public Type FieldType => typeof(System.Collections.IList);

        public IElement CreateWidget(PropertyAccessor a, BuildContext c) => new ListFieldElement(a, c);
    }
}
