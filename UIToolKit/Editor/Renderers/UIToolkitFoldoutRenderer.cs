using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.BuiltIn.Runtime.Navigation;
using W = UniDecl.BuiltIn.Runtime.Widgets;
using UniDecl.Editor.UIToolKit.Style;
using UniDecl.BuiltIn.Runtime.Snapshot;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;
namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitFoldoutRenderer : IElementRenderer<W.Foldout, VisualElement>,
        IRendererEventListener<VisualElement, NavigationEvent>
    {
        private sealed class FoldoutBindingState
        {
            public W.Foldout Element;
            public SnapshotBinding Binding;
            public ElementState State;
        }

        public VisualElement Render(W.Foldout element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is UnityEngine.UIElements.Foldout reused)
            {
                var persistedValue = state?.Value is bool savedValue ? savedValue : element.Value;
                element.Value = persistedValue;
                reused.text = element.Text;
                reused.SetValueWithoutNotify(persistedValue); // 直接赋值会发 ChangeEvent 触发 snapshot 副作用提交
                var existingState = reused.userData as FoldoutBindingState;
                if (existingState != null)
                {
                    existingState.Element = element;
                    existingState.State = state;
                }
                reused.contentContainer.Clear();
                foreach (var child in element.Children)
                {
                    var childElement = manager.RenderElement(child);
                    if (childElement != null)
                        reused.Add(childElement);
                }
                UIToolkitStyleApplier.ApplyElementStyles(element, reused);
                return reused;
            }

            var initialValue = state?.Value is bool restoredValue ? restoredValue : element.Value;
            element.Value = initialValue;
            var foldout = new UnityEngine.UIElements.Foldout
            {
                text = element.Text,
                value = initialValue,
            };

            // Snapshot 绑定——展开状态可撤销（离散型，ChangeEvent 即提交）
            var newState = new FoldoutBindingState { Element = element, State = state };
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => newState.Element.Value,
                (restore, current, changes) =>
                {
                    foldout.SetValueWithoutNotify((bool)restore);
                    newState.Element.Value = (bool)restore;
                    if (newState.State != null)
                        newState.State.Value = (bool)restore;
                });
            newState.Binding = binding;
            foldout.userData = newState;
            foldout.RegisterValueChangedCallback(evt =>
            {
                if (!ReferenceEquals(evt.target, foldout)) return; // 子控件 ChangeEvent 冒泡（如内嵌 Toggle）不处理
                var current = foldout.userData as FoldoutBindingState;
                if (current == null) return;
                current.Element.Value = evt.newValue;
                if (current.State != null)
                    current.State.Value = evt.newValue;
                current.Binding.BreakMerge(); // 离散点击：每次独立 step
                current.Binding.Commit();
                current.Element.NotifyChanged();
            });

            foreach (var child in element.Children)
            {
                var childElement = manager.RenderElement(child);
                if (childElement != null)
                    foldout.Add(childElement);
            }

            UIToolkitStyleApplier.ApplyElementStyles(element, foldout);
            return foldout;
        }

        public void OnEvent(NavigationEvent @event, DOMNode<VisualElement> node, DOMTree<VisualElement> tree)
        {
            if (@event.IsTarget) return;
            var ve = node.RenderResult;
            if (ve is UnityEngine.UIElements.Foldout foldout)
                foldout.value = true;
        }
    }
}
