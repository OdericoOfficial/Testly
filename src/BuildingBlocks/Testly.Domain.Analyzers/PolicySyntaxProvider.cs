using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace Testly.Domain.Analyzers
{
    internal static class PolicySyntaxProvider
    {
        public static bool Predicate(SyntaxNode node, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return node is ClassDeclarationSyntax classDeclaration
                && classDeclaration.AttributeLists.SelectMany(attributeList => attributeList.Attributes)
                    .Any(attribute => attribute.Name.ToString().Contains("Policy"));
        }

        public static INamedTypeSymbol Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(context.Node)!;
        }
    }
}
