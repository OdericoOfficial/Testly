namespace Testly.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class DuplexAttribute<TSentEvent, TReceivedEvent, TRequest> : Attribute
    {
    }
}
