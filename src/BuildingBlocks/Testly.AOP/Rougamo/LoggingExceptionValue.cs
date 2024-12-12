#if ROUGAMO_VERSION_5_0_0_OR_GREATER

using Rougamo.Context;
using Rougamo.Metadatas;
using Rougamo;
using System.Runtime.CompilerServices;

namespace Testly.AOP.Rougamo
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    [Optimization(ForceSync = ForceSync.All, MethodContext = Omit.Arguments)]
    [Advice(Feature.ExceptionHandle)]
    [Lifetime(Lifetime.Singleton)]
    public class LoggingExceptionValueAttribute<TRet> : LoggingExceptionAttribute
        where TTarget : notnull
        where TRet : struct
    {
        protected override void HandledException(MethodContext context)
            => context.HandledException(this, default(TRet));
    }
}

#else

using Microsoft.Extensions.Logging;
using Rougamo.Context;
using Rougamo;
using Testly.AOP.Accessors;

namespace Testly.AOP.Rougamo
{
    public struct LoggingExceptionValue<TRet> : IMo
        where TRet : struct
    {
        public AccessFlags Flags { get; }

        public string? Pattern { get; }

        public Feature Features { get; } = Feature.ExceptionHandle;

        public double Order { get; }

        public Omit MethodContextOmits { get; } = Omit.Arguments;

        public ForceSync ForceSync { get; } = ForceSync.All;

        public LoggingExceptionValue()
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
                var logger = LoggerUnsafeAccessor.Get(context.Target);
                logger.LogError(context.Exception, "Unexpected exception in {MethodName} from {TargetType}",
                    context.TargetType.Name, context.Method.Name);
                context.HandledException(this, default(TRet));
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

#endif