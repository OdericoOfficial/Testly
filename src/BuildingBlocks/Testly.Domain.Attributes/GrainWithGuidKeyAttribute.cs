namespace Testly.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class GrainWithGuidKeyAttribute : Attribute
    {
    }
}
