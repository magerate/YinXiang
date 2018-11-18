using System.Web;
using System.Web.Optimization;

namespace YinXiang
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery/jquery.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery_easing").Include(
                        "~/Scripts/jquery_easing/jquery.easing.js"));
            //bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
            //            "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap-datepicker").Include(
                     "~/Scripts/bootstrap-datepicker/bootstrap-datepicker.min.js"));

            //bundles.Add(new ScriptBundle("~/bundles/bootstrap-datepicker-local").Include(
            //     );

            bundles.Add(new ScriptBundle("~/bundles/sb_admin").Include(
                     "~/Scripts/sb-admin.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap/bootstrap.min.css",
                      "~/Content/bootstrap-datepicker/bootstrap-datepicker3.min.css",
                       "~/Content/fontawesome/all.min.css",
                       "~/Content/datatables/dataTables.bootstrap4.css",
                      "~/Content/sb-admin.css"));
        }
    }
}
