namespace HRwflow.Models
{
    public enum RedirectionModes
    {
        Default,
        Success,
        Warning,
        Danger
    }

    public class RedirectionVM
    {
        public RedirectionVM(RedirectionModes mode = RedirectionModes.Default)
        {
            Mode = mode;
        }

        public RedirectionModes Mode { get; set; }
    }
}
