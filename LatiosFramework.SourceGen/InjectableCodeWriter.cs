// This file was originally written with Claude.
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LatiosFramework.SourceGen
{
    internal static class InjectableCodeWriter
    {
        public static string WriteInjectableCode(StructDeclarationSyntax structDeclaration, ref InjectableSemanticsExtractor.BodyContext bodyContext)
        {
            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, structDeclaration.Parent);
            scopePrinter.PrintOpen(false);
            var printer = scopePrinter.Printer;

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

        static void PrintBody(ref Printer printer, ref InjectableSemanticsExtractor.BodyContext context)
        {
            // __CreateForApi
            printer.PrintLine("void global::Latios.IInjectable.__CreateForApi(ref global::Unity.Entities.SystemState state)");
            printer.OpenScope();
            foreach (var field in context.injectFields)
            {
                printer.PrintBeginLine("this.").Print(field.fieldSymbol.Name).Print(" = ");
                ILatiosApiCodeWriter.PrintFieldConstructionExpression(ref printer, field.initKind, field.type, field.boolValue, field.builtinGetterMethodName);
                printer.PrintEndLine(";");
            }
            printer.CloseScope();
            printer.PrintBeginLine().PrintEndLine();

            // __Inject<T>
            printer.PrintLine("void global::Latios.IInjectable.__Inject<T>(ref global::Unity.Entities.SystemState state, ref T target)");
            printer.OpenScope();
            printer.PrintBeginLine("ref var typedTarget = ref global::Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<T, ").Print(
                context.structFullName).PrintEndLine(">(ref target);");
            foreach (var field in context.injectFields)
            {
                var name = field.fieldSymbol.Name;
                printer.PrintBeginLine("typedTarget.").Print(name).Print(" = this.").Print(name).PrintEndLine(";");
                switch (field.initKind)
                {
                    case LatiosApiSemanticsExtractor.FieldInitKind.Gettable:
                        printer.PrintBeginLine("global::Latios.InternalSourceGen.StaticAPI.UpdateGettable(ref typedTarget.").Print(name).Print(
                            ", ref state)").PrintEndLine(";");
                        break;
                    case LatiosApiSemanticsExtractor.FieldInitKind.GettableBool:
                        printer.PrintBeginLine("global::Latios.InternalSourceGen.StaticAPI.UpdateGettableBool(ref typedTarget.").Print(name).Print(
                            ", ref state)").PrintEndLine(";");
                        break;
                    default:
                        printer.PrintBeginLine("typedTarget.").Print(name).Print(".Update(ref state)").PrintEndLine(";");
                        break;
                }
            }
            printer.CloseScope();
        }
    }
}
