using System;
using System.Windows.Input;

namespace PresentationBase
{
	/// <summary>
	/// The base implementation of commands for view models.
	/// </summary>
	/// <typeparam name="TViewModel">The type of the view model.</typeparam>
	public abstract class ViewModelCommand<TViewModel>
		: IViewModelCommand
		where TViewModel : ViewModel
	{
		/// <summary>
		/// Raised when changes to the view model were made and <see cref="CanExecute"/> should be reevaluated.
		/// </summary>
		public event EventHandler? CanExecuteChanged;

		/// <summary>
		/// Returns if the command can be executed for the given view model.
		/// </summary>
		/// <param name="parameter">The view model this command would be executed on.</param>
		/// <returns>Returns if <see cref="Execute"/> is allowed for the given <paramref name="parameter"/>.</returns>
		public virtual bool CanExecute(TViewModel parameter)
		{
			return parameter != null;
		}

		/// <summary>
		/// Executes the command for the given view model.
		/// </summary>
		/// <param name="parameter">The view model this command is executed on.</param>
		public abstract void Execute(TViewModel parameter);

		/// <inheritdoc/>
		public void RaiseCanExecuteChanged()
		{
			Dispatcher.Dispatch(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));
		}

		#region Explicit interface implementations
		bool ICommand.CanExecute(object parameter)
		{
			if (!(parameter is TViewModel viewModel))
				return true;

			return CanExecute(viewModel);
		}

		void ICommand.Execute(object parameter)
		{
			if (!(parameter is TViewModel viewModel))
				return;

			Execute(viewModel);
		}

		bool IViewModelCommand.CanExecute(ViewModel parameter)
		{
			return CanExecute((TViewModel)parameter);
		}

		void IViewModelCommand.Execute(ViewModel parameter)
		{
			Execute((TViewModel)parameter);
		}
		#endregion
	}
}
