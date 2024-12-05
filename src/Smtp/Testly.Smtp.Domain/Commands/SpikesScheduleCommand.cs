namespace Testly.Smtp.Domain.Commands
{
    public class SpikesScheduleCommand : SmtpScheduleCommand
    {
        public int RandomDelayFrom { get; set; }

        public int RandomDelayTo { get; set; }
    }
}
