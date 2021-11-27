using System;
using System.Collections.Generic;
using System.Linq;

namespace PresentationBase.DtoConverters
{
    /// <summary>
    /// Links a view model to one or multiple data transfer objects.
    /// This enables conversion with <see cref="DtoConverterExtensions.ToDto{TModel}(ViewModel)"/> and <see cref="DtoConverterExtensions.ToViewModel{TViewModel}(object)"/>./>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class DtoAttribute
        : Attribute
    {
        /// <summary>
        /// The data transfer object type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Creates a new <see cref="DtoAttribute"/> instance.
        /// </summary>
        /// <param name="type">The data transfer object type.</param>
        public DtoAttribute(Type type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        /// <summary>
        /// Returns all <see cref="DtoAttribute"/>s found for a given view model type.
        /// </summary>
        /// <param name="viewModelType">The view model type.</param>
        public static IEnumerable<DtoAttribute> GetDtoAttributes(Type viewModelType)
        {
            if (viewModelType == null)
                throw new ArgumentNullException(nameof(viewModelType));

            foreach (var attr in viewModelType.GetCustomAttributes(true).OfType<DtoAttribute>())
            {
                yield return attr;
            }
        }
    }
}