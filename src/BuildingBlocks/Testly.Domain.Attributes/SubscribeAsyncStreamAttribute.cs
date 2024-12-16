namespace Testly.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    public class SubscribeAsyncStreamAttribute : Attribute
    {
    }
}
