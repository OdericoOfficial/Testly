namespace Microsoft.Extensions.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ScopedAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ScopedAttribute<TService> : Attribute
    {
    }
}
