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

        protected override void ScheduleFlush()
        {
            EditorApplication.delayCall += FlushPendingRebuilds;
        }

        protected override void OnRenderResultChanged(IElement element, VisualElement oldVE, VisualElement newVE)
        {
            if (oldVE == null || newVE == null) return;
            if (ReferenceEquals(oldVE, newVE)) return;

            var parentVE = oldVE.parent;
            if (parentVE == null) return;

            int index = parentVE.IndexOf(oldVE);
            parentVE.Remove(oldVE);
            if (index >= 0 && index < parentVE.childCount)
                parentVE.Insert(index, newVE);
            else
                parentVE.Add(newVE);
        }
    }
}
