using System;
using System.Diagnostics;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LatiosFramework.SourceGen
{
    [Generator]
    public class CollectionComponentGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //Debugger.Launch();
            
            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (node, token) => GeneratorFilterMethods.IsSyntaxStructInterfaceMatch(node, token, "ICollectionComponent"),
                transform: (node, token) => GeneratorFilterMethods.GetSemanticStructInterfaceMatch(node, token, "global::Latios.ICollectionComponent")
                ).Where(t => t is { });

            context.RegisterSourceOutput(candidateProvider, (sourceProductionContext, source) =>
            {
                GenerateOutput(sourceProductionContext, source);
            });
        }

        static void GenerateOutput(SourceProductionContext context, StructDeclarationSyntax collectionComponentSyntax)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            try
            {
                var filename       = Path.GetFileNameWithoutExtension(collectionComponentSyntax.SyntaxTree.FilePath);
                var outputFilename = $"{filename}_{collectionComponentSyntax.Identifier}_ICollectionComponent.gen.cs";

                context.AddSource(outputFilename, ComponentCodeWriter.WriteComponentCode(collectionComponentSyntax, "Collection"));
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                    throw;
                context.ReportDiagnostic(
                    Diagnostic.Create(CollectionComponentErrorDescriptor, collectionComponentSyntax.GetLocation(), e.ToUnityPrintableString()));
            }
        }

        public static readonly DiagnosticDescriptor CollectionComponentErrorDescriptor =
            new DiagnosticDescriptor("LATIOS_SG_01", "ICollectionComponent Generator Error",
                                     "This error indicates a bug in the Latios Framework source generators. We'd appreciate a bug report. Thanks! Error message: '{0}'.",
                                     "Latios.ICollectionComponent", DiagnosticSeverity.Error, isEnabledByDefault: true, description: "");
    }
}

