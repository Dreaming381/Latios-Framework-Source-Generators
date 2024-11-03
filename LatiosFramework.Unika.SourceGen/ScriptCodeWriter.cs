using System;
using System.Collections.Generic;
using LatiosFramework.SourceGen;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LatiosFramework.Unika.SourceGen
{
    internal static class ScriptCodeWriter
    {
        public struct BodyContext
        {
            public string       scriptShortName;
            public List<string> unikaInterfaceNames;
        }

        public struct ExtensionClassContext
        {
            public string       scriptFullName;
            public List<string> unikaInterfaceNames;
            public string       modifier;
        }

        public static string WriteScriptCode(StructDeclarationSyntax scriptDeclaration, ref BodyContext bodyContext, ref ExtensionClassContext extensionClassContext)
        {
            var scopePrinter = new SyntaxNodeAccessModifiedScopePrinter(Printer.DefaultLarge, scriptDeclaration.Parent);
            scopePrinter.PrintOpen(true);
            var printer = scopePrinter.Printer;
            printer.PrintLine("[global::System.Runtime.CompilerServices.CompilerGenerated]");
            printer.PrintLine("[global::Unity.Burst.BurstCompile]");
            printer.PrintBeginLine();
            foreach (var m in scriptDeclaration.Modifiers)
                printer.Print(m.ToString()).Print(" ");
            printer.Print("struct ").Print(scriptDeclaration.Identifier.Text).Print(" : global::Latios.Unika.InternalSourceGen.StaticAPI.IUnikaScriptSourceGenerated");
            printer.OpenScope();
            PrintBody(ref printer, ref bodyContext);
            printer.CloseScope();
            scopePrinter.PrintCloseInner();
            if (scopePrinter.mostRestrictiveAccessType != SyntaxNodeAccessModifiedScopePrinter.AccessType.Private)
            {
                extensionClassContext.modifier = scopePrinter.mostRestrictiveAccessType == SyntaxNodeAccessModifiedScopePrinter.AccessType.Public ? "public" : "internal";
                printer                        = scopePrinter.Printer;
                PrintExtensionClass(ref printer, ref extensionClassContext);
            }
            scopePrinter.PrintCloseOuter();
            return scopePrinter.Printer.Result;
        }

        static void PrintBody(ref Printer printer, ref BodyContext context)
        {
            printer.PrintLine("public struct __DowncastHelper");
            {
                printer.OpenScope();
                printer.PrintBeginLine("global::Latios.Unika.Script<").Print(context.scriptShortName).PrintEndLine("> m_script;");
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintBeginLine("public static implicit operator __DowncastHelper(global::Latios.Unika.Script<").Print(context.scriptShortName)
                .PrintEndLine("> script) => new __DowncastHelper { m_script = script };");

                foreach (var i in context.unikaInterfaceNames)
                {
                    printer.PrintBeginLine().PrintEndLine();
                    printer.PrintBeginLine("public static implicit operator ").Print(i).PrintEndLine(".Interface(__DowncastHelper helper)");
                    printer.OpenScope();
                    printer.PrintBeginLine("return global::Latios.Unika.InternalSourceGen.StaticAPI.DownCast<").Print(i).Print(".Interface, ").Print(i).Print(", ")
                    .Print(context.scriptShortName).PrintEndLine(">(helper.m_script);");
                    printer.CloseScope();
                    printer.PrintBeginLine("public static implicit operator ").Print(i).PrintEndLine(".InterfaceRef(__DowncastHelper helper)");
                    printer.OpenScope();
                    printer.PrintLine("global::Latios.Unika.ScriptRef scriptRef = helper.m_script;");
                    printer.PrintBeginLine("return global::Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<global::Latios.Unika.ScriptRef,").Print(i)
                    .Print(".InterfaceRef>(ref scriptRef);");
                    printer.CloseScope();
                }
                printer.CloseScope();
            }
            printer.PrintBeginLine().PrintEndLine();

            foreach (var i in context.unikaInterfaceNames)
            {
                var swapped = i.Replace("::", "_").Replace('.', '_');

                printer.PrintLine("[global::AOT.MonoPInvokeCallback(typeof(global::Latios.Unika.InternalSourceGen.StaticAPI.BurstDispatchScriptDelegate))]");
                printer.PrintLine("[global::UnityEngine.Scripting.Preserve]");
                printer.PrintLine("[global::Unity.Burst.BurstCompile]");
                printer.PrintBeginLine("public static void __BurstDispatch_").Print(swapped).PrintEndLine(
                    "global::Latios.Unika.InternalSourceGen.StaticAPI.ContextPtr context, int operation)");
                printer.OpenScope();
                printer.PrintBeginLine(i).Print(".__Dispatch<").Print(context.scriptShortName).PrintEndLine(">(context, operation);");
                printer.CloseScope();
            }
            printer.CloseScope();
        }

        static void PrintExtensionClass(ref Printer printer, ref ExtensionClassContext context)
        {
            var swapped = context.scriptFullName.Replace("::", "_").Replace('.', '_');
            printer.PrintBeginLine(context.modifier).Print(" static ").Print(swapped).PrintEndLine("_DowncastExtensions");
            printer.OpenScope();
            printer.PrintBeginLine("public static ").Print(context.scriptFullName).Print(".__DowncastHelper ToInterface(this global::Latios.Unika.Script<")
            .Print(context.scriptFullName).PrintEndLine("> script) => script;");
            printer.CloseScope();
        }
    }
}

