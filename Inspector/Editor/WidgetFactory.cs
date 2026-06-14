using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniDecl.Inspector.Runtime;
using UniDecl.Runtime.Core;
using UniDecl.Runtime.Widgets;
using UniDecl.Snapshot;
using UniDecl.Snapshot.Editor;
using UnityEditor;
using UnityEngine;

namespace UniDecl.Inspector.Editor
{
    /// <summary>
    /// Widget 工厂——将 LayoutTree 构建为 UniDecl Element 子树
    /// 
    /// 管线：LayoutTree → WidgetFactory.CreateTree() → Element 子树
    /// 
    /// 对每个 LayoutItem：
    /// - FieldItem → TypeWidgetMapper 决定 Widget 类型 → 创建 Widget Element
    /// - ClassElementItem → 根据属性类型创建控件 Element
    /// 
    /// 然后应用属性装饰（显示/约束/条件/绑定）
    /// </summary>
    public static class WidgetFactory
    {
        /// <summary>
        /// Element 创建委托
        /// </summary>
        /// <param name="label">字段标签</param>
        /// <param name="value">字段当前值</param>
        /// <param name="target">数据类实例</param>
        /// <param name="renderer">Renderer 实例（可能为 null）</param>
        /// <returns>UniDecl Element</returns>
        public delegate IElement ElementCreator(string label, object value, object target, object renderer);

        /// <summary>
        /// 预定义的 Element 创建函数集合
        /// </summary>
        public static class ElementCreators
        {
            public static readonly ElementCreator IntField = (label, val, target, renderer) =>
                new IntegerField(Convert.ToInt32(val));

            public static readonly ElementCreator FloatField = (label, val, target, renderer) =>
                new FloatField(Convert.ToSingle(val));

            public static readonly ElementCreator DoubleField = (label, val, target, renderer) =>
                new DoubleField(label, Convert.ToDouble(val));

            public static readonly ElementCreator LongField = (label, val, target, renderer) =>
                new LongField(label, Convert.ToInt64(val));

            public static readonly ElementCreator TextField = (label, val, target, renderer) =>
                new TextField(val as string ?? "", label);

            public static readonly ElementCreator Toggle = (label, val, target, renderer) =>
                new Toggle(label, val is bool b && b);

            public static readonly ElementCreator EnumField = (label, val, target, renderer) =>
                new EnumField(label, val?.GetType() ?? typeof(Enum), val != null ? Convert.ToInt32(val) : 0);

            public static readonly ElementCreator ColorField = (label, val, target, renderer) =>
                new ColorField(label, val is Color c ? c : Color.white);

            public static readonly ElementCreator Vector2Field = (label, val, target, renderer) =>
                new Vector2Field(label, val is Vector2 v ? v : Vector2.zero);

            public static readonly ElementCreator Vector3Field = (label, val, target, renderer) =>
                new Vector3Field(label, val is Vector3 v ? v : Vector3.zero);

            public static readonly ElementCreator Vector4Field = (label, val, target, renderer) =>
                new Vector4Field(label, val is Vector4 v ? v : Vector4.zero);

            public static readonly ElementCreator Vector2IntField = (label, val, target, renderer) =>
                new Vector2IntField(label, val is Vector2Int v ? v : Vector2Int.zero);

            public static readonly ElementCreator Vector3IntField = (label, val, target, renderer) =>
                new Vector3IntField(label, val is Vector3Int v ? v : Vector3Int.zero);

            public static readonly ElementCreator RectField = (label, val, target, renderer) =>
                new RectField(label, val is Rect r ? r : Rect.zero);

            public static readonly ElementCreator RectIntField = (label, val, target, renderer) =>
                new RectIntField(label, val is RectInt r ? r : new RectInt());

            public static readonly ElementCreator BoundsField = (label, val, target, renderer) =>
                new BoundsField(label, val is Bounds b ? b : new Bounds());

            public static readonly ElementCreator BoundsIntField = (label, val, target, renderer) =>
                new BoundsIntField(label, val is BoundsInt b ? b : new BoundsInt());

            public static readonly ElementCreator CurveField = (label, val, target, renderer) =>
                new CurveField(label, val as AnimationCurve ?? new AnimationCurve());

