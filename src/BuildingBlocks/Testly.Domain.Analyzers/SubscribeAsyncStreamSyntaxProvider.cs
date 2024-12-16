using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace Testly.Domain.Analyzers
{
    internal static class SubscribeAsyncStreamSyntaxProvider
    {
        public static bool Predicate(SyntaxNode node, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return node is FieldDeclarationSyntax fieldDeclaration
                && fieldDeclaration.AttributeLists
                .SelectMany(attributeListSyntax
                    => attributeListSyntax.Attributes)
                .Any(attribute => attribute.Name.ToString() == "SubscribeAsyncStream");
        }

        public static IFieldSymbol Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return (IFieldSymbol)context.SemanticModel.GetDeclaredSymbol(context.Node)!;
        }
    }
}
