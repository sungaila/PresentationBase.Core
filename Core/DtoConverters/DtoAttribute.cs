using System;
using System.Linq;

namespace PresentationBase.DtoConverters
{
	/// <summary>
	/// Links a view model to a data transfer object.
	/// This enables conversion with <see cref="DtoConverterExtensions.ToDto{TModel}(ViewModel)"/> and <see cref="DtoConverterExtensions.ToViewModel{TViewModel}(object)"/>./>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class DtoAttribute
		: Attribute
	{
		/// <summary>
		/// The data transfer object type.
		/// </summary>
		public Type Type { get; private set; }

		/// <summary>
		/// Creates a new <see cref="DtoAttribute"/> instance.
		/// </summary>
		/// <param name="type">The data transfer object type.</param>
		public DtoAttribute(Type type)
		{
			Type = type ?? throw new ArgumentNullException(nameof(type));
		}

		/// <summary>
		/// Returns a <see cref="DtoAttribute"/> if found for a given view model type.
		/// </summary>
		/// <param name="viewModelType">The view model type.</param>
		public static DtoAttribute? GetDtoAttribute(Type viewModelType)
		{
			if (viewModelType == null)
				throw new ArgumentNullException(nameof(viewModelType));

			return viewModelType.GetCustomAttributes(true).OfType<DtoAttribute>().FirstOrDefault();
		}
	}
}
