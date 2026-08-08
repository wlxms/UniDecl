using UniDecl.BuiltIn.Runtime.Core;

namespace UniDecl.BuiltIn.Runtime.Widgets
{
    public class PropertyField : Element
    {
        public string BindingPath { get; set; }
        public string Label { get; set; }

        public override IElement Render() => null;

        public PropertyField(string bindingPath, string label = null)
        {
            BindingPath = bindingPath;
            Label = label;
        }
    }
}