            public static readonly ElementCreator GradientField = (label, val, target, renderer) =>
                new GradientField(label, val as Gradient ?? new Gradient());

            public static readonly ElementCreator LayerField = (label, val, target, renderer) =>
                new LayerField(label, val is int i ? i : 0);

            public static readonly ElementCreator ObjectField = (label, val, target, renderer) =>
                new ObjectField(label, typeof(UnityEngine.Object), val as UnityEngine.Object);

            public static readonly ElementCreator NestedInspector = (label, val, target, renderer) =>
                new InspectorElement(val);

            public static readonly ElementCreator TextArea = (label, val, target, renderer) =>
                new ResizableTextArea(val as string ?? "", label);

            public static readonly ElementCreator EnumToggleButtons = (label, val, target, renderer) =>
                new EnumField(label, val?.GetType() ?? typeof(Enum), val != null ? Convert.ToInt32(val) : 0);

            public static readonly ElementCreator Button = (label, val, target, renderer) =>
                new UniDecl.Runtime.Widgets.Button(label);

            public static readonly ElementCreator FallbackLabel = (label, val, target, renderer) =>
                new Label($"{label}: {val ?? "null"}");
        }

        /// <summary>
        /// 构建 InspectorElement 的 Element 子树上下文
        /// 传递给每个构建步骤，累积绑定信息
        /// </summary>
        public class BuildContext
        {
            public object Target;
            public object Renderer;
            public TypeMeta Meta;
            public EditorSnapshotManager SnapshotManager;
            public Action OnRebuildNeeded;
            public InspectorElement InspectorElement;
            internal bool _isNotifying; // OnValueChanged 重入防护
        }

        /// <summary>
        /// 从 LayoutTree 创建完整的 Element 子树
        /// </summary>
        public static IElement CreateTree(LayoutTree tree, BuildContext ctx)
        {
            var root = BuildNode(tree.Root, ctx);
            return root;
        }

        /// <summary>
        /// 递归构建 LayoutNode → Element
        /// </summary>
        private static IElement BuildNode(LayoutNode node, BuildContext ctx)
        {
            // 叶节点只有 Items，直接构建
            if (node.Children.Count == 0 && node.Type == GroupType.Root)
            {
                return BuildItemsAsContainer(node.Items, ctx);
            }

            // 根据组类型创建容器 Element
            ContainerElement container;
            switch (node.Type)
            {
                case GroupType.Horizontal:
                    container = new HorizontalLayout();
                    break;
                case GroupType.Box:
                case GroupType.Foldout:
                case GroupType.Header:
                case GroupType.Tab:
                    container = new Foldout(node.Title ?? node.Path);
                    break;
                default:
                    container = new VerticalLayout();
                    break;
            }
            container.WithKey($"group_{node.Path}");

            // 添加子节点
            // 先按 Order 排序所有 Items + Children
            var allItems = new List<(int order, IElement element)>();

            foreach (var item in node.Items)
            {
                var element = BuildItem(item, ctx);
                if (element != null)
                    allItems.Add((item.Order, element));
            }

            foreach (var child in node.Children)
            {
                var childElement = BuildNode(child, ctx);
                if (childElement != null)
                    allItems.Add((child.Order, childElement));
            }

            foreach (var (_, element) in allItems.OrderBy(x => x.order))
            {
                container.Add(element);
            }

            return container;
        }

        /// <summary>
        /// 将 Items 列表构建为 VerticalLayout 容器
        /// </summary>
        private static IElement BuildItemsAsContainer(List<LayoutItem> items, BuildContext ctx)
        {
            if (items.Count == 0) return null;
            if (items.Count == 1)
            {
                return BuildItem(items[0], ctx);
            }

            var container = new VerticalLayout();
            container.WithKey("insp_root");
            foreach (var item in items.OrderBy(i => i.Order))
            {
                var element = BuildItem(item, ctx);
                if (element != null)
                    container.Add(element);
            }
            return container;
        }

        /// <summary>
        /// 构建 LayoutItem → Element
        /// </summary>
        private static IElement BuildItem(LayoutItem item, BuildContext ctx)
        {
            if (item is ClassElementItem classItem)
                return BuildClassElement(classItem, ctx);

            if (item is FieldItem fieldItem)
                return BuildFieldElement(fieldItem, ctx);

            return null;
        }

