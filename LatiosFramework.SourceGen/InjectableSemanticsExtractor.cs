// This file was originally written with Claude.
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LatiosFramework.SourceGen
{
    internal static class InjectableSemanticsExtractor
    {
        public struct FieldEntry
        {
            public IFieldSymbol                         fieldSymbol;
            public ITypeSymbol                           type;
            public bool?                                 boolValue;
            public LatiosApiSemanticsExtractor.FieldInitKind initKind;
            // Only populated for BuiltinWithBool / BuiltinNoBool.
            public string builtinGetterMethodName;
        }

        public struct BodyContext
        {
            public string           structFullName;
            public List<FieldEntry> injectFields;
        }

        public static void ExtractInjectableSemantics(StructDeclarationSyntax structDeclarationSyntax,
                                                       Compilation compilation,
                                                       SourceProductionContext context,
                                                       out BodyContext bodyContext)
        {
            bodyContext.injectFields   = new List<FieldEntry>();
            bodyContext.structFullName = null;

            var declaringModel = compilation.GetSemanticModel(structDeclarationSyntax.SyntaxTree);
            var structSymbol   = declaringModel.GetDeclaredSymbol(structDeclarationSyntax, context.CancellationToken) as INamedTypeSymbol;
            if (structSymbol == null)
                return;

            bodyContext.structFullName = structSymbol.ToFullName();

            // [Inject] fields must be declared directly on the type carrying the IInjectable base list
            // (unlike ILatiosApi's Get*() usages, there's no invocation to walk across partial declarations here).
            foreach (var field in structSymbol.GetMembers().OfType<IFieldSymbol>())
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (field.IsStatic || field.IsConst)
                    continue;
                if (!field.HasAttribute("Latios.InjectAttribute"))
                    continue;

                var initKind = LatiosApiSemanticsExtractor.ClassifyReturnType(field.Type, out var builtinGetterMethodName);
                if (initKind == null)
                {
                    // Per design, an [Inject] field whose type isn't in the supported taxonomy is silently
                    // skipped rather than reported as an error, so the attribute can be applied defensively.
                    continue;
                }

                bool? boolValue = null;
                if (initKind == LatiosApiSemanticsExtractor.FieldInitKind.GettableBool ||
                    initKind == LatiosApiSemanticsExtractor.FieldInitKind.BuiltinWithBool)
                {
                    boolValue = field.HasAttribute("Unity.Collections.ReadOnlyAttribute");
                }

                bodyContext.injectFields.Add(new FieldEntry
                {
                    fieldSymbol             = field,
                    type                    = field.Type,
                    boolValue               = boolValue,
                    initKind                = initKind.Value,
                    builtinGetterMethodName = builtinGetterMethodName,
                });
            }
        }
    }
}
