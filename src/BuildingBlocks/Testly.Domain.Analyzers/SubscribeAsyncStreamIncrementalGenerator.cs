using Microsoft.CodeAnalysis;
using static Testly.Domain.Analyzers.SubscribeAsyncStreamSyntaxProvider;

namespace Testly.Domain.Analyzers
{
    internal class SubscribeAsyncStreamIncrementalGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterSourceOutput(context.SyntaxProvider.CreateSyntaxProvider(Predicate, Transform), (context, info) =>
            {
            });
        }
    }
}