        /// <summary>
        /// 构建类级控件 Element
        /// </summary>
        private static IElement BuildClassElement(ClassElementItem item, BuildContext ctx)
        {
            if (item.Source is ButtonAttribute btnAttr)
            {
                var button = new UniDecl.Runtime.Widgets.Button(btnAttr.Label);
                button.WithKey($"clsbtn_{btnAttr.Method}");
                button.OnClick = () =>
                {
                    var method = FieldBinder.FindMethod(ctx.Renderer?.GetType(), btnAttr.Method, ctx.Target?.GetType());
                    if (method != null)
                    {
                        var parameters = method.GetParameters();
                        if (parameters.Length == 0)
                            method.Invoke(ctx.Renderer, null);
                        else if (parameters.Length == 1 && ctx.Target != null)
                            method.Invoke(ctx.Renderer, new[] { ctx.Target });
                    }
                };
                return button;
            }

            if (item.Source is InspectorLabelAttribute labelAttr)
            {
                var text = FieldBinder.ResolveReference(labelAttr.Text, ctx.Renderer, ctx.Target);
                return new Label(text).WithKey($"clslbl_{labelAttr.Text}");
            }

            if (item.Source is InspectorInfoBoxAttribute infoAttr)
            {
                var text = FieldBinder.ResolveReference(infoAttr.Text, ctx.Renderer, ctx.Target);
                var msgType = infoAttr.Type switch
                {
                    InfoBoxType.Warning => HelpBoxMessageType.Warning,
                    InfoBoxType.Error => HelpBoxMessageType.Error,
                    _ => HelpBoxMessageType.Info,
                };
                return new HelpBox(text, msgType).WithKey($"clsinfo_{infoAttr.Text}");
            }

            return null;
        }

        /// <summary>
        /// 构建字段 Element
        /// </summary>
        private static IElement BuildFieldElement(FieldItem item, BuildContext ctx)
        {
            var field = item.Field;
            var attrs = item.Attributes;

            // 检查 [Button] —— 替换字段编辑器为按钮
            var buttonAttr = GetAttr<ButtonAttribute>(attrs);
            if (buttonAttr != null)
            {
                var btn = new UniDecl.Runtime.Widgets.Button(buttonAttr.Label);
                btn.WithKey($"insp_{field.Name}");
                btn.OnClick = () =>
                {
                    var method = FieldBinder.FindMethod(ctx.Renderer?.GetType(), buttonAttr.Method, ctx.Target?.GetType());
                    if (method != null)
                    {
                        var parameters = method.GetParameters();
                        if (parameters.Length == 0)
                            method.Invoke(ctx.Renderer, null);
                        else if (parameters.Length == 1 && ctx.Target != null)
                            method.Invoke(ctx.Renderer, new[] { ctx.Target });
                    }
                };
                return btn;
            }

            // 解析标签
            var labelTextAttr = GetAttr<LabelTextAttribute>(attrs);
            var label = labelTextAttr != null
                ? FieldBinder.ResolveReference(labelTextAttr.Text, ctx.Renderer, ctx.Target)
                : ObjectNames.NicifyVariableName(field.Name);

            // 条件检查
            if (!CheckCondition(attrs, ctx.Target))
                return null;

            // 获取字段值
            var value = field.GetValue(ctx.Target);

            // [Dropdown] 需要先从 Renderer 解析选项与当前索引，不能走通用空实现。
            var dropdownAttr = GetAttr<DropdownAttribute>(attrs);
            if (dropdownAttr != null)
            {
                var dropdownElement = BuildDropdownElement(field, label, value, dropdownAttr, ctx);
                if (dropdownElement is Element dropdownTyped)
                    dropdownTyped.WithKey($"insp_{field.Name}");

                ApplyBinding(dropdownElement, field, attrs, ctx);
                return dropdownElement;
            }

            // 通过 TypeWidgetMapper 获取创建函数
            var creator = TypeWidgetMapper.MapToCreator(field, attrs);
            if (creator == null)
                return new Label($"{label}: {value ?? "null"}").WithKey($"insp_{field.Name}");

            // 创建 Widget Element
            var element = creator(label, value, ctx.Target, ctx.Renderer);
            if (element == null)
                return null;

            Debug.Log($"[Undo] BuildFieldElement: field={field.Name}, widgetValue={value}, elementType={element.GetType().Name}");

            // 分配稳定 Key
            if (element is Element el)
            {
                el.WithKey($"insp_{field.Name}");
            }

            // 嵌套 InspectorElement 继承父级 hostObject
            if (element is InspectorElement nestedInspected)
            {
                nestedInspected.HostObject = ctx.InspectorElement?.HostObject;
            }

            // 应用双向绑定
            ApplyBinding(element, field, attrs, ctx);

            // 应用只读
            if (GetAttr<ReadOnlyAttribute>(attrs) != null)
            {
                // TODO: 通过 DisableContext 或设置 enabled=false
            }

            return element;
        }

