namespace CalculatorContract
{
    public interface IEloquentOperationsHistory
    {
        string[] OperationsHistory { get; }

        void Clear();
    }
}