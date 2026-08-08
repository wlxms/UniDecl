using System;
using UniDecl.BuiltIn.Runtime.Core;

namespace UniDecl.BuiltIn.Runtime.Widgets
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
