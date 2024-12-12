#if ROUGAMO_VERSION_5_0_0_OR_GREATER

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Rougamo;
using Rougamo.Metadatas;
using Rougamo.Context;
using Testly.AOP.Accessors;

namespace Testly.AOP.Rougamo
{
#nullable disable
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    [Optimization(ForceSync = ForceSync.All, MethodContext = Omit.Arguments)]
    [Advice(Feature.ExceptionHandle)]
    [Lifetime(Lifetime.Singleton)]
    public class LoggingExceptionAttribute<TTarget> : MoAttribute
    {
        public override void OnException(MethodContext context)
        {
            if (context.HasException
                && !context.ExceptionHandled
                && context.Exception is not null
                && context.Target is not null
                && context.Target is TTarget target)
            {
                var logger = LoggerUnsafeAccessor<TTarget>.GetLogger(target);
                logger.LogError(context.Exception, "Unexpected exception in {MethodName} from {TargetType}",
                    context.TargetType.Name, context.Method.Name);
                HandledException(context);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void HandledException(MethodContext context)
            => context.HandledException(this, null);
    }
}

#else

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

#endif