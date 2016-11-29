using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace HtmlTools
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [Guid(PackageGuidString)]
    public sealed class HtmlToolsPackage : Package
    {
        public const string PackageGuidString = "d87af1f6-eee2-4a46-885a-372d215b98a3";

        protected override void Initialize()
        {
            base.Initialize();
        }
    }
}
