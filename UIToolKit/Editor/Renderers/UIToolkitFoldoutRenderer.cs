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
        public VisualElement Render(W.Foldout element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is UnityEngine.UIElements.Foldout reused)
            {
                reused.text = element.Text;
                reused.SetValueWithoutNotify(element.Value); // 直接赋值会发 ChangeEvent 触发 snapshot 副作用提交
                return reused;
            }

            var foldout = new UnityEngine.UIElements.Foldout
            {
                text = element.Text,
                value = element.Value,
            };

            // Snapshot 绑定——展开状态可撤销（离散型，ChangeEvent 即提交）
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Value,
                (restore, current, changes) =>
                {
                    foldout.SetValueWithoutNotify((bool)restore);
                    element.Value = (bool)restore;
                });
            foldout.RegisterValueChangedCallback(evt =>
            {
                if (!ReferenceEquals(evt.target, foldout)) return; // 子控件 ChangeEvent 冒泡（如内嵌 Toggle）不处理
                element.Value = evt.newValue;
                binding.BreakMerge(); // 离散点击：每次独立 step
                binding.Commit();
                element.NotifyChanged();
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
