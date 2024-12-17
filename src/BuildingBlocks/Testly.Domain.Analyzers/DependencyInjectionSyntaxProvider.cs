using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;

namespace Testly.Domain.Analyzers
{
    internal static class DependencyInjectionSyntaxProvider
    {
        [Flags]
        internal enum InjectEnum : byte
        {
            None = 0b_00000,
            Singleton = 0b_00001,
            Scoped = 0b_00010,
            Transient = 0b_00100,
            HostedService = 0b_01000,
            Enumerable = 0b_10000
        }

        internal record struct ServiceInfo
        {
            public string ServiceNamespace { get; set; }

            public string ServiceType { get; set; }

            public string ImplantationNamespace { get; set; }

            public string ImplantationType { get; set; }

            public InjectEnum Inject { get; set; }
        }

        private static readonly ImmutableArray<string> _attributeNames
            = ["Transient", "Scoped", "Singleton", "HostedService", "Enumerable"];

        public static bool Predicate(SyntaxNode node, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return node is ClassDeclarationSyntax classDeclaration 
                && classDeclaration.AttributeLists
                .SelectMany(attributeListSyntax
                    => attributeListSyntax.Attributes)
                .Any(attribute =>
                {
                    var attributeName = attribute.Name.ToString();
                    foreach (var name in _attributeNames)
                        if (attributeName.Contains(name))
                            return true;
                    return false;
                });
        }

        public static ServiceInfo Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var implantationSymbol = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(context.Node)!;

            var inject = InjectEnum.None;
            var serviceNamespace = string.Empty;
            var serviceType = string.Empty;

            foreach (var attribute in implantationSymbol.GetAttributes())
            {
                var attributeName = attribute.AttributeClass!.Name;
                if ((attributeName == nameof(TransientAttribute)
                    || attributeName == nameof(ScopedAttribute)
                    || attributeName == nameof(SingletonAttribute))
                    && attribute.AttributeClass.TypeArguments.Length != 0)
                    {
                        var serviceSymbol = attribute.AttributeClass.TypeArguments[0];
                        serviceNamespace = serviceSymbol.ContainingNamespace.ToDisplayString();
                        serviceType = serviceSymbol.Name;
                    }

                if (attributeName == nameof(TransientAttribute))
                    inject |= InjectEnum.Transient;
                else if (attributeName == nameof(ScopedAttribute))
                    inject |= InjectEnum.Scoped;
                else if (attributeName == nameof(SingletonAttribute))
                    inject |= InjectEnum.Singleton;
                else if (attributeName == nameof(HostedServiceAttribute))
                    inject |= InjectEnum.HostedService;
                else if (attributeName == nameof(EnumerableAttribute))
                    inject |= InjectEnum.Enumerable;
            }

            return new ServiceInfo
            {
                ServiceNamespace = serviceNamespace,
                ServiceType = serviceType,
                ImplantationNamespace = implantationSymbol.ContainingNamespace.ToDisplayString(),
                ImplantationType = implantationSymbol.Name,
                Inject = inject
            };
        }
    }
}
