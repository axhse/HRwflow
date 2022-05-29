namespace HRwflow.Models
{
    public class ErrorVM<TError>
    {
        public ErrorVM(TError error)
        {
            Error = error;
        }

        public TError Error { get; set; }
    }
}
