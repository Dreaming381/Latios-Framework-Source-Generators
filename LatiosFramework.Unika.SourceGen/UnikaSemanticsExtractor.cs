using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LatiosFramework.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LatiosFramework.Unika.SourceGen
{
    internal static class UnikaSemanticsExtractor
    {
        public static void ExtractScriptSemantics(StructDeclarationSyntax scriptDeclarationSyntax,
                                                  SemanticModel semanticModel,
                                                  out ScriptCodeWriter.BodyContext bodyContext,
                                                  out ScriptCodeWriter.ExtensionClassContext extensionClassContext)
        {
            bodyContext.scriptShortName = scriptDeclarationSyntax.Identifier.ToString();

            var scriptDeclarationSymbol          = semanticModel.GetDeclaredSymbol(scriptDeclarationSyntax);
            var scriptTypeSymbol                 = scriptDeclarationSymbol.GetSymbolType();
            extensionClassContext.scriptFullName = scriptTypeSymbol.ToFullName();

            var interfaceNames = new List<string>();

            foreach (var iface in scriptTypeSymbol.AllInterfaces)
            {
                if (iface.InheritsFromInterface("global::Latios.Unika.IUnikaInterface"))
                {
                    interfaceNames.Add(iface.ToFullName());
                }
            }
            bodyContext.unikaInterfaceNames           = interfaceNames;
            extensionClassContext.unikaInterfaceNames = interfaceNames;
            extensionClassContext.modifier            = default;
        }

        public static void ExtractInterfaceSemantics(InterfaceDeclarationSyntax interfaceDeclarationSyntax,
                                                     SemanticModel semanticModel,
                                                     out InterfaceCodeWriter.BodyContext bodyContext)
        {
            bodyContext.interfaceShortName = interfaceDeclarationSyntax.Identifier.ToString();

            var interfaceDeclarationSymbol = semanticModel.GetDeclaredSymbol(interfaceDeclarationSyntax);
            var interfaceTypeSymbol        = interfaceDeclarationSymbol.GetSymbolType();

            var interfaceNames = new List<string>();

            var allMembersOfAllInterfaces = new List<ISymbol>(interfaceTypeSymbol.GetMembers());
            foreach (var iface in interfaceTypeSymbol.AllInterfaces)
            {
                if (iface.InheritsFromInterface("global::Latios.Unika.IUnikaInterface"))
                {
                    interfaceNames.Add(iface.ToFullName());
                }
                allMembersOfAllInterfaces.AddRange(iface.GetMembers());
            }
            bodyContext.baseUnikaInterfaceNames = interfaceNames;

            var methodSymbols = allMembersOfAllInterfaces
                                .Where(m => m.Kind == SymbolKind.Method).OfType<IMethodSymbol>()
                                .Where(m => !m.IsStatic && m.DeclaredAccessibility != Accessibility.Private &&
                                       //m.IsVirtual && // Todo: Figure out why this doesn't work
                                       !m.IsGenericMethod &&
                                       !m.IsSealed &&
                                       !m.IsOverride &&
                                       (m.ContainingType == null || m.ContainingType.Name != nameof(Object)));
            var methods = new List<InterfaceCodeWriter.MethodDescription>();
            foreach (var methodSymbol in methodSymbols)
            {
                InterfaceCodeWriter.MethodDescription desc = default;
                desc.methodName                            = methodSymbol.Name;
                if (methodSymbol.ContainingType != null)
                    desc.fullExplicitInterfaceNameIfRequired = methodSymbol.ContainingType.ToFullName();
                desc.accessibility                           = methodSymbol.DeclaredAccessibility == Accessibility.Public ? "public " : "internal ";
                desc.arguments                               = new List<InterfaceCodeWriter.MethodDescription.Arg>();
                foreach (var paramSymbol in methodSymbol.Parameters)
                {
                    if (paramSymbol.IsThis)
                        continue;

                    var arg = new InterfaceCodeWriter.MethodDescription.Arg
                    {
                        argMod          = paramSymbol.RefKind,
                        argFullTypeName = paramSymbol.Type.ToFullName(),
                        argVariableName = paramSymbol.Name,
                    };
                    desc.arguments.Add(arg);
                }
                desc.returnMod = RefKind.None;

                if (!methodSymbol.ReturnsVoid)
                {
                    if (methodSymbol.ReturnsByRef)
                        desc.returnMod = RefKind.Ref;
                    else if (methodSymbol.ReturnsByRefReadonly)
                        desc.returnMod               = RefKind.RefReadOnly;
                    desc.returnFullTypeNameIfNotVoid = methodSymbol.ReturnType.ToFullName();
                }
                methods.Add(desc);
            }

            var comparer = new MethodComparer();
            methods.Sort(comparer);

            bool previousWasEqual = false;

            for (int i = 1; i < methods.Count; i++)
            {
                if (comparer.Compare(methods[i - 1], methods[i]) != 0)
                {
                    if (!previousWasEqual)
                    {
                        var element                                 = methods[i - 1];
                        element.fullExplicitInterfaceNameIfRequired = null;
                        methods[i - 1]                              = element;
                    }
                    previousWasEqual = false;
                }
                else
                    previousWasEqual = true;
            }
            if (!previousWasEqual && methods.Count > 0)
            {
                var element                                 = methods[methods.Count - 1];
                element.fullExplicitInterfaceNameIfRequired = null;
                methods[methods.Count - 1]                  = element;
            }
            bodyContext.methods = methods;
        }

        struct MethodComparer : IComparer<InterfaceCodeWriter.MethodDescription>
        {
            public int Compare(InterfaceCodeWriter.MethodDescription x, InterfaceCodeWriter.MethodDescription y)
            {
                var result = x.methodName.CompareTo(y.methodName);
                if (result != 0)
                    return result;
                result = x.arguments.Count.CompareTo(y.arguments.Count);
                if (result != 0)
                    return result;
                for (int i = 0; i < x.arguments.Count; i++)
                {
                    var argX = x.arguments[i];
                    var argY = y.arguments[i];
                    result   = argX.argFullTypeName.CompareTo(argY.argFullTypeName);
                    if (result != 0)
                        return result;
                }
                return result;
            }
        }
    }
}

