using System.Reflection;
using JetBrains.Annotations;

namespace EloquentObjects.Contracts
{
    /// <summary>
    ///     Represents the description of an event that is declared by the contract.
    /// </summary>
    internal interface IEventDescription
    {
        /// <summary>
        ///     Gets the name of the event description.
        /// </summary>
        [NotNull]
        string Name { get; }

        /// <summary>
        ///     Gets the event info.
        /// </summary>
        EventInfo Event { get; }
        
        /// <summary>
        /// Returns true if this event implements EventHandler or EventHandler{T} type.
        /// </summary>
        bool IsStandardEvent { get; }
    }
}