        private static IElement BuildDropdownElement(FieldInfo field, string label, object value,
            DropdownAttribute dropdownAttr, BuildContext ctx)
        {
            var choices = ResolveDropdownChoices(dropdownAttr, ctx);
            var index = ResolveDropdownIndex(field.FieldType, value, choices);
            return new UniDecl.Runtime.Widgets.Dropdown(label, choices, index);
        }

        private static string[] ResolveDropdownChoices(DropdownAttribute dropdownAttr, BuildContext ctx)
        {
            if (dropdownAttr == null || ctx.Renderer == null)
                return Array.Empty<string>();

            var rendererType = ctx.Renderer.GetType();
            var targetType = ctx.Target?.GetType();
            var method = FieldBinder.FindMethod(rendererType, dropdownAttr.Method, targetType);
            if (method == null)
                return Array.Empty<string>();

            try
            {
                object result = null;
                var parameters = method.GetParameters();
                if (parameters.Length == 0)
                    result = method.Invoke(ctx.Renderer, null);
                else if (parameters.Length == 1 && ctx.Target != null)
                    result = method.Invoke(ctx.Renderer, new[] { ctx.Target });

                if (result is string[] array)
                    return array;

                if (result is IEnumerable<string> enumerable)
                    return enumerable.ToArray();
            }
            catch
            {
                // ignore and fall back to empty choices
            }

            return Array.Empty<string>();
        }

        private static int ResolveDropdownIndex(Type fieldType, object value, string[] choices)
        {
            if (choices == null || choices.Length == 0)
                return 0;

            if (fieldType == typeof(string))
            {
                var stringValue = value as string;
                var foundIndex = Array.IndexOf(choices, stringValue);
                return foundIndex >= 0 ? foundIndex : 0;
            }

            if (fieldType == typeof(int) && value is int intIndex)
                return Mathf.Clamp(intIndex, 0, choices.Length - 1);

            return 0;
        }

