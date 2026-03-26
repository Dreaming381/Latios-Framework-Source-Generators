using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LatiosFramework.SourceGen
{
    [Generator]
    public class VInterfaceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //Debugger.Launch();

            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (node, token) => GeneratorFilterMethods.IsSyntaxInterfaceInterfaceMatch(node, token, "IVInterface"),
                transform: (node, token) => GeneratorFilterMethods.GetSemanticInterfaceInterfaceMatch(node, token, "global::Latios.Unsafe.IVInterface")
                ).Where(t => t is { });

            var compilationProvider = context.CompilationProvider;
            var combinedProviders   = candidateProvider.Combine(compilationProvider);

            context.RegisterSourceOutput(combinedProviders, (sourceProductionContext, sourceProviderTuple) =>
            {
                var (interfaceDeclarationSyntax, compilation) = sourceProviderTuple;
                GenerateOutput(sourceProductionContext, interfaceDeclarationSyntax, compilation);
            });
        }

        static void GenerateOutput(SourceProductionContext context, InterfaceDeclarationSyntax interfaceSyntax, Compilation compilation)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            try
            {
                var syntaxTree     = interfaceSyntax.SyntaxTree;
                var filename       = Path.GetFileNameWithoutExtension(syntaxTree.FilePath);
                var outputFilename = $"{filename}_{interfaceSyntax.Identifier}_IVInterface.gen.cs";

                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                VptrSemanticsExtractor.ExtractInterfaceSemantics(interfaceSyntax, semanticModel, out var bodyContext);
                var code = VInterfaceCodeWriter.WriteInterfaceCode(interfaceSyntax, ref bodyContext);

                context.AddSource(outputFilename, code);
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                    throw;
                context.ReportDiagnostic(
                    Diagnostic.Create(CollectionComponentErrorDescriptor, interfaceSyntax.GetLocation(), e.ToUnityPrintableString()));
            }
        }

        public static readonly DiagnosticDescriptor CollectionComponentErrorDescriptor =
            new DiagnosticDescriptor("LATIOS_SG_03", "IVInterface Generator Error",
                                     "This error indicates a bug in the Latios Framework source generators. We'd appreciate a bug report. Thanks! Error message: '{0}'.",
                                     "Latios.Unsafe.IVInterface", DiagnosticSeverity.Error, isEnabledByDefault: true, description: "");
    }
}

