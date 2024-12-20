﻿using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using static Testly.Domain.Analyzers.DependencyInjectionSyntaxProvider;

namespace Testly.Domain.Analyzers
{
    [Generator(LanguageNames.CSharp)]
#pragma warning disable RS1036 // 指定分析器禁止的 API 强制设置
    internal class DependencyInjectionIncrementalGenerator : IIncrementalGenerator
#pragma warning restore RS1036 // 指定分析器禁止的 API 强制设置
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterSourceOutput(context.SyntaxProvider.CreateSyntaxProvider(Predicate, Transform).Collect(), (context, infos) =>
            {
                var namespaceSet = new HashSet<string>();
                var builder = new StringBuilder();
                builder.AppendLine("// <auto-generated />");
                builder.AppendLine();
                builder.AppendLine("using Microsoft.Extensions.DependencyInjection.Extensions;");
                foreach (var info in infos)
                {
                    if (info.ServiceNamespace != string.Empty
                        && !namespaceSet.Contains(info.ServiceNamespace))
                    {
                        builder.AppendLine($"using {info.ServiceNamespace};");
                        namespaceSet.Add(info.ServiceNamespace);
                    }
                    if (!namespaceSet.Contains(info.ImplantationNamespace))
                    {
                        builder.AppendLine($"using {info.ImplantationNamespace};");
                        namespaceSet.Add(info.ImplantationNamespace);
                    }
                }
                builder.AppendLine(@"
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class IServiceCollectionExtensions
    {
        internal static IServiceCollection AddMarkedServices(this IServiceCollection services)
        {");
                foreach (var info in infos)
                {
                    if (info.Inject.HasFlag(InjectEnum.Enumerable | InjectEnum.Transient) 
                        && info.ServiceType != string.Empty)
                        builder.AppendLine($"            services.TryAddEnumerable(ServiceDescriptor.Transient<{info.ServiceType}, {info.ImplantationType}>());");
                    else if (info.Inject.HasFlag(InjectEnum.Enumerable | InjectEnum.Scoped)
                        && info.ServiceType != string.Empty)
                        builder.AppendLine($"            services.TryAddEnumerable(ServiceDescriptor.Scoped<{info.ServiceType}, {info.ImplantationType}>());");
                    else if (info.Inject.HasFlag(InjectEnum.Enumerable | InjectEnum.Singleton)
                        && info.ServiceType != string.Empty)
                        builder.AppendLine($"            services.TryAddEnumerable(ServiceDescriptor.Singleton<{info.ServiceType}, {info.ImplantationType}>());");
                    else if (info.Inject.HasFlag(InjectEnum.Transient))
                    {
                        if (info.ServiceType != string.Empty)
                            builder.AppendLine($"            services.TryAddTransient<{info.ServiceType}, {info.ImplantationType}>();");
                        else
                            builder.AppendLine($"            services.TryAddTransient<{info.ImplantationType}>();");
                    }
                    else if (info.Inject.HasFlag(InjectEnum.Scoped))
                    {
                        if (info.ServiceType != string.Empty)
                            builder.AppendLine($"            services.TryAddScoped<{info.ServiceType}, {info.ImplantationType}>();");
                        else
                            builder.AppendLine($"            services.TryAddScoped<{info.ImplantationType}>();");
                    }
                    else if (info.Inject.HasFlag(InjectEnum.Singleton))
                    {

                        if (info.ServiceType != string.Empty)
                            builder.AppendLine($"            services.TryAddSingleton<{info.ServiceType}, {info.ImplantationType}>();");
                        else
                            builder.AppendLine($"            services.TryAddSingleton<{info.ImplantationType}>();");
                    }
                    else if (info.Inject.HasFlag(InjectEnum.HostedService))
                        builder.AppendLine($"            services.AddHostedService<{info.ImplantationType}>();");
                }
                builder.AppendLine(@"            return services;
        }
    }
}");
                context.AddSource("IServiceCollectionExtensions.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
            });
        }
    }
}
