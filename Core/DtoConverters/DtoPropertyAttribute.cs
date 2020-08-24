using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace PresentationBase.DtoConverters
{
	/// <summary>
	/// Links a view model property to a data transfer object property.
	/// This enables conversion with <see cref="DtoConverterExtensions.ToDto{TModel}(ViewModel)"/> and <see cref="DtoConverterExtensions.ToViewModel{TViewModel}(object)"/>./>
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
	public class DtoPropertyAttribute
		: Attribute
	{
		/// <summary>
		/// The data transfer object property name.
		/// </summary>
		public string PropertyName { get; private set; }

		/// <summary>
		/// The data transfer object type.
		/// </summary>
		public Type? Type { get; private set; }

		/// <summary>
		/// Creates a new <see cref="DtoPropertyAttribute"/> instance.
		/// </summary>
		/// <param name="propertyName">The data transfer object property name.</param>
		/// <param name="type">Specifies the dto type the <paramref name="propertyName"/> refers to.</param>
		public DtoPropertyAttribute([CallerMemberName]string? propertyName = null, Type? type = null)
		{
			if (string.IsNullOrEmpty(propertyName))
				throw new ArgumentNullException(nameof(propertyName));

			PropertyName = propertyName!;
			Type = type;
		}

		/// <summary>
		/// Returns a <see cref="DtoPropertyAttribute"/> if found for a given <see cref="PropertyInfo"/>.
		/// </summary>
		/// <param name="viewModelPropertyInfo">The property info.</param>
		[Obsolete("A view model property can be linked to multiple data transfer object properties. Please consider to call " + nameof(GetDtoPropertyAttributes) + " instead.")]
		public static DtoPropertyAttribute? GetDtoPropertyAttribute(PropertyInfo viewModelPropertyInfo)
		{
			if (viewModelPropertyInfo == null)
				throw new ArgumentNullException(nameof(viewModelPropertyInfo));

			return viewModelPropertyInfo.GetCustomAttribute<DtoPropertyAttribute>(true);
		}

		/// <summary>
		/// Returns all <see cref="DtoPropertyAttribute"/>s found for a given <see cref="PropertyInfo"/>.
		/// </summary>
		/// <param name="viewModelPropertyInfo">The property info.</param>
		public static IEnumerable<DtoPropertyAttribute> GetDtoPropertyAttributes(PropertyInfo viewModelPropertyInfo)
		{
			if (viewModelPropertyInfo == null)
				throw new ArgumentNullException(nameof(viewModelPropertyInfo));

            foreach (var attr in viewModelPropertyInfo.GetCustomAttributes<DtoPropertyAttribute>(true))
            {
				yield return attr;
            }
		}
	}
}
