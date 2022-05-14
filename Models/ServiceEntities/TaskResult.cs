namespace HRwflow.Models
{
    public class TaskResult<TValue>
    {
        public bool IsCompleted { get; init; }
        public TValue Value { get; init; }

        public static TaskResult<TValue> Uncompleted()
                    => new() { IsCompleted = false };
    }

    public class TaskResult
    {
        public bool IsCompleted { get; init; }

        public static TaskResult Completed() => new() { IsCompleted = true };

        public static TaskResult<TValue> FromValue<TValue>(TValue value)
            => new() { IsCompleted = true, Value = value };

        public static TaskResult Uncompleted() => new() { IsCompleted = false };
    }
}
