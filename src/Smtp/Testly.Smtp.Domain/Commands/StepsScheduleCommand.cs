namespace Testly.Smtp.Domain.Commands
{
    public class StepsScheduleCommand : SmtpScheduleCommand
    {
        public int Init { get; set; }

        public int Adder { get; set; }

        public int InnerEpoch { get; set; }

        public int Delay { get; set; }
    }
}
