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
        public const string PackageGuidString = "3e13f8c1-7dbc-4422-a281-055d45e2909e";

        protected override void Initialize()
        {
            base.Initialize();
        }
    }
}
