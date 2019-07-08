using System.Runtime.Serialization;

namespace CalculatorContract
{
    [DataContract]
    public sealed class OperationResult
    {
        public OperationResult(double value)
        {
            Value = value;
        }
        
        [DataMember] public double Value { get; private set; }
    }
}