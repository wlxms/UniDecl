using System;
using System.Collections.Generic;
using UniDecl.Runtime.Components;
using UnityEngine;

namespace UniDecl.Runtime.Core
{
    /// <summary>
    /// 事件分发器
    /// 管理事件的订阅与分发，支持类型化的事件系统
    /// Subscribe 接收 IEventListener，通过反射发现所有 IEventListener&lt;T&gt; 接口，一次注册所有事件
    /// </summary>
    public class EventDispatcher : IEventDispatcher
    {
        private readonly Dictionary<Type, List<WeakReference<IEventListener>>> _listeners =
            new Dictionary<Type, List<WeakReference<IEventListener>>>();

        /// <summary>
        /// 缓存类型 → 实现的 IEventListener&lt;T&gt; 事件类型列表
        /// </summary>
        private static readonly Dictionary<Type, List<Type>> _eventTypeCache =
            new Dictionary<Type, List<Type>>();

        /// <summary>
        /// 分发事件给所有订阅了该事件类型的监听者
        /// </summary>
        public void Dispatch<T>(T @event) where T : struct
        {
            var eventType = typeof(T);
            if (!_listeners.TryGetValue(eventType, out var listenerRefs))
                return;

            var deadRefs = new List<WeakReference<IEventListener>>();
            foreach (var weakRef in listenerRefs)
            {
                if (weakRef.TryGetTarget(out var listener))
                {
                    if (listener is IEventListener<T> typedListener)
                        typedListener.OnEvent(@event);
                }
                else
                {
                    deadRefs.Add(weakRef);
                }
            }

            if (deadRefs.Count > 0)
            {
                foreach (var deadRef in deadRefs)
                    listenerRefs.Remove(deadRef);

                if (listenerRefs.Count == 0)
                    _listeners.Remove(eventType);
            }
        }

        /// <summary>
        /// 订阅事件
        /// 通过反射发现 listener 实现的所有 IEventListener&lt;T&gt; 接口，一次注册所有事件类型
        /// </summary>
        public void Subscribe(IEventListener listener)
        {
            if (listener == null) return;

            var eventTypes = GetEventListenerTypes(listener.GetType());
            foreach (var eventType in eventTypes)
            {
                if (!_listeners.TryGetValue(eventType, out var listenerRefs))
                {
                    listenerRefs = new List<WeakReference<IEventListener>>();
                    _listeners[eventType] = listenerRefs;
                }
                listenerRefs.Add(new WeakReference<IEventListener>(listener));
            }
        }

        /// <summary>
        /// 取消订阅事件
        /// 移除 listener 在所有 IEventListener&lt;T&gt; 类型上的注册
        /// </summary>
        public void Unsubscribe(IEventListener listener)
        {
            if (listener == null) return;

            var eventTypes = GetEventListenerTypes(listener.GetType());
            foreach (var eventType in eventTypes)
            {
                if (!_listeners.TryGetValue(eventType, out var listenerRefs))
                    continue;

                for (int i = listenerRefs.Count - 1; i >= 0; i--)
                {
                    if (listenerRefs[i].TryGetTarget(out var target) && ReferenceEquals(target, listener))
                    {
                        listenerRefs.RemoveAt(i);
                        break;
                    }
                }

                if (listenerRefs.Count == 0)
                    _listeners.Remove(eventType);
            }
        }

        /// <summary>
        /// 清理所有订阅
        /// </summary>
        public void Clear()
        {
            _listeners.Clear();
        }

        /// <summary>
        /// 获取类型实现的所有 IEventListener&lt;T&gt; 的事件类型 T
        /// 结果会被缓存以避免重复反射
        /// </summary>
        private static List<Type> GetEventListenerTypes(Type listenerType)
        {
            if (_eventTypeCache.TryGetValue(listenerType, out var cached))
                return cached;

            var types = new List<Type>();
            var current = listenerType;
            while (current != null && current != typeof(object))
            {
                foreach (var iface in current.GetInterfaces())
                {
                    if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEventListener<>))
                        types.Add(iface.GetGenericArguments()[0]);
                }
                current = current.BaseType;
            }

            _eventTypeCache[listenerType] = types;
            return types;
        }
    }

    public class EventDispatcher<TRenderResult> : EventDispatcher
    {
        private readonly Func<IElement, IElementRender<TRenderResult>> _rendererLookup;

        /// <summary>
        /// 通过注入 renderer 查找函数构造，使 DispatchAlongPath 能找到渲染器
        /// </summary>
        public EventDispatcher(Func<IElement, IElementRender<TRenderResult>> rendererLookup)
        {
            _rendererLookup = rendererLookup;
        }

        public EventDispatcher() { }

        public void DispatchAlongPath<T>(T @event, IReadOnlyList<DOMNode<TRenderResult>> path) where T : struct
        {
            foreach (var node in path)
            {
                if (node.Element is IEventListener<T> el)
                    el.OnEvent(@event);
                if (node.Element is Element element)
                    foreach (var comp in element.Components)
                    {
                        if (comp is IEventListener<T> cl)
                            cl.OnEvent(@event);
                        if (comp is OnEvent<T> onEvent)
                            onEvent.Handler?.Invoke(@event);
                    }
                if (_rendererLookup != null && node.Element != null)
                {
                    var renderer = _rendererLookup(node.Element);
                    if (renderer is IRendererEventListener<TRenderResult, T> r)
                        r.OnEvent(@event, node);
                }
            }
        }
    }
}
