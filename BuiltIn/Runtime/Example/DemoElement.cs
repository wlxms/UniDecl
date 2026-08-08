using UniDecl.BuiltIn.Runtime.Contexts;
using UniDecl.BuiltIn.Runtime.Components;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.BuiltIn.Runtime.Example
{
    public class DemoElement : Element
    {
        public override IElement Render()
        {
            return new VerticalLayout()
            {
                new DisableContext(true)
                {
                    new ContextConsumer(reader =>
                    {
                        var disabled = reader.Get<DisableContext>();
                        return new Label(disabled != null && disabled.Value ? "Disabled" : "Enabled");
                    })
                },
                new Label("Hello World!"),
            }.With(new InlineStyle("demo-style"));
        }
    }
}