        /// <summary>
        /// 应用双向绑定：Element 值变更 → 写回 target 字段
        /// </summary>
        private static void ApplyBinding(IElement element, FieldInfo field, InspectorAttribute[] attrs, BuildContext ctx)
        {
            var fieldType = field.FieldType;
            var onValueChangedAttr = GetAttr<OnValueChangedAttribute>(attrs);

            // 根据不同 Widget 类型设置值变更回调
            if (element is FloatField floatEl)
            {
                floatEl.OnValueChanged = (newVal, oldVal) =>
                {
                    if (!TryBeginNotify(ctx)) return;
                    try { WriteFieldValue(field, ctx, onValueChangedAttr, newVal, oldVal, true); }
                    finally { EndNotify(ctx); }
                };
            }
            else if (element is Slider sliderEl)
            {
                sliderEl.OnValueChanged = (newVal) =>
                {
                    if (!TryBeginNotify(ctx)) return;
                    try
                    {
                        var oldVal = field.GetValue(ctx.Target);
                        var writeValue = fieldType == typeof(int) ? Mathf.RoundToInt(newVal) : (object)newVal;
                        WriteFieldValue(field, ctx, onValueChangedAttr, writeValue, oldVal, false);
                    }
                    finally { EndNotify(ctx); }
                };

                sliderEl.OnCommit = (_) =>
                {
                    CheckConditionAndRebuild(field.Name, ctx);
                };
            }
            else if (element is MinMaxSlider minMaxSliderEl)
            {
                minMaxSliderEl.OnValueChanged = (newMin, newMax) =>
                {
                    if (!TryBeginNotify(ctx)) return;
                    try
                    {
                        if (fieldType == typeof(Vector2))
                        {
                            var oldVal = field.GetValue(ctx.Target);
                            var newValue = new Vector2(newMin, newMax);
                            WriteFieldValue(field, ctx, onValueChangedAttr, newValue, oldVal, false);
                        }
                    }
                    finally { EndNotify(ctx); }
                };

                minMaxSliderEl.OnCommit = (_, __) =>
                {
                    CheckConditionAndRebuild(field.Name, ctx);
                };
            }
            else if (element is IntegerField intEl)
            {
                intEl.OnValueChanged = (newVal, oldVal) =>
                {
                    if (!TryBeginNotify(ctx)) return;
                    try { WriteFieldValue(field, ctx, onValueChangedAttr, newVal, oldVal, true); }
                    finally { EndNotify(ctx); }
                };
            }
            else if (element is DoubleField doubleEl)
            {
                doubleEl.OnValueChanged = (newVal, oldVal) =>
                {
                    if (!TryBeginNotify(ctx)) return;
                    try { WriteFieldValue(field, ctx, onValueChangedAttr, newVal, oldVal, true); }
                    finally { EndNotify(ctx); }
                };
            }
            else if (element is LongField longEl)
            {
                longEl.OnValueChanged = (newVal, oldVal) =>
                {
                    if (!TryBeginNotify(ctx)) return;
                    try { WriteFieldValue(field, ctx, onValueChangedAttr, newVal, oldVal, true); }
                    finally { EndNotify(ctx); }
                };
            }
            else if (element is TextField textEl)
            {
                textEl.OnValueChange = (newVal, oldVal) =>
                {
                    if (!TryBeginNotify(ctx)) return;
                    try { WriteFieldValue(field, ctx, onValueChangedAttr, newVal, oldVal, false); }
                    finally { EndNotify(ctx); }
                };
                textEl.OnCommit = (newVal) =>
                {
                    CheckConditionAndRebuild(field.Name, ctx);
                };
            }
            else if (element is ResizableTextArea textAreaEl)
            {
                textAreaEl.OnValueChanged = (newVal, oldVal) =>
                {
                    if (!TryBeginNotify(ctx)) return;
                    try { WriteFieldValue(field, ctx, onValueChangedAttr, newVal, oldVal, false); }
                    finally { EndNotify(ctx); }
                };
                textAreaEl.OnCommit = (_) => CheckConditionAndRebuild(field.Name, ctx);
            }
            else if (element is Toggle toggleEl)
            {
                toggleEl.OnValueChanged = (newVal) =>
                {
                    if (!TryBeginNotify(ctx)) return;
                    try
                    {
                        var oldVal = field.GetValue(ctx.Target);
                        WriteFieldValue(field, ctx, onValueChangedAttr, newVal, oldVal, true);
                    }
                    finally { EndNotify(ctx); }
                };
            }
            else if (element is EnumField enumEl)
            {
                enumEl.OnValueChanged = (newVal) =>
                {
                    if (!TryBeginNotify(ctx)) return;
                    try
                    {
                        var oldVal = field.GetValue(ctx.Target);
                        var writeValue = fieldType.IsEnum ? Enum.ToObject(fieldType, newVal) : (object)newVal;
                        WriteFieldValue(field, ctx, onValueChangedAttr, writeValue, oldVal, true);
                    }
                    finally { EndNotify(ctx); }
                };
            }
            else if (element is UniDecl.Runtime.Widgets.Dropdown dropdownEl)
            {
                dropdownEl.OnSelectionChanged = (newIndex) =>
                {
                    if (!TryBeginNotify(ctx)) return;
                    try
                    {
                        var oldVal = field.GetValue(ctx.Target);
                        if (fieldType == typeof(string))
                        {
                            var newValue = dropdownEl.Choices != null
                                && newIndex >= 0
                                && newIndex < dropdownEl.Choices.Length
                                ? dropdownEl.Choices[newIndex]
                                : string.Empty;
                            WriteFieldValue(field, ctx, onValueChangedAttr, newValue, oldVal, true);
                        }
                        else if (fieldType == typeof(int))
                        {
                            WriteFieldValue(field, ctx, onValueChangedAttr, newIndex, oldVal, true);
                        }
                    }
                    finally { EndNotify(ctx); }
                };
            }
            else if (element is ColorField colorEl)
            {
                colorEl.OnValueChanged = (newVal) => WriteSingleValueChange(field, ctx, onValueChangedAttr, newVal, true);
            }
            else if (element is Vector2Field vector2El)
            {
                vector2El.OnValueChanged = (newVal) => WriteSingleValueChange(field, ctx, onValueChangedAttr, newVal, true);
            }
            else if (element is Vector3Field vector3El)
            {
                vector3El.OnValueChanged = (newVal) => WriteSingleValueChange(field, ctx, onValueChangedAttr, newVal, true);
            }
            else if (element is Vector4Field vector4El)
            {
                vector4El.OnValueChanged = (newVal) => WriteSingleValueChange(field, ctx, onValueChangedAttr, newVal, true);
            }
            else if (element is Vector2IntField vector2IntEl)
            {
                vector2IntEl.OnValueChanged = (newVal) => WriteSingleValueChange(field, ctx, onValueChangedAttr, newVal, true);
            }
            else if (element is Vector3IntField vector3IntEl)
            {
                vector3IntEl.OnValueChanged = (newVal) => WriteSingleValueChange(field, ctx, onValueChangedAttr, newVal, true);
            }
            else if (element is RectField rectEl)
            {
                rectEl.OnValueChanged = (newVal) => WriteSingleValueChange(field, ctx, onValueChangedAttr, newVal, true);
            }
            else if (element is RectIntField rectIntEl)
            {
                rectIntEl.OnValueChanged = (newVal) => WriteSingleValueChange(field, ctx, onValueChangedAttr, newVal, true);
            }
            else if (element is BoundsField boundsEl)
            {
                boundsEl.OnValueChanged = (newVal) => WriteSingleValueChange(field, ctx, onValueChangedAttr, newVal, true);
            }
            else if (element is BoundsIntField boundsIntEl)
            {
                boundsIntEl.OnValueChanged = (newVal) => WriteSingleValueChange(field, ctx, onValueChangedAttr, newVal, true);
            }
            else if (element is CurveField curveEl)
            {
                curveEl.OnValueChanged = (newVal) => WriteSingleValueChange(field, ctx, onValueChangedAttr, newVal, true);
            }
            else if (element is GradientField gradientEl)
            {
                gradientEl.OnValueChanged = (newVal) => WriteSingleValueChange(field, ctx, onValueChangedAttr, newVal, true);
            }
            else if (element is LayerField layerEl)
            {
                layerEl.OnValueChanged = (newVal) =>
                {
                    if (!TryBeginNotify(ctx)) return;
                    try
                    {
                        var oldVal = field.GetValue(ctx.Target);
                        var writeValue = fieldType == typeof(LayerMask) ? (object)(LayerMask)newVal : newVal;
                        WriteFieldValue(field, ctx, onValueChangedAttr, writeValue, oldVal, true);
                    }
                    finally { EndNotify(ctx); }
                };
            }
            else if (element is ObjectField objectEl)
            {
                objectEl.OnValueChanged = (newVal) => WriteSingleValueChange(field, ctx, onValueChangedAttr, newVal, true);
            }
            else if (element is InspectorElement nestedEl)
            {
                // 嵌套 InspectorElement 不需要额外绑定——内部自行处理
            }
        }

