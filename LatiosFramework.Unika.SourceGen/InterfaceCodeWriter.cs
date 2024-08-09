using System;
using System.Collections.Generic;
using LatiosFramework.SourceGen;

namespace LatiosFramework.Unika.SourceGen
{
    public static class InterfaceCodeWriter
    {
        struct Context
        {
            public string       interfaceShortName;
            public List<string> baseUnikaInterfaceNames;
            public Printer      interfaceImplementationsPrinter;
        }

        static void PrintBody(ref Printer printer, ref Context context)
        {
            printer.PrintBeginLine("public struct Interface : IInterfaceData, ").PrintEndLine(context.interfaceShortName);
            printer.PrintBeginLine("    global::System.IEquatable<Interface>,");
            printer.PrintBeginLine("    global::System.IComparable<Interface>,");
            printer.PrintBeginLine("    global::System.IEquatable<InterfaceRef>,");
            printer.PrintBeginLine("    global::System.IComparable<InterfaceRef>,");
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
                printer.PrintLine("public gloabl::Latios.Unika.EntityScriptCollection allScripts => data.allScripts;");
                printer.PrintLine("public int indexInEntity => data.indexInEntity;");
                printer.PrintLine("public byte userByte { get => data.userByte; set => data.userByte = value; }");
                printer.PrintLine("public bool userFlagA { get => data.userFlagA; set => data.userFlagA = value; }");
                printer.PrintLine("public bool userFlagB { get => data.userFlagB; set => data.userFlagB = value; }");
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public static implicit operator InterfaceRef(Interface derived) => derived.data.ToRef<InterfaceRef>();");
                printer.PrintLine("public static implicit operator Script(Interface derived) => derived.data.ToScript();");
                printer.PrintLine("public static implicit operator ScriptRef(Interface derived) => derived.data.ToScript();");
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
                printer.CloseScope();
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public static bool operator ==(Interface lhs, Interface rhs) => (Script)lhs == (Script)rhs");
                printer.PrintLine("public static bool operator !=(Interface lhs, Interface rhs) => (Script)lhs != (Script)rhs");
                printer.PrintLine("public static bool operator ==(Interface lhs, InterfaceRef rhs) => (ScriptRef)lhs == (ScriptRef)rhs");
                printer.PrintLine("public static bool operator !=(Interface lhs, InterfaceRef rhs) => (ScriptRef)lhs != (ScriptRef)rhs");
                printer.PrintLine("public static bool operator ==(Interface lhs, Script rhs) => (Script)lhs == rhs");
                printer.PrintLine("public static bool operator !=(Interface lhs, Script rhs) => (Script)lhs != rhs");
                printer.PrintLine("public static bool operator ==(Interface lhs, ScriptRef rhs) => (ScriptRef)lhs == rhs");
                printer.PrintLine("public static bool operator !=(Interface lhs, ScriptRef rhs) => (ScriptRef)lhs != rhs");
                foreach (var b in context.baseUnikaInterfaceNames)
                {
                    printer.PrintBeginLine("public static bool operator ==(Interface lhs, ").Print(b).PrintEndLine(".Interface rhs) => (Script)lhs == (Script)rhs");
                    printer.PrintBeginLine("public static bool operator !=(Interface lhs, ").Print(b).PrintEndLine(".Interface rhs) => (Script)lhs != (Script)rhs");
                    printer.PrintBeginLine("public static bool operator ==(Interface lhs, ").Print(b).PrintEndLine(".InterfaceRef rhs) => (ScriptRef)lhs == (ScriptRef)rhs");
                    printer.PrintBeginLine("public static bool operator !=(Interface lhs, ").Print(b).PrintEndLine(".InterfaceRef rhs) => (ScriptRef)lhs != (ScriptRef)rhs");
                }
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public int CompareTo(Interface other) => ((Script)this).CompareTo((Script)other);");
                printer.PrintLine("public int CompareTo(InterfaceRef other) => ((ScriptRef)this).CompareTo((ScriptRef)other);");
                printer.PrintLine("public int CompareTo(Script other) => ((Script)this).CompareTo(other);");
                printer.PrintLine("public int CompareTo(ScriptRef other) => ((ScriptRef)this).CompareTo(other);");
                foreach (var b in context.baseUnikaInterfaceNames)
                {
                    printer.PrintBeginLine("public int CompareTo(").Print(b).PrintEndLine(".Interface other) => ((Script)this).CompareTo((Script)other);");
                    printer.PrintBeginLine("public int CompareTo(").Print(b).PrintEndLine(".InterfaceRef other) => ((ScriptRef)this).CompareTo((ScriptRef)other);");
                }
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public int Equals(Interface other) => ((Script)this).Equals((Script)other);");
                printer.PrintLine("public int Equals(InterfaceRef other) => ((ScriptRef)this).Equals((ScriptRef)other);");
                printer.PrintLine("public int Equals(Script other) => ((Script)this).Equals(other);");
                printer.PrintLine("public int Equals(ScriptRef other) => ((ScriptRef)this).Equals(other);");
                foreach (var b in context.baseUnikaInterfaceNames)
                {
                    printer.PrintBeginLine("public int Equals(").Print(b).PrintEndLine(".Interface other) => ((Script)this).Equals((Script)other);");
                    printer.PrintBeginLine("public int Equals(").Print(b).PrintEndLine(".InterfaceRef other) => ((ScriptRef)this).Equals((ScriptRef)other);");
                }
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public override bool Equals(object obj) => ((Script)this).Equals(obj);");
                printer.PrintLine("public override int GetHashCode() => ((Script)this).GetHashCode();");
                printer.PrintLine("public override string ToString() => ((Script)this).ToString();");
                printer.PrintLine("public global::Unity.Collections.FixedString128Bytes ToFixedString() => ((Script)this).ToFixedString();");
                printer.PrintLine("public static Interface Null => default;");
                printer.PrintBeginLine().PrintEndLine();
                context.interfaceImplementationsPrinter.Builder.Replace(context.interfaceImplementationsPrinter.CurrentIndent, printer.CurrentIndent);
                printer.PrintLine(context.interfaceImplementationsPrinter.Builder.ToString());
                printer.CloseScope();
            }

