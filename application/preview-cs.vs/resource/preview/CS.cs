
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections;
using System.IO;
using System.Linq;

namespace resource.preview
{
    public class CS : cartridge.AnyPreview
    {
        internal class HINT
        {
            public static string DATA_TYPE = "[[Data type]]";
            public static string METHOD_TYPE = "[[Method type]]";
        }

        protected override void _Execute(atom.Trace context, string url)
        {
            var a_Context = CSharpSyntaxTree.ParseText(File.ReadAllText(url)).WithFilePath(url).GetRoot();
            {
                context.
                    SetFlag(NAME.FLAG.EXPAND).
                    Send(NAME.PATTERN.FOLDER, 1, "[[Info]]");
                {
                    context.
                        SetValue(url).
                        Send(NAME.PATTERN.VARIABLE, 2, "[[File name]]");
                    context.
                        SetValue(a_Context.GetText().Length.ToString()).
                        Send(NAME.PATTERN.VARIABLE, 2, "[[File size]]");
                    context.
                        SetValue(a_Context.Language).
                        Send(NAME.PATTERN.VARIABLE, 2, "[[Language]]");
                }
            }
            if (a_Context.DescendantNodes().OfType<UsingDirectiveSyntax>().Any())
            {
                context.
                    SetComment(__GetArraySize(a_Context.DescendantNodes().OfType<UsingDirectiveSyntax>())).
                    Send(NAME.PATTERN.FOLDER, 1, "[[Dependencies]]");
                foreach (var a_Context1 in a_Context.DescendantNodes().OfType<UsingDirectiveSyntax>())
                {
                    __Execute(a_Context1, 2, context, url);
                }
            }
            if (a_Context.DescendantNodes().OfType<ClassDeclarationSyntax>().Any())
            {
                context.
                    SetComment(__GetArraySize(a_Context.DescendantNodes().OfType<ClassDeclarationSyntax>())).
                    Send(NAME.PATTERN.FOLDER, 1, "[[Classes]]");
                foreach (var a_Context1 in a_Context.DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    __Execute(a_Context1, 2, context, url);
                }
            }
            if (a_Context.DescendantNodes().OfType<StructDeclarationSyntax>().Any())
            {
                context.
                    SetComment(__GetArraySize(a_Context.DescendantNodes().OfType<StructDeclarationSyntax>())).
                    Send(NAME.PATTERN.FOLDER, 1, "[[Structs]]");
                foreach (var a_Context1 in a_Context.DescendantNodes().OfType<StructDeclarationSyntax>())
                {
                    __Execute(a_Context1, 2, context, url);
                }
            }
            if (a_Context.DescendantNodes().OfType<EnumDeclarationSyntax>().Any())
            {
                context.
                    SetComment(__GetArraySize(a_Context.DescendantNodes().OfType<EnumDeclarationSyntax>())).
                    Send(NAME.PATTERN.FOLDER, 1, "[[Enums]]");
                foreach (var a_Context1 in a_Context.DescendantNodes().OfType<EnumDeclarationSyntax>())
                {
                    __Execute(a_Context1, 2, context, url);
                }
            }
            if (a_Context.DescendantNodes().OfType<MethodDeclarationSyntax>().Any())
            {
                context.
                    SetComment(__GetArraySize(a_Context.DescendantNodes().OfType<MethodDeclarationSyntax>())).
                    Send(NAME.PATTERN.FOLDER, 1, "[[Functions]]");
                foreach (var a_Context1 in a_Context.DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    __Execute(a_Context1, 2, context, url, true);
                }
            }
            if (a_Context.GetDiagnostics().Any())
            {
                context.
                    SetComment(__GetArraySize(a_Context.GetDiagnostics())).
                    Send(NAME.PATTERN.FOLDER, 1, "[[Diagnostics]]");
                foreach (var a_Context1 in a_Context.GetDiagnostics())
                {
                    __Execute(a_Context1, 2, context, url);
                }
            }
        }

        private static void __Execute(Diagnostic node, int level, atom.Trace context, string url)
        {
            context.
                SetFlag(__GetFlag(node)).
                SetLine(__GetLine(node.Location)).
                SetPosition(__GetPosition(node.Location)).
                SetUrl(url).
                SetLink("https://www.bing.com/search?q=" + node.Id).
                Send(NAME.PATTERN.ELEMENT, level, node.Descriptor.MessageFormat.ToString());
        }