        private static void WriteSingleValueChange<T>(FieldInfo field, BuildContext ctx,
            OnValueChangedAttribute onValueChangedAttr, T newValue, bool rebuild)
        {
            if (!TryBeginNotify(ctx)) return;
            try
            {
                var oldVal = field.GetValue(ctx.Target);
                WriteFieldValue(field, ctx, onValueChangedAttr, newValue, oldVal, rebuild);
            }
            finally { EndNotify(ctx); }
        }

        private static bool TryBeginNotify(BuildContext ctx)
        {
            if (ctx._isNotifying) return false;
            ctx._isNotifying = true;
            return true;
        }

        private static void EndNotify(BuildContext ctx)
        {
            ctx._isNotifying = false;
        }

        private static void WriteFieldValue(FieldInfo field, BuildContext ctx,
            OnValueChangedAttribute onValueChangedAttr, object newValue, object oldValue, bool rebuild)
        {
            // 新版 Snapshot 框架接入：Register（幂等）+ Record + CommitPending
            // 一个字段 = 一个 Unity 撤销点
            if (ctx.SnapshotManager != null)
            {
                var key = GetFieldKey(ctx.Target, field);
                // setter 闭包捕获 ctx.Target 和 field：接收新值恢复，返回被覆盖的旧值
                ctx.SnapshotManager.Register<object>(key, v =>
                {
                    var old = field.GetValue(ctx.Target);
                    field.SetValue(ctx.Target, v);
                    return old;
                });
                ctx.SnapshotManager.Record(oldValue, key);
            }

            field.SetValue(ctx.Target, newValue);
            ctx.SnapshotManager?.CommitPending();

            InvokeOnValueChanged(onValueChangedAttr, ctx, oldValue, newValue);

            if (rebuild)
                CheckConditionAndRebuild(field.Name, ctx);
        }

