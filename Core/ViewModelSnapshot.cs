using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace PresentationBase
{
    internal class ViewModelSnapshot
    {
        private readonly TrxViewModel _contextViewModel;

        private readonly Dictionary<PropertyDescriptor, object?> _properties = new();

        public ViewModelSnapshot(TrxViewModel contextViewModel)
        {
            _contextViewModel = contextViewModel;

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(_contextViewModel))
            {
                if (property.IsReadOnly ||
                    property.Name == nameof(TrxViewModel.IsChanged) ||
                    property.Name == nameof(TrxViewModel.IsEditing) ||
                    property.Name == "IsRejectingChanges")
                    continue;

                if (HelperExtensions.IsTypeObservableViewModelCollection(property.PropertyType))
                {
                    if (property.GetValue(_contextViewModel) is not IEnumerable existingCollection)
                    {
                        _properties.Add(property, null);
                        continue;
                    }

                    var newCollection = Activator.CreateInstance(property.PropertyType, new[] { _contextViewModel })!;

                    foreach (var item in existingCollection)
                    {
                        ((IList)newCollection).Add(item);
                    }

                    _properties.Add(property, newCollection);

                    continue;
                }

                _properties.Add(property, property.GetValue(_contextViewModel));
            }
        }

        public void ApplySnapshot()
        {
            foreach (var property in _properties)
            {
                property.Key.SetValue(_contextViewModel, property.Value);

                if (HelperExtensions.IsTypeObservableViewModelCollection(property.Key.PropertyType))
                {
                    if (property.Key.GetValue(_contextViewModel) is not IEnumerable existingCollection)
                    {
                        continue;
                    }

                    foreach (ViewModel item in existingCollection)
                    {
                        item.ParentViewModel = _contextViewModel;
                    }
                }
            }
        }
    }
}