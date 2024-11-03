using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LatiosFramework.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LatiosFramework.Unika.SourceGen
{
    [Generator]
    public class InterfaceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //Debugger.Launch();

            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (node, token) => GeneratorFilterMethods.IsSyntaxInterfaceInterfaceMatch(node, token, "IUnikaInterface"),
                transform: (node, token) => GeneratorFilterMethods.GetSemanticInterfaceInterfaceMatch(node, token, "global::Latios.Unika.IUnikaInterface")
                ).Where(t => t is { });

            var compilationProvider = context.CompilationProvider;
            var combinedProviders   = candidateProvider.Combine(compilationProvider);

            context.RegisterSourceOutput(combinedProviders, (sourceProductionContext, sourceProviderTuple) =>
            {
                var (structDeclarationSyntax, compilation) = sourceProviderTuple;
                GenerateOutput(sourceProductionContext, structDeclarationSyntax, compilation);
            });
        }

        static void GenerateOutput(SourceProductionContext context, InterfaceDeclarationSyntax unikaInterfaceSyntax, Compilation compilation)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            try
            {
                var syntaxTree     = unikaInterfaceSyntax.SyntaxTree;
                var filename       = Path.GetFileNameWithoutExtension(syntaxTree.FilePath);
                var outputFilename = $"{filename}_{unikaInterfaceSyntax.Identifier}_IUnikaInterface.gen.cs";

                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                UnikaSemanticsExtractor.ExtractInterfaceSemantics(unikaInterfaceSyntax, semanticModel, out var bodyContext);
                var code = InterfaceCodeWriter.WriteInterfaceCode(unikaInterfaceSyntax, ref bodyContext);

                context.AddSource(outputFilename, code);
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                    throw;
                context.ReportDiagnostic(
                    Diagnostic.Create(CollectionComponentErrorDescriptor, unikaInterfaceSyntax.GetLocation(), e.ToUnityPrintableString()));
            }
        }

        public static readonly DiagnosticDescriptor CollectionComponentErrorDescriptor =
            new DiagnosticDescriptor("Unika_SG_01", "IUnikaInterface Generator Error",
                                     "This error indicates a bug in the Latios Framework source generators. We'd appreciate a bug report. Thanks! Error message: '{0}'.",
                                     "Latios.Unika.IUnikaInterface", DiagnosticSeverity.Error, isEnabledByDefault: true, description: "");
    }
}

