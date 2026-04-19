using System;
using UnityEngine.UIElements;
using UniDecl.Runtime.Components;

namespace UniDecl.Editor.UIToolKit
{
    /// <summary>
    /// UITK 事件组件接口，供 ElementEventApplier 类型匹配用
    /// </summary>
    public interface IUITKEventComponent : IOnEventComponent
    {
        void RegisterTo(VisualElement ve);
    }

    /// <summary>
    /// 泛型 UITK 事件组件，用于 UI Toolkit 的 EventBase 派生事件类型
    /// 
    /// 用法: element.With(new OnUITKEvent&lt;ClickEvent&gt;(_ => HandleClick()))
    /// </summary>
    public sealed class OnUITKEvent<TEvent> : IUITKEventComponent
        where TEvent : EventBase<TEvent>, new()
    {
        private readonly Action<TEvent> _handler;

        public OnUITKEvent(Action<TEvent> handler) => _handler = handler ?? throw new ArgumentNullException(nameof(handler));

        public void RegisterTo(VisualElement ve)
        {
            ve.RegisterCallback<TEvent>(e => _handler(e));
        }
    }
}
