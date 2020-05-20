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

        protected abstract void DispatchImpl(Action action);

        static Dispatcher()
        {
            var implType = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(type => !type.IsAbstract && typeof(Dispatcher).IsAssignableFrom(type) && type != typeof(Dispatcher));

            try
            {
                if (implType != null)
                {
                    Instance = (Dispatcher)Activator.CreateInstance(implType)!;
                    return;
                }

                Debug.Print($"No implementation of {nameof(Dispatcher)} has been found.");
            }
            catch (Exception ex)
            {
                Debug.Fail($"Failed to create an intance of {implType.FullName}: {ex}");
            }
        }
    }
}
