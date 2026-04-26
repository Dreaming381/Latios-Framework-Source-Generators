using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LatiosFramework.SourceGen
{
    public static class VInterfaceCodeWriter
    {
        public struct BodyContext
        {
            public string                    interfaceShortName;
            public List<string>              baseVInterfaceNames;
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

        public static string WriteInterfaceCode(InterfaceDeclarationSyntax interfaceDeclaration, ref BodyContext bodyContext)
        {
            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, interfaceDeclaration.Parent);
            scopePrinter.PrintOpen(false);
            var printer = scopePrinter.Printer;
            printer.PrintLine("[global::System.Runtime.CompilerServices.CompilerGenerated]");
            printer.PrintBeginLine();
            foreach (var m in interfaceDeclaration.Modifiers)
                printer.Print(m.ToString()).Print(" ");
            printer.Print("interface ").PrintEndLine(interfaceDeclaration.Identifier.Text);
            printer.OpenScope();
            PrintBody(ref printer, ref bodyContext);
            printer.CloseScope();
            PrintStaticClass(ref printer, ref bodyContext);
            scopePrinter.PrintClose();
            return scopePrinter.Printer.Result;
        }

        static void PrintBody(ref Printer printer, ref BodyContext context)
        {
            string surfaceDeclaration = context.baseVInterfaceNames.Count > 0 ? "new public " : "public ";
            printer.PrintBeginLine(surfaceDeclaration).PrintEndLine("struct VPtrFunction : global::System.IEquatable<VPtrFunction>");
            printer.OpenScope();
            printer.PrintLine("global::Unity.Burst.FunctionPointer<global::Latios.Unsafe.InternalSourceGen.StaticAPI.BurstDispatchVptrDelegate> __functionPtr;");
            printer.PrintLine("public bool Equals(VPtrFunction other) => __functionPtr.Value == other.__functionPtr.Value;");
            printer.PrintLine("public override int GetHashCode() => __functionPtr.Value.GetHashCode();");
            printer.CloseScope();
            printer.PrintBeginLine().PrintEndLine();
            printer.PrintLine("[global::Unity.Burst.BurstCompile]");
            printer.PrintBeginLine(surfaceDeclaration).PrintEndLine("struct VPtr : ").Print(context.interfaceShortName).Print(", global::Latios.Unsafe.IVPtrFor<").Print(
                context.interfaceShortName).PrintEndLine(">");
            {
                printer.OpenScope();
                printer.PrintLine("global::Latios.Unsafe.InternalSourceGen.StaticAPI.VPtr __ptr;");
                printer.PrintLine("VPtrFunction __function;");
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public VPtrFunction vptrFunction => __function;");
                printer.PrintLine("public global::Latios.Unsafe.UnsafeApiPointer ptr => __ptr.AsPtr();");
                printer.PrintLine("public VPtr(global::Latios.Unsafe.UnsafeApiPointer pointer, VPtrFunction function)");
                printer.OpenScope();
                printer.PrintLine("__ptr      = global::Latios.Unsafe.InternalSourceGen.StaticAPI.VPtr.Create(pointer);");
                printer.PrintLine("__function = function;");
                printer.CloseScope();
                printer.PrintBeginLine("public static VPtr Create<T>(global::Latios.Unsafe.UnsafeApiPointer<T> pointer) where T : unmanaged, ").PrintEndLine(
                    context.interfaceShortName);
                printer.OpenScope();
                printer.PrintBeginLine("var functionPtr = global::Latios.Unsafe.InternalSourceGen.StaticAPI.GetFunctionChecked<").Print(context.interfaceShortName).PrintEndLine(
                    ", T>();");
                printer.PrintLine("return new VPtr(pointer, global::Latios.Unsafe.InternalSourceGen.StaticAPI.ConvertFunctionPointerToWrapper<VPtrFunction>(functionPtr));");
                printer.CloseScope();
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public static void __Initialize()");
                {
                    printer.OpenScope();
                    printer.OpenScope();
                    printer.PrintBeginLine(
                        "var functionPtr = global::Unity.Burst.BurstCompiler.CompileFunctionPointer<global::Latios.Unsafe.InternalSourceGen.StaticAPI.BurstDispatchVptrDelegate>(__")
                    .Print(context.interfaceShortName).Print("_VPtrDispatcherAsAStaticBecauseBurstCompileIsntAllowedOnInterfaceTypes.__BurstDispatch_")
                    .Print(context.interfaceShortName).PrintEndLine(");");
                    printer.PrintBeginLine("global::Latios.Unsafe.InternalSourceGen.StaticAPI.RegisterVptrFunction<").Print(context.interfaceShortName).PrintEndLine(
                        ", VPtr>(functionPtr);");
                    printer.CloseScope();
                    foreach (var b in context.baseVInterfaceNames)
                    {
                        var swapped = b.Replace("::", "_").Replace('.', '_');
                        printer.OpenScope();
                        printer.PrintBeginLine(
                            "var functionPtr = global::Unity.Burst.BurstCompiler.CompileFunctionPointer<global::Latios.Unsafe.InternalSourceGen.StaticAPI.BurstDispatchVptrDelegate>(__")
                        .Print(context.interfaceShortName).Print("_VPtrDispatcherAsAStaticBecauseBurstCompileIsntAllowedOnInterfaceTypes.__BurstDispatch_")
                        .Print(swapped).PrintEndLine(");");
                        printer.PrintBeginLine("global::Latios.Unsafe.InternalSourceGen.StaticAPI.RegisterVptrFunction<").Print(b).PrintEndLine(", VPtr>(functionPtr);");
                        printer.CloseScope();
                    }
                    printer.CloseScope();
                }
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("void global::Latios.Unsafe.IVInterface.__ThisMethodIsSupposedToBeGeneratedByASourceGenerator() {}");
                printer.PrintBeginLine().PrintEndLine();
                PrintPack(ref printer, ref context);
                printer.CloseScope();
            }
            printer.PrintBeginLine().PrintEndLine();
            printer.PrintBeginLine("public static VPtrFunction GetVPtrFunctionFrom<T>() where T : unmanaged, ").PrintEndLine(context.interfaceShortName);
            printer.OpenScope();
            printer.PrintBeginLine("global::Latios.Unsafe.InternalSourceGen.StaticAPI.TryGetFunction<").Print(context.interfaceShortName).PrintEndLine(", T>(out var functionPtr);");
            printer.PrintLine("return global::Latios.Unsafe.InternalSourceGen.StaticAPI.ConvertFunctionPointerToWrapper<VPtrFunction>(functionPtr);");
            printer.CloseScope();
            printer.PrintLine("public static bool TryGetVptrFunctionFrom(long structTypeBurstHash, out VPtrFunction function)");
            printer.OpenScope();
            printer.PrintBeginLine("var result = global::Latios.Unsafe.InternalSourceGen.StaticAPI.TryGetFunction<").Print(context.interfaceShortName).PrintEndLine(
                ">(structTypeBurstHash, out var functionPtr);");
            printer.PrintLine("function   = result ? global::Latios.Unsafe.InternalSourceGen.StaticAPI.ConvertFunctionPointerToWrapper<VPtrFunction>(functionPtr) : default;");
            printer.PrintLine("return result;");
            printer.CloseScope();
            printer.PrintBeginLine().PrintEndLine();
            PrintUnpack(ref printer, ref context);
        }

        // We need a separate class solely because [BurstCompile] is not permitted on interface types.
        static void PrintStaticClass(ref Printer printer, ref BodyContext context)
        {
            printer.PrintLine("[global::Unity.Burst.BurstCompile]");
            printer.PrintBeginLine("static class __").Print(context.interfaceShortName).PrintEndLine("_VPtrDispatcherAsAStaticBecauseBurstCompileIsntAllowedOnInterfaceTypes");
            printer.OpenScope();
            printer.PrintLine("[global::AOT.MonoPInvokeCallback(typeof(global::Latios.Unsafe.InternalSourceGen.StaticAPI.BurstDispatchVptrDelegate))]");
            printer.PrintLine("[global::UnityEngine.Scripting.Preserve]");
            printer.PrintLine("[global::Unity.Burst.BurstCompile]");
            printer.PrintBeginLine("public static void __BurstDispatch_").Print(context.interfaceShortName).PrintEndLine(
                "(global::Latios.Unsafe.InternalSourceGen.StaticAPI.ContextPtr context, int operation)");
            printer.OpenScope();
            printer.PrintBeginLine(context.interfaceShortName).Print(".__Dispatch<").Print(context.interfaceShortName).PrintEndLine(".VPtr>(context, operation);");
            printer.CloseScope();
            foreach (var b in context.baseVInterfaceNames)
            {
                var swapped = b.Replace("::", "_").Replace('.', '_');
                printer.PrintLine("[global::AOT.MonoPInvokeCallback(typeof(global::Latios.Unsafe.InternalSourceGen.StaticAPI.BurstDispatchVptrDelegate))]");
                printer.PrintLine("[global::UnityEngine.Scripting.Preserve]");
                printer.PrintLine("[global::Unity.Burst.BurstCompile]");
                printer.PrintBeginLine("public static void __BurstDispatch_").Print(swapped).PrintEndLine(
                    "(global::Latios.Unsafe.InternalSourceGen.StaticAPI.ContextPtr context, int operation)");
                printer.OpenScope();
                printer.PrintBeginLine(b).Print(".__Dispatch<").Print(context.interfaceShortName).PrintEndLine(".VPtr>(context, operation);");
                printer.CloseScope();
            }
            printer.CloseScope();
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
                            printer.PrintLine("global::Latios.Unsafe.InternalSourceGen.StaticAPI.ContextPtr ret = default;");
                    }
                    printer.PrintLine("var __vptrDelegate = global::Latios.Unsafe.InternalSourceGen.StaticAPI.ConvertFunctionPointerFromWrapper(__function);");
                    p = printer.PrintBeginLine($"global::Latios.Unsafe.InternalSourceGen.StaticAPI.Dispatch(__ptr, __vptrDelegate, {i}");
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
                            printer.PrintBeginLine("return ref global::Latios.Unsafe.InternalSourceGen.StaticAPI.ExtractRefReturn<").Print(method.returnFullTypeNameIfNotVoid).
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
                        printer.PrintLine("global::Latios.Unsafe.InternalSourceGen.StaticAPI.ContextPtr ret = default;");
                    printer.PrintLine($"global::Latios.Unsafe.InternalSourceGen.StaticAPI.Dispatch(__ptr, __function, {i + opId}, ref ret);");
                    if (property.returnMod == Microsoft.CodeAnalysis.RefKind.None)
                        printer.PrintLine("return ret;");
                    else
                    {
                        printer.PrintBeginLine("return ref global::Latios.Unsafe.InternalSourceGen.StaticAPI.ExtractRefReturn<").Print(property.propertyFullTypeName).
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
                    printer.PrintLine($"global::Latios.Unsafe.InternalSourceGen.StaticAPI.Dispatch(__ptr, __function, {i + opId}, ref propertyAssignArg);");
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
                        printer.PrintLine("global::Latios.Unsafe.InternalSourceGen.StaticAPI.ContextPtr ret = default;");
                    for (int j = 0; j < indexer.arguments.Count; j++)
                    {
                        printer.PrintBeginLine($"var arg{j} = ").Print(indexer.arguments[j].argVariableName).PrintEndLine(";");
                    }
                    p = printer.PrintBeginLine($"global::Latios.Unsafe.InternalSourceGen.StaticAPI.Dispatch(__ptr, __function, {i + opId}, ref ret");
                    for (int j = 0; j < indexer.arguments.Count; j++)
                    {
                        p = p.Print($", ref arg{j}");
                    }
                    p.PrintEndLine(");");
                    if (indexer.returnMod == Microsoft.CodeAnalysis.RefKind.None)
                        printer.PrintLine("return ret;");
                    else
                    {
                        printer.PrintBeginLine("return ref global::Latios.Unsafe.InternalSourceGen.StaticAPI.ExtractRefReturn<").Print(indexer.propertyFullTypeName).
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
                    printer.PrintLine($"global::Latios.Unsafe.InternalSourceGen.StaticAPI.Dispatch(__ptr, __function, {i + opId}, ref propertyAssignArg");
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
            string surfaceDeclaration = context.baseVInterfaceNames.Count > 0 ? "new public " : "public ";
            printer.PrintBeginLine(surfaceDeclaration).Print(
                "static void __Dispatch<T>(global::Latios.Unsafe.InternalSourceGen.StaticAPI.ContextPtr __context, int __operation) where T : unmanaged, ")
            .PrintEndLine(context.interfaceShortName);
            printer.OpenScope();
            printer.PrintLine("switch (__operation)");
            printer.OpenScope();
            for (int i = 0; i < context.methods.Count; i++)
            {
                var method    = context.methods[i];
                var hasReturn = !string.IsNullOrEmpty(method.returnFullTypeNameIfNotVoid);
                printer.PrintLine($"case {i}:");
                printer.OpenScope();

                printer.PrintLine("ref var __obj = ref global::Latios.Unsafe.InternalSourceGen.StaticAPI.ExtractObject<T>(__context);");
                int extractCounter = 0;
                if (hasReturn)
                {
                    if (method.returnMod == Microsoft.CodeAnalysis.RefKind.None)
                    {
                        printer.PrintBeginLine("ref var __ret = ref global::Latios.Unsafe.InternalSourceGen.StaticAPI.ExtractArg0<").Print(method.returnFullTypeNameIfNotVoid).
                        PrintEndLine(">(__context);");
                    }
                    else
                    {
                        printer.PrintLine(
                            "ref var __retPtr = ref global::Latios.Unsafe.InternalSourceGen.StaticAPI.ExtractArg0<global::Latios.Unsafe.InternalSourceGen.StaticAPI.ContextPtr>(__context);");
                    }
                    extractCounter++;
                }
                foreach (var arg in method.arguments)
                {
                    printer.PrintBeginLine("ref var ").Print(arg.argVariableName).Print($" = ref global::Latios.Unsafe.InternalSourceGen.StaticAPI.ExtractArg{extractCounter}<").
                    Print(arg.argFullTypeName).PrintEndLine(">(__context);");
                    extractCounter++;
                }
                var p = printer.PrintBeginLine();
                if (hasReturn)
                {
                    if (method.returnMod == Microsoft.CodeAnalysis.RefKind.None)
                        p = p.Print("__ret = ");
                    else if (method.returnMod == Microsoft.CodeAnalysis.RefKind.Ref)
                        p = p.Print("ref var __ret = ref ");
                    else if (method.returnMod == Microsoft.CodeAnalysis.RefKind.RefReadOnly)
                        p = p.Print("ref readonly var __ret = ref ");
                }
                p                      = p.Print("__obj.").Print(method.methodName).Print("(");
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
                    printer.PrintLine("__retPtr = global::Latios.Unsafe.InternalSourceGen.StaticAPI.AssignRefReturn(ref __ret);");
                else if (hasReturn && method.returnMod == Microsoft.CodeAnalysis.RefKind.RefReadOnly)
                    printer.PrintLine("__retPtr = global::Latios.Unsafe.InternalSourceGen.StaticAPI.AssignRefReadonlyReturn(in __ret);");
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
                    printer.PrintLine("ref var __obj = ref global::Latios.Unsafe.InternalSourceGen.StaticAPI.ExtractObject<T>(__context);");
                    if (property.returnMod == Microsoft.CodeAnalysis.RefKind.None)
                    {
                        printer.PrintBeginLine("ref var __ret = ref global::Latios.Unsafe.InternalSourceGen.StaticAPI.ExtractArg0<").Print(property.propertyFullTypeName)
                        .PrintEndLine(">(__context);");
                    }
                    else
                    {
                        printer.PrintLine(
                            "ref var __retPtr = ref global::Latios.Unsafe.InternalSourceGen.StaticAPI.ExtractArg0<global::Latios.Unsafe.InternalSourceGen.StaticAPI.ContextPtr>(__context);");
                    }
                    var p = printer.PrintBeginLine();
                    if (property.returnMod == Microsoft.CodeAnalysis.RefKind.None)
                        p = p.Print("__ret = ");
                    else if (property.returnMod == Microsoft.CodeAnalysis.RefKind.Ref)
                        p = p.Print("ref var __ret = ref ");
                    else if (property.returnMod == Microsoft.CodeAnalysis.RefKind.RefReadOnly)
                        p = p.Print("ref readonly var __ret = ref ");
                    p.Print("__obj.").Print(property.propertyName).PrintEndLine(";");
                    if (property.returnMod == Microsoft.CodeAnalysis.RefKind.Ref)
                        printer.PrintLine("__retPtr = global::Latios.Unsafe.InternalSourceGen.StaticAPI.AssignRefReturn(ref __ret);");
                    else if (property.returnMod == Microsoft.CodeAnalysis.RefKind.RefReadOnly)
                        printer.PrintLine("__retPtr = global::Latios.Unsafe.InternalSourceGen.StaticAPI.AssignRefReadonlyReturn(in __ret);");
                    printer.PrintLine("break;");
                    printer.CloseScope();
                    opId++;
                }
                if (property.hasSetter)
                {
                    printer.PrintLine($"case {opId}:");
                    printer.OpenScope();
                    printer.PrintLine("ref var __obj = ref global::Latios.Unsafe.InternalSourceGen.StaticAPI.ExtractObject<T>(__context);");
                    printer.PrintBeginLine("ref var __propertyAssignArg = ref global::Latios.Unsafe.InternalSourceGen.StaticAPI.ExtractArg0<").Print(property.propertyFullTypeName)
                    .PrintEndLine(">(__context);");
                    printer.PrintBeginLine("__obj.").Print(property.propertyName).PrintEndLine(" = __propertyAssignArg;");
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

                    printer.PrintLine("ref var __obj = ref global::Latios.Unsafe.InternalSourceGen.StaticAPI.ExtractObject<T>(__context);");
                    if (indexer.returnMod == Microsoft.CodeAnalysis.RefKind.None)
                    {
                        printer.PrintBeginLine("ref var __ret = ref global::Latios.Unsafe.InternalSourceGen.StaticAPI.ExtractArg0<").Print(indexer.propertyFullTypeName).
                        PrintEndLine(">(__context);");
                    }
                    else
                    {
                        printer.PrintLine(
                            "ref var __retPtr = ref global::Latios.Unsafe.InternalSourceGen.StaticAPI.ExtractArg0<global::Latios.Unsafe.InternalSourceGen.StaticAPI.ContextPtr>(__context);");
                    }
                    int extractCounter = 1;
                    foreach (var arg in indexer.arguments)
                    {
                        printer.PrintBeginLine("ref var ").Print(arg.argVariableName).Print($" = ref global::Latios.Unsafe.InternalSourceGen.StaticAPI.ExtractArg{extractCounter}<")
                        .
                        Print(arg.argFullTypeName).PrintEndLine(">(__context);");
                        extractCounter++;
                    }
                    var p = printer.PrintBeginLine();

                    if (indexer.returnMod == Microsoft.CodeAnalysis.RefKind.None)
                        p = p.Print("__ret = ");
                    else if (indexer.returnMod == Microsoft.CodeAnalysis.RefKind.Ref)
                        p = p.Print("ref var __ret = ref ");
                    else if (indexer.returnMod == Microsoft.CodeAnalysis.RefKind.RefReadOnly)
                        p = p.Print("ref readonly var __ret = ref ");
                    p     = p.Print("__obj[").Print(indexer.arguments[0].argVariableName);
                    for (int j = 1; j < indexer.arguments.Count; j++)
                    {
                        p = p.Print(", ").Print(indexer.arguments[j].argVariableName);
                    }
                    p.PrintEndLine("];");
                    if (indexer.returnMod == Microsoft.CodeAnalysis.RefKind.Ref)
                        printer.PrintLine("__retPtr = global::Latios.Unsafe.InternalSourceGen.StaticAPI.AssignRefReturn(ref __ret);");
                    else if (indexer.returnMod == Microsoft.CodeAnalysis.RefKind.RefReadOnly)
                        printer.PrintLine("__retPtr = global::Latios.Unsafe.InternalSourceGen.StaticAPI.AssignRefReadonlyReturn(in __ret);");
                    printer.PrintLine("break;");
                    printer.CloseScope();
                    opId++;
                }
                if (indexer.hasSetter)
                {
                    printer.PrintLine($"case {opId}:");
                    printer.OpenScope();

                    printer.PrintLine("ref var __obj = ref global::Latios.Unsafe.InternalSourceGen.StaticAPI.ExtractObject<TS>(__context);");
                    printer.PrintBeginLine("ref var __propertyAssignArg = ref global::Latios.Unsafe.InternalSourceGen.StaticAPI.ExtractArg0<").Print(indexer.propertyFullTypeName)
                    .PrintEndLine(">(__context);");
                    int extractCounter = 1;
                    foreach (var arg in indexer.arguments)
                    {
                        printer.PrintBeginLine("ref var ").Print(arg.argVariableName).Print($" = ref global::Latios.Unsafe.InternalSourceGen.StaticAPI.ExtractArg{extractCounter}<")
                        .
                        Print(arg.argFullTypeName).PrintEndLine(">(__context);");
                        extractCounter++;
                    }
                    var p = printer.PrintBeginLine("__obj[").Print(indexer.arguments[0].argVariableName);
                    for (int j = 1; j < indexer.arguments.Count; j++)
                    {
                        p = p.Print(", ").Print(indexer.arguments[j].argVariableName);
                    }
                    p.PrintEndLine("] = __propertyAssignArg;");
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

