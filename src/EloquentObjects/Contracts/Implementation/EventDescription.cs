using System.Reflection;

namespace EloquentObjects.Contracts.Implementation
{
    internal sealed class EventDescription : IEventDescription
    {
        public EventDescription(string name, EventInfo @event)
        {
            Name = name;
            Event = @event;
        }

        #region Implementation of IEventDescription

        public string Name { get; }
        public EventInfo Event { get; }

        #endregion
    }
}