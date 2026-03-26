using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LatiosFramework.SourceGen
{
    internal class VptrSemanticsExtractor
    {
        public static void ExtractObjSemantics(StructDeclarationSyntax objDeclarationSyntax,
                                               SemanticModel semanticModel,
                                               out VStructCodeWriter.BodyContext bodyContext)
        {
            bodyContext.objShortName = objDeclarationSyntax.Identifier.ToString();

            var objDeclarationSymbol = semanticModel.GetDeclaredSymbol(objDeclarationSyntax);
            var objTypeSymbol        = objDeclarationSymbol.GetSymbolType();

            var interfaceNames = new List<string>();

            foreach (var iface in objTypeSymbol.AllInterfaces)
            {
                if (iface.InheritsFromInterface("global::Latios.Unsafe.IVInterface"))
                {
                    interfaceNames.Add(iface.ToFullName());
                }
            }
            bodyContext.interfaceNames = interfaceNames;
        }

        public static void ExtractInterfaceSemantics(InterfaceDeclarationSyntax interfaceDeclarationSyntax,
                                                     SemanticModel semanticModel,
                                                     out VInterfaceCodeWriter.BodyContext bodyContext)
        {
            bodyContext.interfaceShortName = interfaceDeclarationSyntax.Identifier.ToString();

            var interfaceDeclarationSymbol = semanticModel.GetDeclaredSymbol(interfaceDeclarationSyntax);
            var interfaceTypeSymbol        = interfaceDeclarationSymbol.GetSymbolType();

            var interfaceNames = new List<string>();

            var allMembersOfAllInterfaces = new List<ISymbol>(interfaceTypeSymbol.GetMembers());
            foreach (var iface in interfaceTypeSymbol.AllInterfaces)
            {
                if (iface.InheritsFromInterface("global::Latios.Unsafe.IVInterface"))
                {
                    if (iface.ToFullName() == "global::Latios.Unsafe.IVInterface")
                        continue;
                    interfaceNames.Add(iface.ToFullName());
                }
                allMembersOfAllInterfaces.AddRange(iface.GetMembers());
            }
            bodyContext.baseVInterfaceNames = interfaceNames;

            // Methods
            {
                var methodSymbols = allMembersOfAllInterfaces
                                    .Where(m => m.Kind == SymbolKind.Method).OfType<IMethodSymbol>()
                                    .Where(m => !m.IsStatic && m.DeclaredAccessibility != Accessibility.Private &&
                                           //m.IsVirtual && // Todo: Figure out why this doesn't work
                                           !m.IsGenericMethod &&
                                           !m.IsSealed &&
                                           !m.IsOverride &&
                                           m.MethodKind != MethodKind.PropertyGet && m.MethodKind != MethodKind.PropertySet &&
                                           (m.ContainingType == null || m.ContainingType.Name != nameof(Object)));
                var methods = new List<VInterfaceCodeWriter.MethodDescription>();
                foreach (var methodSymbol in methodSymbols)
                {
                    VInterfaceCodeWriter.MethodDescription desc = default;
                    desc.methodName                             = methodSymbol.Name;
                    if (methodSymbol.ContainingType != null)
                        desc.fullExplicitInterfaceNameIfRequired = methodSymbol.ContainingType.ToFullName();
                    desc.accessibility                           = methodSymbol.DeclaredAccessibility == Accessibility.Public ? "public " : "internal ";
                    desc.arguments                               = new List<VInterfaceCodeWriter.MethodDescription.Arg>();
                    foreach (var paramSymbol in methodSymbol.Parameters)
                    {
                        if (paramSymbol.IsThis)
                            continue;

                        var arg = new VInterfaceCodeWriter.MethodDescription.Arg
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

            // Properties and Indexers
            {
                var propertySymbols = allMembersOfAllInterfaces
                                      .Where(m => m.Kind == SymbolKind.Property).OfType<IPropertySymbol>()
                                      .Where(m => !m.IsStatic && m.DeclaredAccessibility != Accessibility.Private &&
                                             !m.IsSealed && !m.IsOverride &&
                                             (m.ContainingType == null || m.ContainingType.Name != nameof(Object)));
                var properties              = new List<VInterfaceCodeWriter.PropertyDescription>();
                var indexers                = new List<VInterfaceCodeWriter.IndexerDescription>();
                bodyContext.propertyOpCount = 0;
                bodyContext.indexerOpCount  = 0;
                foreach (var propertySymbol in propertySymbols)
                {
                    if (!propertySymbol.Parameters.IsDefaultOrEmpty)
                    {
                        VInterfaceCodeWriter.IndexerDescription desc = default;
                        if (propertySymbol.ContainingType != null)
                            desc.fullExplicitInterfaceNameIfRequired  = propertySymbol.ContainingType.ToFullName();
                        desc.accessibility                            = propertySymbol.DeclaredAccessibility == Accessibility.Public ? "public " : "internal ";
                        desc.propertyFullTypeName                     = propertySymbol.Type.ToFullName();
                        desc.returnMod                                = propertySymbol.RefKind;
                        desc.hasGetter                                = propertySymbol.GetMethod != null;
                        desc.hasSetter                                = propertySymbol.SetMethod != null;
                        bodyContext.indexerOpCount                   += desc.hasGetter ? 1 : 0;
                        bodyContext.indexerOpCount                   += desc.hasSetter ? 1 : 0;
                        desc.arguments                                = new List<VInterfaceCodeWriter.IndexerDescription.Arg>();
                        foreach (var paramSymbol in propertySymbol.Parameters)
                        {
                            var arg = new VInterfaceCodeWriter.IndexerDescription.Arg
                            {
                                argFullTypeName = paramSymbol.Type.ToFullName(),
                                argVariableName = paramSymbol.Name,
                            };
                            desc.arguments.Add(arg);
                        }

                        indexers.Add(desc);
                    }
                    else
                    {
                        VInterfaceCodeWriter.PropertyDescription desc = default;
                        desc.propertyName                             = propertySymbol.Name;
                        if (propertySymbol.ContainingType != null)
                            desc.fullExplicitInterfaceNameIfRequired  = propertySymbol.ContainingType.ToFullName();
                        desc.accessibility                            = propertySymbol.DeclaredAccessibility == Accessibility.Public ? "public " : "internal ";
                        desc.propertyFullTypeName                     = propertySymbol.Type.ToFullName();
                        desc.returnMod                                = propertySymbol.RefKind;
                        desc.hasGetter                                = propertySymbol.GetMethod != null;
                        desc.hasSetter                                = propertySymbol.SetMethod != null;
                        bodyContext.propertyOpCount                  += desc.hasGetter ? 1 : 0;
                        bodyContext.propertyOpCount                  += desc.hasSetter ? 1 : 0;

                        properties.Add(desc);
                    }
                }

                // Properties
                {
                    var comparer = new PropertyComparer();
                    properties.Sort(comparer);
                    bool previousWasEqual = false;

                    for (int i = 1; i < properties.Count; i++)
                    {
                        if (comparer.Compare(properties[i - 1], properties[i]) != 0)
                        {
                            if (!previousWasEqual)
                            {
                                var element                                 = properties[i - 1];
                                element.fullExplicitInterfaceNameIfRequired = null;
                                properties[i - 1]                           = element;
                            }
                            previousWasEqual = false;
                        }
                        else
                            previousWasEqual = true;
                    }
                    if (!previousWasEqual && properties.Count > 0)
                    {
                        var element                                 = properties[properties.Count - 1];
                        element.fullExplicitInterfaceNameIfRequired = null;
                        properties[properties.Count - 1]            = element;
                    }
                    bodyContext.properties = properties;
                }

                // Indexers
                {
                    var comparer = new IndexerComparer();
                    indexers.Sort(comparer);
                    bool previousWasEqual = false;

                    for (int i = 1; i < indexers.Count; i++)
                    {
                        if (comparer.Compare(indexers[i - 1], indexers[i]) != 0)
                        {
                            if (!previousWasEqual)
                            {
                                var element                                 = indexers[i - 1];
                                element.fullExplicitInterfaceNameIfRequired = null;
                                indexers[i - 1]                             = element;
                            }
                            previousWasEqual = false;
                        }
                        else
                            previousWasEqual = true;
                    }
                    if (!previousWasEqual && indexers.Count > 0)
                    {
                        var element                                 = indexers[indexers.Count - 1];
                        element.fullExplicitInterfaceNameIfRequired = null;
                        indexers[indexers.Count - 1]                = element;
                    }
                    bodyContext.indexers = indexers;
                }
            }
        }

        struct MethodComparer : IComparer<VInterfaceCodeWriter.MethodDescription>
        {
            public int Compare(VInterfaceCodeWriter.MethodDescription x, VInterfaceCodeWriter.MethodDescription y)
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

        struct PropertyComparer : IComparer<VInterfaceCodeWriter.PropertyDescription>
        {
            public int Compare(VInterfaceCodeWriter.PropertyDescription x, VInterfaceCodeWriter.PropertyDescription y)
            {
                return x.propertyName.CompareTo(y.propertyName);
            }
        }

        struct IndexerComparer : IComparer<VInterfaceCodeWriter.IndexerDescription>
        {
            public int Compare(VInterfaceCodeWriter.IndexerDescription x, VInterfaceCodeWriter.IndexerDescription y)
            {
                var result = 0;
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

