using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// This writes the shared code structure for IManagedStructComponent and ICollectionComponent.
namespace LatiosFramework.SourceGen
{
    public static class ComponentCodeWriter
    {
        public static string WriteComponentCode(StructDeclarationSyntax componentSyntax, string componentTypeString, bool writeBurst = true)
        {
            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, componentSyntax.Parent);
            scopePrinter.PrintOpen();
            var printer = scopePrinter.Printer;
            printer.PrintLine("[global::System.Runtime.CompilerServices.CompilerGenerated]");
            if (writeBurst)
                printer.PrintLine("[global::Unity.Burst.BurstCompile]");
            printer.PrintBeginLine();
            foreach (var m in componentSyntax.Modifiers)
                printer.Print(m.ToString()).Print(" ");
            printer.Print("struct ").Print(componentSyntax.Identifier.Text).Print(" : global::Latios.InternalSourceGen.StaticAPI.I").Print(componentTypeString).PrintEndLine(
                "ComponentSourceGenerated");
            {
                printer.OpenScope();
                printer.PrintLine("public struct ExistComponent : global::Unity.Entities.IComponentData { }");
                printer.PrintBeginLine("public struct CleanupComponent : global::Unity.Entities.ICleanupComponentData, global::Latios.InternalSourceGen.StaticAPI.I").Print(
                    componentTypeString).
                PrintEndLine("ComponentCleanup");
                {
                    printer.OpenScope();
                    if (writeBurst)
                    {
                        printer.PrintBeginLine("public static unsafe global::Unity.Burst.FunctionPointer<global::Latios.InternalSourceGen.StaticAPI.BurstDispatch").Print(
                            componentTypeString).
                        PrintEndLine("ComponentDelegate> GetBurstDispatchFunctionPtr()");
                        {
                            printer.OpenScope();
                            printer.PrintBeginLine("return global::Unity.Burst.BurstCompiler.CompileFunctionPointer<").Print(
                                "global::Latios.InternalSourceGen.StaticAPI.BurstDispatch")
                            .Print(componentTypeString).PrintEndLine("ComponentDelegate>(BurstDispatch);");
                            printer.CloseScope();
                        }
                        printer.PrintBeginLine().PrintEndLine();
                    }
                    printer.PrintBeginLine("public static global::System.Type Get").Print(componentTypeString).Print("ComponentType() => typeof(").Print(
                        componentSyntax.Identifier.Text).PrintEndLine(");");
                    printer.CloseScope();
                }
                printer.PrintBeginLine().PrintEndLine();
                printer.PrintLine("public global::Unity.Entities.ComponentType componentType => global::Unity.Entities.ComponentType.ReadOnly<ExistComponent>();");
                printer.PrintLine("public global::Unity.Entities.ComponentType cleanupType => global::Unity.Entities.ComponentType.ReadOnly<CleanupComponent>();");
                printer.PrintBeginLine().PrintEndLine();
                if (writeBurst)
                {
                    printer.PrintBeginLine("[global::AOT.MonoPInvokeCallback(typeof(global::Latios.InternalSourceGen.StaticAPI.BurstDispatch")
                    .Print(componentTypeString).PrintEndLine("ComponentDelegate))]");
                    printer.PrintLine("[global::Unity.Burst.BurstCompile]");
                    printer.PrintLine("public static unsafe void BurstDispatch(void* context, int operation)");
                    {
                        printer.OpenScope();
                        printer.PrintBeginLine("global::Latios.InternalSourceGen.StaticAPI.BurstDispatch").Print(componentTypeString).Print("Component<").Print(
                            componentSyntax.Identifier.Text).PrintEndLine(">(context, operation);");
                        printer.CloseScope();
                    }
                }
                printer.CloseScope();
            }
            scopePrinter.PrintClose();
            return printer.Result;
        }
    }
}

