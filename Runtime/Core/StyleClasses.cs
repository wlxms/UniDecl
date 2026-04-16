namespace UniDecl.Runtime.Core
{
    /// <summary>
    /// A lightweight runtime component that carries CSS class names to be applied
    /// when the element is rendered. Use this to attach semantic style classes from
    /// within a widget's <see cref="IElement.Render"/> method without depending on
    /// editor-only style types.
    /// </summary>
    public sealed class StyleClasses : IElementComponent
    {
        public string[] Names { get; }

        public StyleClasses(params string[] names)
        {
            Names = names ?? System.Array.Empty<string>();
        }
    }
}
