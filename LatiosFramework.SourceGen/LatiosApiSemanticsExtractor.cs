// This file was originally written with Claude.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LatiosFramework.SourceGen
{
    internal static class LatiosApiSemanticsExtractor
    {
        public enum FieldInitKind
        {
            // Field type implements ILatiosApiGettable. Initialized via StaticAPI.Create<T>(ref state).
            Gettable,
            // Field type implements ILatiosApiGettableBool. Initialized via StaticAPI.Create<T>(ref state, b).
            GettableBool,
            // Field type is one of the built-in Unity Entities handle/lookup types that take a readOnly bool.
            // Initialized via state.<builtinGetterMethodName><TComponentOrBuffer>(b).
            BuiltinWithBool,
            // Field type is one of the built-in Unity Entities handle/lookup types that take no arguments.
            // Initialized via state.<builtinGetterMethodName>().
            BuiltinNoBool,
            // Field type is one of the built-in Unity Entities handle/lookup types that take no arguments.
            // Initialized via state.<builtinGetterMethodName><TComponentOrBuffer>(b).
            BuiltinNoBoolGeneric,
            // Field type implements IInjectable. Initialized via StaticAPI.CreateInjectable<T>(ref state).
            Injectable,
        }

        public struct FieldEntry
        {
            public ITypeSymbol   type;
            public bool?         boolValue;
            public string        fieldName;
            public FieldInitKind initKind;
            // Only populated for BuiltinWithBool / BuiltinNoBool.
            public string builtinGetterMethodName;
        }

        public struct BodyContext
        {
            public string           structShortName;
            public string           structFullName;
            public List<FieldEntry> fields;
        }

        public static void ExtractApiSemantics(StructDeclarationSyntax structDeclarationSyntax,
                                               Compilation compilation,
                                               SourceProductionContext context,
                                               out BodyContext bodyContext)
        {
            bodyContext.structShortName = structDeclarationSyntax.Identifier.ToString();
            bodyContext.structFullName  = null;
            bodyContext.fields          = new List<FieldEntry>();

            var declaringModel = compilation.GetSemanticModel(structDeclarationSyntax.SyntaxTree);
            var structSymbol   = declaringModel.GetDeclaredSymbol(structDeclarationSyntax, context.CancellationToken) as INamedTypeSymbol;
            if (structSymbol == null)
                return;
            bodyContext.structFullName = structSymbol.ToFullName();

            var stringBuilder = new StringBuilder();

            // A partial type's Get*() usages may live in any of its partial-declaration files, not just the one
            // that happens to carry the ": ISystem, ILatiosApi" base list that this generator matched on.
            foreach (var syntaxRef in structSymbol.DeclaringSyntaxReferences)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                var node          = syntaxRef.GetSyntax(context.CancellationToken);
                var semanticModel = compilation.GetSemanticModel(node.SyntaxTree);

                foreach (var invocation in node.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    context.CancellationToken.ThrowIfCancellationRequested();
                    TryProcessInvocation(invocation, semanticModel, context, bodyContext.fields, stringBuilder);
                }
            }
        }

        static void TryProcessInvocation(InvocationExpressionSyntax invocation,
                                         SemanticModel semanticModel,
                                         SourceProductionContext context,
                                         List<FieldEntry>           fields,
                                         StringBuilder stringBuilder)
        {
            if (!(semanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is IMethodSymbol methodSymbol))
                return;

            if (methodSymbol.Name == "Inject" || methodSymbol.Name == "InjectByRef")
            {
                TryProcessInjectInvocation(methodSymbol, fields, stringBuilder);
                return;
            }

            if (!methodSymbol.Name.StartsWith("Get", StringComparison.Ordinal))
                return;

            var thisType     = methodSymbol.ContainingType;
            var thisOriginal = thisType?.OriginalDefinition;
            if (thisOriginal == null || thisOriginal.Name != "LatiosApiInvoker" ||
                thisOriginal.ContainingNamespace?.ToDisplayString() != "Latios")
                return;

            var returnType    = methodSymbol.ReturnType;
            var boolParameter = methodSymbol.Parameters.FirstOrDefault(p => p.Type.SpecialType == SpecialType.System_Boolean);

            bool? boolValue = null;
            if (boolParameter != null)
            {
                var argumentSyntax = FindArgumentForParameter(invocation, boolParameter);
                var constantValue  = argumentSyntax != null?
                                     semanticModel.GetConstantValue(argumentSyntax.Expression, context.CancellationToken) :
                                         default;
                if (argumentSyntax == null || !constantValue.HasValue || !(constantValue.Value is bool b))
                {
                    context.ReportDiagnostic(Diagnostic.Create(ILatiosApiGenerator.NonConstantBoolArgumentDescriptor,
                                                               (argumentSyntax as SyntaxNode ?? invocation).GetLocation()));
                    return;
                }
                boolValue = b;
            }

            if (TryFindExisting(fields, returnType, boolValue))
                return;

            var initKind = ClassifyReturnType(returnType, out var builtinGetterMethodName);
            if (initKind == null)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(ILatiosApiGenerator.UnsupportedReturnTypeDescriptor, invocation.GetLocation(), returnType.ToFullName()));
                return;
            }

            fields.Add(new FieldEntry
            {
                type                    = returnType,
                boolValue               = boolValue,
                fieldName               = MakeFieldName(fields, returnType, boolValue, stringBuilder),
                initKind                = initKind.Value,
                builtinGetterMethodName = builtinGetterMethodName,
            });
        }

        // Recognizes calls to Latios.LatiosApiCreateExtensions.Inject<TInject, TSystem>()/InjectByRef<TInject, TSystem>()
        // (invoked as injectable.Inject(api) / injectable.InjectByRef(api)) and caches TInject the same way a
        // Gettable field is cached, so ILatiosApi.__Get<TInject>() can later resolve the cached instance for it.
        static void TryProcessInjectInvocation(IMethodSymbol methodSymbol, List<FieldEntry> fields, StringBuilder stringBuilder)
        {
            var thisOriginal = methodSymbol.ContainingType?.OriginalDefinition;
            if (thisOriginal == null || thisOriginal.Name != "LatiosApiCreateExtensions" ||
                thisOriginal.ContainingNamespace?.ToDisplayString() != "Latios")
                return;

            if (methodSymbol.TypeArguments.Length < 1)
                return;
            var injectableType = methodSymbol.TypeArguments[0];

            if (TryFindExisting(fields, injectableType, null))
                return;

            fields.Add(new FieldEntry
            {
                type      = injectableType,
                boolValue = null,
                fieldName = MakeFieldName(fields, injectableType, null, stringBuilder),
                initKind  = FieldInitKind.Injectable,
            });
        }

        static ArgumentSyntax FindArgumentForParameter(InvocationExpressionSyntax invocation, IParameterSymbol parameter)
        {
            var args = invocation.ArgumentList.Arguments;
            foreach (var arg in args)
            {
                if (arg.NameColon != null && arg.NameColon.Name.Identifier.ValueText == parameter.Name)
                    return arg;
            }
            if (parameter.Ordinal < args.Count && args[parameter.Ordinal].NameColon == null)
                return args[parameter.Ordinal];
            return null;
        }

        static bool TryFindExisting(List<FieldEntry> fields, ITypeSymbol type, bool? boolValue)
        {
            foreach (var f in fields)
            {
                if (f.boolValue == boolValue && SymbolEqualityComparer.Default.Equals(f.type, type))
                    return true;
            }
            return false;
        }

        // Shared with InjectableSemanticsExtractor so it can reuse this exact taxonomy for classifying
        // [Inject] field types instead of duplicating the hardcoded Unity.Entities type-name switch.
        internal static FieldInitKind? ClassifyReturnType(ITypeSymbol returnType, out string builtinGetterMethodName)
        {
            builtinGetterMethodName = null;

            if (returnType.InheritsFromInterface("Latios.ILatiosApiGettable"))
                return FieldInitKind.Gettable;
            if (returnType.InheritsFromInterface("Latios.ILatiosApiGettableBool"))
                return FieldInitKind.GettableBool;

            var original = returnType.OriginalDefinition;
            if (original.ContainingNamespace?.ToDisplayString() == "Unity.Entities")
            {
                switch (original.Name)
                {
                    case "ComponentTypeHandle":
                        builtinGetterMethodName = "GetComponentTypeHandle";
                        return FieldInitKind.BuiltinWithBool;
                    case "ComponentLookup":
                        builtinGetterMethodName = "GetComponentLookup";
                        return FieldInitKind.BuiltinWithBool;
                    case "BufferTypeHandle":
                        builtinGetterMethodName = "GetBufferTypeHandle";
                        return FieldInitKind.BuiltinWithBool;
                    case "BufferLookup":
                        builtinGetterMethodName = "GetBufferLookup";
                        return FieldInitKind.BuiltinWithBool;
                    case "SharedComponentTypeHandle":
                        builtinGetterMethodName = "GetSharedComponentTypeHandle";
                        return FieldInitKind.BuiltinNoBoolGeneric;
                    case "EntityTypeHandle":
                        builtinGetterMethodName = "GetEntityTypeHandle";
                        return FieldInitKind.BuiltinNoBool;
                    case "EntityStorageInfoLookup":
                        builtinGetterMethodName = "GetEntityStorageInfoLookup";
                        return FieldInitKind.BuiltinNoBool;
                }
            }
            return null;
        }

        static string MakeFieldName(List<FieldEntry> existingFields, ITypeSymbol type, bool? boolValue, StringBuilder stringBuilder)
        {
            var baseName = "m_" + SanitizeIdentifier(type.ToSimpleName(), stringBuilder);
            if (boolValue.HasValue)
                baseName += boolValue.Value ? "_true" : "_false";

            var candidate = baseName;
            var suffix    = 1;
            while (existingFields.Exists(f => f.fieldName == candidate))
                candidate = $"{baseName}_{++suffix}";
            return candidate;
        }

        static string SanitizeIdentifier(string s, StringBuilder stringBuilder)
        {
            stringBuilder.Clear();
            stringBuilder.EnsureCapacity(s.Length);
            foreach (var c in s)
                stringBuilder.Append(char.IsLetterOrDigit(c) ? c : '_');
            return stringBuilder.ToString();
        }
    }
}

