namespace HRwflow.Models
{
    public class WorkplaceResult<TValue>
    {
        public WorkplaceErrors Error { get; init; }
        public bool HasError => Error != WorkplaceErrors.None;
        public TValue Value { get; init; }
    }

    public class WorkplaceResult
    {
        public WorkplaceErrors Error { get; init; }

        public bool HasError => Error != WorkplaceErrors.None;

        public static WorkplaceResult FromError(WorkplaceErrors error)
            => new() { Error = error };

        public static WorkplaceResult<TValue> FromError<TValue>(WorkplaceErrors error)
            => new() { Error = error };

        public static WorkplaceResult FromServerError()
            => new() { Error = WorkplaceErrors.ServerError };

        public static WorkplaceResult<TValue> FromServerError<TValue>()
            => new() { Error = WorkplaceErrors.ServerError };

        public static WorkplaceResult<TValue> FromValue<TValue>(TValue value)
            => new() { Value = value };

        public static WorkplaceResult Succeed()
            => new() { Error = WorkplaceErrors.None };
    }
}
