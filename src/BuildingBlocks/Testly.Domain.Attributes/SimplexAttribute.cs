namespace Testly.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class SimplexAttribute<TSentEvent, TRequest> : Attribute
    {
    }
}
