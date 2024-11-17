using System;
using System.Collections.Generic;
using System.Text;
using LatiosFramework.SourceGen;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LatiosFramework.Unika.SourceGen
{
    internal static class AuthoringCodeWriter
    {
        public struct Context
        {
            public string       scriptTypeName;
            public List<string> baseUnikaInterfaceNames;
        }

        public static string WriteAuthoringCode(ClassDeclarationSyntax scriptDeclaration, ref Context context)
        {
            if (context.baseUnikaInterfaceNames.Count == 0)
                return "";
            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, scriptDeclaration.Parent);
            scopePrinter.PrintOpen(false);
            var printer = scopePrinter.Printer;
            printer.PrintLine("[global::System.Runtime.CompilerServices.CompilerGenerated]");
            printer.PrintBeginLine();
            foreach (var m in scriptDeclaration.Modifiers)
                printer.Print(m.ToString()).Print(" ");
            printer.Print("class ").Print(scriptDeclaration.Identifier.Text).PrintEndLine(" :");
            int remaining = context.baseUnikaInterfaceNames.Count;
            foreach (var i in context.baseUnikaInterfaceNames)
            {
                remaining--;
                printer.PrintBeginLine("global::Latios.Unika.Authoring.InternalSourceGen.StaticAPI.IUnikaInterfaceAuthoringImpl<").Print(i).Print(".InterfaceRef, ")
                .Print(context.scriptTypeName).PrintEndLine(remaining == 0 ? ">" : ">,");
            }
            printer.OpenScope();
            printer.CloseScope();
            scopePrinter.PrintClose();
            return scopePrinter.Printer.Result;
        }
    }
}

