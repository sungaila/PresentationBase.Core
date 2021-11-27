using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PresentationBase
{
    /// <summary>
    /// The base implementation of <see cref="IBindable"/>.
    /// </summary>
    public abstract class Bindable
        : IBindable
    {
        /// <summary>
        /// Implementation of <see cref="INotifyPropertyChanging.PropertyChanging"/>.
        /// Is used to notify that a property is about to be changed.
        /// <br/>
        /// It is raised before changing the property backing field and <see cref="PropertyChanged"/>.
        /// </summary>
        /// <remarks>
        /// Please note that this does not imply that a property change can be canceled.
        /// </remarks>
        public event PropertyChangingEventHandler? PropertyChanging;

        /// <summary>
        /// Implementation of <see cref="INotifyPropertyChanged.PropertyChanged"/>.
        /// Is used to support bindings between views and bindable properties.
        /// <br/>
        /// It is raised after <see cref="PropertyChanging"/> and changing the property backing field.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Implementation of <see cref="INotifyDataErrorInfo.ErrorsChanged"/>.
        /// Is used to validate bound properties (or the bindable itself) for bindings.
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for all properties.
        /// </summary>
        protected void RaiseAllPropertiesChanged()
        {
            // setting PropertyChangedEventArgs.PropertyName to null or string.Empty indicates that all properties changed
            // see: https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged.propertychanged?redirectedfrom=MSDN&view=netframework-4.8#remarks
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanging"/> event for the given property name.
        /// </summary>
        /// <param name="propertyName">The name of the property which is about to be changed. When omitted the property name will be the member name of the caller (which it is when called from the bindable property setter).</param>
        protected void RaisePropertyChanging(string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            if (propertyName == string.Empty)
                throw new ArgumentException($"The {nameof(propertyName)} cannot be empty.", nameof(propertyName));

            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the given property name.
        /// </summary>
        /// <param name="propertyName">The name of the property which has been changed. When omitted the property name will be the member name of the caller (which it is when called from the bindable property setter).</param>
        protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                Debug.Fail($"{nameof(Bindable)}.{nameof(Bindable.RaisePropertyChanged)} has been called with a null or empty {nameof(propertyName)}.");
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
        /// <param name="propertyName">The name of the property which has been changed. When omitted the property name will be the member name of the caller (which it is when called from the bindable property setter).</param>
        /// <returns>Returns <c>true</c> if the new value was set.</returns>
        protected bool SetProperty<T>(ref T propertyField, T newValue, Func<T, IEnumerable<string>>? propertyValidation = null, [CallerMemberName] string? propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                Debug.Fail($"{nameof(Bindable)}.{nameof(SetProperty)} has been called with a null or empty {nameof(propertyName)}.");
                return false;
            }

            // stop when the new and old value equal
            if (EqualityComparer<T>.Default.Equals(propertyField, newValue))
                return false;

            PreSetProperty(ref propertyField, newValue, propertyValidation, propertyName);

            // notify that the backing field is about to be changed
            RaisePropertyChanging(propertyName!);

            // otherwise set the new value for the property
            propertyField = newValue;

            // validate the new value if needed
            AddPropertyErrors(propertyName!, propertyValidation?.Invoke(newValue));

            PostSetProperty(ref propertyField, newValue, propertyValidation, propertyName);

            // inform bindings about the changed property
            RaisePropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Can be overwritten to extend <see cref="SetProperty{T}(ref T, T, Func{T, IEnumerable{string}}?, string?)"/>.<para/>
        /// This method is called <strong>before</strong> <see cref="PropertyChanging"/> is raised and <strong>before</strong> the backing field <paramref name="propertyField"/> is set.
        /// </summary>
        /// <typeparam name="T">The type of the changed property.</typeparam>
        /// <param name="propertyField">The property field which contains the old value.</param>
        /// <param name="newValue">The new value to set.</param>
        /// <param name="propertyValidation">An optional function used for validation of the changed property. It must return a collection of error messages.</param>
        /// <param name="propertyName">The name of the property which has been changed. When omitted the property name will be the member name of the caller (which it is when called from the bindable property setter).</param>
        protected virtual void PreSetProperty<T>(ref T propertyField, T newValue, Func<T, IEnumerable<string>>? propertyValidation = null, [CallerMemberName] string? propertyName = null)
        {
        }

        /// <summary>
        /// Can be overwritten to extend <see cref="SetProperty{T}(ref T, T, Func{T, IEnumerable{string}}?, string?)"/>.<para/>
        /// This method is called <strong>after</strong> the backing field <paramref name="propertyField"/> is set and <strong>before</strong> <see cref="PropertyChanged"/> is raised.
        /// </summary>
        /// <typeparam name="T">The type of the changed property.</typeparam>
        /// <param name="propertyField">The property field which contains the old value.</param>
        /// <param name="newValue">The new value to set.</param>
        /// <param name="propertyValidation">An optional function used for validation of the changed property. It must return a collection of error messages.</param>
        /// <param name="propertyName">The name of the property which has been changed. When omitted the property name will be the member name of the caller (which it is when called from the bindable property setter).</param>
        protected virtual void PostSetProperty<T>(ref T propertyField, T newValue, Func<T, IEnumerable<string>>? propertyValidation = null, [CallerMemberName] string? propertyName = null)
        {
        }

        private readonly Dictionary<string, List<string>> _errors = new();

        /// <summary>
        /// If any property or the bindable itself has failed validation.<para/>
        /// </summary>
        /// <remarks>Overwrite this for custom validation like requiring valid children.</remarks>
        public virtual bool HasErrors => _errors.Any();

        /// <inheritdoc/>
        public bool IsValid => !HasErrors;

        /// <summary>
        /// Returns all errors for a given <paramref name="propertyName"/> or for the entire bindable.
        /// </summary>
        /// <param name="propertyName">The property name. Set to <see langword="null"/> to get all bindable errors.</param>
        public IEnumerable GetErrors(string? propertyName)
        {
            if (propertyName == null)
                propertyName = string.Empty;

            return _errors.ContainsKey(propertyName)
                ? _errors[propertyName]
                : new List<string>();
        }

        /// <summary>
        /// Clears both bindable and all property errors.
        /// </summary>
        protected void ClearAllErrors()
        {
            ClearBindableErrors();

            foreach (var error in _errors)
                ClearPropertyErrors(error.Key);
        }

        /// <summary>
        /// Clears all bindable errors.
        /// </summary>
        protected void ClearBindableErrors()
        {
            if (!_errors.ContainsKey(string.Empty))
                return;

            _errors.Remove(string.Empty);
            RaiseBindableErrorsChanged();
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
        /// Adds a collection of error messages which invalidate the entire bindable.
        /// </summary>
        /// <param name="errorMessages">The collection of error messages.</param>
        /// <param name="clearPreviousErrors">If the previous errors should be cleared before adding the new ones.</param>
        protected void AddBindableErrors(IEnumerable<string>? errorMessages, bool clearPreviousErrors = true)
        {
            if (clearPreviousErrors)
                ClearBindableErrors();

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
        /// Raises the <see cref="ErrorsChanged"/> event for bindable errors.
        /// </summary>
        protected void RaiseBindableErrorsChanged()
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
                Debug.Fail($"{nameof(Bindable)}.{nameof(Bindable.RaisePropertyErrorsChanged)} has been called with a null or empty {nameof(propertyName)}.");
                return;
            }

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            RaisePropertyChanged(nameof(HasErrors));
            RaisePropertyChanged(nameof(IsValid));
        }
    }
}