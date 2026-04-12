using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class ProgressBar : Element
    {
        public float Value { get; set; }
        public string Label { get; set; }

        public override IElement Render() => null;

        public ProgressBar(float value = 0f, string label = "")
        {
            Value = value;
            Label = label;
        }
    }
}
