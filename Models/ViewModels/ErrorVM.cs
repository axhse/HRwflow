namespace HRwflow.Models
{
    public enum ErrorTypes
    {
        Unknown,
        RequestUnsupported,
        OperationFaulted
    }

    public class ErrorVM
    {
        public ErrorVM(ErrorTypes errorType = ErrorTypes.Unknown)
        {
            ErrorType = errorType;
        }

        public ErrorTypes ErrorType { get; set; }
    }
}
