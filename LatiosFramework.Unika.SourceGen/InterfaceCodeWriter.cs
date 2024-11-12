using System;
using System.Collections.Generic;
using LatiosFramework.SourceGen;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LatiosFramework.Unika.SourceGen
{
    public static class InterfaceCodeWriter
    {
        public struct BodyContext
        {
            public string                  interfaceShortName;
            public List<string>            baseUnikaInterfaceNames;
            public List<MethodDescription> methods;
        }

        public struct MethodDescription
        {
            public struct Arg
            {
                public string                         argFullTypeName;
                public string                         argVariableName;
                public Microsoft.CodeAnalysis.RefKind argMod;
            }

            public string                         methodName;
            public string                         fullExplicitInterfaceNameIfRequired;
            public string                         accessibility;
            public List<Arg>                      arguments;
            public string                         returnFullTypeNameIfNotVoid;
            public Microsoft.CodeAnalysis.RefKind returnMod;
        }

        public static string WriteInterfaceCode(InterfaceDeclarationSyntax scriptDeclaration, ref BodyContext bodyContext)
        {
            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, scriptDeclaration.Parent);
            scopePrinter.PrintOpen(false);
            var printer = scopePrinter.Printer;
            printer.PrintLine("[global::System.Runtime.CompilerServices.CompilerGenerated]");
            printer.PrintBeginLine();
            foreach (var m in scriptDeclaration.Modifiers)
                printer.Print(m.ToString()).Print(" ");
            printer.Print("interface ").Print(scriptDeclaration.Identifier.Text).PrintEndLine(" : global::Latios.Unika.InternalSourceGen.StaticAPI.IUnikaInterfaceSourceGenerated");
            printer.OpenScope();
            PrintBody(ref printer, ref bodyContext);
            printer.CloseScope();
            scopePrinter.PrintClose();
            return scopePrinter.Printer.Result;
        }

        static void PrintBody(ref Printer printer, ref BodyContext context)
        {
            string surfaceDeclaration = context.baseUnikaInterfaceNames.Count > 0 ? "new public " : "public ";
            printer.PrintBeginLine(surfaceDeclaration).Print("struct Interface : global::Latios.Unika.InternalSourceGen.StaticAPI.IInterfaceDataTyped<").Print(
                context.interfaceShortName).PrintEndLine(
                ", Interface>,");
            printer.PrintBeginLine("    ").Print(context.interfaceShortName).PrintEndLine(",");
            printer.PrintLine("    global::System.IEquatable<Interface>,");
            printer.PrintLine("    global::System.IComparable<Interface>,");
            printer.PrintLine("    global::System.IEquatable<InterfaceRef>,");
            printer.PrintLine("    global::System.IComparable<InterfaceRef>,");
            foreach (var b in context.baseUnikaInterfaceNames)
            {
                printer.PrintBeginLine("    global::System.IEquatable<").Print(b).PrintEndLine(".Interface>,");
                printer.PrintBeginLine("    global::System.IComparable<").Print(b).PrintEndLine(".Interface>,");
                printer.PrintBeginLine("    global::System.IEquatable<").Print(b).PrintEndLine(".InterfaceRef>,");
                printer.PrintBeginLine("    global::System.IComparable<").Print(b).PrintEndLine(".InterfaceRef>,");
            }
            printer.PrintLine("    global::System.IEquatable<global::Latios.Unika.Script>,");
            printer.PrintLine("    global::System.IComparable<global::Latios.Unika.Script>,");
            printer.PrintLine("    global::System.IEquatable<global::Latios.Unika.ScriptRef>,");
            printer.PrintLine("    global::System.IComparable<global::Latios.Unika.ScriptRef>");
            {
                printer.OpenScope();
                printer.PrintLine("global::Latios.Unika.InternalSourceGen.StaticAPI.InterfaceData data;");
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public global::Unity.Entities.Entity entity => data.entity;");
                printer.PrintLine("public global::Latios.Unika.EntityScriptCollection allScripts => data.allScripts;");
                printer.PrintLine("public int indexInEntity => data.indexInEntity;");
                printer.PrintLine("public byte userByte { get => data.userByte; set => data.userByte = value; }");
                printer.PrintLine("public bool userFlagA { get => data.userFlagA; set => data.userFlagA = value; }");
                printer.PrintLine("public bool userFlagB { get => data.userFlagB; set => data.userFlagB = value; }");
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public static implicit operator InterfaceRef(Interface derived) =>                  derived.data.ToRef<InterfaceRef>();");
                printer.PrintLine("public static implicit operator global::Latios.Unika.Script(Interface derived) =>    derived.data.ToScript();");
                printer.PrintLine("public static implicit operator global::Latios.Unika.ScriptRef(Interface derived) => derived.data.ToScript();");
                foreach (var b in context.baseUnikaInterfaceNames)
                {
                    printer.PrintBeginLine("public static implicit operator ").Print(b).PrintEndLine(".Interface(Interface derived)");
                    printer.OpenScope();
                    printer.PrintBeginLine("return global::Latios.Unika.InternalSourceGen.StaticAPI.DownCast<").Print(b).Print(".Interface, ").Print(b).PrintEndLine(
                        ">(derived.data);");
                    printer.CloseScope();
                    printer.PrintBeginLine("public static implicit operator ").Print(b).PrintEndLine(".InterfaceRef(Interface derived)");
                    printer.OpenScope();
                    printer.PrintBeginLine("return derived.data.ToRef<").Print(b).PrintEndLine(".InterfaceRef>();");
                    printer.CloseScope();
                }
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine(
                    "public static bool operator ==(Interface lhs, Interface rhs) =>                     (global::Latios.Unika.Script)lhs    == (global::Latios.Unika.Script)rhs;");
                printer.PrintLine(
                    "public static bool operator !=(Interface lhs, Interface rhs) =>                     (global::Latios.Unika.Script)lhs    != (global::Latios.Unika.Script)rhs;");
                printer.PrintLine(
                    "public static bool operator ==(Interface lhs, InterfaceRef rhs) =>                  (global::Latios.Unika.ScriptRef)lhs == (global::Latios.Unika.ScriptRef)rhs;");
                printer.PrintLine(
                    "public static bool operator !=(Interface lhs, InterfaceRef rhs) =>                  (global::Latios.Unika.ScriptRef)lhs != (global::Latios.Unika.ScriptRef)rhs;");
                printer.PrintLine("public static bool operator ==(Interface lhs, global::Latios.Unika.Script rhs) =>    (global::Latios.Unika.Script)lhs    == rhs;");
                printer.PrintLine("public static bool operator !=(Interface lhs, global::Latios.Unika.Script rhs) =>    (global::Latios.Unika.Script)lhs    != rhs;");
                printer.PrintLine("public static bool operator ==(Interface lhs, global::Latios.Unika.ScriptRef rhs) => (global::Latios.Unika.ScriptRef)lhs == rhs;");
                printer.PrintLine("public static bool operator !=(Interface lhs, global::Latios.Unika.ScriptRef rhs) => (global::Latios.Unika.ScriptRef)lhs != rhs;");
                foreach (var b in context.baseUnikaInterfaceNames)
                {
                    printer.PrintBeginLine("public static bool operator ==(Interface lhs, ").Print(b).PrintEndLine(
                        ".Interface rhs) =>    (global::Latios.Unika.Script)lhs ==    (global::Latios.Unika.Script)rhs;");
                    printer.PrintBeginLine("public static bool operator !=(Interface lhs, ").Print(b).PrintEndLine(
                        ".Interface rhs) =>    (global::Latios.Unika.Script)lhs !=    (global::Latios.Unika.Script)rhs;");
                    printer.PrintBeginLine("public static bool operator ==(Interface lhs, ").Print(b).PrintEndLine(
                        ".InterfaceRef rhs) => (global::Latios.Unika.ScriptRef)lhs == (global::Latios.Unika.ScriptRef)rhs;");
                    printer.PrintBeginLine("public static bool operator !=(Interface lhs, ").Print(b).PrintEndLine(
                        ".InterfaceRef rhs) => (global::Latios.Unika.ScriptRef)lhs != (global::Latios.Unika.ScriptRef)rhs;");
                }
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public int CompareTo(Interface other) =>                      ((global::Latios.Unika.Script)this).CompareTo((global::Latios.Unika.Script)other);");
                printer.PrintLine(
                    "public int CompareTo(InterfaceRef other) =>                   ((global::Latios.Unika.ScriptRef)this).CompareTo((global::Latios.Unika.ScriptRef)other);");
                printer.PrintLine("public int CompareTo(global::Latios.Unika.Script other) =>    ((global::Latios.Unika.Script)this).CompareTo(other);");
                printer.PrintLine("public int CompareTo(global::Latios.Unika.ScriptRef other) => ((global::Latios.Unika.ScriptRef)this).CompareTo(other);");
                foreach (var b in context.baseUnikaInterfaceNames)
                {
                    printer.PrintBeginLine("public int CompareTo(").Print(b).PrintEndLine(
                        ".Interface other) =>    ((global::Latios.Unika.Script)this).CompareTo((global::Latios.Unika.Script)other);");
                    printer.PrintBeginLine("public int CompareTo(").Print(b).PrintEndLine(
                        ".InterfaceRef other) => ((global::Latios.Unika.ScriptRef)this).CompareTo((global::Latios.Unika.ScriptRef)other);");
                }
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public bool Equals(Interface other) =>                     ((global::Latios.Unika.Script)this).Equals((global::Latios.Unika.Script)other);");
                printer.PrintLine("public bool Equals(InterfaceRef other) =>                  ((global::Latios.Unika.ScriptRef)this).Equals((global::Latios.Unika.ScriptRef)other);");
                printer.PrintLine("public bool Equals(global::Latios.Unika.Script other) =>    ((global::Latios.Unika.Script)this).Equals(other);");
                printer.PrintLine("public bool Equals(global::Latios.Unika.ScriptRef other) => ((global::Latios.Unika.ScriptRef)this).Equals(other);");
                foreach (var b in context.baseUnikaInterfaceNames)
                {
                    printer.PrintBeginLine("public bool Equals(").Print(b).PrintEndLine(
                        ".Interface other) =>    ((global::Latios.Unika.Script)this).Equals((global::Latios.Unika.Script)other);");
                    printer.PrintBeginLine("public bool Equals(").Print(b).PrintEndLine(
                        ".InterfaceRef other) => ((global::Latios.Unika.ScriptRef)this).Equals((global::Latios.Unika.ScriptRef)other);");
                }
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public override bool Equals(object obj) =>                              ((global::Latios.Unika.Script)this).Equals(obj);");
                printer.PrintLine("public override int GetHashCode() =>                                    ((global::Latios.Unika.Script)this).GetHashCode();");
                printer.PrintLine("public override string ToString() =>                                    ((global::Latios.Unika.Script)this).ToString();");
                printer.PrintLine("public global::Unity.Collections.FixedString128Bytes ToFixedString() => ((global::Latios.Unika.Script)this).ToFixedString();");
                printer.PrintLine("public static Interface Null =>                                         default;");
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public global::Latios.Unika.Script ToScript() => this;");
                printer.PrintLine("global::Latios.Unika.ScriptRef global::Latios.Unika.IScriptExtensionsApi.ToRef() => this;");
                printer.PrintBeginLine("Interface global::Latios.Unika.InternalSourceGen.StaticAPI.IInterfaceDataTyped<").Print(context.interfaceShortName).PrintEndLine(
                    ", Interface>.assign { set => this.data = value.data; }");
                printer.PrintBeginLine().PrintEndLine();
                PrintPack(ref printer, ref context);
                printer.CloseScope();
            }

            printer.PrintBeginLine(surfaceDeclaration).PrintEndLine("struct InterfaceRef : global::Latios.Unika.InternalSourceGen.StaticAPI.IInterfaceRefData,");
            printer.PrintBeginLine("    global::System.IEquatable<InterfaceRef>,");
            printer.PrintBeginLine("    global::System.IComparable<InterfaceRef>,");
            foreach (var b in context.baseUnikaInterfaceNames)
            {
                printer.PrintBeginLine("    global::System.IEquatable<").Print(b).PrintEndLine(".InterfaceRef>,");
                printer.PrintBeginLine("    global::System.IComparable<").Print(b).PrintEndLine(".InterfaceRef>,");
            }
            printer.PrintLine("    global::System.IEquatable<global::Latios.Unika.ScriptRef>,");
            printer.PrintLine("    global::System.IComparable<global::Latios.Unika.ScriptRef>");
            {
                printer.OpenScope();
                printer.PrintLine("global::Latios.Unika.InternalSourceGen.StaticAPI.InterfaceRefData data;");
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public global::Unity.Entities.Entity entity => data.entity;");
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public bool TryResolve(in global::Latios.Unika.EntityScriptCollection allScripts, out Interface script)");
                printer.OpenScope();
                printer.PrintLine("return global::Latios.Unika.InternalSourceGen.StaticAPI.TryResolve<Interface>(ref data, in allScripts, out script);");
                printer.CloseScope();
                printer.PrintLine(
                    "public bool TryResolve<TResolver>(ref TResolver resolver, out Interface script) where TResolver : unmanaged, global::Latios.Unika.IScriptResolverBase");
                printer.OpenScope();
                printer.PrintLine("return global::Latios.Unika.InternalSourceGen.StaticAPI.TryResolve<Interface, TResolver>(ref data, ref resolver, out script);");
                printer.CloseScope();
                printer.PrintLine("public Interface Resolve(in global::Latios.Unika.EntityScriptCollection allScripts)");
                printer.OpenScope();
                printer.PrintLine("return global::Latios.Unika.InternalSourceGen.StaticAPI.Resolve<Interface>(ref data, in allScripts);");
                printer.CloseScope();
                printer.PrintLine("public Interface Resolve<TResolver>(ref TResolver resolver) where TResolver : unmanaged, global::Latios.Unika.IScriptResolverBase");
                printer.OpenScope();
                printer.PrintLine("return global::Latios.Unika.InternalSourceGen.StaticAPI.Resolve<Interface, TResolver>(ref data, ref resolver);");
                printer.CloseScope();
                printer.PrintLine("public static implicit operator global::Latios.Unika.ScriptRef(InterfaceRef derived) => derived.data.ToScriptRef();");
                foreach (var b in context.baseUnikaInterfaceNames)
                {
                    printer.PrintBeginLine("public static implicit operator ").Print(b).PrintEndLine(".InterfaceRef(InterfaceRef derived)");
                    printer.OpenScope();
                    printer.PrintBeginLine("return global::Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<InterfaceRef, ").Print(b).Print(".InterfaceRef>(ref derived);");
                    printer.CloseScope();
                }
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine(
                    "public static bool operator ==(InterfaceRef lhs, InterfaceRef rhs) =>                  (global::Latios.Unika.ScriptRef)lhs == (global::Latios.Unika.ScriptRef)rhs;");
                printer.PrintLine(
                    "public static bool operator !=(InterfaceRef lhs, InterfaceRef rhs) =>                  (global::Latios.Unika.ScriptRef)lhs != (global::Latios.Unika.ScriptRef)rhs;");
                printer.PrintLine("public static bool operator ==(InterfaceRef lhs, global::Latios.Unika.ScriptRef rhs) => (global::Latios.Unika.ScriptRef)lhs == rhs;");
                printer.PrintLine("public static bool operator !=(InterfaceRef lhs, global::Latios.Unika.ScriptRef rhs) => (global::Latios.Unika.ScriptRef)lhs != rhs;");
                foreach (var b in context.baseUnikaInterfaceNames)
                {
                    printer.PrintBeginLine("public static bool operator ==(InterfaceRef lhs, ").Print(b).PrintEndLine(
                        ".InterfaceRef rhs) => (global::Latios.Unika.ScriptRef)lhs == (global::Latios.Unika.ScriptRef)rhs;");
                    printer.PrintBeginLine("public static bool operator !=(InterfaceRef lhs, ").Print(b).PrintEndLine(
                        ".InterfaceRef rhs) => (global::Latios.Unika.ScriptRef)lhs != (global::Latios.Unika.ScriptRef)rhs;");
                }
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine(
                    "public int CompareTo(InterfaceRef other) =>                  ((global::Latios.Unika.ScriptRef)this).CompareTo((global::Latios.Unika.ScriptRef)other);");
                printer.PrintLine("public int CompareTo(global::Latios.Unika.ScriptRef other) => ((global::Latios.Unika.ScriptRef)this).CompareTo(other);");
                foreach (var b in context.baseUnikaInterfaceNames)
                {
                    printer.PrintBeginLine("public int CompareTo(").Print(b).PrintEndLine(
                        ".InterfaceRef other) => ((global::Latios.Unika.ScriptRef)this).CompareTo((global::Latios.Unika.ScriptRef)other);");
                }
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public bool Equals(InterfaceRef other) =>                  ((global::Latios.Unika.ScriptRef)this).Equals((global::Latios.Unika.ScriptRef)other);");
                printer.PrintLine("public bool Equals(global::Latios.Unika.ScriptRef other) => ((global::Latios.Unika.ScriptRef)this).Equals(other);");
                foreach (var b in context.baseUnikaInterfaceNames)
                {
                    printer.PrintBeginLine("public bool Equals(").Print(b).PrintEndLine(
                        ".InterfaceRef other) => ((global::Latios.Unika.ScriptRef)this).Equals((global::Latios.Unika.ScriptRef)other);");
                }
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public override bool Equals(object obj) =>                              ((global::Latios.Unika.ScriptRef)this).Equals(obj);");
                printer.PrintLine("public override int GetHashCode() =>                                    ((global::Latios.Unika.ScriptRef)this).GetHashCode();");
                printer.PrintLine("public override string ToString() =>                                    ((global::Latios.Unika.ScriptRef)this).ToString();");
                printer.PrintLine("public global::Unity.Collections.FixedString128Bytes ToFixedString() => ((global::Latios.Unika.ScriptRef)this).ToFixedString();");
                printer.PrintLine("public static InterfaceRef Null =>                                      default;");
                printer.PrintBeginLine().PrintEndLine();

                printer.CloseScope();
            }

            printer.PrintLine("[global::UnityEngine.Scripting.Preserve]");
            printer.PrintBeginLine(surfaceDeclaration).Print("static void __Initialize() => global::Latios.Unika.InternalSourceGen.StaticAPI.InitializeInterface<").Print(
                context.interfaceShortName).
            PrintEndLine(">();");
            printer.PrintBeginLine().PrintEndLine();
            PrintUnpack(ref printer, ref context);
        }

        static void PrintPack(ref Printer printer, ref BodyContext context)
        {
            for (int i = 0; i < context.methods.Count; i++)
            {
                var  method    = context.methods[i];
                bool hasReturn = !string.IsNullOrEmpty(method.returnFullTypeNameIfNotVoid);
                printer.PrintLine("/// <inheritdoc />");
                var p                  = printer.PrintBeginLine(method.accessibility);
                p                      = hasReturn ? p.Print(method.returnFullTypeNameIfNotVoid) : p.Print("void");
                p                      = p.Print(" ");
                p                      = string.IsNullOrEmpty(method.fullExplicitInterfaceNameIfRequired) ? p : p.Print(method.fullExplicitInterfaceNameIfRequired).Print(".");
                p                      = p.Print(method.methodName).Print("(");
                bool argumentPreceeded = false;
                foreach (var arg in method.arguments)
                {
                    if (argumentPreceeded)
                        p = p.Print(", ");
                    if (arg.argMod != Microsoft.CodeAnalysis.RefKind.None)
                    {
                        if (arg.argMod == Microsoft.CodeAnalysis.RefKind.In)
                            p = p.Print("in ");
                        else if (arg.argMod == Microsoft.CodeAnalysis.RefKind.Out)
                            p = p.Print("out ");
                        else if (arg.argMod == Microsoft.CodeAnalysis.RefKind.Ref)
                            p = p.Print("ref ");
                    }
                    p                 = p.Print(arg.argFullTypeName).Print(" ").Print(arg.argVariableName);
                    argumentPreceeded = true;
                }
                p.PrintEndLine(")");
                {
                    printer.OpenScope();
                    int argCounter = 0;
                    foreach (var arg in method.arguments)
                    {
                        if (arg.argMod == Microsoft.CodeAnalysis.RefKind.None)
                        {
                            printer.PrintBeginLine($"var arg{argCounter} = ").Print(arg.argVariableName).PrintEndLine(";");
                        }
                        else if (arg.argMod == Microsoft.CodeAnalysis.RefKind.In)
                        {
                            printer.PrintBeginLine($"ref var arg{argCounter} = ref global::Unity.Collections.LowLevel.Unsafe.UnsafeUtilityExtensions.AsRef(in ").Print(
                                arg.argVariableName).PrintEndLine(");");
                        }
                        else if (arg.argMod == Microsoft.CodeAnalysis.RefKind.Out)
                        {
                            printer.PrintBeginLine(arg.argVariableName).PrintEndLine(" = default;");
                            printer.PrintBeginLine($"ref var arg{argCounter} = ref ").Print(arg.argVariableName).PrintEndLine(";");
                        }
                        else if (arg.argMod == Microsoft.CodeAnalysis.RefKind.Ref)
                        {
                            printer.PrintBeginLine($"ref var arg{argCounter} = ref ").Print(arg.argVariableName).PrintEndLine(";");
                        }
                        argCounter++;
                    }
                    if (hasReturn)
                    {
                        if (method.returnMod == Microsoft.CodeAnalysis.RefKind.None)
                            printer.PrintBeginLine(method.returnFullTypeNameIfNotVoid).PrintEndLine(" ret = default");
                        else
                            printer.PrintLine("global::Latios.Unika.InternalSourceGen.StaticAPI.ContextPtr ret = default;");
                    }
                    p = printer.PrintBeginLine($"global::Latios.Unika.InternalSourceGen.StaticAPI.Dispatch(ref data, {i}");
                    if (hasReturn)
                        p.Print(", ref ret");
                    argCounter = 0;
                    foreach (var arg in method.arguments)
                    {
                        p = printer.Print($", ref arg{argCounter}");
                        argCounter++;
                    }
                    p.PrintEndLine(");");
                    if (hasReturn)
                    {
                        if (method.returnMod == Microsoft.CodeAnalysis.RefKind.None)
                            printer.PrintLine("return ret;");
                        else
                        {
                            printer.PrintBeginLine("return ref global::Latios.Unika.InternalSourceGen.StaticAPI.ExtractRefReturn<").Print(method.returnFullTypeNameIfNotVoid).
                            PrintEndLine(">(ret);");
                        }
                    }
                    printer.CloseScope();
                }
                printer.PrintBeginLine().PrintEndLine();
            }
        }

        static void PrintUnpack(ref Printer printer, ref BodyContext context)
        {
            string surfaceDeclaration = context.baseUnikaInterfaceNames.Count > 0 ? "new public " : "public ";
            printer.PrintBeginLine(surfaceDeclaration).Print(
                "static void __Dispatch<TScriptType>(global::Latios.Unika.InternalSourceGen.StaticAPI.ContextPtr __context, int __operation) where TScriptType : unmanaged, ")
            .Print(context.interfaceShortName).PrintEndLine(", global::Latios.Unika.IUnikaScript, global::Latios.Unika.InternalSourceGen.StaticAPI.IUnikaScriptSourceGenerated");
            printer.OpenScope();
            printer.PrintLine("switch (__operation)");
            printer.OpenScope();
            for (int i = 0; i < context.methods.Count; i++)
            {
                var method    = context.methods[i];
                var hasReturn = !string.IsNullOrEmpty(method.returnFullTypeNameIfNotVoid);
                printer.PrintLine($"case {i}:");
                printer.OpenScope();

                printer.PrintLine("ref var script = ref global::Latios.Unika.InternalSourceGen.StaticAPI.ExtractScript<TScriptType>(__context);");
                int extractCounter = 0;
                if (hasReturn)
                {
                    if (method.returnMod == Microsoft.CodeAnalysis.RefKind.None)
                    {
                        printer.PrintBeginLine("ref var ret = ref global::Latios.Unika.InternalSourceGen.StaticAPI.ExtractArg0<").Print(method.returnFullTypeNameIfNotVoid).
                        PrintEndLine(">(__context);");
                    }
                    else
                    {
                        printer.PrintLine(
                            "ref var retPtr = ref global::Latios.Unika.InternalSourceGen.StaticAPI.ExtractArg0<global::Latios.Unika.InternalSourceGen.StaticAPI.ContextPtr>(__context);");
                    }
                    extractCounter++;
                }
                foreach (var arg in method.arguments)
                {
                    printer.PrintBeginLine("ref var ").Print(arg.argVariableName).Print($" = ref global::Latios.Unika.InternalSourceGen.StaticAPI.ExtractArg{extractCounter}<").
                    Print(arg.argFullTypeName).PrintEndLine(">(__context);");
                    extractCounter++;
                }
                var p = printer.PrintBeginLine();
                if (hasReturn)
                {
                    if (method.returnMod == Microsoft.CodeAnalysis.RefKind.None)
                        p = p.Print("ret = ");
                    else if (method.returnMod == Microsoft.CodeAnalysis.RefKind.Ref)
                        p = p.Print("ref var ret = ");
                    else if (method.returnMod == Microsoft.CodeAnalysis.RefKind.RefReadOnly)
                        p = p.Print("ref readonly var ret = ");
                }
                p                      = p.Print("script.").Print(method.methodName).Print("(");
                bool argumentPreceeded = false;
                foreach (var arg in method.arguments)
                {
                    if (argumentPreceeded)
                        p = p.Print(", ");
                    if (arg.argMod != Microsoft.CodeAnalysis.RefKind.None)
                    {
                        if (arg.argMod == Microsoft.CodeAnalysis.RefKind.In)
                            p = p.Print("in ");
                        else if (arg.argMod == Microsoft.CodeAnalysis.RefKind.Out)
                            p = p.Print("out ");
                        else if (arg.argMod == Microsoft.CodeAnalysis.RefKind.Ref)
                            p = p.Print("ref ");
                    }
                    p                 = p.Print(arg.argVariableName);
                    argumentPreceeded = true;
                }
                p.PrintEndLine(");");
                if (hasReturn && method.returnMod == Microsoft.CodeAnalysis.RefKind.Ref)
                    printer.PrintLine("retPtr = global::Latios.Unika.InternalSourceGen.StaticAPI.AssignRefReturn(ref ret);");
                else if (hasReturn && method.returnMod == Microsoft.CodeAnalysis.RefKind.RefReadOnly)
                    printer.PrintLine("retPtr = global::Latios.Unika.InternalSourceGen.StaticAPI.AssignRefReadonlyReturn(in ret);");
                printer.PrintLine("break;");
                printer.CloseScope();
            }
            printer.CloseScope();
            printer.CloseScope();
        }
    }
}

