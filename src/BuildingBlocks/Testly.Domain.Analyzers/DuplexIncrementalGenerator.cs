﻿using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using static Testly.Domain.Analyzers.DuplexSyntaxProvider;

namespace Testly.Domain.Analyzers
{
    [Generator(LanguageNames.CSharp)]
#pragma warning disable RS1036 // 指定分析器禁止的 API 强制设置
    internal class DuplexIncrementalGenerator : IIncrementalGenerator
#pragma warning restore RS1036 // 指定分析器禁止的 API 强制设置
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterSourceOutput(context.SyntaxProvider.CreateSyntaxProvider(Predicate, Transform), (context, symbols) =>
            {
                var path = Path.Combine(Path.GetTempPath(), "policy.txt");

                foreach (var policy in File.ReadAllLines(path))
                {
                    foreach (var item in symbols.Item2)
                    {
                        var prefix = item.TypeArguments[0].Name.Substring(0, item.TypeArguments[0].Name.LastIndexOf('S'));
                        var postfix = policy;
                        var realize = $@"// <auto-generated />

using Microsoft.Extensions.Logging;
using Testly.Domain.Commands;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Factories.Abstractions;
using Testly.Domain.Policies.Abstractions;
using Testly.Domain.Grains.Abstractions;
using {item.TypeArguments[0].ContainingNamespace.ToDisplayString()};
using {item.TypeArguments[2].ContainingNamespace.ToDisplayString()};

namespace {symbols.Item1}.Grains
{{
    [ImplicitStreamSubscription]
    internal sealed class {prefix}Duplex{postfix}Grain : Duplex{postfix}Grain<{item.TypeArguments[0].Name}, {item.TypeArguments[1].Name}, {item.TypeArguments[2].Name}>
    {{
        public {prefix}Duplex{postfix}Grain(ILogger<{prefix}Duplex{postfix}Grain> logger, 
            ISentPolicy<{postfix}Command> policy, 
            ISentFactory<{item.TypeArguments[2].Name}> sentFactory, 
            ISentEventFactory<{item.TypeArguments[0].Name}, {item.TypeArguments[2].Name}> sentEventFactory) : base(logger, policy, sentFactory, sentEventFactory)
        {{
        }}
    }}
}}";
                        context.AddSource($"{prefix}Duplex{postfix}Grain.g.cs", SourceText.From(realize, Encoding.UTF8));
                    }
                }
            });
        }
    }
}
