using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PresentationBase
{
    /// <summary>
    /// Inherits the property change tracking and validation from <see cref="IBindable"/>.
    /// Adds commands, a tree structure and useful properties.
    /// </summary>
    public interface IViewModel
        : IBindable
    {
        /// <summary>
        /// A dictionary filled with commands for this view model. The key is the <see cref="Type"/> of the command.
        /// </summary>
        Dictionary<Type, IViewModelCommand> Commands { get; }

        /// <summary>
        /// The logical parent of this view model.
        /// </summary>
        /// <remarks>
        /// Please note that circular references are not supported.
        /// This is a weak reference and can be <see langword="null"/> if the parent has been garbage collected.
        /// </remarks>
        ViewModel? ParentViewModel { get; set; }

        /// <summary>
        /// The top most parent of this view model.
        /// </summary>
        ViewModel? RootViewModel { get; }

        /// <summary>
        /// Indicates that there are changes made to this view model since its creation.
        /// </summary>
        bool IsDirty { get; set; }

        /// <summary>
        /// Indicates that this view model is changing and others should avoid interfering.
        /// </summary>
        bool IsRefreshing { get; set; }

        /// <summary>
        /// A multi purpose <see cref="object"/> tag for this view model.
        /// </summary>
        /// <remarks>Use it for e.g. identifying this view model.</remarks>
        object? Tag { get; set; }
    }
}