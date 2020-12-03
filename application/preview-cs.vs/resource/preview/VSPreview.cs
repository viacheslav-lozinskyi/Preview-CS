
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections;
using System.IO;
using System.Linq;

namespace resource.preview
{
    internal class VSPreview : cartridge.AnyPreview
    {
        internal class HINT
        {
            public static string DATA_TYPE = "[[Data type]]";
            public static string METHOD_TYPE = "[[Method type]]";
        }

        protected override void _Execute(atom.Trace context, string url)
        {
            var a_Context = CSharpSyntaxTree.ParseText(File.ReadAllText(url)).WithFilePath(url).GetRoot();
            var a_IsFound = GetProperty(NAME.PROPERTY.DEBUGGING_SHOW_PRIVATE) != 0;
            if (a_Context == null)
            {
                return;
            }
            else
            {
                context.
                    SetState(NAME.STATE.HEADER).
                    Send(NAME.SOURCE.PREVIEW, NAME.TYPE.FOLDER, 1, "[[Info]]");
                {
                    context.
                        SetValue(url).
                        Send(NAME.SOURCE.PREVIEW, NAME.TYPE.VARIABLE, 2, "[[File Name]]");
                    context.
                        SetValue(a_Context.GetText().Length.ToString()).
                        Send(NAME.SOURCE.PREVIEW, NAME.TYPE.VARIABLE, 2, "[[File Size]]");
                    context.
                        SetValue(a_Context.Language).
                        Send(NAME.SOURCE.PREVIEW, NAME.TYPE.VARIABLE, 2, "[[Language]]");
                }
            }
            if (a_Context.DescendantNodes().OfType<UsingDirectiveSyntax>().Any())
            {
                context.
                    SetComment(__GetArraySize(a_Context.DescendantNodes().OfType<UsingDirectiveSyntax>())).
                    Send(NAME.SOURCE.PREVIEW, NAME.TYPE.FOLDER, 1, "[[Dependencies]]");
                foreach (var a_Context1 in a_Context.DescendantNodes().OfType<UsingDirectiveSyntax>())
                {
                    __Execute(a_Context1, 2, context, url);
                }
            }
            if (a_Context.DescendantNodes().OfType<ClassDeclarationSyntax>().Any())
            {
                context.
                    SetComment(__GetArraySize(a_Context.DescendantNodes().OfType<ClassDeclarationSyntax>())).
                    Send(NAME.SOURCE.PREVIEW, NAME.TYPE.FOLDER, 1, "[[Classes]]");
                foreach (var a_Context1 in a_Context.DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    __Execute(a_Context1, 2, context, url, a_IsFound);
                }
            }
            if (a_Context.DescendantNodes().OfType<StructDeclarationSyntax>().Any())
            {
                context.
                    SetComment(__GetArraySize(a_Context.DescendantNodes().OfType<StructDeclarationSyntax>())).
                    Send(NAME.SOURCE.PREVIEW, NAME.TYPE.FOLDER, 1, "[[Structs]]");
                foreach (var a_Context1 in a_Context.DescendantNodes().OfType<StructDeclarationSyntax>())
                {
                    __Execute(a_Context1, 2, context, url, a_IsFound);
                }
            }
            if (a_Context.DescendantNodes().OfType<EnumDeclarationSyntax>().Any())
            {
                context.
                    SetComment(__GetArraySize(a_Context.DescendantNodes().OfType<EnumDeclarationSyntax>())).
                    Send(NAME.SOURCE.PREVIEW, NAME.TYPE.FOLDER, 1, "[[Enums]]");
                foreach (var a_Context1 in a_Context.DescendantNodes().OfType<EnumDeclarationSyntax>())
                {
                    __Execute(a_Context1, 2, context, url, a_IsFound);
                }
            }
            if (a_Context.DescendantNodes().OfType<MethodDeclarationSyntax>().Any())
            {
                context.
                    SetComment(__GetArraySize(a_Context.DescendantNodes().OfType<MethodDeclarationSyntax>())).
                    Send(NAME.SOURCE.PREVIEW, NAME.TYPE.FOLDER, 1, "[[Functions]]");
                foreach (var a_Context1 in a_Context.DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    __Execute(a_Context1, 2, context, url, true, a_IsFound);
                }
            }
            if (a_Context.GetDiagnostics().Any())
            {
                context.
                    SetState(NAME.STATE.FOOTER).
                    SetComment(__GetArraySize(a_Context.GetDiagnostics())).
                    Send(NAME.SOURCE.PREVIEW, NAME.TYPE.ERROR, 1, "[[Diagnostics]]");
                foreach (var a_Context1 in a_Context.GetDiagnostics())
                {
                    __Execute(a_Context1, 2, context, url);
                }
            }
            if (GetState() == STATE.CANCEL)
            {
                context.
                    SendWarning(1, NAME.WARNING.TERMINATED);
            }
        }

