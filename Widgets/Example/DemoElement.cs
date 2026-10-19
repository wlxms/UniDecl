using UniDecl.Core.Contexts;
using UniDecl.Core.Components;
using UniDecl.Core;
using UniDecl.Widgets;

namespace UniDecl.Widgets.Example
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