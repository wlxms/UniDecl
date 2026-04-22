using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UniDecl.Editor.UIToolKit.Effects
{
    /// <summary>
    /// 叠加层生命周期句柄，管理单个 VE 从显示到回收的完整生命周期。
    /// 只有所有回调执行完毕后才释放回池，保证复用时 VE 是干净状态。
    /// </summary>
    public sealed class OverlayHandle
    {
        private static readonly List<OverlayHandle> _pool = new List<OverlayHandle>();

        public VisualElement Element { get; private set; }
        private long _pendingCallbacks;
        private bool _inUse;
        private bool _dismissed;

        /// <summary>
        /// VE 是否仍处于活跃状态（尚未回收）。
        /// </summary>
        public bool IsActive => _inUse;

        /// <summary>
        /// 创建 VE 并定位到目标上方，所有回调结束后自动回收。
        /// </summary>
        public static OverlayHandle Acquire(VisualElement target, string ussClass, float padding = 3f)
        {
            OverlayHandle handle;
            if (_pool.Count > 0)
            {
                int last = _pool.Count - 1;
                handle = _pool[last];
                _pool.RemoveAt(last);
            }
            else
            {
                handle = new OverlayHandle();
            }

            handle._inUse = true;
            handle._pendingCallbacks = 0;

            if (handle.Element == null)
                handle.Element = new VisualElement();
            else
                handle.Element.ClearClassList();

            handle.Element.AddToClassList(ussClass);
            handle.Element.style.position = Position.Absolute;
            handle.Element.style.opacity = 1f;
            handle.Element.pickingMode = PickingMode.Ignore;

            var ve = handle.Element;
            ve.style.left = target.layout.x - padding;
            ve.style.top = target.layout.y - padding;
            ve.style.width = target.layout.width + padding * 2;
            ve.style.height = target.layout.height + padding * 2;

            target.hierarchy.parent?.Add(ve);

            return handle;
        }

        /// <summary>
        /// 注册延迟回调，回调全部执行完毕后自动回收 VE 到池。
        /// </summary>
        public void ScheduleDelayed(long delayMs, System.Action<OverlayHandle> action)
        {
            _pendingCallbacks++;
            Element.schedule.Execute(() =>
            {
                if (!_inUse) return;
                action?.Invoke(this);
                _pendingCallbacks--;
                TryRelease();
            }).StartingIn(delayMs);
        }

        /// <summary>
        /// 强制关闭视觉效果，但等到所有回调完成后才回池。
        /// </summary>
        public void Dismiss()
        {
            if (!_inUse || _dismissed) return;
            _dismissed = true;
            // 立即移除视觉，但保留 handle 等回调跑完
            Element.RemoveFromHierarchy();
            CleanStyles();
            if (_pendingCallbacks <= 0)
                ReturnToPool();
        }

        private void RemoveAndRelease()
        {
            Element.RemoveFromHierarchy();
            CleanStyles();
            ReturnToPool();
        }

        private void CleanStyles()
        {
            Element.ClearClassList();
            Element.style.opacity = StyleKeyword.Null;
            Element.style.position = StyleKeyword.Null;
            Element.style.left = StyleKeyword.Null;
            Element.style.top = StyleKeyword.Null;
            Element.style.width = StyleKeyword.Null;
            Element.style.height = StyleKeyword.Null;
        }

        private void ReturnToPool()
        {
            _inUse = false;
            _dismissed = false;
            _pool.Add(this);
        }

        private void TryRelease()
        {
            if (_pendingCallbacks <= 0 && _inUse)
            {
                if (_dismissed)
                    ReturnToPool();
                else
                    RemoveAndRelease();
            }
        }
    }

    /// <summary>
    /// 通用叠加效果管理器。提供 Ping（导航聚焦高亮）等便捷方法。
    /// </summary>
    public static class OverlayEffectManager
    {
        /// <summary>
        /// 在目标元素上方显示 Ping 光圈效果（金色圆角边框 + 淡出），结束后自动回收。
        /// </summary>
        public static OverlayHandle Ping(VisualElement target, string ussClass = "ud-nav-ping-overlay",
            float padding = 3f, long highlightMs = 1200, long fadeMs = 700)
        {
            if (target == null) return null;

            var handle = OverlayHandle.Acquire(target, ussClass, padding);

            handle.ScheduleDelayed(highlightMs, h => h.Element.style.opacity = 0f);
            handle.ScheduleDelayed(highlightMs + fadeMs, h => { });

            return handle;
        }

        /// <summary>
        /// 在目标元素上方显示自定义叠加效果。
        /// 调用方通过返回的 handle 管理生命周期。
        /// </summary>
        public static OverlayHandle Show(VisualElement target, string ussClass, float padding = 3f)
        {
            if (target == null) return null;
            return OverlayHandle.Acquire(target, ussClass, padding);
        }
    }
}
