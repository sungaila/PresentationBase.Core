using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace PresentationBase
{
    /// <summary>
    /// The base implementation of <see cref="ITrxViewModel"/>.
    /// </summary>
    public abstract class TrxViewModel
        : ViewModel, ITrxViewModel
    {
        /// <summary>
        /// Creates a new <see cref="TrxViewModel"/> instance.
        /// Ensures that a snapshot is available for rollbacks.
        /// </summary>
#pragma warning disable CS8618 // the non-nullable field _currentSnapshot will be initialized in AcceptChanges
        protected TrxViewModel()
#pragma warning restore CS8618
        {
            AcceptChanges();
            PropertyChanged += (s, e) =>
            {
                if (!IsRejectingChanges &&
                    ((ParentViewModel as TrxViewModel)?.IsRejectingChanges != true) &&
                    e.PropertyName != nameof(IsChanged) &&
                    e.PropertyName != nameof(IsDirty))
                {
                    IsChanged = true;
                }
            };
        }

        private ViewModelSnapshot _currentSnapshot;

        private bool _isEditing;

        /// <inheritdoc/>
        public bool IsEditing
        {
            get => _isEditing;
            protected set => SetProperty(ref _isEditing, value);
        }

        private bool _isChanged;

        /// <inheritdoc/>
        /// <remarks>
        /// Please note that <see cref="IsChanged"/> is set to <see langword="true"/> whenever <see cref="Bindable.PropertyChanged"/> is raised.
        /// <see cref="ViewModel.IsDirty"/> is a similar but independent property.
        /// </remarks>
        public bool IsChanged
        {
            get => _isChanged;
            protected set => SetProperty(ref _isChanged, value);
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">Thrown when a previous <see cref="BeginEdit"/> has not ended or canceled.</exception>
        public virtual void BeginEdit()
        {
            if (IsEditing)
                throw new InvalidOperationException($"{nameof(BeginEdit)} cannot be called while a transaction is ongoing. Call {nameof(EndEdit)} or {nameof(CancelEdit)} first.");

            try
            {
                AcceptChanges();
                IsEditing = true;
            }
            catch (Exception)
            {
                IsEditing = false;
                throw;
            }
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">Thrown when there is no ongoing <see cref="BeginEdit"/>.</exception>
        public virtual void CancelEdit()
        {
            if (!IsEditing)
                throw new InvalidOperationException($"{nameof(CancelEdit)} cannot be called with no ongoing transaction. Call {nameof(BeginEdit)} first.");

            RejectChanges();
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">Thrown when there is no ongoing <see cref="BeginEdit"/>.</exception>
        public virtual void EndEdit()
        {
            if (!IsEditing)
                throw new InvalidOperationException($"{nameof(EndEdit)} cannot be called with no ongoing transaction. Call {nameof(BeginEdit)} first.");

            AcceptChanges();
        }

        private static readonly MethodInfo _acceptChangesInfo = typeof(TrxViewModel).GetMethod(nameof(AcceptChanges), BindingFlags.Instance | BindingFlags.Public)!;

        /// <inheritdoc/>
        public virtual void AcceptChanges()
        {
            _currentSnapshot = new ViewModelSnapshot(this);

            // apply changes from top-down
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(this))
            {
                if (typeof(TrxViewModel).IsAssignableFrom(property.PropertyType))
                {
                    if (property.GetValue(this) is not TrxViewModel trxViewModel)
                        continue;

                    trxViewModel.AcceptChanges();
                }
                else if (HelperExtensions.IsTypeObservableViewModelCollection(property.PropertyType))
                {
                    if (property.GetValue(this) is not IEnumerable collection)
                        continue;

                    foreach (var item in collection)
                    {
                        _acceptChangesInfo.Invoke(item, null);
                    }
                }
            }

            IsEditing = false;
            IsChanged = false;
        }

        /// <summary>
        /// Indicates that <see cref="RejectChanges"/> is ongoing.
        /// </summary>
        protected bool IsRejectingChanges { get; private set; }

        private static readonly MethodInfo _rejectChangesInfo = typeof(TrxViewModel).GetMethod(nameof(RejectChanges), BindingFlags.Instance | BindingFlags.Public)!;

        /// <inheritdoc/>
        public virtual void RejectChanges()
        {
            if (_currentSnapshot == null)
                throw new InvalidOperationException("There is no snapshot to rollback the transaction.");

            try
            {
                IsRejectingChanges = true;

                // reject changes from bottom-up
                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(this))
                {
                    if (typeof(TrxViewModel).IsAssignableFrom(property.PropertyType))
                    {
                        if (property.GetValue(this) is not TrxViewModel trxViewModel)
                            continue;

                        trxViewModel.RejectChanges();
                    }
                    else if (HelperExtensions.IsTypeObservableViewModelCollection(property.PropertyType))
                    {
                        if (property.GetValue(this) is not IEnumerable collection)
                            continue;

                        foreach (var item in collection)
                        {
                            _rejectChangesInfo.Invoke(item, null);
                        }
                    }
                }

                _currentSnapshot.ApplySnapshot();
                RaiseAllPropertiesChanged();

                IsEditing = false;
                IsChanged = false;
            }
            finally
            {
                IsRejectingChanges = false;
            }
        }
    }
}