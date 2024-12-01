﻿using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TransientAttribute : ServiceAttribute
    {
        public override ServiceLifetime ServiceLifetime
            => ServiceLifetime.Transient;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TransientAttribute<TService> : SingletonAttribute
    {
        public override Type? ServiceType { get; set; } = typeof(TService);
    }
}
