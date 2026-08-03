using System;
using System.IO;
using System.Threading;
using LatiosFramework.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Notice: I threw the requirements for this generator at Claude. The output this generates seems to meet the common use case.
// However, there are a couple of potential gotchas in this generator's implementation that might present as bugs in the future.
// First, Claude embedded the interface generation implementation of AuthoringGenerator within this generator, rather than make
// the other generator support Auto-Authoring.
// Second, this generator changes internal fields to private fields when replicating to authoring.
// Third, this generator copies usings so that field attributes can be copied as strings directly. Unity's ISystem generator
// does this too. However, I don't know if Claude's implementation handles the weird edge cases such as putting usings inside of
// namespaces.

namespace LatiosFramework.Unika.SourceGen
{
    [Generator]
    public class AutoAuthoringGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //Debugger.Launch();

            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (node, token) => GeneratorFilterMethods.IsSyntaxClassGenericMatch(node, token, "UnikaScriptAutoAuthoring"),
                transform: (node, token) => GetSemanticDirectGenericMatch(node, token, "global::Latios.Unika.Authoring.UnikaScriptAutoAuthoring<T>")
                ).Where(t => t is { });

            var compilationProvider = context.CompilationProvider;
            var combinedProviders   = candidateProvider.Combine(compilationProvider);

            context.RegisterSourceOutput(combinedProviders, (sourceProductionContext, sourceProviderTuple) =>
            {
                var (classDeclarationSyntax, compilation) = sourceProviderTuple;
                GenerateOutput(sourceProductionContext, classDeclarationSyntax, compilation);
            });
        }

        // Similar to GeneratorFilterMethods.GetSemanticClassGenericMatch, except it matches against the
        // class's own base type rather than its base type's base type, since UnikaScriptAutoAuthoring<T>
        // is the type directly inherited by the user's authoring class.
        static ClassDeclarationSyntax GetSemanticDirectGenericMatch(GeneratorSyntaxContext ctx, CancellationToken cancellationToken, string fullSemanticGenericDefinitionName)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var classDeclarationSyntax = (ClassDeclarationSyntax)ctx.Node;
            foreach (var baseTypeSyntax in classDeclarationSyntax.BaseList !.Types)
            {
                var type = ctx.SemanticModel.GetTypeInfo(baseTypeSyntax.Type).Type;
                if (type != null && type.OriginalDefinition.ToFullName() == fullSemanticGenericDefinitionName)
                    return classDeclarationSyntax;
            }
            return null;
        }

        static void GenerateOutput(SourceProductionContext context, ClassDeclarationSyntax unikaAutoAuthoringSyntax, Compilation compilation)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            try
            {
                var syntaxTree     = unikaAutoAuthoringSyntax.SyntaxTree;
                var filename       = Path.GetFileNameWithoutExtension(syntaxTree.FilePath);
                var outputFilename = $"{filename}_{unikaAutoAuthoringSyntax.Identifier}_IUnikaAutoAuthoring.gen.cs";

                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                UnikaSemanticsExtractor.ExtractAutoAuthoringSemantics(unikaAutoAuthoringSyntax, semanticModel, out var bodyContext);
                var code = AutoAuthoringCodeWriter.WriteAutoAuthoringCode(unikaAutoAuthoringSyntax, ref bodyContext);

                context.AddSource(outputFilename, code);
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                    throw;
                context.ReportDiagnostic(
                    Diagnostic.Create(CollectionComponentErrorDescriptor, unikaAutoAuthoringSyntax.GetLocation(), e.ToUnityPrintableString()));
            }
        }

        public static readonly DiagnosticDescriptor CollectionComponentErrorDescriptor =
            new DiagnosticDescriptor("Unika_SG_04", "UnikaScriptAutoAuthoring Generator Error",
                                     "This error indicates a bug in the Latios Framework source generators. We'd appreciate a bug report. Thanks! Error message: '{0}'.",
                                     "Latios.Unika.Authoring.UnikaScriptAutoAuthoring<>", DiagnosticSeverity.Error, isEnabledByDefault: true, description: "");
    }
}

