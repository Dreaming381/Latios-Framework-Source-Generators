using System.Collections.Generic;
using LatiosFramework.SourceGen;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LatiosFramework.Unika.SourceGen
{
    internal static class AutoAuthoringCodeWriter
    {
        public enum FieldKind
        {
            Mirror,
            ScriptRef,
            InterfaceRef,
            Entity,
        }

        public struct FieldInfo
        {
            public string       fieldName;
            public bool         isPublic;
            public string       authoringFieldTypeName;
            public string       scriptFieldTypeName;
            public List<string> attributeTexts;
            public FieldKind    kind;
        }

        public struct Context
        {
            public string                  scriptFullTypeName;
            public List<FieldInfo>         fields;
            public List<string>            usingDirectiveTexts;
            public StructDeclarationSyntax scriptDeclarationSyntax;
            public List<string>            baseUnikaInterfaceNames;
        }

        const string kApplyPrivateFieldsMethodName = "__UnikaAutoAuthoring_ApplyNonPublicFields";

        public static string WriteAutoAuthoringCode(ClassDeclarationSyntax authoringDeclaration, ref Context context)
        {
            List<FieldInfo> nonPublicFields = new List<FieldInfo>();
            foreach (var field in context.fields)
            {
                if (!field.isPublic)
                    nonPublicFields.Add(field);
            }

            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, authoringDeclaration.Parent);
            var printer      = scopePrinter.Printer;
            // The copied field attributes (e.g. [Range], [Tooltip]) are only valid if whatever namespaces
            // they came from in the original script file are also imported here, since this generated file
            // has none of the original file's usings otherwise.
            foreach (var usingDirectiveText in context.usingDirectiveTexts)
                printer.PrintLine(usingDirectiveText);
            scopePrinter.Printer = printer;
            scopePrinter.PrintOpen(false);
            printer = scopePrinter.Printer;
            printer.PrintLine("[global::System.Runtime.CompilerServices.CompilerGenerated]");
            printer.PrintBeginLine();
            foreach (var m in authoringDeclaration.Modifiers)
                printer.Print(m.ToString()).Print(" ");
            printer.Print("class ").Print(authoringDeclaration.Identifier.Text);
            if (context.baseUnikaInterfaceNames.Count == 0)
                printer.PrintEndLine();
            else
            {
                printer.PrintEndLine(" :");
                int remaining = context.baseUnikaInterfaceNames.Count;
                foreach (var i in context.baseUnikaInterfaceNames)
                {
                    remaining--;
                    printer.PrintBeginLine("global::Latios.Unika.Authoring.InternalSourceGen.StaticAPI.IUnikaInterfaceAuthoringImpl<").Print(i).Print(".InterfaceRef, ")
                    .Print(context.scriptFullTypeName).PrintEndLine(remaining == 0 ? ">" : ">,");
                }
            }
            printer.OpenScope();

            foreach (var field in context.fields)
            {
                foreach (var attributeText in field.attributeTexts)
                    printer.PrintLine(attributeText);
                printer.PrintBeginLine(field.isPublic ? "public " : "[global::UnityEngine.SerializeField] private ");
                printer.Print(field.authoringFieldTypeName).Print(" ").Print(field.fieldName).PrintEndLine(";");
            }

            printer.PrintEndLine();
            printer.PrintLine(
                "public override void Bake(global::Unity.Entities.IBaker baker, ref global::Latios.Unika.Authoring.AuthoredScriptAssignment toAssign, global::Unity.Entities.Entity smartPostProcessTarget)");
            printer.OpenScope();
            printer.PrintBeginLine("var script = new ").Print(context.scriptFullTypeName).PrintEndLine("();");
            foreach (var field in context.fields)
            {
                // Non-public script fields are assigned via a generated helper method on the script struct
                // itself, since this authoring class cannot access another type's private fields directly.
                if (!field.isPublic)
                    continue;
                printer.PrintBeginLine("script.").Print(field.fieldName).Print(" = ");
                PrintFieldValueExpression(printer, field);
                printer.PrintEndLine(";");
            }
            if (nonPublicFields.Count > 0)
            {
                printer.PrintBeginLine("script.").Print(kApplyPrivateFieldsMethodName).Print("(");
                for (int i = 0; i < nonPublicFields.Count; i++)
                {
                    if (i > 0)
                        printer.Print(", ");
                    PrintFieldValueExpression(printer, nonPublicFields[i]);
                }
                printer.PrintEndLine(");");
            }
            printer.PrintLine("OnAutoBake(baker, ref script, ref toAssign, smartPostProcessTarget);");
            printer.PrintLine("toAssign.Assign(ref script);");
            printer.CloseScope();

            printer.CloseScope();
            scopePrinter.PrintClose();
            printer = scopePrinter.Printer;

            if (nonPublicFields.Count > 0)
            {
                var scriptScopePrinter = new SyntaxNodeScopePrinter(printer, context.scriptDeclarationSyntax.Parent);
                scriptScopePrinter.PrintOpen(false);
                printer = scriptScopePrinter.Printer;
                // Note: no [CompilerGenerated] here - ScriptGenerator's own partial for this same
                // struct type already carries one, and the attribute does not allow duplicates.
                printer.PrintBeginLine();
                foreach (var m in context.scriptDeclarationSyntax.Modifiers)
                    printer.Print(m.ToString()).Print(" ");
                printer.Print("struct ").PrintEndLine(context.scriptDeclarationSyntax.Identifier.Text);
                printer.OpenScope();

                printer.PrintBeginLine("internal void ").Print(kApplyPrivateFieldsMethodName).Print("(");
                for (int i = 0; i < nonPublicFields.Count; i++)
                {
                    if (i > 0)
                        printer.Print(", ");
                    printer.Print(nonPublicFields[i].scriptFieldTypeName).Print(" ").Print(nonPublicFields[i].fieldName);
                }
                printer.PrintEndLine(")");
                printer.OpenScope();
                foreach (var field in nonPublicFields)
                    printer.PrintBeginLine("this.").Print(field.fieldName).Print(" = ").Print(field.fieldName).PrintEndLine(";");
                printer.CloseScope();

                printer.CloseScope();
                scriptScopePrinter.PrintClose();
                printer = scriptScopePrinter.Printer;
            }

            return printer.Result;
        }

        static void PrintFieldValueExpression(Printer printer, in FieldInfo field)
        {
            switch (field.kind)
            {
                case FieldKind.Mirror:
                    printer.Print(field.fieldName);
                    break;
                case FieldKind.ScriptRef:
                    printer.Print("baker.GetScriptRefOrDefaultFrom(").Print(field.fieldName).Print(")");
                    break;
                case FieldKind.InterfaceRef:
                    printer.Print("baker.GetInterfaceRefOrDefaultFrom(").Print(field.fieldName).Print(")");
                    break;
                case FieldKind.Entity:
                    printer.Print(field.fieldName).Print(" == null ? default : baker.GetEntity(").Print(field.fieldName)
                    .Print(", global::Unity.Entities.TransformUsageFlags.None)");
                    break;
            }
        }
    }
}
