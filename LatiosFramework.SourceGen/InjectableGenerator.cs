// This file was originally written with Claude.
using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LatiosFramework.SourceGen
{
    [Generator]
    public class InjectableGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //Debugger.Launch();

            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (node, token) => GeneratorFilterMethods.IsSyntaxStructInterfaceMatch(node, token, "IInjectable"),
                transform: (node, token) => GeneratorFilterMethods.GetSemanticStructInterfaceMatch(node, token, "global::Latios.IInjectable")
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
                InjectableSemanticsExtractor.ExtractInjectableSemantics(structSyntax, compilation, context, out var bodyContext);
                var code = InjectableCodeWriter.WriteInjectableCode(structSyntax, ref bodyContext);

                // The hint name must be unique across the WHOLE generator for the WHOLE compilation, not just
                // within this file - a name built from just the source file name + immediate struct identifier
                // collides whenever two different outer types in the same file each nest a same-named job struct
                // (a common pattern, e.g. several systems each with their own nested "FindChunksNeedingCopyingJob").
                // Roslyn throws on a duplicate hint name from AddSource, which disables this generator for the
                // ENTIRE compilation (silently - no per-candidate diagnostic), so every other IInjectable type in
                // the same assembly loses its codegen too. The fully-qualified type name is unique per compilation,
                // so basing the hint name on that instead avoids the collision entirely.
                var outputFilename = SanitizeHintName(bodyContext.structFullName) + "_IInjectable.gen.cs";

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
            new DiagnosticDescriptor("LATIOS_SG_08", "IInjectable Generator Error",
                                     "This error indicates a bug in the Latios Framework source generators. We'd appreciate a bug report. Thanks! Error message: '{0}'.",
                                     "Latios.IInjectable", DiagnosticSeverity.Error, isEnabledByDefault: true, description: "");
    }
}
