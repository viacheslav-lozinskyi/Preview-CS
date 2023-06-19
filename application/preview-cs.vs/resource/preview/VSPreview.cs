using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections;
using System.IO;
using System.Linq;

namespace resource.preview
{
    internal class VSPreview : extension.AnyPreview
    {
        internal class HINT
        {
            public static string DATA_TYPE = "[[[Data Type]]]";
            public static string METHOD_TYPE = "[[[Method Type]]]";
        }

        protected override void _Execute(atom.Trace context, int level, string url, string file)
        {
            var a_Context = CSharpSyntaxTree.ParseText(File.ReadAllText(file)).WithFilePath(file).GetRoot();
            var a_IsFound = GetProperty(NAME.PROPERTY.DEBUGGING_SHOW_PRIVATE, true) != 0;
            if (a_Context == null)
            {
                return;
            }
            else
            {
                context.Send(NAME.SOURCE.PREVIEW, NAME.EVENT.HEADER, level, "[[[Info]]]");
                {
                    context.Send(NAME.SOURCE.PREVIEW, NAME.EVENT.PARAMETER, level + 1, "[[[File Name]]]", url);
                    context.Send(NAME.SOURCE.PREVIEW, NAME.EVENT.PARAMETER, level + 1, "[[[File Size]]]", a_Context.GetText().Length.ToString());
                    context.Send(NAME.SOURCE.PREVIEW, NAME.EVENT.PARAMETER, level + 1, "[[[Language]]]", a_Context.Language);
                }
            }
            if (a_Context.DescendantNodes().OfType<UsingDirectiveSyntax>().Any())
            {
                context.
                    SetComment(__GetArraySize(a_Context.DescendantNodes().OfType<UsingDirectiveSyntax>())).
                    Send(NAME.SOURCE.PREVIEW, NAME.EVENT.FOLDER, level, "[[[Dependencies]]]");
                foreach (var a_Context1 in a_Context.DescendantNodes().OfType<UsingDirectiveSyntax>())
                {
                    __Execute(context, level + 1, a_Context1, file);
                }
            }
            if (a_Context.DescendantNodes().OfType<ClassDeclarationSyntax>().Any())
            {
                context.
                    SetComment(__GetArraySize(a_Context.DescendantNodes().OfType<ClassDeclarationSyntax>())).
                    Send(NAME.SOURCE.PREVIEW, NAME.EVENT.FOLDER, level, "[[[Classes]]]");
                foreach (var a_Context1 in a_Context.DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    __Execute(context, level + 1, a_Context1, file, a_IsFound);
                }
            }
            if (a_Context.DescendantNodes().OfType<StructDeclarationSyntax>().Any())
            {
                context.
                    SetComment(__GetArraySize(a_Context.DescendantNodes().OfType<StructDeclarationSyntax>())).
                    Send(NAME.SOURCE.PREVIEW, NAME.EVENT.FOLDER, level, "[[[Structs]]]");
                foreach (var a_Context1 in a_Context.DescendantNodes().OfType<StructDeclarationSyntax>())
                {
                    __Execute(context, level + 1, a_Context1, file, a_IsFound);
                }
            }
            if (a_Context.DescendantNodes().OfType<EnumDeclarationSyntax>().Any())
            {
                context.
                    SetComment(__GetArraySize(a_Context.DescendantNodes().OfType<EnumDeclarationSyntax>())).
                    Send(NAME.SOURCE.PREVIEW, NAME.EVENT.FOLDER, level, "[[[Enums]]]");
                foreach (var a_Context1 in a_Context.DescendantNodes().OfType<EnumDeclarationSyntax>())
                {
                    __Execute(context, level + 1, a_Context1, file, a_IsFound);
                }
            }
            if (a_Context.DescendantNodes().OfType<MethodDeclarationSyntax>().Any())
            {
                context.
                    SetComment(__GetArraySize(a_Context.DescendantNodes().OfType<MethodDeclarationSyntax>())).
                    Send(NAME.SOURCE.PREVIEW, NAME.EVENT.FOLDER, level, "[[[Functions]]]");
                foreach (var a_Context1 in a_Context.DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    __Execute(context, level + 1, a_Context1, file, true, a_IsFound);
                }
            }
            if (a_Context.GetDiagnostics().Any())
            {
                context.
                    SendPreview(NAME.EVENT.ERROR, url).
                    SetComment(__GetArraySize(a_Context.GetDiagnostics())).
                    Send(NAME.SOURCE.PREVIEW, NAME.EVENT.ERROR, level, "[[[Diagnostics]]]");
                foreach (var a_Context1 in a_Context.GetDiagnostics())
                {
                    __Execute(context, level + 1, a_Context1, file);
                }
            }
        }

