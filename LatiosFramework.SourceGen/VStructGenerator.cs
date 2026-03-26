using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LatiosFramework.SourceGen
{
    [Generator]
    public class VStructGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //Debugger.Launch();

            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (node, token) => GeneratorFilterMethods.IsSyntaxStructInterfaceMatch(node, token, "IVInterface"),
                transform: (node, token) => GeneratorFilterMethods.GetSemanticStructInterfaceMatch(node, token, "global::Latios.Unsafe.IVInterface")
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
                var outputFilename = $"{filename}_{structSyntax.Identifier}_IVInterface_Impl.gen.cs";

                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                VptrSemanticsExtractor.ExtractObjSemantics(structSyntax, semanticModel, out var bodyContext);
                var code = VStructCodeWriter.WriteObjCode(structSyntax, ref bodyContext);

                context.AddSource(outputFilename, code);
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                    throw;
                context.ReportDiagnostic(
                    Diagnostic.Create(CollectionComponentErrorDescriptor, structSyntax.GetLocation(), e.ToUnityPrintableString()));
            }
        }

        public static readonly DiagnosticDescriptor CollectionComponentErrorDescriptor =
            new DiagnosticDescriptor("LATIOS_SG_04", "IVInterface Generator Error",
                                     "This error indicates a bug in the Latios Framework source generators. We'd appreciate a bug report. Thanks! Error message: '{0}'.",
                                     "Latios.Unika.IVInterface", DiagnosticSeverity.Error, isEnabledByDefault: true, description: "");
    }
}

