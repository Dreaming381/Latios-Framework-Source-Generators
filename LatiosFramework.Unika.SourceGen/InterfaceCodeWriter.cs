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
            public string                    interfaceShortName;
            public List<string>              baseUnikaInterfaceNames;
            public List<MethodDescription>   methods;
            public List<PropertyDescription> properties;
            public List<IndexerDescription>  indexers;
            public int                       propertyOpCount;
            public int                       indexerOpCount;
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

        public struct PropertyDescription
        {
            public string                         propertyName;
            public string                         fullExplicitInterfaceNameIfRequired;
            public string                         accessibility;
            public string                         propertyFullTypeName;
            public Microsoft.CodeAnalysis.RefKind returnMod;
            public bool                           hasGetter;
            public bool                           hasSetter;
        }

        public struct IndexerDescription
        {
            public struct Arg
            {
                public string argFullTypeName;
                public string argVariableName;
            }

            public string                         fullExplicitInterfaceNameIfRequired;
            public string                         accessibility;
            public string                         propertyFullTypeName;
            public List<Arg>                      arguments;
            public Microsoft.CodeAnalysis.RefKind returnMod;
            public bool                           hasGetter;
            public bool                           hasSetter;
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
            printer.PrintBeginLine(surfaceDeclaration).PrintEndLine("struct Interface : global::Latios.Unika.InternalSourceGen.StaticAPI.IInterfaceData,");
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
                printer.PrintLine("global::Latios.Unika.InternalSourceGen.StaticAPI.InterfaceData __data;");
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public global::Unity.Entities.Entity entity => __data.entity;");
                printer.PrintLine("public global::Latios.Unika.EntityScriptCollection allScripts => __data.allScripts;");
                printer.PrintLine("public int indexInEntity => __data.indexInEntity;");
                printer.PrintLine("public byte userByte { get => __data.userByte; set => __data.userByte = value; }");
                printer.PrintLine("public bool userFlagA { get => __data.userFlagA; set => __data.userFlagA = value; }");
                printer.PrintLine("public bool userFlagB { get => __data.userFlagB; set => __data.userFlagB = value; }");
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public static implicit operator InterfaceRef(Interface derived) =>                  derived.__data.ToRef<InterfaceRef>();");
                printer.PrintLine("public static implicit operator global::Latios.Unika.Script(Interface derived) =>    derived.__data.ToScript();");
                printer.PrintLine("public static implicit operator global::Latios.Unika.ScriptRef(Interface derived) => derived.__data.ToScript();");
                foreach (var b in context.baseUnikaInterfaceNames)
                {
                    printer.PrintBeginLine("public static implicit operator ").Print(b).PrintEndLine(".Interface(Interface derived)");
                    printer.OpenScope();
                    printer.PrintBeginLine("return global::Latios.Unika.InternalSourceGen.StaticAPI.DownCast<").Print(b).Print(".Interface, ").Print(b).PrintEndLine(
                        ">(derived.__data);");
                    printer.CloseScope();
                    printer.PrintBeginLine("public static implicit operator ").Print(b).PrintEndLine(".InterfaceRef(Interface derived)");
                    printer.OpenScope();
                    printer.PrintBeginLine("return derived.__data.ToRef<").Print(b).PrintEndLine(".InterfaceRef>();");
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
                printer.PrintBeginLine(
                    "bool global::Latios.Unika.IScriptTypedExtensionsApi.Is(in global::Latios.Unika.Script script) => global::Latios.Unika.InternalSourceGen.StaticAPI.IsInterface<")
                .Print(context.interfaceShortName).PrintEndLine(">(in script);");
                printer.PrintBeginLine(
                    "bool global::Latios.Unika.IScriptTypedExtensionsApi.TryCastInit(in global::Latios.Unika.Script script, global::Latios.Unika.IScriptTypedExtensionsApi.WrappedThisPtr thisPtr) => global::Latios.Unika.InternalSourceGen.StaticAPI.TryCastInitInterface<")
                .Print(context.interfaceShortName).PrintEndLine(">(in script, thisPtr);");
                printer.PrintBeginLine(
                    "global::Latios.Unika.IScriptTypedExtensionsApi.WrappedIdAndMask global::Latios.Unika.IScriptTypedExtensionsApi.GetIdAndMask() => global::Latios.Unika.InternalSourceGen.StaticAPI.GetIdAndMaskInterface<")
                .Print(context.interfaceShortName).PrintEndLine(">();");
                printer.PrintBeginLine().PrintEndLine();
                PrintPack(ref printer, ref context);
                printer.CloseScope();
            }

            printer.PrintBeginLine(surfaceDeclaration).PrintEndLine("struct InterfaceRef : global::Latios.Unika.InternalSourceGen.StaticAPI.IInterfaceRefData,");
            printer.PrintLine("    global::System.IEquatable<InterfaceRef>,");
            printer.PrintLine("    global::System.IComparable<InterfaceRef>,");
            foreach (var b in context.baseUnikaInterfaceNames)
            {
                printer.PrintBeginLine("    global::System.IEquatable<").Print(b).PrintEndLine(".InterfaceRef>,");
                printer.PrintBeginLine("    global::System.IComparable<").Print(b).PrintEndLine(".InterfaceRef>,");
            }
            printer.PrintLine("    global::System.IEquatable<global::Latios.Unika.ScriptRef>,");
            printer.PrintLine("    global::System.IComparable<global::Latios.Unika.ScriptRef>");
            {
                printer.OpenScope();
                printer.PrintLine("global::Latios.Unika.InternalSourceGen.StaticAPI.InterfaceRefData __data;");
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public global::Unity.Entities.Entity entity => __data.entity;");
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public bool TryResolve(in global::Latios.Unika.EntityScriptCollection allScripts, out Interface script)");
                printer.OpenScope();
                printer.PrintLine("return global::Latios.Unika.InternalSourceGen.StaticAPI.TryResolve<Interface>(ref __data, in allScripts, out script);");
                printer.CloseScope();
                printer.PrintLine(
                    "public bool TryResolve<TResolver>(ref TResolver resolver, out Interface script) where TResolver : unmanaged, global::Latios.Unika.IScriptResolverBase");
                printer.OpenScope();
                printer.PrintLine("return global::Latios.Unika.InternalSourceGen.StaticAPI.TryResolve<Interface, TResolver>(ref __data, ref resolver, out script);");
                printer.CloseScope();
                printer.PrintLine("public Interface Resolve(in global::Latios.Unika.EntityScriptCollection allScripts)");
                printer.OpenScope();
                printer.PrintLine("return global::Latios.Unika.InternalSourceGen.StaticAPI.Resolve<Interface>(ref __data, in allScripts);");
                printer.CloseScope();
                printer.PrintLine("public Interface Resolve<TResolver>(ref TResolver resolver) where TResolver : unmanaged, global::Latios.Unika.IScriptResolverBase");
                printer.OpenScope();
                printer.PrintLine("return global::Latios.Unika.InternalSourceGen.StaticAPI.Resolve<Interface, TResolver>(ref __data, ref resolver);");
                printer.CloseScope();
                printer.PrintLine("public static implicit operator global::Latios.Unika.ScriptRef(InterfaceRef derived) => derived.__data.ToScriptRef();");
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
                printer.PrintLine("global::Latios.Unika.ScriptRef global::Latios.Unika.IScriptRefTypedExtensionsApi.ToScriptRef() => this;");
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
                var p = printer.PrintBeginLine(method.accessibility);
                if (method.returnMod == Microsoft.CodeAnalysis.RefKind.Ref)
                    p = printer.Print("ref ");
                else if (method.returnMod == Microsoft.CodeAnalysis.RefKind.RefReadOnly)
                    p                  = printer.Print("ref readonly ");
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
                            printer.PrintBeginLine(method.returnFullTypeNameIfNotVoid).PrintEndLine(" ret = default;");
                        else
                            printer.PrintLine("global::Latios.Unika.InternalSourceGen.StaticAPI.ContextPtr ret = default;");
                    }
                    p = printer.PrintBeginLine($"global::Latios.Unika.InternalSourceGen.StaticAPI.Dispatch(ref __data, {i}");
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
            int opId = context.methods.Count;
            for (int i = 0; i < context.properties.Count; i++)
            {
                var property = context.properties[i];
                printer.PrintLine("/// <inheritdoc />");
                var p = printer.PrintBeginLine(property.accessibility);
                if (property.returnMod == Microsoft.CodeAnalysis.RefKind.Ref)
                    p = printer.Print("ref ");
                else if (property.returnMod == Microsoft.CodeAnalysis.RefKind.RefReadOnly)
                    p = printer.Print("ref readonly ");
                p     = p.Print(property.propertyFullTypeName).Print(" ");
                p     = string.IsNullOrEmpty(property.fullExplicitInterfaceNameIfRequired) ? p : p.Print(property.fullExplicitInterfaceNameIfRequired).Print(".");
                p.PrintEndLine(property.propertyName);
                printer.OpenScope();
                if (property.hasGetter)
                {
                    printer.PrintLine("get");
                    printer.OpenScope();
                    if (property.returnMod == Microsoft.CodeAnalysis.RefKind.None)
                        printer.PrintBeginLine(property.propertyFullTypeName).PrintEndLine(" ret = default;");
                    else
                        printer.PrintLine("global::Latios.Unika.InternalSourceGen.StaticAPI.ContextPtr ret = default;");
                    printer.PrintLine($"global::Latios.Unika.InternalSourceGen.StaticAPI.Dispatch(ref __data, {i + opId}, ref ret);");
                    if (property.returnMod == Microsoft.CodeAnalysis.RefKind.None)
                        printer.PrintLine("return ret;");
                    else
                    {
                        printer.PrintBeginLine("return ref global::Latios.Unika.InternalSourceGen.StaticAPI.ExtractRefReturn<").Print(property.propertyFullTypeName).
                        PrintEndLine(">(ret);");
                    }
                    printer.CloseScope();
                    opId++;
                }
                if (property.hasSetter)
                {
                    printer.PrintLine("set");
                    printer.OpenScope();
                    printer.PrintLine("var propertyAssignArg = value;");
                    printer.PrintLine($"global::Latios.Unika.InternalSourceGen.StaticAPI.Dispatch(ref __data, {i + opId}, ref propertyAssignArg);");
                    printer.CloseScope();
                    opId++;
                }
                printer.CloseScope();
            }
            for (int i = 0; i < context.indexers.Count; i++)
            {
                var indexer = context.indexers[i];
                printer.PrintLine("/// <inheritdoc />");
                var p = printer.PrintBeginLine(indexer.accessibility);
                if (indexer.returnMod == Microsoft.CodeAnalysis.RefKind.Ref)
                    p = printer.Print("ref ");
                else if (indexer.returnMod == Microsoft.CodeAnalysis.RefKind.RefReadOnly)
                    p = printer.Print("ref readonly ");
                p     = p.Print(indexer.propertyFullTypeName).Print(" this[").Print(indexer.arguments[0].argFullTypeName).Print(" ").Print(indexer.arguments[0].argVariableName);
                for (int j = 1; j < indexer.arguments.Count; j++)
                {
                    p = p.Print(", ").Print(indexer.arguments[j].argFullTypeName).Print(" ").Print(indexer.arguments[j].argVariableName);
                }
                p.PrintEndLine("]");
                printer.OpenScope();
                if (indexer.hasGetter)
                {
                    printer.PrintLine("get");
                    printer.OpenScope();
                    if (indexer.returnMod == Microsoft.CodeAnalysis.RefKind.None)
                        printer.PrintBeginLine(indexer.propertyFullTypeName).PrintEndLine(" ret = default;");
                    else
                        printer.PrintLine("global::Latios.Unika.InternalSourceGen.StaticAPI.ContextPtr ret = default;");
                    for (int j = 0; j < indexer.arguments.Count; j++)
                    {
                        printer.PrintBeginLine($"var arg{j} = ").Print(indexer.arguments[j].argVariableName).PrintEndLine(";");
                    }
                    p = printer.PrintBeginLine($"global::Latios.Unika.InternalSourceGen.StaticAPI.Dispatch(ref __data, {i + opId}, ref ret");
                    for (int j = 0; j < indexer.arguments.Count; j++)
                    {
                        p = p.Print($", ref arg{j}");
                    }
                    p.PrintEndLine(");");
                    if (indexer.returnMod == Microsoft.CodeAnalysis.RefKind.None)
                        printer.PrintLine("return ret;");
                    else
                    {
                        printer.PrintBeginLine("return ref global::Latios.Unika.InternalSourceGen.StaticAPI.ExtractRefReturn<").Print(indexer.propertyFullTypeName).
                        PrintEndLine(">(ret);");
                    }
                    printer.CloseScope();
                    opId++;
                }
                if (indexer.hasSetter)
                {
                    printer.PrintLine("set");
                    printer.OpenScope();
                    printer.PrintLine("var propertyAssignArg = value;");
                    for (int j = 0; j < indexer.arguments.Count; j++)
                    {
                        printer.PrintBeginLine($"var arg{j} = ").Print(indexer.arguments[j].argVariableName).PrintEndLine(";");
                    }
                    printer.PrintLine($"global::Latios.Unika.InternalSourceGen.StaticAPI.Dispatch(ref __data, {i + opId}, ref propertyAssignArg");
                    for (int j = 0; j < indexer.arguments.Count; j++)
                    {
                        p = p.Print($", ref arg{j}");
                    }
                    p.PrintEndLine(");");
                    printer.CloseScope();
                    opId++;
                }
                printer.CloseScope();
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
                        p = p.Print("ref var ret = ref ");
                    else if (method.returnMod == Microsoft.CodeAnalysis.RefKind.RefReadOnly)
                        p = p.Print("ref readonly var ret = ref ");
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
            int opId = context.methods.Count;
            for (int i = 0; i < context.properties.Count; i++)
            {
                var property = context.properties[i];
                if (property.hasGetter)
                {
                    printer.PrintLine($"case {opId}:");
                    printer.OpenScope();
                    printer.PrintLine("ref var script = ref global::Latios.Unika.InternalSourceGen.StaticAPI.ExtractScript<TScriptType>(__context);");
                    if (property.returnMod == Microsoft.CodeAnalysis.RefKind.None)
                    {
                        printer.PrintBeginLine("ref var ret = ref global::Latios.Unika.InternalSourceGen.StaticAPI.ExtractArg0<").Print(property.propertyFullTypeName)
                        .PrintEndLine(">(__context);");
                    }
                    else
                    {
                        printer.PrintLine(
                            "ref var retPtr = ref global::Latios.Unika.InternalSourceGen.StaticAPI.ExtractArg0<global::Latios.Unika.InternalSourceGen.StaticAPI.ContextPtr>(__context);");
                    }
                    var p = printer.PrintBeginLine();
                    if (property.returnMod == Microsoft.CodeAnalysis.RefKind.None)
                        p = p.Print("ret = ");
                    else if (property.returnMod == Microsoft.CodeAnalysis.RefKind.Ref)
                        p = p.Print("ref var ret = ref ");
                    else if (property.returnMod == Microsoft.CodeAnalysis.RefKind.RefReadOnly)
                        p = p.Print("ref readonly var ret = ref ");
                    p.Print("script.").Print(property.propertyName).PrintEndLine(";");
                    if (property.returnMod == Microsoft.CodeAnalysis.RefKind.Ref)
                        printer.PrintLine("retPtr = global::Latios.Unika.InternalSourceGen.StaticAPI.AssignRefReturn(ref ret);");
                    else if (property.returnMod == Microsoft.CodeAnalysis.RefKind.RefReadOnly)
                        printer.PrintLine("retPtr = global::Latios.Unika.InternalSourceGen.StaticAPI.AssignRefReadonlyReturn(in ret);");
                    printer.PrintLine("break;");
                    printer.CloseScope();
                    opId++;
                }
                if (property.hasSetter)
                {
                    printer.PrintLine($"case {opId}:");
                    printer.OpenScope();
                    printer.PrintLine("ref var script = ref global::Latios.Unika.InternalSourceGen.StaticAPI.ExtractScript<TScriptType>(__context);");
                    printer.PrintBeginLine("ref var propertyAssignArg = ref global::Latios.Unika.InternalSourceGen.StaticAPI.ExtractArg0<").Print(property.propertyFullTypeName)
                    .PrintEndLine(">(__context);");
                    printer.PrintBeginLine("script.").Print(property.propertyName).PrintEndLine(" = propertyAssignArg;");
                    printer.PrintLine("break;");
                    printer.CloseScope();
                    opId++;
                }
            }
            for (int i = 0; i < context.indexers.Count; i++)
            {
                var indexer = context.indexers[i];
                if (indexer.hasGetter)
                {
                    printer.PrintLine($"case {opId}:");
                    printer.OpenScope();

                    printer.PrintLine("ref var script = ref global::Latios.Unika.InternalSourceGen.StaticAPI.ExtractScript<TScriptType>(__context);");
                    if (indexer.returnMod == Microsoft.CodeAnalysis.RefKind.None)
                    {
                        printer.PrintBeginLine("ref var ret = ref global::Latios.Unika.InternalSourceGen.StaticAPI.ExtractArg0<").Print(indexer.propertyFullTypeName).
                        PrintEndLine(">(__context);");
                    }
                    else
                    {
                        printer.PrintLine(
                            "ref var retPtr = ref global::Latios.Unika.InternalSourceGen.StaticAPI.ExtractArg0<global::Latios.Unika.InternalSourceGen.StaticAPI.ContextPtr>(__context);");
                    }
                    int extractCounter = 1;
                    foreach (var arg in indexer.arguments)
                    {
                        printer.PrintBeginLine("ref var ").Print(arg.argVariableName).Print($" = ref global::Latios.Unika.InternalSourceGen.StaticAPI.ExtractArg{extractCounter}<").
                        Print(arg.argFullTypeName).PrintEndLine(">(__context);");
                        extractCounter++;
                    }
                    var p = printer.PrintBeginLine();

                    if (indexer.returnMod == Microsoft.CodeAnalysis.RefKind.None)
                        p = p.Print("ret = ");
                    else if (indexer.returnMod == Microsoft.CodeAnalysis.RefKind.Ref)
                        p = p.Print("ref var ret = ref ");
                    else if (indexer.returnMod == Microsoft.CodeAnalysis.RefKind.RefReadOnly)
                        p = p.Print("ref readonly var ret = ref ");
                    p     = p.Print("script[").Print(indexer.arguments[0].argVariableName);
                    for (int j = 1; j < indexer.arguments.Count; j++)
                    {
                        p = p.Print(", ").Print(indexer.arguments[j].argVariableName);
                    }
                    p.PrintEndLine("];");
                    if (indexer.returnMod == Microsoft.CodeAnalysis.RefKind.Ref)
                        printer.PrintLine("retPtr = global::Latios.Unika.InternalSourceGen.StaticAPI.AssignRefReturn(ref ret);");
                    else if (indexer.returnMod == Microsoft.CodeAnalysis.RefKind.RefReadOnly)
                        printer.PrintLine("retPtr = global::Latios.Unika.InternalSourceGen.StaticAPI.AssignRefReadonlyReturn(in ret);");
                    printer.PrintLine("break;");
                    printer.CloseScope();
                    opId++;
                }
                if (indexer.hasSetter)
                {
                    printer.PrintLine($"case {opId}:");
                    printer.OpenScope();

                    printer.PrintLine("ref var script = ref global::Latios.Unika.InternalSourceGen.StaticAPI.ExtractScript<TScriptType>(__context);");
                    printer.PrintBeginLine("ref var propertyAssignArg = ref global::Latios.Unika.InternalSourceGen.StaticAPI.ExtractArg0<").Print(indexer.propertyFullTypeName)
                    .PrintEndLine(">(__context);");
                    int extractCounter = 1;
                    foreach (var arg in indexer.arguments)
                    {
                        printer.PrintBeginLine("ref var ").Print(arg.argVariableName).Print($" = ref global::Latios.Unika.InternalSourceGen.StaticAPI.ExtractArg{extractCounter}<").
                        Print(arg.argFullTypeName).PrintEndLine(">(__context);");
                        extractCounter++;
                    }
                    var p = printer.PrintBeginLine("script[").Print(indexer.arguments[0].argVariableName);
                    for (int j = 1; j < indexer.arguments.Count; j++)
                    {
                        p = p.Print(", ").Print(indexer.arguments[j].argVariableName);
                    }
                    p.PrintEndLine("] = propertyAssignArg;");
                    printer.PrintLine("break;");
                    printer.CloseScope();
                    opId++;
                }
            }
            printer.CloseScope();
            printer.CloseScope();
        }
    }
}

