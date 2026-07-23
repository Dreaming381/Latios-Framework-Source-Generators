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
                switch (field.initKind)
                {
                    case LatiosApiSemanticsExtractor.FieldInitKind.Gettable:
                        printer.Print("global::Latios.InternalSourceGen.StaticAPI.Create<").Print(field.type.ToFullName()).Print(">(ref state)");
                        break;
                    case LatiosApiSemanticsExtractor.FieldInitKind.GettableBool:
                        printer.Print("global::Latios.InternalSourceGen.StaticAPI.Create<").Print(field.type.ToFullName()).Print(">(ref state, ").Print(
                            field.boolValue.Value ? "true" : "false").Print(")");
                        break;
                    case LatiosApiSemanticsExtractor.FieldInitKind.BuiltinWithBool:
                        printer.Print("state.").Print(field.builtinGetterMethodName).Print("<").Print(GetSoleTypeArgumentFullName(field.type)).Print(">(").Print(
                            field.boolValue.Value ? "true" : "false").Print(")");
                        break;
                    case LatiosApiSemanticsExtractor.FieldInitKind.BuiltinNoBool:
                        printer.Print("state.").Print(field.builtinGetterMethodName).Print("()");
                        break;
                }
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

        static string GetSoleTypeArgumentFullName(ITypeSymbol type)
        {
            if (type is INamedTypeSymbol named && named.TypeArguments.Length == 1)
                return named.TypeArguments[0].ToFullName();
            return type.ToFullName();
        }
    }
}

