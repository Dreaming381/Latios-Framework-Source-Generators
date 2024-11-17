using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using LatiosFramework.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LatiosFramework.Unika.SourceGen
{
    [Generator]
    public class AuthoringGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //Debugger.Launch();

            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (node, token) => GeneratorFilterMethods.IsSyntaxClassGenericMatch(node, token, "UnikaScriptAuthoring"),
                transform: (node, token) => GeneratorFilterMethods.GetSemanticClassGenericMatch(node, token, "global::Latios.Unika.Authoring.UnikaScriptAuthoringBase")
                ).Where(t => t is { });

            var compilationProvider = context.CompilationProvider;
            var combinedProviders   = candidateProvider.Combine(compilationProvider);

            context.RegisterSourceOutput(combinedProviders, (sourceProductionContext, sourceProviderTuple) =>
            {
                var (structDeclarationSyntax, compilation) = sourceProviderTuple;
                GenerateOutput(sourceProductionContext, structDeclarationSyntax, compilation);
            });
        }

        static void GenerateOutput(SourceProductionContext context, ClassDeclarationSyntax unikaAuthoringSyntax, Compilation compilation)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            try
            {
                var syntaxTree     = unikaAuthoringSyntax.SyntaxTree;
                var filename       = Path.GetFileNameWithoutExtension(syntaxTree.FilePath);
                var outputFilename = $"{filename}_{unikaAuthoringSyntax.Identifier}_IUnikaAuthoring.gen.cs";

                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                UnikaSemanticsExtractor.ExtractAuthoringSemantics(unikaAuthoringSyntax, semanticModel, out var bodyContext);
                var code = AuthoringCodeWriter.WriteAuthoringCode(unikaAuthoringSyntax, ref bodyContext);

                context.AddSource(outputFilename, code);
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                    throw;
                context.ReportDiagnostic(
                    Diagnostic.Create(CollectionComponentErrorDescriptor, unikaAuthoringSyntax.GetLocation(), e.ToUnityPrintableString()));
            }
        }

        public static readonly DiagnosticDescriptor CollectionComponentErrorDescriptor =
            new DiagnosticDescriptor("Unika_SG_03", "UnikaScriptAuthoring Generator Error",
                                     "This error indicates a bug in the Latios Framework source generators. We'd appreciate a bug report. Thanks! Error message: '{0}'.",
                                     "Latios.Unika.Authoring.UnikaScriptAuthoring<>", DiagnosticSeverity.Error, isEnabledByDefault: true, description: "");
    }
}