        private static void __Execute(Diagnostic node, int level, atom.Trace context, string url)
        {
            context.
                SetState(__GetSeverity(node)).
                SetUrlLine(__GetLine(node.Location)).
                SetUrlPosition(__GetPosition(node.Location)).
                SetUrl(url).
                SetLink("https://www.bing.com/search?q=" + node.Id).
                Send(NAME.SOURCE.PREVIEW, NAME.TYPE.INFO, level, node.Descriptor.MessageFormat.ToString());
        }

        private static void __Execute(UsingDirectiveSyntax node, int level, atom.Trace context, string url)
        {
            context.
                SetComment("using").
                SetCommentHint(HINT.DATA_TYPE).
                SetUrlLine(__GetLine(node.GetLocation())).
                SetUrlPosition(__GetPosition(node.GetLocation())).
                SetUrl(url).
                Send(NAME.SOURCE.PREVIEW, NAME.TYPE.INFO, level, node.Name.ToString());
        }

        private static void __Execute(ClassDeclarationSyntax node, int level, atom.Trace context, string url, bool isShowPrivate)
        {
            if (__IsEnabled(node, isShowPrivate))
            {
                context.
                    SetComment(__GetType(node, "class")).
                    SetCommentHint(HINT.DATA_TYPE).
                    SetUrlLine(__GetLine(node.GetLocation())).
                    SetUrlPosition(__GetPosition(node.GetLocation())).
                    SetUrl(url).
                    Send(NAME.SOURCE.PREVIEW, NAME.TYPE.CLASS, level, __GetName(node, true));
                foreach (var a_Context in node.Members.OfType<MethodDeclarationSyntax>())
                {
                    __Execute(a_Context, level + 1, context, url, false, isShowPrivate);
                }
                foreach (var a_Context in node.Members.OfType<PropertyDeclarationSyntax>())
                {
                    __Execute(a_Context, level + 1, context, url, isShowPrivate);
                }
                foreach (var a_Context in node.Members.OfType<FieldDeclarationSyntax>())
                {
                    __Execute(a_Context, level + 1, context, url, isShowPrivate);
                }
            }
        }

        private static void __Execute(EnumDeclarationSyntax node, int level, atom.Trace context, string url, bool isShowPrivate)
        {
            if (__IsEnabled(node, isShowPrivate))
            {
                context.
                    SetComment(__GetType(node, "enum")).
                    SetCommentHint(HINT.DATA_TYPE).
                    SetUrlLine(__GetLine(node.GetLocation())).
                    SetUrlPosition(__GetPosition(node.GetLocation())).
                    SetUrl(url).
                    Send(NAME.SOURCE.PREVIEW, NAME.TYPE.CLASS, level, __GetName(node, true));
                foreach (var a_Context in node.Members.OfType<EnumMemberDeclarationSyntax>())
                {
                    __Execute(a_Context, level + 1, context, url, isShowPrivate);
                }
            }
        }

        private static void __Execute(EnumMemberDeclarationSyntax node, int level, atom.Trace context, string url, bool isShowPrivate)
        {
            if (__IsEnabled(node, isShowPrivate))
            {
                context.
                    SetComment(__GetType(node, "int")).
                    SetCommentHint(HINT.DATA_TYPE).
                    SetUrlLine(__GetLine(node.GetLocation())).
                    SetUrlPosition(__GetPosition(node.GetLocation())).
                    SetUrl(url).
                    Send(NAME.SOURCE.PREVIEW, NAME.TYPE.INFO, level, node.Identifier.ValueText);
            }
        }

