using System.Web;
using System.Web.Optimization;

namespace FGA
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {           
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/theme/dist/css/AdminLTE.min.css",
                "~/Content/theme/dist/css/skins/_all-skins.min.css",
                "~/Content/notifire/noti.css",
                "~/Content/Site.css",
                "~/Content/highchart.css",
                "~/Content/theme/bower_components/bootstrap/dist/css/bootstrap.min.css",
                "~/Content/theme/bower_components/font-awesome/css/font-awesome.min.css",
                "~/Content/theme/bower_components/Ionicons/css/ionicons.min.css",
                "~/Content/theme/bower_components/select2/dist/css/select2.min.css",
                "~/Content/jquery.datetimepicker.css",
                "~/Content/theme/plugins/iCheck/flat/blue.css",
                "~/Content/theme/plugins/bootstrap-wysihtml5/bootstrap3-wysihtml5.min.css",
                "~/Content/tabs/JqueryUi.css",
                "~/Content/DataTable/new/dataTables.bootstrap.css",
                "~/Content/DataTable/new/jquery.dataTables.min.css",
                "~/Content/DataTable/new/buttons.dataTables.min.css",
                "~/Content/jquery-steps/demo/css/jquery.steps.css",
                "~/Content/jquery-ui.css",
                "~/Content/dashboard_modern.css"));

            bundles.Add(new ScriptBundle("~/bundles/javascript").Include(
                    "~/Content/theme/plugins/jQuery/jQuery-2.1.3.min.js",
                    "~/Content/theme/plugins/jQueryUI/jquery-ui.min.js",
                    "~/Scripts/respond.js",
                    "~/Scripts/jquery.maskedinput.js",                    
                    "~/Content/select2/dist/js/select2.full.min.js",
                    "~/Content/notifire/notij.js",
                    "~/Scripts/jquery.datetimepicker.js",
                    "~/Content/DataTable/new/jquery.dataTables.min.js",
                    "~/Content/DataTable/new/dataTables.buttons.min.js",
                    "~/Content/DataTable/new/jszip.min.js",
                    "~/Content/DataTable/new/buttons.html5.min.js",
                    "~/Content/DataTable/new/buttons.print.min.js",
                    "~/Content/DataTable/new/buttons.colVis.min.js",
                    "~/Content/theme/bower_components/bootstrap/dist/js/bootstrap.min.js",
                    "~/Content/theme/bower_components/jquery-slimscroll/jquery.slimscroll.min.js",
                    "~/Content/theme/bower_components/fastclick/lib/fastclick.js",
                    "~/Scripts/Site.js",
                    "~/Content/theme/dist/js/adminlte.min.js",
                    "~/Content/theme/dist/js/demo.js",
                    "~/Content/theme/dist/js/app.min.js",
                    "~/Scripts/modernizr-2.8.3.js",
                    "~/Content/ModelPopup/jquery.unobtrusive-ajax.js",
                    "~/Content/ModelPopup/modaldialog.js",
                    "~/Content/theme/plugins/iCheck/icheck.min.js",
                    "~/Scripts/jquery.form.js",                   
                    "~/Scripts/autocomplete.js"
            ));
        }
    }
}   