        private static void __Execute(atom.Trace context, int level, Diagnostic data, string file)
        {
            context.
                SetUrl(file, __GetLine(data.Location), __GetPosition(data.Location)).
                SetUrlInfo("https://www.bing.com/search?q=" + data.Id).
                Send(NAME.SOURCE.PREVIEW, __GetType(data), level, data.Descriptor.MessageFormat.ToString());
        }

        private static void __Execute(atom.Trace context, int level, UsingDirectiveSyntax data, string file)
        {
            context.
                SetComment("using", HINT.DATA_TYPE).
                SetUrl(file, __GetLine(data.GetLocation()), __GetPosition(data.GetLocation())).
                Send(NAME.SOURCE.PREVIEW, NAME.EVENT.FILE, level, data.Name.ToString());
        }

        private static void __Execute(atom.Trace context, int level, ClassDeclarationSyntax data, string file, bool isShowPrivate)
        {
            if (__IsEnabled(data, isShowPrivate))
            {
                context.
                    SetComment(__GetType(data, "class"), HINT.DATA_TYPE).
                    SetUrl(file, __GetLine(data.GetLocation()), __GetPosition(data.GetLocation())).
                    Send(NAME.SOURCE.PREVIEW, NAME.EVENT.CLASS, level, __GetName(data, true));
                foreach (var a_Context in data.Members.OfType<MethodDeclarationSyntax>())
                {
                    __Execute(context, level + 1, a_Context, file, false, isShowPrivate);
                }
                foreach (var a_Context in data.Members.OfType<PropertyDeclarationSyntax>())
                {
                    __Execute(context, level + 1, a_Context, file, isShowPrivate);
                }
                foreach (var a_Context in data.Members.OfType<FieldDeclarationSyntax>())
                {
                    __Execute(context, level + 1, a_Context, file, isShowPrivate);
                }
            }
        }

        private static void __Execute(atom.Trace context, int level, EnumDeclarationSyntax data, string file, bool isShowPrivate)
        {
            if (__IsEnabled(data, isShowPrivate))
            {
                context.
                    SetComment(__GetType(data, "enum"), HINT.DATA_TYPE).
                    SetUrl(file, __GetLine(data.GetLocation()), __GetPosition(data.GetLocation())).
                    Send(NAME.SOURCE.PREVIEW, NAME.EVENT.CLASS, level, __GetName(data, true));
                foreach (var a_Context in data.Members.OfType<EnumMemberDeclarationSyntax>())
                {
                    __Execute(context, level + 1, a_Context, file, isShowPrivate);
                }
            }
        }

        private static void __Execute(atom.Trace context, int level, EnumMemberDeclarationSyntax data, string file, bool isShowPrivate)
        {
            if (__IsEnabled(data, isShowPrivate))
            {
                context.
                    SetComment(__GetType(data, "int"), HINT.DATA_TYPE).
                    SetUrl(file, __GetLine(data.GetLocation()), __GetPosition(data.GetLocation())).
                    Send(NAME.SOURCE.PREVIEW, NAME.EVENT.PARAMETER, level, data.Identifier.ValueText);
            }
        }

        private static void __Execute(atom.Trace context, int level, StructDeclarationSyntax data, string file, bool isShowPrivate)
        {
            if (__IsEnabled(data, isShowPrivate))
            {
                context.
                    SetComment(__GetType(data, "struct"), HINT.DATA_TYPE).
                    SetUrl(file, __GetLine(data.GetLocation()), __GetPosition(data.GetLocation())).
                    Send(NAME.SOURCE.PREVIEW, NAME.EVENT.CLASS, level, __GetName(data, true));
                foreach (var a_Context in data.Members.OfType<MethodDeclarationSyntax>())
                {
                    __Execute(context, level + 1, a_Context, file, false, isShowPrivate);
                }
                foreach (var a_Context in data.Members.OfType<PropertyDeclarationSyntax>())
                {
                    __Execute(context, level + 1, a_Context, file, isShowPrivate);
                }
                foreach (var a_Context in data.Members.OfType<FieldDeclarationSyntax>())
                {
                    __Execute(context, level + 1, a_Context, file, isShowPrivate);
                }
            }
        }

