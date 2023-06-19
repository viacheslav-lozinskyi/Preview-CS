using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace resource.package
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(CONSTANT.GUID)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.ShellInitialized_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class PreviewCS : AsyncPackage
    {
        internal static class CONSTANT
        {
            public const string APPLICATION = "Visual Studio";
            public const string COMPANY = "Viacheslav Lozinskyi";
            public const string COPYRIGHT = "Copyright (c) 2020-2023 by Viacheslav Lozinskyi. All rights reserved.";
            public const string DESCRIPTION = "Quick preview of CS files";
            public const string GUID = "5F8DF008-CD59-49F9-91C1-7A6FD770FDF2";
            public const string HOST = "MetaOutput";
            public const string NAME = "Preview-CS";
            public const string VERSION = "1.1.0";
        }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            extension.AnyPreview.Connect(CONSTANT.APPLICATION, CONSTANT.NAME);
            extension.AnyPreview.Register(".CS", new preview.VSPreview());
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        }

        protected override int QueryClose(out bool canClose)
        {
            extension.AnyPreview.Disconnect();
            canClose = true;
            return VSConstants.S_OK;
        }
    }
}
