// This file was originally written with Claude.
using System;
using System.IO;
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
                var syntaxTree     = structSyntax.SyntaxTree;
                var filename       = Path.GetFileNameWithoutExtension(syntaxTree.FilePath);
                var outputFilename = $"{filename}_{structSyntax.Identifier}_ILatiosApi.gen.cs";

                LatiosApiSemanticsExtractor.ExtractApiSemantics(structSyntax, compilation, context, out var bodyContext);
                var code = ILatiosApiCodeWriter.WriteApiCode(structSyntax, ref bodyContext);

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

