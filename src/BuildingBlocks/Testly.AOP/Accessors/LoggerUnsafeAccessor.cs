using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Testly.AOP.Accessors
{
    internal static class LoggerUnsafeAccessor
    {
        private static readonly Type _loggerType = typeof(ILogger);
        private const string _loggerName = "_logger";

        public static ILogger Get(object target)
        {
            var filedInfo = target.GetType().GetRuntimeFields()
                .First(info => _loggerType.IsAssignableFrom(info.FieldType) && info.Name == _loggerName);
            var logger = filedInfo.GetValueUnsafe<ILogger>(target);
            return logger;
        }
    }
}
