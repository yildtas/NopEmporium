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
                return; // Eklenti bulunamadıysa sessizce çık.

            var systemName = plugin.PluginDescriptor.SystemName;
            var isActive = _paymentPluginManager.IsPluginActive(plugin);

            if (garantiPosSettings.Enable && !isActive)
            {
                if (!_paymentSettings.ActivePaymentMethodSystemNames.Contains(systemName))
                    _paymentSettings.ActivePaymentMethodSystemNames.Add(systemName);

                await _settingService.SaveSettingAsync(_paymentSettings, storeScope);
            }
            else if (!garantiPosSettings.Enable && isActive)
            {
                if (_paymentSettings.ActivePaymentMethodSystemNames.Contains(systemName))
                    _paymentSettings.ActivePaymentMethodSystemNames.Remove(systemName);

                await _settingService.SaveSettingAsync(_paymentSettings, storeScope);
            }
        }
    }
}
