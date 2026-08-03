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

            // Properties and Indexers
            {
                var propertySymbols = allMembersOfAllInterfaces
                                      .Where(m => m.Kind == SymbolKind.Property).OfType<IPropertySymbol>()
                                      .Where(m => !m.IsStatic && m.DeclaredAccessibility != Accessibility.Private &&
                                             !m.IsSealed && !m.IsOverride &&
                                             (m.ContainingType == null || m.ContainingType.Name != nameof(Object)));
                var properties              = new List<InterfaceCodeWriter.PropertyDescription>();
                var indexers                = new List<InterfaceCodeWriter.IndexerDescription>();
                bodyContext.propertyOpCount = 0;
                bodyContext.indexerOpCount  = 0;
                foreach (var propertySymbol in propertySymbols)
                {
                    if (!propertySymbol.Parameters.IsDefaultOrEmpty)
                    {
                        InterfaceCodeWriter.IndexerDescription desc = default;
                        if (propertySymbol.ContainingType != null)
                            desc.fullExplicitInterfaceNameIfRequired  = propertySymbol.ContainingType.ToFullName();
                        desc.accessibility                            = propertySymbol.DeclaredAccessibility == Accessibility.Public ? "public " : "internal ";
                        desc.propertyFullTypeName                     = propertySymbol.Type.ToFullName();
                        desc.returnMod                                = propertySymbol.RefKind;
                        desc.hasGetter                                = propertySymbol.GetMethod != null;
                        desc.hasSetter                                = propertySymbol.SetMethod != null;
                        bodyContext.indexerOpCount                   += desc.hasGetter ? 1 : 0;
                        bodyContext.indexerOpCount                   += desc.hasSetter ? 1 : 0;
                        desc.arguments                                = new List<InterfaceCodeWriter.IndexerDescription.Arg>();
                        foreach (var paramSymbol in propertySymbol.Parameters)
                        {
                            var arg = new InterfaceCodeWriter.IndexerDescription.Arg
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
                        InterfaceCodeWriter.PropertyDescription desc = default;
                        desc.propertyName                            = propertySymbol.Name;
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

        public static void ExtractAuthoringSemantics(ClassDeclarationSyntax classDeclarationSyntax,
                                                     SemanticModel semanticModel,
                                                     out AuthoringCodeWriter.Context context)
        {
            var classDeclarationSymbol      = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
            var classTypeSymbol             = classDeclarationSymbol.GetSymbolType();
            var genericAuthoringType        = classTypeSymbol.BaseType;
            var scriptType                  = genericAuthoringType.TypeArguments[0];
            context.scriptTypeName          = scriptType.ToFullName();
            context.baseUnikaInterfaceNames = new List<string>();

            foreach (var iface in scriptType.AllInterfaces)
            {
                if (iface.InheritsFromInterface("global::Latios.Unika.IUnikaInterface"))
                {
                    context.baseUnikaInterfaceNames.Add(iface.ToFullName());
                }
            }
        }

        public static void ExtractAutoAuthoringSemantics(ClassDeclarationSyntax classDeclarationSyntax,
                                                         SemanticModel semanticModel,
                                                         out AutoAuthoringCodeWriter.Context context)
        {
            var classDeclarationSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
            var classTypeSymbol        = classDeclarationSymbol.GetSymbolType();
            var genericAuthoringType   = classTypeSymbol.BaseType;
            var scriptType             = genericAuthoringType.TypeArguments[0];
            context.scriptFullTypeName = scriptType.ToFullName();
            context.fields             = new List<AutoAuthoringCodeWriter.FieldInfo>();

            var usingDirectiveTexts = new List<string>();
            var seenUsingTexts      = new HashSet<string>();
            foreach (var syntaxRef in scriptType.DeclaringSyntaxReferences)
            {
                if (syntaxRef.GetSyntax().SyntaxTree.GetRoot() is CompilationUnitSyntax compilationUnit)
                {
                    foreach (var usingDirective in compilationUnit.Usings)
                    {
                        var text = usingDirective.ToString();
                        if (seenUsingTexts.Add(text))
                            usingDirectiveTexts.Add(text);
                    }
                }
            }
            context.usingDirectiveTexts = usingDirectiveTexts;

            // Needed to reopen the script struct's own namespace/type scope, so that an internal helper
            // method can be added to it for assigning its non-public [SerializeField] fields (an authoring
            // class is a different type and cannot assign private fields on the script struct directly).
            context.scriptDeclarationSyntax = (StructDeclarationSyntax)scriptType.DeclaringSyntaxReferences[0].GetSyntax();

            // UnikaAutoScriptAuthoring<T> subclasses don't get matched by AuthoringGenerator (which only
            // matches direct UnikaScriptAuthoring<T> subclasses), so this generator implements the
            // IUnikaInterfaceAuthoringImpl<...> boilerplate itself for every Unika interface the script implements.
            context.baseUnikaInterfaceNames = new List<string>();
            foreach (var iface in scriptType.AllInterfaces)
            {
                if (iface.InheritsFromInterface("global::Latios.Unika.IUnikaInterface"))
                    context.baseUnikaInterfaceNames.Add(iface.ToFullName());
            }

            foreach (var fieldSymbol in scriptType.GetMembers().OfType<IFieldSymbol>())
            {
                if (fieldSymbol.IsStatic || fieldSymbol.IsConst || fieldSymbol.IsReadOnly)
                    continue;

                bool isPublic = fieldSymbol.DeclaredAccessibility == Accessibility.Public;
                if (isPublic)
                {
                    if (fieldSymbol.HasAttribute("System.NonSerializedAttribute"))
                        continue;
                }
                else if (!fieldSymbol.HasAttribute("UnityEngine.SerializeField"))
                    continue;

                var fieldType = fieldSymbol.Type;

                // BlobAssetReference<T> fields are intentionally ignored. The user is responsible for
                // assigning them from OnAutoBake, immediately or via a Smart Blobber.
                if (TryGetGenericTypeArgument(fieldType, "Unity.Entities", "BlobAssetReference", out _))
                    continue;

                AutoAuthoringCodeWriter.FieldKind kind;
                string                            authoringFieldTypeName;
                string                            scriptFieldTypeName;

                if (fieldType is INamedTypeSymbol namedFieldType && namedFieldType.Name == "ScriptRef" &&
                    namedFieldType.ContainingNamespace?.ToDisplayString() == "Latios.Unika")
                {
                    kind                 = AutoAuthoringCodeWriter.FieldKind.ScriptRef;
                    scriptFieldTypeName  = fieldType.ToFullName();
                    authoringFieldTypeName  = namedFieldType.IsGenericType ?
                                              $"global::Latios.Unika.Authoring.UnikaScriptAuthoring<{namedFieldType.TypeArguments[0].ToFullName()}>" :
                                              "global::Latios.Unika.Authoring.UnikaScriptAuthoringBase";
                }
                // A generator cannot semantically see the InterfaceRef struct that InterfaceGenerator nests
                // inside a IUnikaInterface type (generators never observe each other's generated output), so
                // this is detected and reconstructed by name/convention rather than by inspecting fieldType's
                // members, exactly like AuthoringCodeWriter already does for the same reason.
                else if (fieldType.Name == "InterfaceRef" && fieldType.ContainingType != null &&
                         fieldType.ContainingType.TypeKind == TypeKind.Interface &&
                         fieldType.ContainingType.InheritsFromInterface("global::Latios.Unika.IUnikaInterface"))
                {
                    kind                    = AutoAuthoringCodeWriter.FieldKind.InterfaceRef;
                    scriptFieldTypeName     = $"{fieldType.ContainingType.ToFullName()}.InterfaceRef";
                    authoringFieldTypeName  = $"global::Latios.Unika.Authoring.IUnikaInterfaceAuthoring<{scriptFieldTypeName}>";
                }
                else if (fieldType.ToFullName() == "global::Unity.Entities.Entity")
                {
                    kind                    = AutoAuthoringCodeWriter.FieldKind.Entity;
                    scriptFieldTypeName     = fieldType.ToFullName();
                    authoringFieldTypeName  = "global::UnityEngine.GameObject";
                }
                else if (TryGetGenericTypeArgument(fieldType, "Latios", "EntityWith", out _) ||
                         TryGetGenericTypeArgument(fieldType, "Latios", "EntityWithBuffer", out _))
                {
                    kind                    = AutoAuthoringCodeWriter.FieldKind.Entity;
                    scriptFieldTypeName     = fieldType.ToFullName();
                    authoringFieldTypeName  = "global::UnityEngine.GameObject";
                }
                else
                {
                    kind                    = AutoAuthoringCodeWriter.FieldKind.Mirror;
                    scriptFieldTypeName     = fieldType.ToFullName();
                    authoringFieldTypeName  = scriptFieldTypeName;
                }

                var attributeTexts = new List<string>();
                foreach (var syntaxRef in fieldSymbol.DeclaringSyntaxReferences)
                {
                    if (syntaxRef.GetSyntax() is VariableDeclaratorSyntax variableDeclarator &&
                        variableDeclarator.Parent?.Parent is FieldDeclarationSyntax fieldDeclaration)
                    {
                        foreach (var attributeList in fieldDeclaration.AttributeLists)
                        {
                            foreach (var attribute in attributeList.Attributes)
                            {
                                var attributeName = attribute.Name.ToString();
                                if (attributeName == "SerializeField" || attributeName == "NonSerialized")
                                    continue;
                                attributeTexts.Add($"[{attribute}]");
                            }
                        }
                    }
                }

                context.fields.Add(new AutoAuthoringCodeWriter.FieldInfo
                {
                    fieldName              = fieldSymbol.Name,
                    isPublic               = isPublic,
                    authoringFieldTypeName = authoringFieldTypeName,
                    scriptFieldTypeName    = scriptFieldTypeName,
                    attributeTexts         = attributeTexts,
                    kind                   = kind,
                });
            }
        }

        static bool TryGetGenericTypeArgument(ITypeSymbol type, string ns, string name, out ITypeSymbol typeArgument)
        {
            typeArgument = null;
            if (type is INamedTypeSymbol named && named.IsGenericType && named.Arity == 1 &&
                named.Name == name && named.ContainingNamespace?.ToDisplayString() == ns)
            {
                typeArgument = named.TypeArguments[0];
                return true;
            }
            return false;
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

        struct PropertyComparer : IComparer<InterfaceCodeWriter.PropertyDescription>
        {
            public int Compare(InterfaceCodeWriter.PropertyDescription x, InterfaceCodeWriter.PropertyDescription y)
            {
                return x.propertyName.CompareTo(y.propertyName);
            }
        }

        struct IndexerComparer : IComparer<InterfaceCodeWriter.IndexerDescription>
        {
            public int Compare(InterfaceCodeWriter.IndexerDescription x, InterfaceCodeWriter.IndexerDescription y)
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

