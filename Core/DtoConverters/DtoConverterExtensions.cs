using System;
using System.Linq;

namespace PresentationBase.DtoConverters
{
	/// <summary>
	/// Contains extension methods for converstion between view models and data transfer objects.
	/// </summary>
	public static class DtoConverterExtensions
	{
		/// <summary>
		/// Creates a view model from a given data transfer object.
		/// The view model must use <see cref="DtoAttribute"/> and <see cref="DtoPropertyAttribute"/> for the conversion to work.
		/// </summary>
		/// <typeparam name="TViewModel">The view model type to create.</typeparam>
		/// <param name="dto">The data transfer object to convert.</param>
		/// <returns>The converted view model.</returns>
		public static TViewModel ToViewModel<TViewModel>(this object dto)
			where TViewModel : ViewModel
		{
			if (dto == null)
				throw new ArgumentNullException(nameof(dto));

			var dtoAttrs = DtoAttribute.GetDtoAttributes(typeof(TViewModel));
			if (!dtoAttrs.Any())
				throw new InvalidOperationException("The given view model has no DtoAttribute defined.");

			var dtoAttr = dtoAttrs.FirstOrDefault(attr => attr.Type.IsAssignableFrom(dto.GetType()));
			if (dtoAttr == null)
				throw new InvalidCastException($"There is no DtoAttribute assignable to the given type {dto.GetType().Name}.");

			TViewModel result = (TViewModel)Activator.CreateInstance(typeof(TViewModel))!;

			foreach (var propertyInfo in typeof(TViewModel).GetProperties())
			{
				var dtoPropertyAttrs = DtoPropertyAttribute.GetDtoPropertyAttributes(propertyInfo);
				if (!dtoPropertyAttrs.Any())
					continue;

				var dtoPropertyAttr = dtoPropertyAttrs.FirstOrDefault(attr => attr.Type == null || attr.Type.IsAssignableFrom(dto.GetType()));
				if (dtoPropertyAttr == null)
					continue;

				var dtoPropertyInfo = dtoAttr.Type.GetProperty(dtoPropertyAttr.PropertyName);
				if (dtoPropertyInfo == null)
					throw new InvalidOperationException($"There is no property called {dtoPropertyAttr.PropertyName} for the data transfer object type {dtoAttr.Type.Name}.");

				propertyInfo.SetValue(result, dtoPropertyInfo.GetValue(dto, null), null);
			}

			return result;
		}

		/// <summary>
		/// Creates a data transfer object from a given view model.
		/// The view model must use <see cref="DtoAttribute"/> and <see cref="DtoPropertyAttribute"/> for the conversion to work.
		/// </summary>
		/// <typeparam name="TDto">The data transfer object type to create.</typeparam>
		/// <param name="viewModel">The view model to convert.</param>
		/// <returns>The converted data transfer object.</returns>
		public static TDto ToDto<TDto>(this ViewModel viewModel)
			where TDto : class
		{
			if (viewModel == null)
				throw new ArgumentNullException(nameof(viewModel));

			var dtoAttrs = DtoAttribute.GetDtoAttributes(viewModel.GetType());
			if (!dtoAttrs.Any())
				throw new InvalidOperationException("The given view data transfer object has no DtoAttribute defined.");

			var dtoAttr = dtoAttrs.FirstOrDefault(attr => attr.Type.IsAssignableFrom(typeof(TDto)));
			if (dtoAttr == null)
				throw new InvalidCastException($"There is is no DtoAttribute assignable to the desired type {typeof(TDto).Name}.");

			TDto result = (TDto)Activator.CreateInstance(dtoAttr.Type)!;

			foreach (var propertyInfo in viewModel.GetType().GetProperties())
			{
				var dtoPropertyAttrs = DtoPropertyAttribute.GetDtoPropertyAttributes(propertyInfo);
				if (!dtoPropertyAttrs.Any())
					continue;

				var dtoPropertyAttr = dtoPropertyAttrs.FirstOrDefault(attr => attr.Type == null || attr.Type.IsAssignableFrom(typeof(TDto)));
				if (dtoPropertyAttr == null)
					continue;

				var dtoPropertyInfo = dtoAttr.Type.GetProperty(dtoPropertyAttr.PropertyName);
				if (dtoPropertyInfo == null)
					throw new InvalidOperationException($"There is no property called {dtoPropertyAttr.PropertyName} for the data transfer object type {dtoAttr.Type.Name}.");

				dtoPropertyInfo.SetValue(result, propertyInfo.GetValue(viewModel, null), null);
			}

			return result;
		}
	}
}