        private static void __Execute(atom.Trace context, int level, MethodDeclarationSyntax data, string file, bool isFullName, bool isShowPrivate)
        {
            if (__IsEnabled(data, isShowPrivate))
            {
                context.
                    SetComment(__GetType(data, data.ReturnType?.ToString()), HINT.DATA_TYPE).
                    SetUrl(file, __GetLine(data.GetLocation()), __GetPosition(data.GetLocation())).
                    Send(NAME.SOURCE.PREVIEW, NAME.EVENT.FUNCTION, level, __GetName(data, isFullName));
            }
        }

        private static void __Execute(atom.Trace context, int level, PropertyDeclarationSyntax data, string file, bool isShowPrivate)
        {
            if (__IsEnabled(data, isShowPrivate))
            {
                context.
                    SetComment(__GetType(data, data.Type?.ToString()), HINT.DATA_TYPE).
                    SetUrl(file, __GetLine(data.GetLocation()), __GetPosition(data.GetLocation())).
                    SetValue(data.Initializer?.Value?.ToString()).
                    Send(NAME.SOURCE.PREVIEW, NAME.EVENT.PARAMETER, level, data.Identifier.ValueText);
            }
        }

        private static void __Execute(atom.Trace context, int level, FieldDeclarationSyntax data, string file, bool isShowPrivate)
        {
            if (__IsEnabled(data, isShowPrivate))
            {
                context.
                    SetComment(__GetType(data, data.Declaration.Type?.ToString()), HINT.DATA_TYPE).
                    SetUrl(file, __GetLine(data.GetLocation()), __GetPosition(data.GetLocation())).
                    SetValue(data.Declaration.Variables.First()?.Initializer?.Value?.ToString()).
                    Send(NAME.SOURCE.PREVIEW, NAME.EVENT.VARIABLE, level, data.Declaration.Variables.First()?.Identifier.ValueText);
            }
        }

        private static bool __IsEnabled(MemberDeclarationSyntax data, bool isShowPrivate)
        {
            if (GetState() == NAME.STATE.WORK.CANCEL)
            {
                return false;
            }
            if (isShowPrivate == false)
            {
                var a_Context = data.Modifiers.ToString();
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

        private static string __GetType(MemberDeclarationSyntax data, string typeName)
        {
            if (data.Modifiers != null)
            {
                return data.Modifiers.ToString().Trim() + " " + typeName;
            }
            return typeName;
        }

        internal static string __GetArraySize(IEnumerable data)
        {
            var a_Result = 0;
            foreach (var a_Context in data)
            {
                a_Result++;
            }
            return "[[[Found]]]: " + a_Result.ToString();
        }

        private static string __GetType(Diagnostic data)
        {
            switch (data.Severity)
            {
                case DiagnosticSeverity.Hidden: return NAME.EVENT.DEBUG;
                case DiagnosticSeverity.Info: return NAME.EVENT.PARAMETER;
                case DiagnosticSeverity.Warning: return NAME.EVENT.WARNING;
                case DiagnosticSeverity.Error: return NAME.EVENT.ERROR;
            }
            return NAME.EVENT.PARAMETER;
        }

        private static string __GetName(SyntaxNode data, bool isFullName)
        {
            var a_Result = "";
            var a_Context = data;
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

        private static string __GetParams(BaseMethodDeclarationSyntax data)
        {
            var a_Result = "";
            var a_Context = "";
            foreach (var a_Context1 in data.ParameterList.Parameters)
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

        private static int __GetLine(Location data)
        {
            if (data.Kind != LocationKind.None)
            {
                return data.GetLineSpan().StartLinePosition.Line + 1;
            }
            return 0;
        }

        private static int __GetPosition(Location data)
        {
            if (data.Kind != LocationKind.None)
            {
                return data.GetLineSpan().StartLinePosition.Character + 1;
            }
            return 0;
        }
    };
}
