using System;

namespace EloquentObjects
{
    /// <summary>
    /// Indicates that an interface defines a contract that can be used with EloquentObjects framework to provide remote access for objects that implement this interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public sealed class EloquentContractAttribute : Attribute
    {
    }
    
    /// <summary>
    /// Indicates that a method can be accessed remotely when a parent object is hosted by EloquentObjects framework.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class EloquentMethodAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value that indicates whether a void method returns a reply message.
        /// </summary>
        /// <remarks>
        /// Use the true value to minimize communication channel loading.
        /// Exceptions are not replied to client if IsOneWay = true.
        /// The value is ignored for non-void methods.
        /// </remarks>
        public bool IsOneWay { get; set; }
    }
    
    /// <summary>
    /// Indicates that a property can be accessed remotely when a parent object is hosted by EloquentObjects framework.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EloquentPropertyAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value that indicates whether a property setter returns a reply message.
        /// </summary>
        /// <remarks>
        /// Use the true value to minimize communication channel loading.
        /// Exceptions are not replied to client if IsOneWay = true.
        /// The value is ignored for getters.
        /// </remarks>
        public bool IsOneWay { get; set; }
    }
    
    /// <summary>
    /// Indicates that a client can subscribe to the event remotely when a parent object is hosted by EloquentObjects framework.
    /// </summary>
    [AttributeUsage(AttributeTargets.Event)]
    public sealed class EloquentEventAttribute : Attribute
    {
    }
}