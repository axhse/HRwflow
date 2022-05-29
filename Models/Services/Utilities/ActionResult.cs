namespace HRwflow.Models
{
    public class ActionResult<TError>
    {
        public TError Error { get; private set; }

        public bool HasError { get; private set; }

        public ActionResult()
        {
            HasError = false;
        }

        public ActionResult(TError error)
        {
            Error = error;
            HasError = true;
        }
    }
}
