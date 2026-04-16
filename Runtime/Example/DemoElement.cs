using UniDecl.Runtime.Contexts;
using UniDecl.Runtime.Components;
using UniDecl.Runtime.Core;
using UniDecl.Runtime.Widgets;

namespace UniDecl.Runtime.Example
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