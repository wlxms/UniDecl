using UnityEngine;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class InspectorElement : Element
    {
        public UnityEngine.Object Target { get; set; }

        public override IElement Render() => null;

        public InspectorElement(UnityEngine.Object target)
        {
            Target = target;
        }
    }
}
