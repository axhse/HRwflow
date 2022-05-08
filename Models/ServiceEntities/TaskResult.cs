namespace HRwflow.Models
{
    public class TaskResult<TValue>
    {
        public bool IsCompleted { get; init; }
        public bool IsSuccessful { get; init; }
        public TValue Value { get; init; }
    }

    public class TaskResult
    {
        public bool IsCompleted { get; init; }
        public bool IsSuccessful { get; init; }

        public static TaskResult FromCondition(bool isSuccessful)
            => new() { IsCompleted = true, IsSuccessful = isSuccessful };

        public static TaskResult<TValue> Successful<TValue>(TValue value)
                    => new() { IsCompleted = true, IsSuccessful = true, Value = value };

        public static TaskResult Successful() => FromCondition(true);

        public static TaskResult<TValue> Uncompleted<TValue>()
                    => new() { IsCompleted = false, IsSuccessful = false };

        public static TaskResult Uncompleted()
            => new() { IsCompleted = false, IsSuccessful = false };

        public static TaskResult<TValue> Unsuccessful<TValue>()
                    => new() { IsCompleted = true, IsSuccessful = false };

        public static TaskResult Unsuccessful() => FromCondition(false);
    }
}
