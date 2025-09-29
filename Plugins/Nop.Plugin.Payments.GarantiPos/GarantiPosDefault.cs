namespace Nop.Plugin.Payments.GarantiPos
{
    public class GarantiPosDefault
    {
        public static string SystemName => "Payments.GarantiPos";

        public static string OnePageCheckoutRouteName => "CheckoutOnePage";

        public static string CheckoutRouteName => "CheckoutPaymentInfo";

        public static string ConfigurationRouteName => "Plugin.Payments.GarantiPos.Configure";

        public static string VuePath => "~/Plugins/Payments.GarantiPos/Scripts/vue.min.js";

        public static string VueMaskPath => "~/Plugins/Payments.GarantiPos/Scripts/vue-the-mask.js";

        public static string AxiosPath => "~/Plugins/Payments.GarantiPos/Scripts/axios.min.js";

        public static string JqueryScriptPath => "//cdn.jsdelivr.net/npm/jquery@3.6.1/dist/jquery.min.js";

        public static string InstallmentTableStylePath => "~/Plugins/Payments.GarantiPos/Content/taksit.css";

        public static string CardCardStylePath => "~/Plugins/Payments.GarantiPos/Content/credicart.css";

        public static string UserAgent => "nopCommerce-4.70";

        public static string SweetAlertScriptPath => "//cdn.jsdelivr.net/npm/sweetalert2@11";

        public static string CreateRouteName => "CreateBin";

        public static string EditRouteName => "EditBin";

        public static string DeleteRouteName => "DeleteBin";

        public static string InstallmentCreate => "InstallmentCreate";

        public static string InstallmentEdit => "InstallmentEdit";

        public static string InstallmentDelete => "InstallmentDelete";

        public static string InstallmentList => "InstallmentList";

        public static string ListRouteName => "ListRouteName";

        public static string CategoryInstallmentCreate => "CategoryInstallmentCreate";

        public static string CategoryInstallmentEdit => "CategoryInstallmentEdit";

        public static string CategoryInstallmentDelete => "CategoryInstallmentDelete";

        public static string CategoryInstallmentList => "CategoryInstallmentList";

        public static string SynchronizationTask => "Nop.Plugin.Payments.GarantiPos.Services.CheckDeliveryConfirmationTask";

        public static int DefaultSynchronizationPeriod => 24;

        public static string SynchronizationTaskName => "Synchronization (GarantiPos)";
    }

}
