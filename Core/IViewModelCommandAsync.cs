using System.Threading.Tasks;

namespace PresentationBase
{
    /// <summary>
    /// The interface for <strong>asynchronous</strong> view model commands.
    /// </summary>
    public interface IViewModelCommandAsync
        : IViewModelCommand
    {
        /// <summary>
        /// Executes the command for the given view model <strong>asynchronously</strong>.
        /// </summary>
        /// <param name="parameter">The view model this command is executed on.</param>
        Task ExecuteAsync(ViewModel parameter);

        /// <summary>
        /// Indicates if the command is running right now.
        /// </summary>
        bool IsWorking { get; }
    }
}