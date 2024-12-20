﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rougamo;
using Testly.AOP.Rougamo;
using Testly.AOP.Tests.Mocks.Abstractions;

namespace Testly.AOP.Tests.Mocks
{
    [Singleton<IThrowExceptionValueMock>, Enumerable]
    internal class ThrowExceptionMockClassValueSimple : IThrowExceptionValueMock,
        IRougamo<LoggingExceptionValue<int>>

    {
        private readonly ILogger _logger;

        public ThrowExceptionMockClassValueSimple(ILogger<ThrowExceptionMockClassValueSimple> logger)
            => _logger = logger;

        public int NotThrow()
            => 0;

        public async Task<int> NotThrowAsync()
        {
            await Task.Delay(100);
            return 0;
        }

        public int ThrowException()
            => throw new NotImplementedException();

        public async Task<int> ThrowExceptionAsync()
        {
            await Task.Delay(100);
            throw new NotImplementedException();
        }
    }
}