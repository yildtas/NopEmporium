using System;
using Nop.Core.Domain.Customers;
using Nop.Services.Events;
using Nop.Services.Payments;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.UI;

namespace Nop.Plugin.Payments.GarantiPos.Services;

/// <summary>
/// Sayfa oluþturma olaylarýný dinleyerek ödeme sayfalarýna Garanti Pos kaynaklarýný enjekte eder.
/// </summary>
public class EventConsumer : IConsumer<PageRenderingEvent>
{
    private readonly IPaymentPluginManager _paymentPluginManager;

    public EventConsumer(IPaymentPluginManager paymentPluginManager)
    {
        _paymentPluginManager = paymentPluginManager;
    }

    public async Task HandleEventAsync(PageRenderingEvent eventMessage)
    {
        // Koruma kontrolleri
        if (eventMessage == null)
            return;

        if (!await _paymentPluginManager.IsPluginActiveAsync(GarantiPosDefault.SystemName, (Customer)null, 0))
            return;

        var routeName = eventMessage.GetRouteName(false) ?? string.Empty;
        // Checkout ile ilgili tüm sayfalarda kaynaklarý enjekte et (route adý farklýlýk riskine karþý)
        if (string.IsNullOrWhiteSpace(routeName) || !routeName.Contains("Checkout", StringComparison.OrdinalIgnoreCase))
            return;

        // Yardýmcý bazý durumlarda null olabilir
        var helper = eventMessage.Helper;
        if (helper == null)
            return;

        // Stil dosyalarý
        helper.AddCssFileParts(GarantiPosDefault.CardCardStylePath);
        helper.AddCssFileParts(GarantiPosDefault.InstallmentTableStylePath);

        // Çekirdek framework baðýmlýlýklarý (HEAD): Vue -> v-mask -> axios
        helper.AddScriptParts(ResourceLocation.Head, GarantiPosDefault.VuePath);
        helper.AddScriptParts(ResourceLocation.Head, GarantiPosDefault.VueMaskPath);
        helper.AddScriptParts(ResourceLocation.Head, GarantiPosDefault.AxiosPath);

        // Opsiyonel bildirim kütüphanesi (Footer'da kalabilir)
        helper.AddScriptParts(ResourceLocation.Footer, GarantiPosDefault.SweetAlertScriptPath);
    }
}
