namespace Testly.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ImplicitSubscribeAsyncStreamAttribute<TEvent> : Attribute
    {
    }
}
