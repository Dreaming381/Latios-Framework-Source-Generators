using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LatiosFramework.SourceGen
{
    internal static class VStructCodeWriter
    {
        public struct BodyContext
        {
            public string       objShortName;
            public List<string> interfaceNames;
        }

        public static string WriteObjCode(StructDeclarationSyntax objDeclaration, ref BodyContext bodyContext)
        {
            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, objDeclaration.Parent);
            scopePrinter.PrintOpen(true);
            var printer = scopePrinter.Printer;
            printer.PrintLine("[global::System.Runtime.CompilerServices.CompilerGenerated]");
            printer.PrintLine("[global::Unity.Burst.BurstCompile]");
            printer.PrintBeginLine();
            foreach (var m in objDeclaration.Modifiers)
                printer.Print(m.ToString()).Print(" ");
            printer.Print("struct ").PrintEndLine(objDeclaration.Identifier.Text);
            printer.OpenScope();
            PrintBody(ref printer, ref bodyContext);
            printer.CloseScope();
            scopePrinter.PrintClose();
            return scopePrinter.Printer.Result;
        }

        static void PrintBody(ref Printer printer, ref BodyContext context)
        {
            foreach (var i in context.interfaceNames)
            {
                var swapped = i.Replace("::", "_").Replace('.', '_');

                printer.PrintLine("[global::AOT.MonoPInvokeCallback(typeof(global::Latios.Unsafe.InternalSourceGen.StaticAPI.BurstDispatchVptrDelegate))]");
                printer.PrintLine("[global::UnityEngine.Scripting.Preserve]");
                printer.PrintLine("[global::Unity.Burst.BurstCompile]");
                printer.PrintBeginLine("public static void __BurstDispatch_").Print(swapped).PrintEndLine(
                    "(global::Latios.Unsafe.InternalSourceGen.StaticAPI.ContextPtr context, int operation)");
                printer.OpenScope();
                printer.PrintBeginLine(i).Print(".__Dispatch<").Print(context.objShortName).PrintEndLine(">(context, operation);");
                printer.CloseScope();
            }
            printer.PrintBeginLine().PrintEndLine();
            printer.PrintLine("[global::UnityEngine.Scripting.Preserve]");
            printer.PrintLine("public static void __Initialize()");
            {
                printer.OpenScope();
                foreach (var i in context.interfaceNames)
                {
                    var swapped = i.Replace("::", "_").Replace('.', '_');

                    printer.OpenScope();
                    printer.PrintBeginLine(
                        "var functionPtr = global::Unity.Burst.BurstCompiler.CompileFunctionPointer<global::Latios.Unsafe.InternalSourceGen.StaticAPI.BurstDispatchVptrDelegate>(__BurstDispatch_")
                    .Print(swapped).PrintEndLine(");");
                    printer.PrintBeginLine("global::Latios.Unsafe.InternalSourceGen.StaticAPI.RegisterVptrFunction<").Print(i).Print(", ").Print(context.objShortName).PrintEndLine(
                        ">(functionPtr);");
                    printer.CloseScope();
                }
                printer.CloseScope();
            }
            printer.PrintBeginLine().PrintEndLine();
            printer.PrintLine("void global::Latios.Unsafe.IVInterface.__ThisMethodIsSupposedToBeGeneratedByASourceGenerator()");
            printer.OpenScope();
            printer.CloseScope();
        }
    }
}

