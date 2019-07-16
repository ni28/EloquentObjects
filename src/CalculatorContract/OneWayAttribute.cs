using System;

namespace CalculatorContract
{
    /// <summary>
    /// Indicates that a server will not send a response for a void method or a property setter call.
    /// </summary>
    public sealed class OneWayAttribute : Attribute
    {
        
    }
}