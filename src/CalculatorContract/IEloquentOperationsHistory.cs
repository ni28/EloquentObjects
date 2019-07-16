namespace CalculatorContract
{
    public interface IEloquentOperationsHistory
    {
        string[] Entries { get; }

        void Clear();
    }
}