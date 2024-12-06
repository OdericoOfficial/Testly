using Microsoft.Extensions.Logging;
using System.Reflection;
using Rougamo;
using Rougamo.Context;
using Testly.Reflection;

namespace Testly.AOP.Rougamo
{
    public struct LoggingException : IMo
    {
        private static readonly object _stob = new object();
        private static readonly Type _interfaceType = typeof(ILogger);

        public AccessFlags Flags { get; }

        public string? Pattern { get; }

        public Feature Features { get; } = Feature.ExceptionHandle;

        public double Order { get; }

        public Omit MethodContextOmits { get; } = Omit.Arguments;

        public ForceSync ForceSync { get; } = ForceSync.OnException;

        public LoggingException()
        {
        }

        public void OnEntry(MethodContext context)
        {
        }

        public ValueTask OnEntryAsync(MethodContext context)
            => ValueTask.CompletedTask;

        public void OnException(MethodContext context)
        {
            if (context.HasException
                && !context.ExceptionHandled
                && context.Exception is not null
                && context.Target is not null)
            {
                var targetType = context.Target.GetType();
                var loggerField = targetType.GetRuntimeFields()
                    .Where(item => _interfaceType.IsAssignableFrom(item.FieldType))
                    .FirstOrDefault();

                if (loggerField is not null)
                {
                    var logger = loggerField.GetFieldValueUnsafe<ILogger>(context.Target);
                    logger.LogError(context.Exception, "Unexpected exception in {MethodName} from {TargetType}",
                        targetType.Name, context.Method.Name);

                    context.HandledException(this, _stob);
                }
            }
        }

        public ValueTask OnExceptionAsync(MethodContext context)
            => ValueTask.CompletedTask;

        public void OnExit(MethodContext context)
        {
        }

        public ValueTask OnExitAsync(MethodContext context)
            => ValueTask.CompletedTask;

        public void OnSuccess(MethodContext context)
        {
        }

        public ValueTask OnSuccessAsync(MethodContext context)
            => ValueTask.CompletedTask;
    }
}