            printer.PrintLine("public struct InterfaceRef : IInterfaceRefData,");
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
                printer.PrintLine("public static implicit operator ScriptRef(Interface derived) => derived.data.ToScriptRef();");
                foreach (var b in context.baseUnikaInterfaceNames)
                {
                    printer.PrintBeginLine("public static implicit operator ").Print(b).PrintEndLine(".InterfaceRef(InterfaceRef derived)");
                    printer.OpenScope();
                    printer.PrintBeginLine("return global::Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<InterfaceRef, ").Print(b).Print(".InterfaceRef>(ref derived);");
                    printer.CloseScope();
                }
                printer.CloseScope();
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public static bool operator ==(InterfaceRef lhs, InterfaceRef rhs) => (ScriptRef)lhs == (ScriptRef)rhs");
                printer.PrintLine("public static bool operator !=(InterfaceRef lhs, InterfaceRef rhs) => (ScriptRef)lhs != (ScriptRef)rhs");
                printer.PrintLine("public static bool operator ==(InterfaceRef lhs, ScriptRef rhs) => (ScriptRef)lhs == rhs");
                printer.PrintLine("public static bool operator !=(InterfaceRef lhs, ScriptRef rhs) => (ScriptRef)lhs != rhs");
                foreach (var b in context.baseUnikaInterfaceNames)
                {
                    printer.PrintBeginLine("public static bool operator ==(InterfaceRef lhs, ").Print(b).PrintEndLine(".InterfaceRef rhs) => (ScriptRef)lhs == (ScriptRef)rhs");
                    printer.PrintBeginLine("public static bool operator !=(InterfaceRef lhs, ").Print(b).PrintEndLine(".InterfaceRef rhs) => (ScriptRef)lhs != (ScriptRef)rhs");
                }
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public int CompareTo(InterfaceRef other) => ((ScriptRef)this).CompareTo((ScriptRef)other);");
                printer.PrintLine("public int CompareTo(ScriptRef other) => ((ScriptRef)this).CompareTo(other);");
                foreach (var b in context.baseUnikaInterfaceNames)
                {
                    printer.PrintBeginLine("public int CompareTo(").Print(b).PrintEndLine(".InterfaceRef other) => ((ScriptRef)this).CompareTo((ScriptRef)other);");
                }
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public int Equals(InterfaceRef other) => ((ScriptRef)this).Equals((ScriptRef)other);");
                printer.PrintLine("public int Equals(ScriptRef other) => ((ScriptRef)this).Equals(other);");
                foreach (var b in context.baseUnikaInterfaceNames)
                {
                    printer.PrintBeginLine("public int Equals(").Print(b).PrintEndLine(".InterfaceRef other) => ((ScriptRef)this).Equals((ScriptRef)other);");
                }
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public override bool Equals(object obj) => ((ScriptRef)this).Equals(obj);");
                printer.PrintLine("public override int GetHashCode() => ((ScriptRef)this).GetHashCode();");
                printer.PrintLine("public override string ToString() => ((ScriptRef)this).ToString();");
                printer.PrintLine("public global::Unity.Collections.FixedString128Bytes ToFixedString() => ((ScriptRef)this).ToFixedString();");
                printer.PrintLine("public static InterfaceRef Null => default;");
                printer.PrintBeginLine().PrintEndLine();

                printer.CloseScope();
            }
        }
    }
}

