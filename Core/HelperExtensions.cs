using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