        /// <summary>
        /// 为 (target, field) 生成稳定的 snapshot key。
        /// 用 target 实例哈希 + 字段名，确保嵌套 InspectorElement 共享同一 SnapshotManager 时仍能隔离各 target 的同名字段。
        /// </summary>
        private static string GetFieldKey(object target, FieldInfo field)
        {
            return $"{System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(target):X}:{field.DeclaringType.Name}.{field.Name}";
        }

        /// <summary>
        /// 检查条件属性（ShowIf/HideIf/EnableIf）
        /// </summary>
        private static bool CheckCondition(InspectorAttribute[] attrs, object target)
        {
            foreach (var attr in attrs)
            {
                if (attr is ShowIfAttribute showIf)
                {
                    var val = FieldBinder.ResolveConditionValue(showIf.Member, target);
                    if (!IsConditionTrue(val, showIf.Value)) return false;
                }
                else if (attr is HideIfAttribute hideIf)
                {
                    var val = FieldBinder.ResolveConditionValue(hideIf.Member, target);
                    if (IsConditionTrue(val, hideIf.Value)) return false;
                }
            }
            return true;
        }

        private static bool IsConditionTrue(object memberValue, object expectedValue)
        {
            if (expectedValue == null)
            {
                // 无期望值时，truthy 检查
                if (memberValue is bool b) return b;
                return memberValue != null;
            }

            if (memberValue == null) return false;
            return expectedValue.Equals(memberValue);
        }

        /// <summary>
        /// 检查当前字段是否被其他字段的条件依赖引用，如果是则触发 Rebuild
        /// </summary>
        private static void CheckConditionAndRebuild(string fieldName, BuildContext ctx)
        {
            if (ctx.Meta?.ConditionDependencies == null) return;

            if (ctx.Meta.ConditionDependencies.ContainsKey(fieldName))
            {
                ctx.OnRebuildNeeded?.Invoke();
            }
        }

        /// <summary>
        /// 调用 OnValueChanged 回调
        /// </summary>
        private static void InvokeOnValueChanged(OnValueChangedAttribute attr, BuildContext ctx, object oldVal, object newVal)
        {
            if (attr == null || ctx.Renderer == null) return;

            var method = FieldBinder.FindMethod(ctx.Renderer.GetType(), attr.Method, ctx.Target?.GetType());
            if (method == null) return;

            var parameters = method.GetParameters();
            if (parameters.Length == 0)
                method.Invoke(ctx.Renderer, null);
            else if (parameters.Length == 3)
                method.Invoke(ctx.Renderer, new[] { oldVal, newVal, ctx.Target });
            else if (parameters.Length == 2)
                method.Invoke(ctx.Renderer, new[] { oldVal, newVal });
        }

        private static T GetAttr<T>(InspectorAttribute[] attrs) where T : InspectorAttribute
        {
            for (int i = 0; i < attrs.Length; i++)
            {
                if (attrs[i] is T t) return t;
            }
            return null;
        }
    }
}
