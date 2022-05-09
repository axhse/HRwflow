namespace HRwflow.Models
{
    public enum ControllerErrors
    {
        Unknown,
        OperationFaulted,
        RequestUnsupported
    }

    public class ControllerErrorVM
    {
        public ControllerErrorVM(ControllerErrors error = ControllerErrors.Unknown)
        {
            Error = error;
        }

        public ControllerErrors Error { get; set; }
    }
}