        private static void __Execute(UsingDirectiveSyntax node, int level, atom.Trace context, string url)
        {
            context.
                SetComment("using").
                SetHint(HINT.DATA_TYPE).
                SetLine(__GetLine(node.GetLocation())).
                SetPosition(__GetPosition(node.GetLocation())).
                SetUrl(url).
                Send(NAME.PATTERN.ELEMENT, level, node.Name.ToString());
        }

        private static void __Execute(ClassDeclarationSyntax node, int level, atom.Trace context, string url)
        {
            if (__IsEnabled(node))
            {
                context.
                    SetComment("class").
                    SetHint(HINT.DATA_TYPE).
                    SetLine(__GetLine(node.GetLocation())).
                    SetPosition(__GetPosition(node.GetLocation())).
                    SetUrl(url).
                    Send(NAME.PATTERN.CLASS, level, __GetName(node, true));
                foreach (var a_Context in node.Members.OfType<MethodDeclarationSyntax>())
                {
                    __Execute(a_Context, level + 1, context, url, false);
                }
                foreach (var a_Context in node.Members.OfType<PropertyDeclarationSyntax>())
                {
                    __Execute(a_Context, level + 1, context, url);
                }
                foreach (var a_Context in node.Members.OfType<FieldDeclarationSyntax>())
                {
                    __Execute(a_Context, level + 1, context, url);
                }
            }
        }

        private static void __Execute(EnumDeclarationSyntax node, int level, atom.Trace context, string url)
        {
            if (__IsEnabled(node))
            {
                context.
                    SetComment("enum").
                    SetHint(HINT.DATA_TYPE).
                    SetLine(__GetLine(node.GetLocation())).
                    SetPosition(__GetPosition(node.GetLocation())).
                    SetUrl(url).
                    Send(NAME.PATTERN.CLASS, level, __GetName(node, true));
                foreach (var a_Context in node.Members.OfType<EnumMemberDeclarationSyntax>())
                {
                    __Execute(a_Context, level + 1, context, url);
                }
            }
        }

        private static void __Execute(EnumMemberDeclarationSyntax node, int level, atom.Trace context, string url)
        {
            if (__IsEnabled(node))
            {
                context.
                    SetComment("int").
                    SetHint(HINT.DATA_TYPE).
                    SetLine(__GetLine(node.GetLocation())).
                    SetPosition(__GetPosition(node.GetLocation())).
                    SetUrl(url).
                    Send(NAME.PATTERN.ELEMENT, level, node.Identifier.ValueText);
            }
        }

        private static void __Execute(StructDeclarationSyntax node, int level, atom.Trace context, string url)
        {
            if (__IsEnabled(node))
            {
                context.
                    SetComment("struct").
                    SetHint(HINT.DATA_TYPE).
                    SetLine(__GetLine(node.GetLocation())).
                    SetPosition(__GetPosition(node.GetLocation())).
                    SetUrl(url).
                    Send(NAME.PATTERN.CLASS, level, __GetName(node, true));
                foreach (var a_Context in node.Members.OfType<MethodDeclarationSyntax>())
                {
                    __Execute(a_Context, level + 1, context, url, false);
                }
                foreach (var a_Context in node.Members.OfType<PropertyDeclarationSyntax>())
                {
                    __Execute(a_Context, level + 1, context, url);
                }
                foreach (var a_Context in node.Members.OfType<FieldDeclarationSyntax>())
                {
                    __Execute(a_Context, level + 1, context, url);
                }
            }
        }

        private static void __Execute(MethodDeclarationSyntax node, int level, atom.Trace context, string url, bool isFullName)
        {
            if (__IsEnabled(node))
            {
                context.
                    SetComment(node.ReturnType?.ToString()).
                    SetHint(HINT.DATA_TYPE).
                    SetLine(__GetLine(node.GetLocation())).
                    SetPosition(__GetPosition(node.GetLocation())).
                    SetUrl(url).
                    Send(NAME.PATTERN.FUNCTION, level, __GetName(node, isFullName));
            }
        }

        private static void __Execute(PropertyDeclarationSyntax node, int level, atom.Trace context, string url)
        {
            if (__IsEnabled(node))
            {
                context.
                    SetComment(node.Type?.ToString()).
                    SetHint(HINT.DATA_TYPE).
                    SetLine(__GetLine(node.GetLocation())).
                    SetPosition(__GetPosition(node.GetLocation())).
                    SetUrl(url).
                    SetValue(node.Initializer?.Value?.ToString()).
                    Send(NAME.PATTERN.PARAMETER, level, node.Identifier.ValueText);
            }
        }

