using System;
using System.Diagnostics;
using System.Linq;

namespace PresentationBase
{
    /// <summary>
    /// A dispatcher used internally for actions which must be executed in a special context.
    /// </summary>
    public abstract class Dispatcher
    {
        private static Dispatcher? Instance { get; set; }

        /// <summary>
        /// Dispatches the given <paramref name="action"/>.
        /// </summary>
        /// <param name="action">The action to dispatch.</param>
        public static void Dispatch(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Instance != null)
                Instance.DispatchImpl(action);
            else
                action.Invoke();
        }

        /// <summary>
        /// Specific dispatcher implementation.
        /// </summary>
        /// <param name="action">The action to dispatch.</param>
        protected abstract void DispatchImpl(Action action);

        static Dispatcher()
        {
            Type? implType = null;

            try
            {
                implType = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(type => !type.IsAbstract && typeof(Dispatcher).IsAssignableFrom(type) && type != typeof(Dispatcher));
            }
            catch (Exception ex)
            {
                Debug.Fail($"Failed to query all assembly types: {ex}");
            }

            if (implType == null)
            {
                Debug.Print($"No implementation of {nameof(Dispatcher)} has been found.");
                return;
            }

            try
            {
                Instance = (Dispatcher)Activator.CreateInstance(implType)!;
                return;
            }
            catch (Exception ex)
            {
                Debug.Fail($"Failed to create an intance of {implType.FullName}: {ex}");
            }
        }
    }
}
