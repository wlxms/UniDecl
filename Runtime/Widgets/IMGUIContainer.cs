using System;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
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
