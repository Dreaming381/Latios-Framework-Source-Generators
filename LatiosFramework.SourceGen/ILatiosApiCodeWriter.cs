// This file was originally written with Claude.
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LatiosFramework.SourceGen
{
    internal static class ILatiosApiCodeWriter
    {
        public static string WriteApiCode(StructDeclarationSyntax structDeclaration, ref LatiosApiSemanticsExtractor.BodyContext bodyContext)
        {
            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, structDeclaration.Parent);
            scopePrinter.PrintOpen(false);
            var printer = scopePrinter.Printer;

            // We can't emit this for real because the Entities ISystem generator also emits this, and this attribute doesn't allow duplicates.
            printer.PrintLine("// [global::System.Runtime.CompilerServices.CompilerGenerated]");
            printer.PrintBeginLine();
            foreach (var m in structDeclaration.Modifiers)
                printer.Print(m.ToString()).Print(" ");
            printer.Print("struct ").PrintEndLine(structDeclaration.Identifier.Text);
            printer.OpenScope();
            PrintBody(ref printer, ref bodyContext);
            printer.CloseScope();

            scopePrinter.PrintClose();
            return printer.Result;
        }

        static void PrintBody(ref Printer printer, ref LatiosApiSemanticsExtractor.BodyContext context)
        {
            // Nested cache struct
            printer.PrintLine("private struct __LatiosApiState");
            printer.OpenScope();
            printer.PrintLine("public global::Latios.LatiosWorldUnmanaged latiosWorldUnmanaged;");
            foreach (var field in context.fields)
                printer.PrintBeginLine("public ").Print(field.type.ToFullName()).Print(" ").Print(field.fieldName).PrintEndLine(";");
            printer.CloseScope();
            printer.PrintBeginLine().PrintEndLine();

            printer.PrintLine("private __LatiosApiState __latiosApiState;");
            printer.PrintBeginLine().PrintEndLine();

            // __OnCreateForLatios
            printer.PrintLine("void global::Latios.ILatiosApi.__OnCreateForLatios(ref global::Unity.Entities.SystemState state)");
            printer.OpenScope();
            printer.PrintLine("__latiosApiState = default;");
            printer.PrintLine("__latiosApiState.latiosWorldUnmanaged = global::Latios.InternalSourceGen.StaticAPI.GetLatiosWorldUnmanaged(ref state);");
            foreach (var field in context.fields)
            {
                printer.PrintBeginLine("__latiosApiState.").Print(field.fieldName).Print(" = ");
                PrintFieldConstructionExpression(ref printer, field.initKind, field.type, field.boolValue, field.builtinGetterMethodName);
                printer.PrintEndLine(";");
            }
            printer.CloseScope();
            printer.PrintBeginLine().PrintEndLine();

            // __Get<T>()
            printer.PrintLine("T global::Latios.ILatiosApi.__Get<T>()");
            printer.OpenScope();
            foreach (var field in context.fields)
            {
                if (field.boolValue.HasValue)
                    continue;
                printer.PrintBeginLine("if (typeof(T) == typeof(").Print(field.type.ToFullName()).Print(
                    ")) return global::Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<").Print(field.type.ToFullName()).Print(", T>(ref __latiosApiState.").Print(
                    field.fieldName).PrintEndLine(");");
            }
            printer.PrintLine(
                "throw new System.NotImplementedException(\"This type was not registered by the ILatiosApi source generator. Was it accessed through a code path the generator could not see?\");");
            printer.CloseScope();
            printer.PrintBeginLine().PrintEndLine();

            // __Get<T>(bool)
            printer.PrintLine("T global::Latios.ILatiosApi.__Get<T>(bool b)");
            printer.OpenScope();
            foreach (var field in context.fields)
            {
                if (!field.boolValue.HasValue)
                    continue;
                printer.PrintBeginLine("if (typeof(T) == typeof(").Print(field.type.ToFullName()).Print(") && b == ").Print(
                    field.boolValue.Value ? "true" : "false").Print(
                    ") return global::Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<").Print(field.type.ToFullName()).Print(", T>(ref __latiosApiState.").Print(
                    field.fieldName).PrintEndLine(");");
            }
            printer.PrintLine(
                "throw new System.NotImplementedException(\"This type/bool combination was not registered by the ILatiosApi source generator. Was it accessed through a code path the generator could not see?\");");
            printer.CloseScope();
            printer.PrintBeginLine().PrintEndLine();

            // __GetLatiosWorldUnmanaged
            printer.PrintLine("global::Latios.LatiosWorldUnmanaged global::Latios.ILatiosApi.__GetLatiosWorldUnmanaged() => __latiosApiState.latiosWorldUnmanaged;");
        }

        // Shared with InjectableCodeWriter, since __CreateForApi's per-[Inject]-field construction is the same
        // taxonomy of expressions as __OnCreateForLatios's per-cached-field construction above.
        internal static void PrintFieldConstructionExpression(ref Printer printer,
                                                              LatiosApiSemanticsExtractor.FieldInitKind initKind,
                                                              ITypeSymbol type,
                                                              bool?                                     boolValue,
                                                              string builtinGetterMethodName)
        {
            switch (initKind)
            {
                case LatiosApiSemanticsExtractor.FieldInitKind.Gettable:
                    printer.Print("global::Latios.InternalSourceGen.StaticAPI.Create<").Print(type.ToFullName()).Print(">(ref state)");
                    break;
                case LatiosApiSemanticsExtractor.FieldInitKind.GettableBool:
                    printer.Print("global::Latios.InternalSourceGen.StaticAPI.Create<").Print(type.ToFullName()).Print(">(ref state, ").Print(
                        boolValue.Value ? "true" : "false").Print(")");
                    break;
                case LatiosApiSemanticsExtractor.FieldInitKind.BuiltinWithBool:
                    printer.Print("state.").Print(builtinGetterMethodName).Print("<").Print(GetSoloTypeArgumentFullName(type)).Print(">(").Print(
                        boolValue.Value ? "true" : "false").Print(")");
                    break;
                case LatiosApiSemanticsExtractor.FieldInitKind.BuiltinNoBool:
                    printer.Print("state.").Print(builtinGetterMethodName).Print("()");
                    break;
                case LatiosApiSemanticsExtractor.FieldInitKind.BuiltinNoBoolGeneric:
                    printer.Print("state.").Print(builtinGetterMethodName).Print("<").Print(GetSoloTypeArgumentFullName(type)).Print(">()");
                    break;
                case LatiosApiSemanticsExtractor.FieldInitKind.Injectable:
                    printer.Print("global::Latios.InternalSourceGen.StaticAPI.CreateInjectable<").Print(type.ToFullName()).Print(">(ref state)");
                    break;
            }
        }

        static string GetSoloTypeArgumentFullName(ITypeSymbol type)
        {
            if (type is INamedTypeSymbol named && named.TypeArguments.Length == 1)
                return named.TypeArguments[0].ToFullName();
            return type.ToFullName();
        }
    }
}

