namespace HRwflow.Models
{
    public class IdVM<TId>
    {
        public IdVM(TId id)
        {
            Id = id;
        }

        public TId Id { get; set; }
    }
}