        private static void __Execute(StructDeclarationSyntax node, int level, atom.Trace context, string url, bool isShowPrivate)
        {
            if (__IsEnabled(node, isShowPrivate))
            {
                context.
                    SetComment(__GetType(node, "struct")).
                    SetCommentHint(HINT.DATA_TYPE).
                    SetUrlLine(__GetLine(node.GetLocation())).
                    SetUrlPosition(__GetPosition(node.GetLocation())).
                    SetUrl(url).
                    Send(NAME.SOURCE.PREVIEW, NAME.TYPE.CLASS, level, __GetName(node, true));
                foreach (var a_Context in node.Members.OfType<MethodDeclarationSyntax>())
                {
                    __Execute(a_Context, level + 1, context, url, false, isShowPrivate);
                }
                foreach (var a_Context in node.Members.OfType<PropertyDeclarationSyntax>())
                {
                    __Execute(a_Context, level + 1, context, url, isShowPrivate);
                }
                foreach (var a_Context in node.Members.OfType<FieldDeclarationSyntax>())
                {
                    __Execute(a_Context, level + 1, context, url, isShowPrivate);
                }
            }
        }

        private static void __Execute(MethodDeclarationSyntax node, int level, atom.Trace context, string url, bool isFullName, bool isShowPrivate)
        {
            if (__IsEnabled(node, isShowPrivate))
            {
                context.
                    SetComment(__GetType(node, node.ReturnType?.ToString())).
                    SetCommentHint(HINT.DATA_TYPE).
                    SetUrlLine(__GetLine(node.GetLocation())).
                    SetUrlPosition(__GetPosition(node.GetLocation())).
                    SetUrl(url).
                    Send(NAME.SOURCE.PREVIEW, NAME.TYPE.FUNCTION, level, __GetName(node, isFullName));
            }
        }

        private static void __Execute(PropertyDeclarationSyntax node, int level, atom.Trace context, string url, bool isShowPrivate)
        {
            if (__IsEnabled(node, isShowPrivate))
            {
                context.
                    SetComment(__GetType(node, node.Type?.ToString())).
                    SetCommentHint(HINT.DATA_TYPE).
                    SetUrlLine(__GetLine(node.GetLocation())).
                    SetUrlPosition(__GetPosition(node.GetLocation())).
                    SetUrl(url).
                    SetValue(node.Initializer?.Value?.ToString()).
                    Send(NAME.SOURCE.PREVIEW, NAME.TYPE.PARAMETER, level, node.Identifier.ValueText);
            }
        }

        private static void __Execute(FieldDeclarationSyntax node, int level, atom.Trace context, string url, bool isShowPrivate)
        {
            if (__IsEnabled(node, isShowPrivate))
            {
                context.
                    SetComment(__GetType(node, node.Declaration.Type?.ToString())).
                    SetCommentHint(HINT.DATA_TYPE).
                    SetUrlLine(__GetLine(node.GetLocation())).
                    SetUrlPosition(__GetPosition(node.GetLocation())).
                    SetUrl(url).
                    SetValue(node.Declaration.Variables.First()?.Initializer?.Value?.ToString()).
                    Send(NAME.SOURCE.PREVIEW, NAME.TYPE.VARIABLE, level, node.Declaration.Variables.First()?.Identifier.ValueText);
            }
        }

        private static bool __IsEnabled(MemberDeclarationSyntax node, bool isShowPrivate)
        {
            if (GetState() == STATE.CANCEL)
            {
                return false;
            }
            if (isShowPrivate == false)
            {
                var a_Context = node.Modifiers.ToString();
                if (string.IsNullOrEmpty(a_Context) == false)
                {
                    if (a_Context.Contains("private"))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static string __GetType(MemberDeclarationSyntax node, string typeName)
        {
            if (node.Modifiers != null)
            {
                return node.Modifiers.ToString().Trim() + " " + typeName;
            }    
            return typeName;
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

        private static string __GetSeverity(Diagnostic node)
        {
            switch (node.Severity)
            {
                case DiagnosticSeverity.Hidden: return NAME.TYPE.DEBUG;
                case DiagnosticSeverity.Info: return NAME.TYPE.INFO;
                case DiagnosticSeverity.Warning: return NAME.TYPE.WARNING;
                case DiagnosticSeverity.Error: return NAME.TYPE.ERROR;
            }
            return NAME.TYPE.INFO;
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
