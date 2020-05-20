using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace PresentationBase
{
    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> for view models.
    /// It ensures that the <see cref="ViewModel.ParentViewModel"/> property is correctly set when working on collections.
    /// Also adds methods for collection manipulation.
    /// </summary>
    /// <typeparam name="TViewModel">The view model type of this collection.</typeparam>
    public class ObservableViewModelCollection<TViewModel>
        : ObservableCollection<TViewModel>
        where TViewModel : ViewModel
    {
        private ViewModel OwnerViewModel { get; }

        /// <summary>
        /// Creates a new <see cref="ObservableCollection{T}"/> instance.
        /// </summary>
        /// <param name="viewModel">The parent view model.</param>
        public ObservableViewModelCollection(ViewModel viewModel)
        {
            OwnerViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            CollectionChanged += ObservableViewModelCollection_CollectionChanged;
        }

        private void ObservableViewModelCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (TViewModel item in e.OldItems.OfType<TViewModel>())
                    item.ParentViewModel = null;
            }

            if (e.NewItems != null)
            {
                foreach (TViewModel item in e.NewItems.OfType<TViewModel>())
                    item.ParentViewModel = OwnerViewModel;
            }

            OwnerViewModel.IsDirty = true;
        }

        /// <summary>
        /// Observes the child view models for changes to the properties defined in <paramref name="propertyNames"/>.
        /// When changes are detected then <paramref name="action"/> is invoked.
        /// </summary>
        /// <param name="action">The action to invoke on observed change.</param>
        /// <param name="propertyNames">The properties to observe for changes.</param>
        public void Observe(Action action, params string[] propertyNames)
        {
            Observe((name) => action.Invoke(), propertyNames);
        }

        /// <summary>
        /// Observes the child view models for changes to the properties defined in <paramref name="propertyNames"/>.
        /// When changes are detected then <paramref name="action"/> is invoked.
        /// </summary>
        /// <param name="action">The action to invoke on observed change. The parameter is the name of the changed property.</param>
        /// <param name="propertyNames">The properties to observe for changes.</param>
        public void Observe(Action<string> action, params string[] propertyNames)
        {
            if (action == null || propertyNames == null)
                return;

            void propertyChangedHandler(object sender, PropertyChangedEventArgs e)
            {
                OwnerViewModel.IsDirty = true;

                if (!propertyNames.Contains(e.PropertyName))
                    return;

                action.Invoke(e.PropertyName);
            }

            void collectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems.OfType<ViewModel>())
                        item.PropertyChanged -= propertyChangedHandler;
                }

                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems.OfType<ViewModel>())
                        item.PropertyChanged += propertyChangedHandler;
                }
            }

            CollectionChanged -= collectionChangedHandler;
            CollectionChanged += collectionChangedHandler;

            foreach (ViewModel item in this)
            {
                item.PropertyChanged -= propertyChangedHandler;
                item.PropertyChanged += propertyChangedHandler;
            }
        }

        /// <summary>
        /// Adds a view model to the end of the collection.
        /// </summary>
        /// <param name="item">The view model to add.</param>
        new public void Add(TViewModel item)
        {
            Dispatcher.Dispatch(() => base.Add(item));
        }

        /// <summary>
        /// Adds multiple view models to the end of the collection.
        /// </summary>
        /// <param name="collection">The view models to add.</param>
        public void AddRange(IEnumerable<TViewModel> collection)
        {
            Dispatcher.Dispatch(() =>
            {
                foreach (var item in collection)
                {
                    base.Add(item);
                }
            });
        }

        /// <summary>
        /// Clears the collection and adds the given view models.
        /// </summary>
        /// <param name="collection">The replacement view models.</param>
        public void Replace(params TViewModel[] collection)
        {
            Dispatcher.Dispatch(() =>
            {
                base.Clear();
                foreach (var item in collection)
                {
                    base.Add(item);
                }
            });
        }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        new public void Clear()
        {
            Dispatcher.Dispatch(() => base.Clear());
        }

        /// <summary>
        /// Inserts a view model at the given <paramref name="index"/> to the collection.
        /// </summary>
        /// <param name="index">The index at which the view model is inserted.</param>
        /// <param name="item">The view model to insert.</param>
        new public void Insert(int index, TViewModel item)
        {
            Dispatcher.Dispatch(() => base.Insert(index, item));
        }

        /// <summary>
        /// Removes a view model from the collection.
        /// </summary>
        /// <param name="item">The view model to remove.</param>
        /// <returns>Returns <c>true</c> if the view model was found and removed. Returns <c>false</c> otherwise.</returns>
        new public bool Remove(TViewModel item)
        {
            bool result = false;
            Dispatcher.Dispatch(() => { result = base.Remove(item); });
            return result;
        }

        /// <summary>
        /// Removes a view model at the given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index of the view model to remove.</param>
        new public void RemoveAt(int index)
        {
            Dispatcher.Dispatch(() => base.RemoveAt(index));
        }
    }
}
