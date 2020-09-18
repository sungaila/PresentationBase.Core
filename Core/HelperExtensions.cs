using System;
using System.Collections.Generic;
using System.Linq;

namespace PresentationBase
{
    internal static class HelperExtensions
    {
        public static bool HasGenericTypeArgument(this Type type, Type genericType)
        {
            if (type.GenericTypeArguments.Length == 1 && genericType.IsAssignableFrom(type.GenericTypeArguments[0]))
                return true;

            if (type.BaseType == null)
                return false;

            return HasGenericTypeArgument(type.BaseType, genericType);
        }

        public static bool IsTypeIList(this Type type)
        {
            return type.IsGenericType && type.GetInterfaces().Any(i => i.IsGenericType && (typeof(IList<>).IsAssignableFrom(i.GetGenericTypeDefinition()) || IsTypeIList(i)));
        }

        public static bool IsTypeObservableViewModelCollection(this Type type)
        {
            return type.IsGenericType && typeof(ObservableViewModelCollection<>).IsAssignableFrom(type.GetGenericTypeDefinition());
        }
    }
}
