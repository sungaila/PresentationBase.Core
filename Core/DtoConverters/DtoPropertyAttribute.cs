using System;
using System.Reflection;

namespace PresentationBase.DtoConverters
{
	/// <summary>
	/// Links a view model property to a data transfer object property.
	/// This enables conversion with <see cref="DtoConverterExtensions.ToDto{TModel}(ViewModel)"/> and <see cref="DtoConverterExtensions.ToViewModel{TViewModel}(object)"/>./>
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public class DtoPropertyAttribute
		: Attribute
	{
		/// <summary>
		/// The data transfer object property name.
		/// </summary>
		public string PropertyName { get; private set; }

		/// <summary>
		/// Creates a new <see cref="DtoPropertyAttribute"/> instance.
		/// </summary>
		/// <param name="propertyName">The data transfer object property name.</param>
		public DtoPropertyAttribute(string propertyName)
		{
			if (string.IsNullOrEmpty(propertyName))
				throw new ArgumentNullException(nameof(propertyName));

			PropertyName = propertyName;
		}

		/// <summary>
		/// Returns a <see cref="DtoPropertyAttribute"/> if found for a given <see cref="PropertyInfo"/>.
		/// </summary>
		/// <param name="viewModelPropertyInfo">The property info.</param>
		public static DtoPropertyAttribute? GetDtoPropertyAttribute(PropertyInfo viewModelPropertyInfo)
		{
			if (viewModelPropertyInfo == null)
				throw new ArgumentNullException(nameof(viewModelPropertyInfo));

			return viewModelPropertyInfo.GetCustomAttribute<DtoPropertyAttribute>(true);
		}
	}
}
