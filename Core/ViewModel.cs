using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PresentationBase
{
	/// <summary>
	/// The base implementation of every view model.
	/// </summary>
	public abstract class ViewModel
		: INotifyPropertyChanged, INotifyDataErrorInfo
	{
		/// <summary>
		/// Creates a new <see cref="ViewModel"/> instance.
		/// </summary>
		public ViewModel()
		{
			// add all matching commands found with reflection
			AddCommands(KnownCommands.Where(cmd => cmd.GetType().HasGenericTypeArgument(GetType())).ToArray());
		}

		/// <summary>
		/// Implementation of <see cref="INotifyPropertyChanged.PropertyChanged"/>.
		/// Is used to support bindings between views and view model properties.
		/// </summary>
		public event PropertyChangedEventHandler? PropertyChanged;

		/// <summary>
		/// Implementation of <see cref="INotifyDataErrorInfo.ErrorsChanged"/>.
		/// Is used to validate bound properties (or the view model itself) for bindings.
		/// </summary>
		public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event for all properties.
		/// </summary>
		protected void RaiseAllPropertiesChanged()
		{
			// setting PropertyChangedEventArgs.PropertyName to null or string.Empty indicates that all properties changed
			// see: https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged.propertychanged?redirectedfrom=MSDN&view=netframework-4.8#remarks
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
		}

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event for the given property name.
		/// </summary>
		/// <param name="propertyName">The name of the property which has been changed. When omitted the property name will be the member name of the caller (which it is when called from the view model property setter).</param>
		protected void RaisePropertyChanged([CallerMemberName]string? propertyName = null)
		{
			if (string.IsNullOrEmpty(propertyName))
			{
				Debug.Fail($"{nameof(ViewModel)}.{nameof(ViewModel.RaisePropertyChanged)} has been called with a null or empty {nameof(propertyName)}.");
				return;
			}

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Sets the property value, ensures bindings are updated and validates (if <paramref name="propertyValidation"/> is set).<para/>
		/// Updates will be skipped (except for validation) if the <paramref name="newValue"/> equals the previous property value.
		/// </summary>
		/// <typeparam name="T">The type of the changed property.</typeparam>
		/// <param name="propertyField">The property field which contains the old value.</param>
		/// <param name="newValue">The new value to set.</param>
		/// <param name="propertyValidation">An optional function used for validation of the changed property. It must return a collection of error messages.</param>
		/// <param name="propertyName">The name of the property which has been changed. When omitted the property name will be the member name of the caller (which it is when called from the view model property setter).</param>
		/// <returns>Returns <c>true</c> if the new value was set.</returns>
		protected bool SetProperty<T>(ref T propertyField, T newValue, Func<T, IEnumerable<string>>? propertyValidation = null, [CallerMemberName]string? propertyName = null)
		{
			if (string.IsNullOrEmpty(propertyName))
			{
				Debug.Fail($"{nameof(ViewModel)}.{nameof(SetProperty)} has been called with a null or empty {nameof(propertyName)}.");
				return false;
			}

			// stop when the new and old value equal
			if (EqualityComparer<T>.Default.Equals(propertyField, newValue))
				return false;

			// if the value was a view model then make sure to update its ParentViewModel
			if (propertyName != nameof(ParentViewModel) && typeof(ViewModel).IsAssignableFrom(typeof(T)) && newValue == null)
				(propertyField as ViewModel)!.ParentViewModel = null;

			// otherwise set the new value for the property
			propertyField = newValue;

			// if the value was a view model then make sure to update its ParentViewModel
			if (propertyName != nameof(ParentViewModel) && typeof(ViewModel).IsAssignableFrom(typeof(T)) && newValue != null)
				(newValue as ViewModel)!.ParentViewModel = this;

			// validate the new value if needed
			AddPropertyErrors(propertyName!, propertyValidation?.Invoke(newValue));

			// inform bindings about the changed property
			RaisePropertyChanged(propertyName);

			// set the IsDirty flag to true (unless it is forbidden for this property name)
			if (!AlwaysIgnoredDirtyProperties.Contains(propertyName) && IgnoredDirtyProperties != null && !IgnoredDirtyProperties.Contains(propertyName))
			{
				IsDirty = true;

				// bubble the IsDirty flag to any ParentViewModel found
				if (ParentViewModel != null)
					ParentViewModel.IsDirty = true;
			}

			return true;
		}

		private WeakReference<ViewModel>? _parentViewModel = null;

		/// <summary>
		/// The logical parent of this view model.
		/// </summary>
		/// <remarks>
		/// Please note that circular references are not supported.
		/// This is a weak reference and can be <see langword="null"/> if the parent has been garbage collected.
		/// </remarks>
		public ViewModel? ParentViewModel
		{
			get
			{
				if (_parentViewModel == null || !_parentViewModel.TryGetTarget(out ViewModel? target))
					return null;

				return target;
			}
			set
			{
				ViewModel? oldValue = ParentViewModel;
				if (SetProperty(ref oldValue, value))
                {
					_parentViewModel = value != null
						? new WeakReference<ViewModel>(value)
						: null;
				}
			}
		}

		/// <summary>
		/// The top most parent of this view model.
		/// </summary>
		public ViewModel? RootViewModel
		{
			get
			{
				ViewModel? parent = ParentViewModel;

				while (parent != null)
				{
					if (parent.ParentViewModel == null)
						return parent;

					parent = parent.ParentViewModel;
				}

				return null;
			}
		}

		/// <summary>
		/// Adds commands to this view model.
		/// This ensures that <see cref="ICommand.CanExecute(object)"/> is called whenever a property was changed.
		/// </summary>
		/// <param name="commands">The commands to add.</param>
		private void AddCommands(params IViewModelCommand[] commands)
		{
			foreach (var command in commands)
			{
				if (command == null)
					continue;

				Type key = command.GetType();

				if (key == null || Commands.ContainsKey(key))
					continue;

				Commands.Add(key, command);
				PropertyChanged += (sender, e) => command.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		/// Removes existing commands for this view model.
		/// </summary>
		/// <param name="commands">The commands to remove.</param>
		private void RemoveCommands(params IViewModelCommand[] commands)
		{
			foreach (var command in commands)
			{
				if (command == null)
					continue;

				Type key = command.GetType();

				if (key == null || !Commands.ContainsKey(key))
					continue;

				Commands.Remove(key);
				PropertyChanged -= (sender, e) => command.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		/// A dictionary filled with commands for this view model. The key is the <see cref="Type"/> of the command.
		/// </summary>
		public Dictionary<Type, IViewModelCommand> Commands { get; private set; } = new Dictionary<Type, IViewModelCommand>();

		/// <summary>
		/// A multi purpose <see cref="object"/> tag for this view model.
		/// Use it for e.g. identifying this view model.
		/// </summary>
		public object? Tag { get; set; }

		private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

		/// <summary>
		/// If any property or the view model itself has failed validation.<para/>
		/// </summary>
		public bool HasErrors => _errors.Any();

		/// <summary>
		/// If all properties and the view model itself succeeded validation.<para/>
		/// </summary>
		/// <remarks>Overwrite this if e.g. children must be valid aswell.</remarks>
		public virtual bool IsValid => !HasErrors;

		/// <summary>
		/// Returns all errors for a given <paramref name="propertyName"/> or for the entire view model.
		/// </summary>
		/// <param name="propertyName">The property name. Set to <see langword="null"/> to get all view model errors.</param>
		public IEnumerable? GetErrors(string propertyName)
		{
			if (propertyName == null)
				propertyName = string.Empty;

			return _errors.ContainsKey(propertyName)
				? _errors[propertyName]
				: null;
		}

		/// <summary>
		/// Clears both view model and all property errors.
		/// </summary>
		protected void ClearAllErrors()
		{
			ClearViewModelErrors();

			foreach (var error in _errors)
				ClearPropertyErrors(error.Key);
		}

		/// <summary>
		/// Clears all view model errors.
		/// </summary>
		protected void ClearViewModelErrors()
		{
			if (!_errors.ContainsKey(string.Empty))
				return;

			_errors.Remove(string.Empty);
			RaiseViewModelErrorsChanged();
		}

		/// <summary>
		/// Clears all property errors for the given <paramref name="propertyName"/>.
		/// </summary>
		/// <param name="propertyName">The property name.</param>
		protected void ClearPropertyErrors(string propertyName)
		{
			if (string.IsNullOrEmpty(propertyName) || !_errors.ContainsKey(propertyName))
				return;

			_errors.Remove(propertyName);
			RaisePropertyErrorsChanged(propertyName);
		}

		private void AddError(string propertyName, string errorMessage)
		{
			if (!_errors.ContainsKey(propertyName))
			{
				_errors[propertyName] = new List<string>();
			}

			if (!_errors[propertyName].Contains(errorMessage))
			{
				_errors[propertyName].Add(errorMessage);
				RaisePropertyErrorsChanged(propertyName);
			}
		}

		/// <summary>
		/// Adds a collection of error messages which invalidate the entire view model.
		/// </summary>
		/// <param name="errorMessages">The collection of error messages.</param>
		/// <param name="clearPreviousErrors">If the previous errors should be cleared before adding the new ones.</param>
		protected void AddViewModelErrors(IEnumerable<string>? errorMessages, bool clearPreviousErrors = true)
		{
			if (clearPreviousErrors)
				ClearViewModelErrors();

			if (errorMessages == null)
				return;

			foreach (string errorMessage in errorMessages)
			{
				AddError(string.Empty, errorMessage);
			}
		}

		/// <summary>
		/// Adds a collection of error messages for the given <paramref name="propertyName"/>.
		/// </summary>
		/// <param name="propertyName">The property name.</param>
		/// <param name="errorMessages">The collection of error messages.</param>
		/// <param name="clearPreviousErrors">If the previous errors should be cleared before adding the new ones.</param>
		protected void AddPropertyErrors(string propertyName, IEnumerable<string>? errorMessages, bool clearPreviousErrors = true)
		{
			if (string.IsNullOrEmpty(propertyName))
				return;

			if (clearPreviousErrors)
				ClearPropertyErrors(propertyName);

			if (errorMessages == null)
				return;

			foreach (string errorMessage in errorMessages)
				AddError(propertyName, errorMessage);
		}

		/// <summary>
		/// Raises the <see cref="ErrorsChanged"/> event for view model errors.
		/// </summary>
		protected void RaiseViewModelErrorsChanged()
		{
			ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(null));
			RaisePropertyChanged(nameof(HasErrors));
			RaisePropertyChanged(nameof(IsValid));
		}

		/// <summary>
		/// Raises the <see cref="ErrorsChanged"/> event for the given <paramref name="propertyName"/>.
		/// </summary>
		/// <param name="propertyName">The property name.</param>
		protected void RaisePropertyErrorsChanged(string propertyName)
		{
			if (string.IsNullOrEmpty(propertyName))
			{
				Debug.Fail($"{nameof(ViewModel)}.{nameof(ViewModel.RaisePropertyErrorsChanged)} has been called with a null or empty {nameof(propertyName)}.");
				return;
			}

			ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
			RaisePropertyChanged(nameof(HasErrors));
			RaisePropertyChanged(nameof(IsValid));
		}

		/// <summary>
		/// A collection of properties which are always ignored (and cannot be overwritten like <see cref="IgnoredDirtyProperties"/>).
		/// </summary>
		/// <remarks>Overwrite this to ignore properties.</remarks>
		private IEnumerable<string> AlwaysIgnoredDirtyProperties => new List<string> { nameof(IsDirty), nameof(IsRefreshing), nameof(Tag) };

		/// <summary>
		/// A collection of property names which do not set the <see cref="IsDirty"/> flag when changed.
		/// </summary>
		protected virtual IEnumerable<string> IgnoredDirtyProperties { get => new List<string>(); }

		private bool _isDirty;

		/// <summary>
		/// Indicates that there are changes made to this view model since its creation.
		/// </summary>
		public bool IsDirty
		{
			get => _isDirty;
			set => SetProperty(ref _isDirty, value);
		}

		private bool _isRefreshing;

		/// <summary>
		/// Indicates that this view model is changing and others should avoid interfering.
		/// </summary>
		public bool IsRefreshing
		{
			get => _isRefreshing;
			set => SetProperty(ref _isRefreshing, value);
		}

		/// <summary>
		/// A list containing all known commands found with reflection.
		/// </summary>
		private static readonly List<IViewModelCommand> KnownCommands = new List<IViewModelCommand>();

		static ViewModel()
		{
			ReInitializeCommands();
		}

		/// <summary>
		/// Reinitializes the <see cref="KnownCommands"/> list for command and view model interaction.
		/// </summary>
		public static void ReInitializeCommands()
		{
			ReInitializeCommands(
				AppDomain.CurrentDomain
					.GetAssemblies()
					.SelectMany(a => a.GetTypes())
					.Where(type => !type.IsAbstract && typeof(IViewModelCommand).IsAssignableFrom(type) && type.HasGenericTypeArgument(typeof(ViewModel)))
					);
		}

		/// <summary>
		/// Tries to instantiate the given types and adds them to <see cref="KnownCommands"/>.
		/// The <see cref="KnownCommands"/> list will be cleared first.
		/// </summary>
		/// <param name="commandTypes">The command types to instantiate and add.</param>
		private static void ReInitializeCommands(IEnumerable<Type> commandTypes)
		{
			KnownCommands.Clear();

			foreach (var commandType in commandTypes)
			{
				if (commandType.IsAbstract)
					throw new ArgumentException($"{commandType.FullName} cannot be abstract.", nameof(commandTypes));

				if (!typeof(IViewModelCommand).IsAssignableFrom(commandType))
					throw new ArgumentException($"{commandType.FullName} must implement {nameof(IViewModelCommand)}.", nameof(commandTypes));

				if (!commandType.HasGenericTypeArgument(typeof(ViewModel)))
					throw new ArgumentException($"{commandType.FullName} must have {nameof(ViewModel)} as its only generic type argument.", nameof(commandTypes));

				try
				{
					KnownCommands.Add((IViewModelCommand)Activator.CreateInstance(commandType)!);
				}
				catch (Exception ex)
				{
					Debug.Fail($"Failed to add the command {commandType.FullName}: {ex}");
				}
			}
		}
	}
}
