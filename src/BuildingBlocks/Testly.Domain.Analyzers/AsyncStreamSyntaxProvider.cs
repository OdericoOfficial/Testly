using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Testly.Domain.Attributes;

namespace Testly.Domain.Analyzers
{
    internal static class AsyncStreamSyntaxProvider
    {
        internal class AsyncStreamInfo
        {
            public string EventNamespace { get; set; } = string.Empty;

            public string EventName { get; set; } = string.Empty;

            public bool IsImplicit { get; set; }

            public bool NeedSubscribe { get; set; }
        }

        public static bool Predicate(SyntaxNode node, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return node is ClassDeclarationSyntax classDeclaration
                && classDeclaration.Identifier.ValueText.EndsWith("Grain")
                && (classDeclaration.AttributeLists.SelectMany(attributeList => attributeList.Attributes)
                    .Any(attribute => attribute.Name.ToString().Contains("AsyncStream"))
                    || classDeclaration.ChildNodes().Any(node =>
                        node is FieldDeclarationSyntax fieldDeclaration
                            && fieldDeclaration.AttributeLists.SelectMany(attributeList => attributeList.Attributes)
                                .Any(attribute => attribute.Name.ToString().Contains("AsyncStream"))));
        }

        public static (INamedTypeSymbol GrainType, List<AsyncStreamInfo> Infos) Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var grainType = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(context.Node)!;
            var infos = new List<AsyncStreamInfo>();

            var asyncStreamQuery = context.Node.ChildNodes().Where(node =>
                node is FieldDeclarationSyntax fieldDeclaration
                    && fieldDeclaration.AttributeLists.SelectMany(attributeList => attributeList.Attributes)
                        .Any(attribute => attribute.Name.ToString()== "AsyncStream")).Select(node => (FieldDeclarationSyntax)node);

            var subscribeAsyncStream = context.Node.ChildNodes().Where(node =>
                node is FieldDeclarationSyntax fieldDeclaration
                    && fieldDeclaration.AttributeLists.SelectMany(attributeList => attributeList.Attributes)
                        .Any(attribute => attribute.Name.ToString() == "SubscribeAsyncStream")).Select(node => (FieldDeclarationSyntax)node);

            foreach (var item in asyncStreamQuery)
            {
                var variableDeclaration = item.Declaration;
                var variableDeclarator = variableDeclaration.Variables.First();
                var field = (IFieldSymbol)context.SemanticModel.GetDeclaredSymbol(variableDeclarator)!;
                var type = (INamedTypeSymbol)field.Type;
                var eventType = type.TypeArguments.First();
                infos.Add(new AsyncStreamInfo
                {
                    EventNamespace = eventType.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                    EventName = eventType.Name,
                    NeedSubscribe = false,
                    IsImplicit = false
                });
            }

            foreach (var item in subscribeAsyncStream)
            {
                var variableDeclaration = item.Declaration;
                var variableDeclarator = variableDeclaration.Variables.First();
                var field = (IFieldSymbol)context.SemanticModel.GetDeclaredSymbol(variableDeclarator)!;
                var type = (INamedTypeSymbol)field.Type;
                var eventType = type.TypeArguments.First();
                infos.Add(new AsyncStreamInfo
                {
                    EventNamespace = eventType.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                    EventName = eventType.Name,
                    NeedSubscribe = true,
                    IsImplicit = false
                });
            }

            foreach (var attribute in grainType.GetAttributes())
            {
                var attributeName = attribute.AttributeClass!.Name;
                if ((attributeName == "ImplicitAsyncStreamAttribute"
                    || attributeName == "ImplicitSubscribeAsyncStreamAttribute")
                    && attribute.AttributeClass.TypeArguments.Length != 0)
                {
                    var eventSymbol = attribute.AttributeClass.TypeArguments[0];
                    infos.Add(new AsyncStreamInfo
                    {
                        EventNamespace = eventSymbol.ContainingNamespace.ToDisplayString(),
                        EventName = eventSymbol.Name,
                        IsImplicit = true,
                        NeedSubscribe = attributeName == "ImplicitSubscribeAsyncStreamAttribute"
                    });
                }
            }

            return (grainType, infos);
        }
    }
}
