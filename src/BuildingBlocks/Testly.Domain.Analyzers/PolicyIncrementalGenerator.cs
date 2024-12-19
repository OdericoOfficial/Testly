﻿using static Testly.Domain.Analyzers.PolicySyntaxProvider;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Testly.Domain.Analyzers
{
    [Generator(LanguageNames.CSharp)]
#pragma warning disable RS1036 // 指定分析器禁止的 API 强制设置
    internal class PolicyIncrementalGenerator : IIncrementalGenerator
#pragma warning restore RS1036 // 指定分析器禁止的 API 强制设置
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterSourceOutput(context.SyntaxProvider.CreateSyntaxProvider(Predicate, Transform).Collect(), (context, symbols) =>
            {
                var path = Path.Combine(Path.GetTempPath(), "policy.txt");
                var list = new List<string>();

                foreach (var symbol in symbols)
                {
                    var prefix = symbol.Name.Substring(0, symbol.Name.LastIndexOf('P'));
                    var duplex = @$"// <auto-generated />

using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Testly.Domain.Commands;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Factories.Abstractions;
using Testly.Domain.Policies.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{{
    public abstract class Duplex{prefix}Grain<TSentEvent, TReceivedEvent, TRequest> : DuplexUnitGrain<TSentEvent, TReceivedEvent, TRequest, {prefix}Command>
        where TSentEvent : SentEvent
        where TReceivedEvent : ReceivedEvent
    {{
        protected Duplex{prefix}Grain(ILogger logger, 
            ISentPolicy<{prefix}Command> policy, 
            ISentFactory<TRequest> sentFactory, 
            ISentEventFactory<TSentEvent, TRequest> sentEventFactory) : base(logger, policy, sentFactory, sentEventFactory)
        {{
        }}
    }}
}}";
                    var simplex = @$"using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Testly.Domain.Commands;
using Testly.Domain.Events.Abstractions;
using Testly.Domain.Factories.Abstractions;
using Testly.Domain.Policies.Abstractions;

namespace Testly.Domain.Grains.Abstractions
{{
    public abstract class Simplex{prefix}Grain<TSentEvent, TRequest> : SimplexUnitGrain<TSentEvent, TRequest, {prefix}Command>
        where TSentEvent : SentEvent
    {{
        protected Simplex{prefix}Grain(ILogger logger, 
            ISentPolicy<{prefix}Command> policy, 
            ISentFactory<TRequest> sentFactory, 
            ISentEventFactory<TSentEvent, TRequest> sentEventFactory) : base(logger, policy, sentFactory, sentEventFactory)
        {{
        }}
    }}
}}";

                    list.Add(prefix);
                    context.AddSource($"Duplex{prefix}Grain.g.cs", SourceText.From(duplex, Encoding.UTF8));
                    context.AddSource($"Simplex{prefix}Grain.g.cs", SourceText.From(simplex, Encoding.UTF8));
                }

                if (list.Count > 0)
                    File.WriteAllLines(path, list, Encoding.UTF8);
            });
        }
    }
}