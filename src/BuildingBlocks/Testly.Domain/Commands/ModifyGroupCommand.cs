namespace Testly.Domain.Commands
{
    public record struct ModifyGroupCommand
    {
        public string Subject { get; init; } 

        public string Description { get; init; } 
    }
}
