using Microsoft.Extensions.DependencyInjection;

namespace Testly.Domain.Grains.Abstractions
{
    public abstract class ScopedGrain : Grain
    {
        private AsyncServiceScope _scope;

        protected IServiceProvider ScopedProvider
            => _scope.ServiceProvider;

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _scope = ServiceProvider.CreateAsyncScope();
            return Task.CompletedTask;
        }

        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
            => await _scope.DisposeAsync();
    }

    public abstract class ScopedGrain<TState> : Grain<TState>
    {
        private AsyncServiceScope _scope;

        protected IServiceProvider ScopedProvider
            => _scope.ServiceProvider;

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _scope = ServiceProvider.CreateAsyncScope();
            return Task.CompletedTask;
        }

        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
            => await _scope.DisposeAsync();
    }
}
