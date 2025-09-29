using Nop.Core;
using Nop.Core.Domain.Payments;
using Nop.Services.Configuration;
using Nop.Services.Payments;

namespace Nop.Plugin.Payments.GarantiPos.Services
{
    public class ControlManager
    {
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly PaymentSettings _paymentSettings;

        public ControlManager(
            IStoreContext storeContext,
            ISettingService settingService,
            IPaymentPluginManager paymentPluginManager,
            PaymentSettings paymentSettings)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _paymentPluginManager = paymentPluginManager;
            _paymentSettings = paymentSettings;
        }

        /// <summary>
        /// Ayara göre GarantiPos ödeme eklentisini aktif/pasif eder.
        /// </summary>
        public async Task CheckDeliveryPaymentAsync()
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var garantiPosSettings = await _settingService.LoadSettingAsync<GarantiPosSettings>(storeScope);

            var plugin = await _paymentPluginManager.LoadPluginBySystemNameAsync(GarantiPosDefault.SystemName);
            if (plugin is null)
                return;

            var systemName = plugin.PluginDescriptor.SystemName;
            var activeList = _paymentSettings.ActivePaymentMethodSystemNames;
            var isActive = activeList.Contains(systemName);

            if (garantiPosSettings.Enable && !isActive)
            {
                activeList.Add(systemName);
                await _settingService.SaveSettingAsync(_paymentSettings, storeScope);
            }
            else if (!garantiPosSettings.Enable && isActive)
            {
                activeList.Remove(systemName);
                await _settingService.SaveSettingAsync(_paymentSettings, storeScope);
            }
        }
    }
}
