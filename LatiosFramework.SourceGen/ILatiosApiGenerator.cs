// This file was originally written with Claude.
using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LatiosFramework.SourceGen
{
    [Generator]
    public class ILatiosApiGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //Debugger.Launch();

            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (node, token) => GeneratorFilterMethods.IsSyntaxStructInterfaceMatch(node, token, "ILatiosApi"),
                transform: (node, token) => GeneratorFilterMethods.GetSemanticStructInterfaceMatch(node, token, "global::Latios.ILatiosApi")
                ).Where(t => t is { });

            var compilationProvider = context.CompilationProvider;
            var combinedProviders   = candidateProvider.Combine(compilationProvider);

            context.RegisterSourceOutput(combinedProviders, (sourceProductionContext, sourceProviderTuple) =>
            {
                var (structDeclarationSyntax, compilation) = sourceProviderTuple;
                GenerateOutput(sourceProductionContext, structDeclarationSyntax, compilation);
            });
        }

        static void GenerateOutput(SourceProductionContext context, StructDeclarationSyntax structSyntax, Compilation compilation)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            try
            {
                LatiosApiSemanticsExtractor.ExtractApiSemantics(structSyntax, compilation, context, out var bodyContext);
                var code = ILatiosApiCodeWriter.WriteApiCode(structSyntax, ref bodyContext);

                // See the identical comment in InjectableGenerator.GenerateOutput: the hint name must be unique
                // across the whole compilation, not just this file, so it's built from the fully-qualified type
                // name rather than just the file name + immediate identifier (which would collide if two files
                // sharing a name each declared a same-named ILatiosApi system).
                var outputFilename = SanitizeHintName(bodyContext.structFullName) + "_ILatiosApi.gen.cs";

                context.AddSource(outputFilename, code);
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                    throw;
                context.ReportDiagnostic(
                    Diagnostic.Create(InternalErrorDescriptor, structSyntax.GetLocation(), e.ToUnityPrintableString()));
            }
        }

        static string SanitizeHintName(string fullyQualifiedName)
        {
            var sb = new StringBuilder(fullyQualifiedName.Length);
            foreach (var c in fullyQualifiedName)
                sb.Append(char.IsLetterOrDigit(c) ? c : '_');
            return sb.ToString();
        }

        public static readonly DiagnosticDescriptor InternalErrorDescriptor =
            new DiagnosticDescriptor("LATIOS_SG_05", "ILatiosApi Generator Error",
                                     "This error indicates a bug in the Latios Framework source generators. We'd appreciate a bug report. Thanks! Error message: '{0}'.",
                                     "Latios.ILatiosApi", DiagnosticSeverity.Error, isEnabledByDefault: true, description: "");

        public static readonly DiagnosticDescriptor NonConstantBoolArgumentDescriptor =
            new DiagnosticDescriptor("LATIOS_SG_06", "LatiosApiInvoker Get() bool argument must be a compile-time constant",
                                     "The bool argument passed to this LatiosApiInvoker.Get(...) call must be a compile-time constant",
                                     "Latios.ILatiosApi", DiagnosticSeverity.Error, isEnabledByDefault: true, description: "");

        public static readonly DiagnosticDescriptor UnsupportedReturnTypeDescriptor =
            new DiagnosticDescriptor("LATIOS_SG_07",
                                     "Unsupported LatiosApiInvoker Get() return type",
                                     "The type '{0}' returned by this LatiosApiInvoker.Get(...) call is not supported by the ILatiosApi source generator. It must implement ILatiosApiGettable/ILatiosApiGettableBool, or be one of the built-in Unity Entities handle/lookup types.",
                                     "Latios.ILatiosApi",
                                     DiagnosticSeverity.Error,
                                     isEnabledByDefault: true,
                                     description: "");
    }
}

