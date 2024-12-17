namespace Testly.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class SubscribeAsyncStreamAttribute : Attribute
    {
    }
}
