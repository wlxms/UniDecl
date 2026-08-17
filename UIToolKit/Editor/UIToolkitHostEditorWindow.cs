using UniDecl.BuiltIn.Editor;
using UniDecl.Editor.UIToolKit;
using UniDecl.BuiltIn.Runtime.Core;
using UnityEditor;
using UnityEngine.UIElements;

namespace UniDecl.Editor.UIToolKit
{
    public abstract class UIToolkitHostEditorWindow<TManager> : EditorWindow, IDeclHostWindow
        where TManager : UIToolkitRenderManager, new()
    {
        protected TManager Manager { get; private set; }

        public IElementRenderHostBase GetHost() => Manager;

        protected abstract IElement BuildContent();

        /// <summary>
        /// 创建渲染管理器。子类可重写以注入 SnapshotManager（如 EditorSnapshotManager）。
        /// </summary>
        protected virtual TManager CreateManager() => new TManager();

        protected virtual string HostName => GetType().Name;

        protected virtual void LoadStyles(TManager manager) { }

        public virtual void CreateGUI()
        {
            rootVisualElement.Clear();

            Manager = CreateManager();
            LoadStyles(Manager);

            var root = BuildContent();
            var ve = Manager.RenderRoot(root);
            if (ve != null)
                rootVisualElement.Add(ve);

            EditorHostManager.Instance.Register(HostName, Manager);
        }
    }
}
