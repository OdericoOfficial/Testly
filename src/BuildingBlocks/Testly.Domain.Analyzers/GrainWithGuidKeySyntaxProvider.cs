using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace Testly.Domain.Analyzers
{
    internal static class GrainWithGuidKeySyntaxProvider
    {
        public static bool Predicate(SyntaxNode node, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return node is ClassDeclarationSyntax classDeclaration
                && classDeclaration.AttributeLists
                .SelectMany(attributeListSyntax
                    => attributeListSyntax.Attributes)
                .Any(attribute => attribute.Name.ToString() == "GrainWithGuidKey");
        }

        public static INamedTypeSymbol Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(context.Node)!;
        }
    }
}
