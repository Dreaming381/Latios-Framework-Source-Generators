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
    public class ScriptGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //Debugger.Launch();

            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (node, token) => GeneratorFilterMethods.IsSyntaxStructInterfaceMatch(node, token, "IUnikaScript"),
                transform: (node, token) => GeneratorFilterMethods.GetSemanticStructInterfaceMatch(node, token, "global::Latios.Unika.IUnikaScript")
                ).Where(t => t is { });

            var compilationProvider = context.CompilationProvider;
            var combinedProviders   = candidateProvider.Combine(compilationProvider);

            context.RegisterSourceOutput(combinedProviders, (sourceProductionContext, sourceProviderTuple) =>
            {
                var (structDeclarationSyntax, compilation) = sourceProviderTuple;
                GenerateOutput(sourceProductionContext, structDeclarationSyntax, compilation);
            });
        }

        static void GenerateOutput(SourceProductionContext context, StructDeclarationSyntax unikaScriptSyntax, Compilation compilation)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            try
            {
                var syntaxTree     = unikaScriptSyntax.SyntaxTree;
                var filename       = Path.GetFileNameWithoutExtension(syntaxTree.FilePath);
                var outputFilename = $"{filename}_{unikaScriptSyntax.Identifier}_IUnikaScript.gen.cs";

                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                UnikaSemanticsExtractor.ExtractScriptSemantics(unikaScriptSyntax, semanticModel, out var bodyContext, out var extensionClassContext);
                var code = ScriptCodeWriter.WriteScriptCode(unikaScriptSyntax, ref bodyContext, ref extensionClassContext);

                context.AddSource(outputFilename, code);
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                    throw;
                context.ReportDiagnostic(
                    Diagnostic.Create(CollectionComponentErrorDescriptor, unikaScriptSyntax.GetLocation(), e.ToUnityPrintableString()));
            }
        }

        public static readonly DiagnosticDescriptor CollectionComponentErrorDescriptor =
            new DiagnosticDescriptor("Unika_SG_02", "IUnikaScript Generator Error",
                                     "This error indicates a bug in the Latios Framework source generators. We'd appreciate a bug report. Thanks! Error message: '{0}'.",
                                     "Latios.Unika.IUnikaScript", DiagnosticSeverity.Error, isEnabledByDefault: true, description: "");
    }
}

