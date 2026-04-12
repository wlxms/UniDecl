using System.Collections.Generic;
using UnityEditor;
using VisualElement = UnityEngine.UIElements.VisualElement;
using UniDecl.Runtime.Contexts;
using UniDecl.Runtime.Core;
using UniDecl.Runtime.Widgets;
using UniDecl.Editor.UIToolkit.Renderers;

namespace UniDecl.Editor.UIToolkit
{
    public class UIToolkitRenderManager : ElementRenderHost<VisualElement>
    {
        private readonly Dictionary<IElement, VisualElement> _elementToVE = new();

        protected override void DiscoverRenderers()
        {
            RegisterRenderer<Label>(new UIToolkitLabelRenderer());
            RegisterRenderer<Button>(new UIToolkitButtonRenderer());
            RegisterRenderer<TextField>(new UIToolkitTextFieldRenderer());
            RegisterRenderer<VerticalLayout>(new UIToolkitVerticalLayoutRenderer());
            RegisterRenderer<HorizontalLayout>(new UIToolkitHorizontalLayoutRenderer());
            RegisterRenderer<Panel>(new UIToolkitPanelRenderer());
            RegisterRenderer<DisableContext>(new UIToolkitDisableContextRenderer());
            RegisterRenderer<Toggle>(new UIToolkitToggleRenderer());
            RegisterRenderer<IntegerField>(new UIToolkitIntegerFieldRenderer());
            RegisterRenderer<FloatField>(new UIToolkitFloatFieldRenderer());
            RegisterRenderer<Dropdown>(new UIToolkitDropdownRenderer());
            RegisterRenderer<EnumField>(new UIToolkitEnumFieldRenderer());
            RegisterRenderer<ColorField>(new UIToolkitColorFieldRenderer());
            RegisterRenderer<Slider>(new UIToolkitSliderRenderer());
            RegisterRenderer<MinMaxSlider>(new UIToolkitMinMaxSliderRenderer());
            RegisterRenderer<ScrollView>(new UIToolkitScrollViewRenderer());
            RegisterRenderer<Foldout>(new UIToolkitFoldoutRenderer());
            RegisterRenderer<HelpBox>(new UIToolkitHelpBoxRenderer());
            RegisterRenderer<ProgressBar>(new UIToolkitProgressBarRenderer());
        }

        public VisualElement RenderRoot(IElement rootElement)
        {
            if (rootElement == null) return null;
            BuildDOM(rootElement);
            return RenderElement(rootElement);
        }

        protected override void OnElementRendered(IElement element, VisualElement ve)
        {
            if (element != null && ve != null)
                _elementToVE[element] = ve;
        }

        protected override void ScheduleFlush()
        {
            EditorApplication.delayCall += FlushPendingRebuilds;
        }

        protected override void OnAfterRebuild(IElement element)
        {
            if (element == null) return;

            // 先保存旧 VE 引用（RenderElement 内部的 OnElementRendered 会覆盖 _elementToVE）
            _elementToVE.TryGetValue(element, out var oldVE);

            // 清除被移除节点的缓存映射
            RemoveStaleMappings();

            // 重新渲染目标元素（RenderElement 内部会走 Update 复用路径）
            var newVE = RenderElement(element);
            if (newVE == null) return;

            // 首次渲染，无需替换
            if (oldVE == null) return;

            // VE 复用，无需替换
            if (ReferenceEquals(oldVE, newVE)) return;

            // VE 引用变了，需要在 VE 树中替换
            var parentVE = oldVE.parent;
            if (parentVE == null) return;

            int index = parentVE.IndexOf(oldVE);
            parentVE.Remove(oldVE);
            if (index >= 0 && index < parentVE.childCount)
                parentVE.Insert(index, newVE);
            else
                parentVE.Add(newVE);
        }

        private void RemoveStaleMappings()
        {
            var toRemove = new List<IElement>();
            foreach (var kv in _elementToVE)
            {
                if (DOMTree.GetNode(kv.Key) == null)
                    toRemove.Add(kv.Key);
            }
            foreach (var e in toRemove)
                _elementToVE.Remove(e);
        }
    }
}
