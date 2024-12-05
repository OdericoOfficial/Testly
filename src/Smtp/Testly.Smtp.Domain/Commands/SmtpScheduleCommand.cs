using System.Collections.Immutable;
using Testly.Domain.Commands;

namespace Testly.Smtp.Domain.Commands
{
    public abstract class SmtpScheduleCommand : ScheduleCommand
    {
        public string? CertainFrom { get; set; }

        public string? CertainTo { get; set; }

        public string? CertainSubject { get; set; }

        public (string Username, string Password)? LoginTuple { get; set; }

        public ImmutableArray<string> EmailDomains { get; set; }

        public string MTAAddress { get; set; } = string.Empty;

        public int MTAPort { get; set; }
    }
}
