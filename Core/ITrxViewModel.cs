using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PresentationBase
{
    /// <summary>
    /// Extends the base <see cref="ViewModel"/> implementation with the ability to rollback property changes.
    /// <br/><br/>
    /// <strong>Short-term</strong> transactions are managed with <see cref="IEditableObject.BeginEdit"/>, <see cref="IEditableObject.EndEdit"/> and <see cref="IEditableObject.CancelEdit"/>.
    /// <br/>
    /// <strong>Long-term</strong> transactions use <see cref="IChangeTracking.AcceptChanges"/> and <see cref="IRevertibleChangeTracking.RejectChanges"/>.
    /// </summary>
    /// <remarks>
    /// Please note that only <see langword="public"/> properties (found by <see cref="TypeDescriptor"/>) can be rolled back.
    /// </remarks>
    public interface ITrxViewModel
        : IViewModel, IEditableObject, IRevertibleChangeTracking
    {
        /// <summary>
        /// Indicates that <see cref="IEditableObject.BeginEdit"/> has been called and a transaction is ongoing.
        /// Remains <see langword="true"/> until <see cref="IEditableObject.EndEdit"/> or <see cref="IEditableObject.CancelEdit"/> are called.
        /// </summary>
        bool IsEditing { get; }
    }
}
