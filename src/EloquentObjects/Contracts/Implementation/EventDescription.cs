using System;
using System.Reflection;

namespace EloquentObjects.Contracts.Implementation
{
    internal sealed class EventDescription : IEventDescription
    {
        public EventDescription(string name, EventInfo @event)
        {
            Name = name;
            Event = @event;
            IsStandardEvent = Event.EventHandlerType == typeof(EventHandler) ||
                              Event.EventHandlerType.IsGenericType && Event.EventHandlerType.GetGenericTypeDefinition() == typeof(EventHandler<>);
        }

        #region Implementation of IEventDescription

        public string Name { get; }
        public EventInfo Event { get; }
        public bool IsStandardEvent { get; }

        #endregion
    }
}