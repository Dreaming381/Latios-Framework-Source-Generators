﻿using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LatiosFramework.SourceGen
{
    public static class GeneratorFilterMethods
    {
        // Based on Unity's IJobEntity source generator
        public static bool IsSyntaxStructInterfaceMatch(SyntaxNode syntaxNode, CancellationToken cancellationToken, in string interfaceName)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Is Struct
            if (syntaxNode is StructDeclarationSyntax structDeclarationSyntax)
            {
                // Has Base List
                if (structDeclarationSyntax.BaseList == null)
                    return false;

                // Has interface identifier
                var hasInterfaceNameIdentifier = false;
                foreach (var baseType in structDeclarationSyntax.BaseList.Types)
                {
                    if (baseType.Type is IdentifierNameSyntax s1)
                    {
                        if (s1.Identifier != null && s1.Identifier.ValueText != null && s1.Identifier.ValueText == interfaceName)
                        {
                            hasInterfaceNameIdentifier = true;
                            break;
                        }
                    }
                    else if (baseType.Type is QualifiedNameSyntax s2)
                    {
                        if (s2.Right.Identifier != null && s2.Right.Identifier.ValueText != null && s2.Right.Identifier.ValueText == interfaceName)
                        {
                            hasInterfaceNameIdentifier = true;
                            break;
                        }
                    }
                }
                if (!hasInterfaceNameIdentifier)
                    return false;

                // Has Partial keyword
                var hasPartial = false;
                foreach (var m in structDeclarationSyntax.Modifiers)
                {
                    if (m.IsKind(SyntaxKind.PartialKeyword))
                    {
                        hasPartial = true;
                        break;
                    }
                }

                return hasPartial;
            }
            return false;
        }

        // Based on Unity's IJobEntity source generator
        public static StructDeclarationSyntax GetSemanticStructInterfaceMatch(GeneratorSyntaxContext ctx, CancellationToken cancellationToken, string fullSemanticInterfaceName)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var structDeclarationSyntax = (StructDeclarationSyntax)ctx.Node;
            foreach (var baseTypeSyntax in structDeclarationSyntax.BaseList !.Types)
                if (ctx.SemanticModel.GetTypeInfo(baseTypeSyntax.Type).Type.ToFullName() == fullSemanticInterfaceName)
                    return structDeclarationSyntax;
            return null;
        }

        public static bool IsSyntaxInterfaceInterfaceMatch(SyntaxNode syntaxNode, CancellationToken cancellationToken, in string interfaceName)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Is Interface
            if (syntaxNode is InterfaceDeclarationSyntax interfaceDeclarationSyntax)
            {
                // Has Base List
                if (interfaceDeclarationSyntax.BaseList == null)
                    return false;

                // Has interface identifier
                var hasInterfaceNameIdentifier = false;
                foreach (var baseType in interfaceDeclarationSyntax.BaseList.Types)
                {
                    if (baseType.Type is IdentifierNameSyntax s1)
                    {
                        if (s1.Identifier != null && s1.Identifier.ValueText != null && s1.Identifier.ValueText == interfaceName)
                        {
                            hasInterfaceNameIdentifier = true;
                            break;
                        }
                    }
                    else if (baseType.Type is QualifiedNameSyntax s2)
                    {
                        if (s2.Right.Identifier != null && s2.Right.Identifier.ValueText != null && s2.Right.Identifier.ValueText == interfaceName)
                        {
                            hasInterfaceNameIdentifier = true;
                            break;
                        }
                    }
                }
                if (!hasInterfaceNameIdentifier)
                    return false;

                // Has Partial keyword
                var hasPartial = false;
                foreach (var m in interfaceDeclarationSyntax.Modifiers)
                {
                    if (m.IsKind(SyntaxKind.PartialKeyword))
                    {
                        hasPartial = true;
                        break;
                    }
                }

                return hasPartial;
            }
            return false;
        }

        public static InterfaceDeclarationSyntax GetSemanticInterfaceInterfaceMatch(GeneratorSyntaxContext ctx,
                                                                                    CancellationToken cancellationToken,
                                                                                    string fullSemanticInterfaceName)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var interfaceDeclarationSyntax = (InterfaceDeclarationSyntax)ctx.Node;
            foreach (var baseTypeSyntax in interfaceDeclarationSyntax.BaseList !.Types)
                if (ctx.SemanticModel.GetTypeInfo(baseTypeSyntax.Type).Type.ToFullName() == fullSemanticInterfaceName)
                    return interfaceDeclarationSyntax;
            return null;
        }

        public static bool IsSyntaxClassGenericMatch(SyntaxNode syntaxNode, CancellationToken cancellationToken, in string classNameWithoutGenerics)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Is Class
            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
            {
                // Has Base List
                if (classDeclarationSyntax.BaseList == null)
                    return false;

                // Has derived class identifier
                var hasClassNameIdentifier = false;
                foreach (var baseType in classDeclarationSyntax.BaseList.Types)
                {
                    //if (baseType.Span.ToString().Contains(classNameWithoutGenerics))
                    //{
                    //    hasClassNameIdentifier = true;
                    //    break;
                    //}
                    if (baseType.Type is GenericNameSyntax s1)
                    {
                        if (s1.Identifier != null && s1.Identifier.ValueText == classNameWithoutGenerics)
                        {
                            hasClassNameIdentifier = true;
                            break;
                        }
                    }
                    else if (baseType.Type is QualifiedNameSyntax s2)
                    {
                        if (s2.Right.Identifier != null && s2.Right.Identifier.ValueText != null && s2.Right.Identifier.ValueText == classNameWithoutGenerics)
                        {
                            hasClassNameIdentifier = true;
                            break;
                        }
                    }
                }
                if (!hasClassNameIdentifier)
                    return false;

                // Has Partial keyword
                var hasPartial = false;
                foreach (var m in classDeclarationSyntax.Modifiers)
                {
                    if (m.IsKind(SyntaxKind.PartialKeyword))
                    {
                        hasPartial = true;
                        break;
                    }
                }

                return hasPartial;
            }
            return false;
        }

        public static ClassDeclarationSyntax GetSemanticClassGenericMatch(GeneratorSyntaxContext ctx,
                                                                          CancellationToken cancellationToken,
                                                                          string fullSemanticBaseClassName)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var classDeclarationSyntax = (ClassDeclarationSyntax)ctx.Node;
            foreach (var baseTypeSyntax in classDeclarationSyntax.BaseList !.Types)
            {
                var type = ctx.SemanticModel.GetTypeInfo(baseTypeSyntax.Type).Type;
                if (type.BaseType != null && type.BaseType.ToFullName() == fullSemanticBaseClassName)
                    return classDeclarationSyntax;
            }
            return null;
        }
    }
}

