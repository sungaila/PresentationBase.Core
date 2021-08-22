using System;
using System.Threading.Tasks;

namespace PresentationBase
{
    /// <summary>
    /// The base implementation of <strong>asynchronous</strong> commands for view models.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    public abstract class ViewModelCommandAsync<TViewModel>
        : ViewModelCommand<TViewModel>, IViewModelCommandAsync
        where TViewModel : ViewModel
    {
        /// <summary>
        /// The implementation of the asynchronous execution.
        /// It should make use of the async/await pattern.
        /// </summary>
        /// <param name="parameter">The view model this command is executed on.</param>
        protected abstract Task ExecutionAsync(TViewModel parameter);

        /// <summary>
        /// Executes the command asynchronously in a <strong>fire and forget</strong> way.<para/>
        /// This means the command is executed and its result is forgotten.
        /// Any uncaught exceptions will be handled in <see cref="HandleUncaughtException"/>.
        /// </summary>
        /// <param name="parameter">The view model this command is executed on.</param>
        public sealed override void Execute(TViewModel parameter)
        {
            try
            {
                IsWorking = true;
                ExecutionAsync(parameter)
                    .ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                            HandleUncaughtException(parameter, task.Exception!);

                        IsWorking = false;
                    });
            }
            catch (Exception ex)
            {
                HandleUncaughtException(parameter, ex);
                IsWorking = false;
            }
        }

        /// <inheritdoc/>
        public override bool CanExecute(TViewModel parameter)
        {
            return base.CanExecute(parameter) && !IsWorking;
        }

        /// <summary>
        /// Executes the command for the given view model <strong>asynchronously</strong>.
        /// </summary>
        /// <param name="parameter">The view model this command is executed on.</param>
        public async Task ExecuteAsync(TViewModel parameter)
        {
            try
            {
                IsWorking = true;
                await ExecutionAsync(parameter);
            }
            finally
            {
                IsWorking = false;
            }
        }

        /// <summary>
        /// Handles any uncaught exceptions thrown by <see cref="Execute"/>.
        /// </summary>
        /// <param name="parameter">The view model this command was executed on.</param>
        /// <param name="ex">The uncaught exception from execution.</param>
        protected virtual void HandleUncaughtException(TViewModel parameter, Exception ex) { }

        private bool _isWorking;

        /// <inheritdoc/>
        public bool IsWorking
        {
            get
            {
                return _isWorking;
            }
            private set
            {
                _isWorking = value;
                RaiseCanExecuteChanged();
            }
        }

        #region Explicit interface implementations
        async Task IViewModelCommandAsync.ExecuteAsync(ViewModel parameter)
        {
            await ExecuteAsync((TViewModel)parameter);
        }
        #endregion
    }
}