        private static void __Execute(FieldDeclarationSyntax node, int level, atom.Trace context, string url)
        {
            if (__IsEnabled(node))
            {
                context.
                    SetComment(node.Declaration.Type?.ToString()).
                    SetHint(HINT.DATA_TYPE).
                    SetLine(__GetLine(node.GetLocation())).
                    SetPosition(__GetPosition(node.GetLocation())).
                    SetUrl(url).
                    SetValue(node.Declaration.Variables.First()?.Initializer?.Value?.ToString()).
                    Send(NAME.PATTERN.VARIABLE, level, node.Declaration.Variables.First()?.Identifier.ValueText);
            }
        }

        private static bool __IsEnabled(MemberDeclarationSyntax node)
        {
            var a_Context = node.Modifiers.ToString();
            if (string.IsNullOrEmpty(a_Context) == false)
            {
                if (a_Context.Contains("private"))
                {
                    return false;
                }
            }
            return true;
        }

        internal static string __GetArraySize(IEnumerable value)
        {
            var a_Result = 0;
            foreach (var a_Context in value)
            {
                a_Result++;
            }
            return "[[Found]]: " + a_Result.ToString();
        }

        private static string __GetFlag(Diagnostic node)
        {
            switch (node.Severity)
            {
                case DiagnosticSeverity.Hidden: return NAME.FLAG.DEBUG;
                case DiagnosticSeverity.Info: return NAME.FLAG.NONE;
                case DiagnosticSeverity.Warning: return NAME.FLAG.WARNING;
                case DiagnosticSeverity.Error: return NAME.FLAG.ERROR;
            }
            return "";
        }

        private static string __GetName(SyntaxNode node, bool isFullName)
        {
            var a_Result = "";
            var a_Context = node;
            while (a_Context != null)
            {
                if (isFullName)
                {
                    if ((a_Context is NamespaceDeclarationSyntax) && (((a_Context as NamespaceDeclarationSyntax).Name as IdentifierNameSyntax) != null))
                    {
                        a_Result = ((a_Context as NamespaceDeclarationSyntax).Name as IdentifierNameSyntax).Identifier.ValueText + "." + a_Result;
                    }
                    if (a_Context is ClassDeclarationSyntax)
                    {
                        a_Result = (a_Context as ClassDeclarationSyntax).Identifier.ValueText + (string.IsNullOrEmpty(a_Result) ? "" : ("." + a_Result));
                    }
                }
                else
                {
                    if ((a_Context is ClassDeclarationSyntax) && string.IsNullOrEmpty(a_Result))
                    {
                        a_Result = (a_Context as ClassDeclarationSyntax).Identifier.ValueText;
                    }
                }
                if (a_Context is MethodDeclarationSyntax)
                {
                    a_Result = (a_Context as MethodDeclarationSyntax).Identifier.ValueText + __GetParams(a_Context as MethodDeclarationSyntax);
                }
                if (a_Context is PropertyDeclarationSyntax)
                {
                    a_Result = (a_Context as PropertyDeclarationSyntax).Identifier.ValueText;
                }
                if (a_Context is EnumDeclarationSyntax)
                {
                    a_Result = (a_Context as EnumDeclarationSyntax).Identifier.ValueText;
                }
                if (a_Context is StructDeclarationSyntax)
                {
                    a_Result = (a_Context as StructDeclarationSyntax).Identifier.ValueText;
                }
                {
                    a_Context = a_Context.Parent;
                }
            }
            return a_Result;
        }

        private static string __GetParams(BaseMethodDeclarationSyntax node)
        {
            var a_Result = "";
            var a_Context = "";
            foreach (var a_Context1 in node.ParameterList.Parameters)
            {
                if (a_Context1.Type != null)
                {
                    a_Result += a_Context;
                    a_Result += a_Context1.Type?.ToString() + " ";
                    a_Result += a_Context1.Identifier.ValueText;
                    a_Context = ", ";
                }
            }
            return "(" + a_Result + ")";
        }

        private static int __GetLine(Location node)
        {
            if (node.Kind != LocationKind.None)
            {
                return node.GetLineSpan().StartLinePosition.Line + 1;
            }
            return 0;
        }

        private static int __GetPosition(Location node)
        {
            if (node.Kind != LocationKind.None)
            {
                return node.GetLineSpan().StartLinePosition.Character + 1;
            }
            return 0;
        }
    };
}
