namespace HRwflow.Models
{
    public class FuncResult<TValue, TError>
    {
        public TError Error { get; private set; }
        public TValue Value { get; private set; }

        public bool HasError { get; private set; }

        public FuncResult(TValue value)
        {
            HasError = false;
            Value = value;
        }

        public FuncResult(
            TError error, TValue value = default)
        {
            Error = error;
            HasError = true;
            Value = value;
        }

        public bool TryGetValue(out TValue value)
        {
            value = Value;
            return !HasError;
        }
    }
}
