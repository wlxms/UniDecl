using System;
using System.Collections.Generic;

namespace UniDecl.Runtime.Core
{
    public static class ElementDomExpanderRegistry
    {
        private static readonly Dictionary<Type, Func<IElement, IElementRenderHostBase, IElement>> _expanders = new();

        public static void Register<T>(Func<T, IElementRenderHostBase, IElement> expander) where T : class, IElement
        {
            if (expander == null)
                throw new ArgumentNullException(nameof(expander));

            _expanders[typeof(T)] = (element, host) => expander((T)element, host);
        }

        public static void Unregister<T>() where T : class, IElement
        {
            _expanders.Remove(typeof(T));
        }

        public static bool TryExpand(IElement element, IElementRenderHostBase host, out IElement expanded)
        {
            expanded = null;
            if (element == null) return false;

            var elementType = element.GetType();
            if (!_expanders.TryGetValue(elementType, out var expander) && elementType.IsGenericType)
            {
                _expanders.TryGetValue(elementType.GetGenericTypeDefinition(), out expander);
            }

            if (expander == null) return false;

            expanded = expander(element, host);
            return expanded != null && !ReferenceEquals(expanded, element);
        }
    }
}