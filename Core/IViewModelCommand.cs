using System.Windows.Input;

namespace PresentationBase
{
	/// <summary>
	/// The interface for view model commands.
	/// </summary>
	public interface IViewModelCommand
		: ICommand
	{
		/// <summary>
		/// Raises <see cref="ICommand.CanExecuteChanged"/> and causes <see cref="CanExecute"/> to be reevaluated.
		/// </summary>
		void RaiseCanExecuteChanged();

		/// <summary>
		/// Returns if the command can be executed for the given view model.
		/// </summary>
		/// <param name="parameter">The view model this command would be executed on.</param>
		/// <returns>Returns if <see cref="Execute"/> is allowed for the given <paramref name="parameter"/>.</returns>
		bool CanExecute(ViewModel parameter);

		/// <summary>
		/// Executes the command for the given view model.
		/// </summary>
		/// <param name="parameter">The view model this command is executed on.</param>
		void Execute(ViewModel parameter);
	}
}
