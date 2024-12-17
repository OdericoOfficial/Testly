using Microsoft.Extensions.Logging;
using Rougamo.Context;
using Rougamo;
using Testly.AOP.Accessors;

namespace Testly.AOP.Rougamo
{
    public struct LoggingException : IMo
    {
        public AccessFlags Flags { get; }

        public string? Pattern { get; }

        public Feature Features { get; } = Feature.ExceptionHandle;

        public double Order { get; }

        public Omit MethodContextOmits { get; } = Omit.Arguments;

        public ForceSync ForceSync { get; } = ForceSync.All;

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
#nullable disable
            if (context.HasException
                && !context.ExceptionHandled
                && context.Exception is not null
                && context.Target is not null)
            {
                var logger = LoggerUnsafeAccessor.Get(context.Target);
                logger.LogError(context.Exception, "Unexpected exception in {MethodName} from {TargetType}",
                    context.TargetType.Name, context.Method.Name);
                context.HandledException(this, null);
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