using System.ComponentModel;

namespace PresentationBase
{
    /// <summary>
    /// Provides an interface for binding to properties, keeping track of property changes and property validation.
    /// </summary>
    public interface IBindable
        : INotifyPropertyChanging, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        /// <summary>
        /// This is the inverse of <see cref="INotifyDataErrorInfo.HasErrors"/>.
        /// </summary>
        bool IsValid { get; }
    }
}