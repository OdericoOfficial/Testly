using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace Testly.Domain.Analyzers
{
    internal static class SimplexSyntaxProvider
    {
        public static bool Predicate(SyntaxNode node, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return node is ClassDeclarationSyntax classDeclaration
                && classDeclaration.AttributeLists.SelectMany(attributeList => attributeList.Attributes)
                    .Any(attribute => attribute.Name.ToString().Contains("Simplex"));
        }

        public static (string, IEnumerable<INamedTypeSymbol>) Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var type = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(context.Node)!;
            return (type.ContainingNamespace.ToDisplayString() ,type.GetAttributes()
                .Select(attributeData => attributeData.AttributeClass!)
                .Where(attributeClass => attributeClass.Name == "SimplexAttribute"));
        }
    }
}
