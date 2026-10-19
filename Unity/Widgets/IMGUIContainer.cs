using System;
using UniDecl.Core;

namespace UniDecl.Unity.Widgets
{
    public class IMGUIContainer : Element
    {
        public Action OnGUIHandler { get; set; }

        public override IElement Render() => null;

        public IMGUIContainer(Action onGUIHandler)
        {
            OnGUIHandler = onGUIHandler;
        }
    }
}
