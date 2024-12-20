﻿using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;

namespace Testly.AOP.Tests.Mocks.Abstractions
{
    public abstract class ThrowExceptionMockClassRefAbstract : IThrowExceptionRefMock,
        IRougamo<LoggingExceptionRef<object>>
    {
        protected readonly ILogger _logger;

        protected ThrowExceptionMockClassRefAbstract(ILogger logger)
            => _logger = logger;

        public object NotThrow()
            => new object();

        public async Task<object> NotThrowAsync()
        {
            await Task.Delay(100);
            return new object();
        }

        public object ThrowException()
            => throw new NotImplementedException();

        public async Task<object> ThrowExceptionAsync()
        {
            await Task.Delay(100);
            throw new NotImplementedException();
        }
    }
}
