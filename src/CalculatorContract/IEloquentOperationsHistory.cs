using EloquentObjects;

namespace CalculatorContract
{
    [EloquentContract]
    public interface IEloquentOperationsHistory
    {
        [EloquentProperty]
        string[] OperationsHistory { get; }

        [EloquentMethod(IsOneWay = true)]
        void Clear();
    }
}