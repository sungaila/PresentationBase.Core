using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
			return ToViewModel<TViewModel>(dto, new List<object>());
        }

		private static MethodInfo _internalToViewModelInfo = typeof(DtoConverterExtensions).GetMethod(nameof(ToViewModel), BindingFlags.Static | BindingFlags.NonPublic);

		private static TViewModel ToViewModel<TViewModel>(object dto, List<object> visitedDtos)
			where TViewModel : ViewModel
		{
			if (dto == null)
				throw new ArgumentNullException(nameof(dto));

			if (visitedDtos.Contains(dto))
				throw new NotSupportedException("Cyclic relationships are not supported!");

			visitedDtos.Add(dto);

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

				object? valueToConvert = dtoPropertyInfo.GetValue(dto, null);
				object? valueToSet;

				if (typeof(ViewModel).IsAssignableFrom(propertyInfo.PropertyType))
                {
					if (valueToConvert == null)
						continue;

					MethodInfo genericToViewModel = _internalToViewModelInfo.MakeGenericMethod(propertyInfo.PropertyType);
					valueToSet = genericToViewModel.Invoke(null, new[] { valueToConvert, visitedDtos });
				}
				else if (IsTypeIList(dtoPropertyInfo.PropertyType) && IsTypeObservableViewModelCollection(propertyInfo.PropertyType))
                {
					if (valueToConvert == null)
						continue;

					var collection = Activator.CreateInstance(propertyInfo.PropertyType, new[] { result });

					MethodInfo genericToViewModel = _internalToViewModelInfo.MakeGenericMethod(propertyInfo.PropertyType.GenericTypeArguments.First());

					foreach (var item in (IEnumerable)valueToConvert)
					{
						((IList)collection).Add(genericToViewModel.Invoke(null, new[] { item, visitedDtos }));
					}

					valueToSet = collection;
				}
				else
                {
					valueToSet = valueToConvert;
				}

				propertyInfo.SetValue(result, valueToSet, null);
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
			return ToDto<TDto>(viewModel, new List<ViewModel>());
        }

		private static MethodInfo _internalToDtoInfo = typeof(DtoConverterExtensions).GetMethod(nameof(ToDto), BindingFlags.Static | BindingFlags.NonPublic);

		private static TDto ToDto<TDto>(ViewModel viewModel, List<ViewModel> visitedViewModels)
			where TDto : class
		{
			if (viewModel == null)
				throw new ArgumentNullException(nameof(viewModel));

			if (visitedViewModels.Contains(viewModel))
				throw new NotSupportedException("Cyclic relationships are not supported!");

			visitedViewModels.Add(viewModel);

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

				object? valueToConvert = propertyInfo.GetValue(viewModel, null);
				object? valueToSet;

				if (typeof(ViewModel).IsAssignableFrom(propertyInfo.PropertyType))
				{
					if (valueToConvert == null)
						continue;

					MethodInfo genericToDto = _internalToDtoInfo.MakeGenericMethod(dtoPropertyInfo.PropertyType);
					valueToSet = genericToDto.Invoke(null, new[] { valueToConvert, visitedViewModels });
				}
				else if (IsTypeIList(dtoPropertyInfo.PropertyType) && IsTypeObservableViewModelCollection(propertyInfo.PropertyType))
				{
					if (valueToConvert == null)
						continue;

					var collection = Activator.CreateInstance(dtoPropertyInfo.PropertyType);

					MethodInfo genericToDto = _internalToDtoInfo.MakeGenericMethod(dtoPropertyInfo.PropertyType.GenericTypeArguments.First());

					foreach (var item in (IEnumerable)valueToConvert)
					{
						((IList)collection).Add(genericToDto.Invoke(null, new[] { item, visitedViewModels }));
					}

					valueToSet = collection;
				}
				else
				{
					valueToSet = valueToConvert;
				}

				dtoPropertyInfo.SetValue(result, valueToSet, null);
			}

			return result;
		}

		private static bool IsTypeIList(Type type)
		{
			return type.IsGenericType && type.GetInterfaces().Any(i => i.IsGenericType && (typeof(IList<>).IsAssignableFrom(i.GetGenericTypeDefinition()) || IsTypeIList(i)));
		}

		private static bool IsTypeObservableViewModelCollection(Type type)
		{
			return type.IsGenericType && typeof(ObservableViewModelCollection<>).IsAssignableFrom(type.GetGenericTypeDefinition());
		}
	}
}
