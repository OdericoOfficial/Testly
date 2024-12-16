using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Testly.Domain.Analyzers
{
    internal static class AsyncStreamSyntaxProvider
    {
        public static bool Predicate(SyntaxNode node, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return node is FieldDeclarationSyntax fieldDeclaration
                && fieldDeclaration.AttributeLists
                .SelectMany(attributeListSyntax
                    => attributeListSyntax.Attributes)
                .Any(attribute => attribute.Name.ToString() == "AsyncStream");
        }

        public static IFieldSymbol Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return (IFieldSymbol)context.SemanticModel.GetDeclaredSymbol(context.Node)!;
        }
    }
}
