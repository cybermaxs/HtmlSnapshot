using Microsoft.Web.Infrastructure.DynamicModuleHelper;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(HtmlSnapshot.HtmlSnapshotPackage), "PreStart")]

namespace HtmlSnapshot
{
    public static class HtmlSnapshotPackage
    {
        public static void PreStart()
        {
            //Make sure the HtmlSnapshotModule handles BeginRequest
            DynamicModuleUtility.RegisterModule(typeof(HtmlSnapshot.HtmlSnapshotModule));
        }

    }
}