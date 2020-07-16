namespace Nop.Plugin.Api
{
    using Nop.Core;
    using Nop.Services.Configuration;
    using Nop.Services.Localization;
    using Nop.Services.Plugins;

    public class ApiPlugin : BasePlugin
    {
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;

        public ApiPlugin(
            ISettingService settingService, 
            IWorkContext workContext,
            ILocalizationService localizationService, 
            IWebHelper webHelper)
        {
            _settingService = settingService;
            _workContext = workContext;
            _localizationService = localizationService;
            _webHelper = webHelper;
        }

        public override void Install()
        {
            base.Install();
        }

        public override void Uninstall()
        {
            base.Uninstall();
        }
    }
}
