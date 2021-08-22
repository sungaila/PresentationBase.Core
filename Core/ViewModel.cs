using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PresentationBase
{
    /// <summary>
    /// The base implementation of <see cref="IViewModel"/>.
    /// </summary>
    public abstract class ViewModel
        : Bindable, IViewModel
    {
        /// <summary>
        /// Creates a new <see cref="ViewModel"/> instance.
        /// </summary>
        protected ViewModel()
        {
            // add all matching commands found with reflection
            AddCommands(KnownCommands.Where(cmd => cmd.GetType().HasGenericTypeArgument(GetType())).ToArray());
        }

        /// <inheritdoc/>
        protected override void PreSetProperty<T>(ref T propertyField, T newValue, Func<T, IEnumerable<string>>? propertyValidation = null, [CallerMemberName] string? propertyName = null)
        {
            // if the value was a view model then make sure to update its ParentViewModel
            if (propertyName != nameof(ParentViewModel) && typeof(ViewModel).IsAssignableFrom(typeof(T)) && newValue == null)
                (propertyField as ViewModel)!.ParentViewModel = null;
        }

        /// <inheritdoc/>
        protected override void PostSetProperty<T>(ref T propertyField, T newValue, Func<T, IEnumerable<string>>? propertyValidation = null, [CallerMemberName] string? propertyName = null)
        {
            // if the value was a view model then make sure to update its ParentViewModel
            if (propertyName != nameof(ParentViewModel) && typeof(ViewModel).IsAssignableFrom(typeof(T)) && newValue != null)
                (newValue as ViewModel)!.ParentViewModel = this;

            // set the IsDirty flag to true (unless it is forbidden for this property name)
            if (!AlwaysIgnoredDirtyProperties.Contains(propertyName) && IgnoredDirtyProperties != null && !IgnoredDirtyProperties.Contains(propertyName))
            {
                IsDirty = true;

                // bubble the IsDirty flag to any ParentViewModel found
                if (ParentViewModel != null)
                    ParentViewModel.IsDirty = true;
            }
        }

        private WeakReference<ViewModel>? _parentViewModel = null;

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public Dictionary<Type, IViewModelCommand> Commands { get; private set; } = new Dictionary<Type, IViewModelCommand>();

        /// <inheritdoc/>
        public object? Tag { get; set; }

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

        /// <inheritdoc/>
        public bool IsDirty
        {
            get => _isDirty;
            set => SetProperty(ref _isDirty, value);
        }

        private bool _isRefreshing;

        /// <inheritdoc/>
        /// <remarks>
        /// There is no interaction with this property in the <see cref="ViewModel"/> base class.
        /// Derived classes can use it for their own purposes.
        /// </remarks>
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
#if !NET5_0
            ReInitializeCommands();
#endif
        }

        /// <summary>
        /// Reinitializes the <see cref="KnownCommands"/> list for command and view model interaction.
        /// </summary>
#if NET5_0
        [ModuleInitializer]
#endif
        public static void ReInitializeCommands()
        {
            try
            {
                ReInitializeCommands(
                    AppDomain.CurrentDomain
                        .GetAssemblies()
                        .SelectMany(a => a.GetTypes())
                        .Where(type => !type.IsAbstract && typeof(IViewModelCommand).IsAssignableFrom(type) && type.HasGenericTypeArgument(typeof(ViewModel)))
                    );
            }
            catch (Exception ex)
            {
                Debug.Fail($"Failed to query all assembly types: {ex}");
            }
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
