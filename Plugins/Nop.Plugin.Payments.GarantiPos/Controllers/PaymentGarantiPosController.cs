using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.GarantiPos;
using Nop.Plugin.Payments.GarantiPos.Domains;
using Nop.Plugin.Payments.GarantiPos.Helpers;
using Nop.Plugin.Payments.GarantiPos.Models.BankBin;
using Nop.Plugin.Payments.GarantiPos.Models.Category;
using Nop.Plugin.Payments.GarantiPos.Models.Installment;
using Nop.Plugin.Payments.GarantiPos.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using System.Globalization;
using System.Net;
using System.Text;
using System.Xml;
using SessionExt = Nop.Core.Http.Extensions.SessionExtensions;

/// <summary>
/// Garanti POS ödeme süreci controller'ı. Ana sorumluluklar:
/// - Yönetim (Ayarlar, Taksit, Kategori Taksit, BIN CRUD)
/// - 3D Secure dönüşleri (Success / Cancel)
/// - Dinamik taksit hesaplama (BIN + kategori kuralı)
/// </summary>
public class PaymentGarantiPosController : BasePaymentController
{
    #region Fields
    private readonly IWorkContext _workContext;
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;
    private readonly IStoreContext _storeContext;
    private readonly IOrderService _orderService;
    private readonly ILogger _logger;
    private readonly IPermissionService _permissionService;
    private readonly INotificationService _notificationService;
    private readonly ICustomerService _customerService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly OrderSettings _orderSettings;
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly IWebHelper _webHelper;
    private readonly IOrderTotalCalculationService _orderTotalCalculationService;
    private readonly ICurrencyService _currencyService;
    private readonly IPaymentPosService _paymentPosService;
    private readonly IBankBinService _bankBinService;
    private readonly ICategoryService _categoryService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly GarantiPosSettings _garantiPosSettings;
    private readonly IProductService _productService;
    private readonly CurrencySettings _currencySettings;
    #endregion

    public PaymentGarantiPosController(
        IWorkContext workContext,
        ISettingService settingService,
        ILocalizationService localizationService,
        IStoreContext storeContext,
        IOrderService orderService,
        ILogger logger,
        IPermissionService permissionService,
        INotificationService notificationService,
        ICustomerService customerService,
        IShoppingCartService shoppingCartService,
        OrderSettings orderSettings,
        IOrderProcessingService orderProcessingService,
        IPaymentPluginManager paymentPluginManager,
        IWebHelper webHelper,
        IOrderTotalCalculationService orderTotalCalculationService,
        ICurrencyService currencyService,
        IPaymentPosService paymentPosService,
        IBankBinService bankBinService,
        ICategoryService categoryService,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        GarantiPosSettings garantiPosSettings,
        IProductService productService,
        CurrencySettings currencySettings)
    {
        _workContext = workContext;
        _settingService = settingService;
        _localizationService = localizationService;
        _storeContext = storeContext;
        _orderService = orderService;
        _logger = logger;
        _permissionService = permissionService;
        _notificationService = notificationService;
        _customerService = customerService;
        _shoppingCartService = shoppingCartService;
        _orderSettings = orderSettings;
        _orderProcessingService = orderProcessingService;
        _paymentPluginManager = paymentPluginManager;
        _webHelper = webHelper;
        _orderTotalCalculationService = orderTotalCalculationService;
        _currencyService = currencyService;
        _paymentPosService = paymentPosService;
        _bankBinService = bankBinService;
        _categoryService = categoryService;
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _garantiPosSettings = garantiPosSettings;
        _productService = productService;
        _currencySettings = currencySettings;
    }

    #region Private helpers
    /// <summary>
    /// 3D başarısız veya kullanıcı iptallerinde siparişin iptal edilmesi ve sepetin eski haline döndürülmesi.
    /// </summary>
    private async Task CancelOrderAsync(Order order, string message = "")
    {
        await _orderProcessingService.ReOrderAsync(order); // sepet geri yükleme

        await _orderProcessingService.CancelOrderAsync(order, true);

        order.PaymentStatus = (PaymentStatus)10; // custom pending benzeri

        _notificationService.ErrorNotification(message, true);

        await _logger.InsertLogAsync((LogLevel)20, "Ödeme Alınamadı.", string.Empty, (Customer)null);
    }

    /// <summary>
    /// Tek bir taksit satırı hesaplayıp modele ekler.
    /// </summary>
    private static Task AddInstallmentAsync(InstallmentViewModel model, decimal rate, int installment, string text)
    {
        var total = model.TotalAmount;
        var totalWithRate = rate > 0m
            ? Math.Round(total + total * rate / 100m, 2, MidpointRounding.AwayFromZero)
            : total;
        var perInstallment = Math.Round(totalWithRate / installment, 2, MidpointRounding.AwayFromZero);

        model.InstallmentItems.Add(new InstallmentViewModel.InstallmentItem
        {
            Text = $"{installment} {text}",
            Installment = installment,
            Rate = rate,
            Amount = perInstallment.ToString("N2"),
            AmountValue = perInstallment,
            TotalAmountValue = totalWithRate,
            TotalAmount = totalWithRate.ToString("N2")
        });
        return Task.CompletedTask;
    }

    private async Task GetInstallmentAsync(InstallmentViewModel model, IList<PaymentGarantiInstallment> installments)
    {
        foreach (var inst in installments)
            await AddInstallmentAsync(model, inst.Rate, inst.Installment, await _localizationService.GetResourceAsync("Plugins.Payments.GarantiPos.installmentdisplay"));
    }

    private async Task GetInstallmentAsync(InstallmentViewModel model, IList<PaymentGarantiCategoryInstallment> installments)
    {
        foreach (var inst in installments)
            await AddInstallmentAsync(model, inst.Rate, inst.Installment, await _localizationService.GetResourceAsync("Plugins.Payments.GarantiPos.installmentdisplay"));
    }

    /// <summary>
    /// Aktif ödeme eklentisini yükler; devre dışı / kurulu değilse hata fırlatır.
    /// </summary>
    private async Task<GarantiPosProcessor> LoadActiveProcessorOrThrowAsync()
    {
        var processor = await _paymentPluginManager.LoadPluginBySystemNameAsync(GarantiPosDefault.SystemName) as GarantiPosProcessor;
        if (processor == null || !_paymentPluginManager.IsPluginActive(processor) || !processor.PluginDescriptor.Installed)
            throw new NopException("GarantiPos module cannot be loaded");
        return processor;
    }

    /// <summary>
    /// Sepetteki ürünlerin benzersiz kategori listesini getirir.
    /// </summary>
    private async Task<IList<Category>> GetDistinctCartCategoriesAsync(IList<ShoppingCartItem> cartItems)
    {
        var result = new List<Category>();
        var seen = new HashSet<int>();

        foreach (var item in cartItems)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, false);

            foreach (var pc in productCategories)
            {
                if (pc == null)
                    continue;

                if (seen.Add(pc.CategoryId))
                {
                    var category = await _categoryService.GetCategoryByIdAsync(pc.CategoryId);
                    if (category != null)
                        result.Add(category);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Verilen kategori listesi için kategori bazlı taksit kuralını uygular. Uygulanırsa true döner.
    /// </summary>
    private async Task<bool> TryApplyCategoryInstallmentsAsync(InstallmentViewModel model,
        IEnumerable<Category> categories,
        IList<PaymentGarantiCategoryInstallment> allCategoryInstallments)
    {
        var matched = allCategoryInstallments
            .Where(ci => categories.Any(c => c.Id == ci.CategoryId))
            .ToList();

        if (matched.Count == 0)
            return false;

        await GetInstallmentAsync(model, matched);
        await SessionExt.SetAsync(_httpContextAccessor.HttpContext.Session, "InstallmentViewModel", model);
        return true;
    }
    #endregion

    #region Configure
    [AuthorizeAdmin(false)]
    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> ConfigureAsync()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
            return AccessDeniedView();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<GarantiPosSettings>(storeScope);

        var model = new ConfigurationModel
        {
            CompanyName = settings.CompanyName,
            Installment = settings.Installment,
            Email = settings.Email,
            MerchantId = settings.MerchantId,
            Password = settings.Password,
            SecurityLevel = settings.SecurityLevel,
            StoreKey = settings.StoreKey,
            TerminalId = settings.TerminalId,
            TerminalProvUserId = settings.TerminalProvUserId,
            TerminalUserId = settings.TerminalUserId,
            TestMode = settings.TestMode,
            Version = settings.Version,
            AdditionalFee = settings.AdditionalFee,
            AdditionalFeePercentage = settings.AdditionalFeePercentage,
            Bank3DUrl = settings.Bank3DUrl,
            BankNone3DUrl = settings.BankNone3DUrl,
            NopCommerceNumber = settings.NopCommerceNumber,
            Enable = settings.Enable
        };
        return View("~/Plugins/Payments.GarantiPos/Views/Configure.cshtml", model);
    }

    [AuthorizeAdmin(false)]
    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [HttpPost]
    public async Task<IActionResult> ConfigureAsync(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
            return AccessDeniedView();

        if (!ModelState.IsValid)
            return await ConfigureAsync();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<GarantiPosSettings>(storeScope);

        settings.CompanyName = model.CompanyName;
        settings.Installment = model.Installment;
        settings.MerchantId = model.MerchantId;
        settings.TerminalProvUserId = model.TerminalProvUserId;
        settings.TerminalUserId = model.TerminalUserId;
        settings.Password = model.Password;
        settings.StoreKey = model.StoreKey;
        settings.TerminalId = model.TerminalId;
        settings.TestMode = model.TestMode;
        settings.Version = model.Version;
        settings.Email = model.Email;
        settings.SecurityLevel = model.SecurityLevel;
        settings.AdditionalFee = model.AdditionalFee;
        settings.AdditionalFeePercentage = model.AdditionalFeePercentage;
        settings.Bank3DUrl = model.Bank3DUrl;
        settings.BankNone3DUrl = model.BankNone3DUrl;
        settings.NopCommerceNumber = model.NopCommerceNumber;
        settings.Enable = model.Enable;

        await _settingService.SaveSettingAsync(settings, 0);
        await _settingService.ClearCacheAsync();
        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"), true);

        return await ConfigureAsync();
    }
    #endregion

    #region 3D Secure callbacks
    /// <summary>
    /// Bankadan başarılı 3D dönüşü. Hash doğrulanır, mdStatus ve işlem kodu kontrol edilir.
    /// Order toplamı taksitli ise PaidPrice üzerinden güncellenir.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SuccessAsync()
    {
        await LoadActiveProcessorOrThrowAsync();

        var garantiPaySettings = await _settingService.LoadSettingAsync<GarantiPosSettings>(0);
        var form = Request.Form;
        string amountStr = form["txnamount"]; // banka *100 formatında göndermiş olabilir
        string currencyCode = form["txncurrencycode"];
        string orderGuidStr = form["oid"];
        string terminalId = form["clientid"];
        string provisionPassword = garantiPaySettings.Password;
        string procReturnCode = form["procreturncode"]; // 00 başarılı
        string mdStatus = form["mdstatus"]; // 1,2,3,4 -> başarılı kabul edilen durumlar
        string mdStatusText = form["mderrormessage"]; // bilgilendirme
        string securityData = HelperOptions.Sha1Upper(provisionPassword + "0" + terminalId);

        string hashDataFromBank = form["secure3dhash"];
        string hashData = HelperOptions.Sha512(orderGuidStr + terminalId + amountStr + currencyCode + securityData);

        var paymentOrder = await _paymentPosService.GetPosOrderGuid(new Guid(orderGuidStr));
        var order = await _orderService.GetOrderByGuidAsync(paymentOrder.OrderNumber);

        var targetCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
        var sourceCurrency = await _currencyService.GetCurrencyByCodeAsync(paymentOrder.BasketId);

        // mdStatus 1..4 arası ve işlem sonucu 00 ise başarılı ödeme
        if (mdStatus == "1" || mdStatus == "2" || mdStatus == "3" || mdStatus == "4")
        {
            string responseHashparams = form["hashparams"];
            string responseHash = form["hash"];
            string digestData = string.Empty;
            char[] separator = new char[] { ':' };
            string[] paramList = responseHashparams.Split(separator);

            foreach (string param in paramList)
            {
                if (form.ContainsKey(param))
                {
                    var value = form[param].ToString();
                    digestData += string.IsNullOrEmpty(value) ? string.Empty : value;
                }
            }

            digestData += garantiPaySettings.StoreKey;
            string hashCalculated = HelperOptions.Sha512(digestData);

            if (!responseHash.Equals(hashCalculated))
            {
                _notificationService.ErrorNotification("3D işlem onayı alınamadı. Lütfen bilgilerinizi kontrol ettikten sonra tekrar deneyiniz.", true);

                // Başarısız veya hash uyuşmadı – ödeme adımına geri dön.
                if (_orderSettings.OnePageCheckoutEnabled)
                {
                    return RedirectToAction("OpcSavePaymentInfo", "Checkout");
                }

                return RedirectToAction("PaymentMethod", "Checkout");
            }

            string mode = form["mode"];
            string apiVersion = form["apiversion"];
            string terminalProvUserId = form["terminalprovuserid"];
            string terminalUserId = form["terminaluserid"];
            string clientId = form["clientid"];
            string terminalMerchantid = form["terminalmerchantid"];
            string customerIpaddress = form["customeripaddress"];
            string txntype = form["txntype"];
            string txnInstallmentCount = form["txninstallmentcount"];
            string txnAmount = form["txnamount"];
            string cavv = form["cavv"];
            string eci = form["eci"];
            string oId = form["oid"];
            string xid = form["xid"];
            string md = form["md"];

            #region GVPS XML
            StringBuilder gvpsXml = new StringBuilder();
            gvpsXml.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            gvpsXml.Append("<GVPSRequest>");
            gvpsXml.AppendFormat("<Mode>{0}</Mode>", mode);
            gvpsXml.AppendFormat("<Version>{0}</Version>", apiVersion);
            gvpsXml.Append("<Terminal>");
            gvpsXml.AppendFormat("<ProvUserID>{0}</ProvUserID>", terminalProvUserId);
            gvpsXml.AppendFormat("<HashData>{0}</HashData>", hashData);
            gvpsXml.AppendFormat("<UserID>{0}</UserID>", terminalUserId);
            gvpsXml.AppendFormat("<ID>{0}</ID>", clientId);
            gvpsXml.AppendFormat("<MerchantID>{0}</MerchantID>", terminalMerchantid);
            gvpsXml.Append("</Terminal>");
            gvpsXml.Append("<Customer>");
            gvpsXml.AppendFormat("<IPAddress>{0}</IPAddress>", customerIpaddress);
            gvpsXml.AppendFormat("<EmailAddress>{0}</EmailAddress>", "");
            gvpsXml.Append("</Customer>");
            gvpsXml.Append("<Order>");
            gvpsXml.AppendFormat("<OrderID>{0}</OrderID>", oId);
            gvpsXml.Append("<GroupID/>");
            gvpsXml.Append("</Order>");
            gvpsXml.Append("<Transaction>");
            gvpsXml.AppendFormat("<Type>{0}</Type>", txntype);
            gvpsXml.AppendFormat("<InstallmentCnt>{0}</InstallmentCnt>", txnInstallmentCount);
            gvpsXml.AppendFormat("<Amount>{0}</Amount>", txnAmount);
            gvpsXml.AppendFormat("<CurrencyCode>{0}</CurrencyCode>", currencyCode);
            gvpsXml.AppendFormat("<CardholderPresentCode>{0}</CardholderPresentCode>", 13);
            gvpsXml.AppendFormat("<MotoInd>{0}</MotoInd>", "N");
            gvpsXml.Append("<Secure3D>");
            gvpsXml.AppendFormat("<AuthenticationCode>{0}</AuthenticationCode>", cavv);
            gvpsXml.AppendFormat("<SecurityLevel>{0}</SecurityLevel>", eci);
            gvpsXml.AppendFormat("<TxnID>{0}</TxnID>", xid);
            gvpsXml.AppendFormat("<Md>{0}</Md>", md);
            gvpsXml.Append("</Secure3D>");
            gvpsXml.Append("</Transaction>");
            gvpsXml.Append("</GVPSRequest>");
            #endregion

            string gelenXml = "";

            string data = "data=" + gvpsXml.ToString();

            WebRequest webRequest = WebRequest.Create(_garantiPosSettings.BankNone3DUrl);
            webRequest.Method = "POST";

            byte[] byteArray = Encoding.UTF8.GetBytes(data);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = byteArray.Length;

            Stream dataStream = webRequest.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse webResponse = webRequest.GetResponse();
            dataStream = webResponse.GetResponseStream();

            StreamReader reader = new StreamReader(dataStream);
            gelenXml = reader.ReadToEnd();

            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(gelenXml);

            #region Payment Process result
            XmlElement xEReasonCode = xDoc.SelectSingleNode("//GVPSResponse/Transaction/Response/ReasonCode") as XmlElement;
            XmlElement xEErrorMsg = xDoc.SelectSingleNode("//GVPSResponse/Transaction/Response/ErrorMsg") as XmlElement;
            XmlElement xESysErrMsg = xDoc.SelectSingleNode("//GVPSResponse/Transaction/Response/SysErrMsg") as XmlElement;
            XmlElement xERetrefNum = xDoc.SelectSingleNode("//GVPSResponse/Transaction/RetrefNum") as XmlElement;
            XmlElement xEAuthCode = xDoc.SelectSingleNode("//GVPSResponse/Transaction/AuthCode") as XmlElement;

            procReturnCode = xEReasonCode != null ? xEReasonCode.InnerText : "";
            string hostRefNum = xERetrefNum != null ? xERetrefNum.InnerText : "";
            string hostMessage = xESysErrMsg != null ? xESysErrMsg.InnerText : "";
            string errorMessage = xEErrorMsg != null ? xEErrorMsg.InnerText : "";
            string authCode = xEAuthCode != null ? xEAuthCode.InnerText : "";

            // Log with customerId and parsed bank response fields
            try
            {
                var customer = order != null ? await _customerService.GetCustomerByIdAsync(order.CustomerId) : null;
                var shortMessage = "GarantiPos 3D Payment Process Result";
                var fullMessage = $"CustomerId: {order?.CustomerId}, OrderId: {order?.Id}, ProcReturnCode: {procReturnCode}, HostRefNum: {hostRefNum}, HostMessage: {hostMessage}, ErrorMessage: {errorMessage}, AuthCode: {authCode}";
                await _logger.InsertLogAsync(LogLevel.Information, shortMessage, fullMessage, customer);
            }
            catch { /* ignore logging failures */ }

            #endregion

            if (gelenXml.Contains("<ReasonCode>00</ReasonCode>") && gelenXml.Contains("<Message>Approved</Message>"))
            {
                //3D işlem onayı alındı. İşlem Başarılı
                await _orderProcessingService.MarkOrderAsPaidAsync(order);

                if (paymentOrder.PaidPrice.HasValue)
                {
                    if (sourceCurrency.CurrencyCode != targetCurrency.CurrencyCode)
                        order.OrderTotal = await _currencyService.ConvertCurrencyAsync(paymentOrder.PaidPrice.Value, sourceCurrency, targetCurrency);
                    else
                        order.OrderTotal = paymentOrder.PaidPrice.Value;
                }

                order.AuthorizationTransactionId = form["authcode"];
                var note = new OrderNote
                {
                    Note = "Ödeme Garanti Pos ile Yapıldı" + mdStatusText,
                    OrderId = order.Id,
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                };
                await _orderService.InsertOrderNoteAsync(note);
                await _orderService.UpdateOrderAsync(order);

                paymentOrder.StatusId = 30; // Paid custom
                paymentOrder.MarkAsPaid(DateTime.Now);
                paymentOrder.BankRequest = string.Empty;
                paymentOrder.BankResponse = string.Empty;
                paymentOrder.PaymentInfo = string.Empty;
                await _paymentPosService.UpdatePosOrderAsync(paymentOrder);

                _notificationService.SuccessNotification("Ödemeniz Alınmıştır.", true);
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
            }
            else
            {
                //3D işlem onayı alınamadı
                errorMessage = GetGarantiErrorMessage(procReturnCode);

                _notificationService.ErrorNotification(errorMessage, true);

                // Başarısız veya hash uyuşmadı – ödeme adımına geri dön.
                if (_orderSettings.OnePageCheckoutEnabled)
                {
                    return RedirectToAction("OpcSavePaymentInfo", "Checkout");
                }

                return RedirectToAction("PaymentMethod", "Checkout");
            }
        }

        // mdStatus değerine göre başarısızlık nedeni
        await CancelOrderAsync(order, "3D işlem onayı alınamadı. 3D şifrenizi yanlış girmiş olabilirsiniz veya kartınız 3D'ye kayıtlı olmayabilir.");

        // Başarısız veya hash uyuşmadı – ödeme adımına geri dön.
        if (_orderSettings.OnePageCheckoutEnabled)
        {
            return RedirectToAction("OpcSavePaymentInfo", "Checkout");
        }

        return RedirectToAction("PaymentMethod", "Checkout");
    }

    private string GetGarantiErrorMessage(string procReturnCode)
    {
        switch (procReturnCode.TrimStart('0'))
        {
            case "1":
                return "Bankanızdan provizyon alınız.";
            case "2":
                return "Bankanızdan VISA kartınız için provizyon alınız.";
            case "4"://Karta El koyunuz!!!
            case "7"://KartaElKoyunuz.
            case "9"://KartYenilenmiş.Müşteridenisteyin
            case "18"://Kapalı kart
            case "34"://MuhtemelenÇalıntıKart!!!ElKoyunuz.
            case "36"://SınırlandırılmışKart!!ElKoyunuz.
            case "37"://LütfenBankaGüvenliğiniArayınız.
            case "38"://ŞifreGirişLimitiAşıldı!!ElKoyunuz.
            case "41"://KayıpKart!!!KartaElKoyunuz.
            case "43"://ÇalıntıKart!!!KartaElKoyunuz.
                return "İşleminiz gerçekleştiremiyoruz. Detaylı bilgi için lütfen bankanızla görüşün. ";
            case "5"://İşlem onaylanmadı.
                return "İşleminiz onaylanmadı. Kredi kartı bilgilerinizi kontrol ettikten sonra tekrar deneyiniz.";
            case "6"://İsteminiz Kabul Edilmedi.
                return "İsteminiz kabul edilmedi. Kredi kartı bilgilerinizi kontrol ettikten sonra tekrar deneyiniz.";
            case "33"://KartınSüresiDolmuş!KartaElKoyunuz.
                return "Kartınızın süresi dolmuş. Detaylı bilgi için lütfen bankanızla görüşün. ";
            case "11":
                return "İşleminiz gerçekleştirildi (VIP). Bankanızı arayarak teyit ediniz. ";
            case "13"://Geçersiz tutar.
                return "Gönderdiğiniz tutar geçerli formatta değil. Kredi kartı bilgilerinizi kontrol ettikten sonra tekrar deneyiniz.";
            case "14"://kart numarası hatalı
            case "15"://Bankası bulunamadı.
            case "55"://ŞifresiHatalı.
            case "56"://BuKartMevcutDeğil.
            case "3":
                return "Kredi kartı bilgileriniz hatalı. Kredi kartı bilgilerinizi kontrol ettikten sonra tekrar deneyiniz. ";
            case "16":
                return "Kredi kartınızın bakiyesi yetersiz. Başka bir kredi kartı ile tekrar deneyiniz.";
            case "19"://BirKereDahaProvizyonTalepEdiniz.
                return "İşleminizi gerçekleştiremiyoruz. Birkez daha provizyon talep ediniz.";
            case "17"://İşlemİptalEdildi.
            case "25"://BöyleBirBilgiBulunamadı.
            case "28"://Orijinalirededilmiş/Dosyaservisdışı.
            case "30"://MesajınFormatıHatalı.
            case "31"://Issuersign-onolmamış.
            case "77"://Orjinalişlemileuyumsuzbilgialındı.
            case "78"://AccountBalanceNotAvailable.
            case "81"://Şifreleme/YabancıNetworkhatası.
            case "83"://ŞifreDoğrulanamıyor./İletişimhatası.
            case "89"://Authenticationhatası.
                return "İşleminizi gerçekleştiremiyoruz. Kredi kartı bilgilerinizi kontrol ettikten sonra tekrar deneyiniz.";
            case "21":
            case "29"://İptalyapılamadı.(Orjinalibulunamadı)
                return "İşlem iptal edilemedi. Lütfen daha sonra tekrar deneyiniz.";
            case "32":
                return "İşleminiz kısmen gerçekleştirildi. Hata ile ilgili lütfen bizimle irtibata geçiniz.";
            case "39"://Kredihesabıtanımsız.
            case "51"://Hesapmüsaitdeğil.
            case "52"://ÇekHesabıTanımsız.
            case "53"://HesapTanımsız.
                return "Hesabınız tanımsız. Başka bir kredi kartı ile tekrar deneyiniz.";
            case "54":
                return "Kartınızın son kullanım tarihi hatalı. Başka bir kredi kartı ile tekrar deneyiniz.";
            case "57":
                return "İşleminizi gerçekleştiremiyoruz. Debit kart veya kart sahibine acık olmayan bir işlem deniyor olabilirsiniz. ";
            case "58":
                return "İşleminizi gerçekleştiremiyoruz. Mevcut Sanal POS yetkileri kısıtlanmış olabilir.";
            case "61":
                return "İşleminizi gerçekleştiremiyoruz. Para çekme limitiniz aşılıyor.";
            case "63":
                return "İşleminizi gerçekleştiremiyoruz. Bu işlemi yapmaya yetkili değilsiniz.";
            case "64":
                return "İşleminizi gerçekleştiremiyoruz. Kartınız takside uygun değil.";
            case "65":
                return "İşleminizi gerçekleştiremiyoruz. Günlük işlem adediniz dolmuş.";
            case "75":
            case "76":
                return "İşleminizi gerçekleştiremiyoruz. Şifre giriş limitiniz aşıldı.";
            case "80":
                return "İşleminizi gerçekleştiremiyoruz. Tarih bilginiz hatalı.";
            case "82":
                return "Kredi kartı güvenlik kodu hatalı. Kredi kartı bilgilerinizi kontrol ettikten sonra tekrar deneyiniz.";
            case "12":
                return "İşleminizi gerçekleştiremiyoruz. Kredi kartı bilgilerinizi kontrol ettikten sonra tekrar deneyiniz.";
            case "86":
            case "88":
                return "İşleminizi gerçekleştiremiyoruz. Şifreniz doğrulanamıyor.";
            case "90":
                return "İşleminizi gerçekleştiremiyoruz. Günsonu işlemleri yapılıyor.";
            case "95":
                return "İşleminizi gerçekleştiremiyoruz. Günlük toplamlar hatalı.";
            case "91":
            case "92":
            case "96":
                return "Bankanızdan cevap alınamıyor. Lütfen daha sonra tekrar deneyiniz.";
            case "93":
                return "Hukiki nedenlerden dolayı işleminiz reddedildi. Detaylı bilgi için lütfen bankanızla görüşün.";
            case "214":
                return "Iade tutari, satis tutarindan büyük olamaz";
            default:
                return "İşleminizi gerçekleştiremiyoruz. Kredi kartı bilgilerinizi kontrol ettikten sonra tekrar deneyiniz.";
        }
    }

    /// <summary>
    /// Bankadan olumsuz dönüş veya kullanıcı iptali. Hata mesajları işlenir ve sipariş iptal edilir.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CancelAsync()
    {
        await LoadActiveProcessorOrThrowAsync();

        var model = new CancelModel();
        try
        {
            var form = Request.Form;
            model.MdErrrorMessage = form["mderrormessage"];
            model.ErrorMessage = form["errmsg"];
            model.MdStatus = form["mdstatus"];
            model.Clientid = form["clientid"];
            var orderGuidStr = form["oid"];
            string procReturnCode = form["procreturncode"]; // 00 başarılı

            var paymentOrder = await _paymentPosService.GetPosOrderGuid(new Guid(orderGuidStr));
            var order = await _orderService.GetOrderByGuidAsync(paymentOrder.OrderNumber);

            // Eski kod && kullanıyordu (asla true olmuyor), burada OR mantığı daha anlamlı.
            //if (model.ErrorMessage == string.Empty || model.ErrorMessage == "Not authenticated detail=( ) vendorCode=")
            //    model.Message = "Kartınız İnternet Kapalı yada işlem Yapılamıyor Başka Bir Kartla Deneyiniz";

            //model.Message = $"MdErrorMessage{model.MdErrrorMessage}ErrorMessage{model.ErrorMessage} Ref No: {model.MdStatus}";

            //if (model.ErrorMessage.Contains("0809"))
            //    model.MdStatus += "Bayi Kodu ve Kredi Kartı numaranız sisteme kayıtlı olmadığı için işleminiz gerçekleşmemiştir. Lütfen müşteri temsilciniz ile iletişime geçiniz. ";

            model.Message = "3D işlem onayı alınamadı. " +
                "3D şifrenizi yanlış girmiş olabilirsiniz veya kartınız 3D'ye kayıtlı olmayabilir.";

            await CancelOrderAsync(order, model.Message);

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToAction("OnePageCheckout", "Checkout");
            return RedirectToAction("PaymentMethod", "Checkout");
        }
        catch (Exception ex)
        {
            model.ErrorMessage += ex.Message;
            await _notificationService.ErrorNotificationAsync(ex, true);
            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToAction("OnePageCheckout", "Checkout");
            return RedirectToAction("PaymentMethod", "Checkout");
        }
    }
    #endregion

    /// <summary>
    /// 3D form HTML içeriğini session'dan okuyup ekrana yansıtır. Otomatik post içerir.
    /// İçerik yoksa siparişi iptal eder.
    /// </summary>
    public async Task<IActionResult> TreeDContentAsync()
    {
        var session = _httpContextAccessor.HttpContext.Session;
        if (session.Keys.Contains("htmlContent"))
        {
            var response = session.GetString("htmlContent");
            if (response != null)
                return Content(response, "text/html");
        }
        var paymentOrder = await SessionExt.GetAsync<PaymentGarantiOrder>(session, "PaymentOrder");
        await CancelOrderAsync(await _orderService.GetOrderByGuidAsync(paymentOrder.OrderNumber), "Ödemeniz alınamadı Lütfen Tekrar Deneyiniz.");
        return RedirectToAction("Cart", "ShoppingCart");
    }

    /// <summary>
    /// BIN numarasına göre taksit bilgilerini getirir.
    /// İşleyiş: Local veritabanında yoksa bankaya BIN inquiry -> kayıt -> kategori oranı veya genel oran uygulama.
    /// </summary>
    [IgnoreAntiforgeryToken]
    public async Task<ActionResult> GetInstallmentAsync(string binNumber)
    {
        // Para birimi dönüşüm gerekli mi kontrolü (şimdilik bayrak kullanılmıyor)
        var currencies = (await _currencyService.GetAllCurrenciesAsync(false, 0)).Where(c => c.Published).ToList();
        var customerCurrency = await _workContext.GetWorkingCurrencyAsync();
        bool convertCurrency = false; // ileride kullanılabilir
        foreach (var currency in currencies)
        {
            if (currency.Rate == 1m)
            {
                var primaryCurrency = await _currencyService.GetCurrencyByIdAsync(currency.Id);
                if (customerCurrency.Id != primaryCurrency.Id)
                    convertCurrency = true;
            }
        }

        var installmentInfo = new InstallmentViewModel
        {
            TotalAmount = Math.Round(await HelperOptions.GetOrderTotalAsync(_shoppingCartService, _workContext, _storeContext, _orderTotalCalculationService, _currencyService, withCommission: false, convertCurrency), 2)
        };
        installmentInfo.AddCashRate(installmentInfo.TotalAmount, await _localizationService.GetResourceAsync("Plugins.Payments.GarantiPos.InstallmentEmpty"));

        var customer = await _workContext.GetCurrentCustomerAsync();
        var cartItems = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, 0);
        var settings = await _settingService.LoadSettingAsync<GarantiPosSettings>(0);

        if (!_garantiPosSettings.Installment || string.IsNullOrEmpty(binNumber))
        {
            await SessionExt.SetAsync(_httpContextAccessor.HttpContext.Session, "InstallmentViewModel", installmentInfo);
            return Json(installmentInfo);
        }

        installmentInfo.BinNumber = binNumber;
        var binCode = await _bankBinService.GetBankBin(binNumber);

        if (binCode == null)
        {
            // Bankaya BIN sorgusu
            int amountInt = (int)Math.Round(installmentInfo.TotalAmount * 100m); // kuruş cinsinden
            string amount = amountInt.ToString(CultureInfo.InvariantCulture);

            string mode = settings.TestMode ? "TEST" : "PROD";

            // Email fallback
            string customerEmail;
            if (customer == null || string.IsNullOrEmpty(customer.Email))
            {
                var billing = await _customerService.GetCustomerBillingAddressAsync(customer);
                customerEmail = billing?.Email ?? "test@example.com";
            }
            else
            {
                customerEmail = customer.Email;
            }

            // OrderId: hem hash’te hem XML’de aynı kullanılmalı
            string orderId = Guid.NewGuid().ToString("N");

            // Banka formülü: "0" + terminalId ve binNumber yok
            string hashedPassword = HelperOptions.Sha1Upper(settings.Password + "0" + settings.TerminalId);

            string securityData = string.Concat(orderId, settings.TerminalId, amount, hashedPassword);
            string hashData = HelperOptions.Sha1Upper(securityData); // zaten Upper HEX dönüyor

            // Build XML with AppendFormat for readability
            StringBuilder sb = new StringBuilder(512);
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<GVPSRequest>");
            sb.AppendFormat("<Mode>{0}</Mode>", mode);
            sb.AppendFormat("<Version>{0}</Version>", settings.Version);
            sb.AppendLine("<Terminal>");
            sb.AppendFormat("<ProvUserID>{0}</ProvUserID>", settings.TerminalProvUserId);
            sb.AppendFormat("<HashData>{0}</HashData>", hashData);
            sb.AppendFormat("<UserID>{0}</UserID>", settings.TerminalUserId);
            sb.AppendFormat("<ID>{0}</ID>", settings.TerminalId);
            sb.AppendFormat("<MerchantID>{0}</MerchantID>", settings.MerchantId);
            sb.AppendLine("</Terminal>");
            sb.AppendLine("<Customer>");
            sb.AppendFormat("<IPAddress>{0}</IPAddress>", _webHelper.GetCurrentIpAddress());
            sb.AppendFormat("<EmailAddress>{0}</EmailAddress>", customerEmail);
            sb.AppendLine("</Customer>");
            sb.AppendLine("<Order>");
            sb.AppendFormat("<OrderID>{0}</OrderID>", orderId);
            sb.AppendLine("<GroupID></GroupID>");
            sb.AppendLine("<Description></Description>");
            sb.AppendLine("</Order>");
            sb.AppendLine("<Transaction>");
            sb.AppendLine("<Type>bininq</Type>");
            sb.AppendFormat("<Amount>{0}</Amount>", amount);
            sb.AppendLine("<BINInq>");
            sb.AppendLine("<Group>A</Group>");
            sb.AppendLine("<CardType>A</CardType>");
            sb.AppendFormat("<BinNumber>{0}</BinNumber>", binNumber);
            sb.AppendLine("</BINInq>");
            sb.AppendLine("</Transaction>");
            sb.AppendLine("</GVPSRequest>");
            string requestXml = sb.ToString();

            var data = "data=" + requestXml.Trim();
            var client = _httpClientFactory.CreateClient();

            // Use configured non-3D URL
            var paymentUrl = settings.BankNone3DUrl;

            client.BaseAddress = new Uri(paymentUrl);
            var content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");
            var responseString = await (await client.PostAsync(paymentUrl, content)).Content.ReadAsStringAsync();
            var xDoc = new XmlDocument();
            xDoc.LoadXml(responseString);
            string message = xDoc.SelectSingleNode("GVPSResponse/Transaction/Response/Message")?.InnerText;
            string errorMessage = xDoc.SelectSingleNode("GVPSResponse/Transaction/Response/ErrorMsg")?.InnerText;
            string sysErrorMessage = xDoc.SelectSingleNode("GVPSResponse/Transaction/Response/SysErrMsg")?.InnerText;
            installmentInfo.ErrorMessage = $"{message}-{errorMessage}-{sysErrorMessage}";

            if (string.IsNullOrEmpty(errorMessage))
            {
                // Yeni BIN kayıtlarını DB'ye ekle
                var binNodes = xDoc.SelectNodes("/GVPSResponse/Order/BINInqResult/BINList/BIN");
                if (binNodes != null)
                {
                    foreach (XmlNode binNode in binNodes)
                    {
                        var newBinNumber = binNode?["BINNum"]?.InnerText;
                        if (!string.IsNullOrEmpty(newBinNumber) && await _bankBinService.GetBankBin(newBinNumber) == null)
                        {
                            var bin = new PaymentGarantiBin
                            {
                                BinNumber = newBinNumber,
                                CardAssociation = binNode["Organization"]?.InnerText,
                                BankCode = binNode["BankCode"].InnerText,
                                Product = binNode["Product"].InnerText,
                                BankName = binNode["BankName"].InnerText,
                                InstallmentInd = binNode["InstallmentInd"].InnerText,
                                CardType = binNode["CardType"].InnerText
                            };
                            await _bankBinService.InsertBankBin(bin);
                        }
                    }
                }
                binCode = await _bankBinService.GetBankBin(binNumber);
                if (binCode != null)
                {
                    await PrepareInstallmentByCategoryOrDefault(installmentInfo, cartItems, binCode);
                }
            }
        }
        else if (binCode.InstallmentInd == "Y")
        {
            await PrepareInstallmentByCategoryOrDefault(installmentInfo, cartItems, binCode);
        }

        await SessionExt.SetAsync(_httpContextAccessor.HttpContext.Session, "InstallmentViewModel", installmentInfo);
        return Json(installmentInfo);
    }

    /// <summary>
    /// Kategori bazlı özel taksit oranı var ise onları uygular; yoksa genel banka taksit listesini kullanır.
    /// Basit ve anlaşılır akış: Kart bilgileri -> kategori listesi -> karar -> oranları ekle.
    /// </summary>
    private async Task PrepareInstallmentByCategoryOrDefault(InstallmentViewModel installmentInfo, IList<ShoppingCartItem> cartItems, PaymentGarantiBin binCode)
    {
        // Kart bilgileri
        installmentInfo.CardType = binCode.CardType;
        installmentInfo.CardAssociation = binCode.CardAssociation;
        installmentInfo.CardFamily = binCode.Product;

        // Kategori bazlı oranlar var mı?
        var categoryRates = await _paymentPosService.GetBankInstallmentCategoryList();
        if (categoryRates.Count == 0)
        {
            var generalRates = (IList<PaymentGarantiInstallment>)await _paymentPosService.GetBankInstallmentList();
            await GetInstallmentAsync(installmentInfo, generalRates);
            return;
        }

        // Sepetteki benzersiz kategoriler
        var categories = await GetDistinctCartCategoriesAsync(cartItems);

        // Birden fazla kategori: kullanıcıyı bilgilendir, genel oranları uygula
        if (categories.Count > 1)
        {
            var firstName = categories.FirstOrDefault()?.Name;
            installmentInfo.InfoMessage += firstName + " Ürünlerindeki komisyondan faydalanmak için diğer ürünleri sepetten çıkarın ve tekrar ödemeyi deneyin. İstemiyorsanız devam edebilirsiniz.";

            var generalRates = (IList<PaymentGarantiInstallment>)await _paymentPosService.GetBankInstallmentList();
            await GetInstallmentAsync(installmentInfo, generalRates);
            return;
        }

        // Tek kategori: eşleşen kategori oranlarını uygula
        if (categories.Count == 1)
        {
            var catId = categories[0].Id;
            var matched = categoryRates.Where(ci => ci.CategoryId == catId).ToList();
            if (matched.Count > 0)
            {
                await GetInstallmentAsync(installmentInfo, matched);
                return;
            }
        }

        // Hiç eşleşme yoksa genel oranları uygula
        {
            var generalRates = (IList<PaymentGarantiInstallment>)await _paymentPosService.GetBankInstallmentList();
            await GetInstallmentAsync(installmentInfo, generalRates);
        }
    }

    #region Installment CRUD
    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin(false)]
    public async Task<IActionResult> InstallmentListAsync()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();
        var model = new SearchInstallmentModel();
        model.SetGridPageSize();
        return View("~/Plugins/Payments.GarantiPos/Views/Installment/List.cshtml", model);
    }

    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin(false)]
    [HttpPost]
    public async Task<IActionResult> InstallmentGridListAsync(SearchInstallmentModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return await AccessDeniedDataTablesJson();

        var installments = await _paymentPosService.GetBankInstallmentList(searchModel.Page - 1, searchModel.PageSize);

        var listModel = new InstallmentListModel();
        var dataProjection = installments.Select(inst => new InstallmentModel
        {
            Installment = inst.Installment,
            Rate = inst.Rate,
            Id = inst.Id
        });

        var gridModel = listModel.PrepareToGrid(searchModel, installments, () => dataProjection);
        return Json(gridModel);
    }

    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin(false)]
    public async Task<IActionResult> InstallmentCreateAsync()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();
        return View("~/Plugins/Payments.GarantiPos/Views/Installment/Create.cshtml", new InstallmentModel());
    }

    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin(false)]
    [HttpPost]
    [ParameterBasedOnFormName("save-continue", "continueEditing")]
    public async Task<IActionResult> InstallmentCreateAsync(InstallmentModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();
        if (ModelState.IsValid)
        {
            var installment = new PaymentGarantiInstallment { Installment = model.Installment, Rate = model.Rate };
            await _paymentPosService.InsertBankPosInstallment(installment);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Payments.GarantiPos.Installment.Added"), true);
            return continueEditing ? RedirectToAction("InstallmentEdit", new { id = installment.Id }) : RedirectToAction("InstallmentList");
        }
        return View("~/Plugins/Payments.GarantiPos/Views/Installment/Create.cshtml", model);
    }

    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin(false)]
    public async Task<IActionResult> InstallmentEditAsync(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();
        var installment = await _paymentPosService.GetBankInstallmentId(id);
        var model = new InstallmentModel { Installment = installment.Installment, Rate = installment.Rate, Id = installment.Id };
        return View("~/Plugins/Payments.GarantiPos/Views/Installment/Edit.cshtml", model);
    }

    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin(false)]
    [HttpPost]
    [ParameterBasedOnFormName("save-continue", "continueEditing")]
    public async Task<IActionResult> InstallmentEditAsync(InstallmentModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmailAccounts))
            return AccessDeniedView();
        var installment = await _paymentPosService.GetBankInstallmentId(model.Id);
        if (ModelState.IsValid)
        {
            installment.Installment = model.Installment;
            installment.Rate = model.Rate;
            await _paymentPosService.UpdateBankPosInstallment(installment);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Payments.GarantiPos.Installment.Updated"), true);
            return continueEditing ? RedirectToAction("InstallmentEdit", new { id = installment.Id }) : RedirectToAction("InstallmentList");
        }
        return View("~/Plugins/Payments.GarantiPos/Views/Installment/Edit.cshtml", model);
    }

    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin(false)]
    public async Task<IActionResult> InstallmentDeleteAsync(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();
        var installment = await _paymentPosService.GetBankInstallmentId(id);
        if (installment != null)
            await _paymentPosService.DeleteBankPosInstallment(installment);
        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Payments.GarantiPos.Installment.Deleted"), true);
        return RedirectToAction("InstallmentList");
    }
    #endregion

    #region Category Installment CRUD
    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin(false)]
    public async Task<IActionResult> CategoryInstallmentListAsync()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();
        var model = new CategorySearchModel();
        model.SetGridPageSize();
        return View("~/Plugins/Payments.GarantiPos/Views/Category/List.cshtml", model);
    }

    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin(false)]
    [HttpPost]
    public async Task<IActionResult> CategoryInstallmentGridListAsync(CategorySearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return await AccessDeniedDataTablesJson();

        var installments = await _paymentPosService.GetBankInstallmentCategoryList(searchModel.Page - 1, searchModel.PageSize);

        var listModel = new CategoryListModel();
        var dataProjection = installments.Select(inst => new CategoryInstallmentModel
        {
            Installment = inst.Installment,
            Rate = inst.Rate,
            CategoryName = inst.CategoryName,
            CategoryId = inst.Id,
            Id = inst.Id
        });

        var gridModel = listModel.PrepareToGrid(searchModel, installments, () => dataProjection);
        return Json(gridModel);
    }

    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin(false)]
    public async Task<IActionResult> CategoryInstallmentCreateAsync()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();
        var model = new CategoryInstallmentModel();
        var categories = await _categoryService.GetAllCategoriesAsync(0, false);
        foreach (var category in categories)
            model.AvailableCategories.Add(new SelectListItem { Text = category.Name, Value = category.Id.ToString() });
        return View("~/Plugins/Payments.GarantiPos/Views/Category/Create.cshtml", model);
    }

    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin(false)]
    [HttpPost]
    [ParameterBasedOnFormName("save-continue", "continueEditing")]
    public async Task<IActionResult> CategoryInstallmentCreateAsync(CategoryInstallmentModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();
        if (ModelState.IsValid)
        {
            var installment = new PaymentGarantiCategoryInstallment
            {
                Installment = model.Installment,
                Rate = model.Rate,
                CategoryId = model.CategoryId,
                CategoryName = (await _categoryService.GetCategoryByIdAsync(model.CategoryId)).Name
            };
            await _paymentPosService.InsertBankInstallmentCategory(installment);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Payments.GarantiPos.CategoryInstallment.Added"), true);
            return continueEditing ? RedirectToAction("CategoryInstallmentEdit", new { id = installment.Id }) : RedirectToAction("CategoryInstallmentList");
        }
        return View("~/Plugins/Payments.GarantiPos/Views/Category/Create.cshtml", model);
    }

    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin(false)]
    public async Task<IActionResult> CategoryInstallmentEditAsync(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();
        var installment = await _paymentPosService.GetBankInstallmentCategoryId(id);
        var model = new CategoryInstallmentModel
        {
            Installment = installment.Installment,
            Rate = installment.Rate,
            CategoryName = installment.CategoryName,
            CategoryId = installment.CategoryId,
            Id = installment.Id
        };
        foreach (var category in await _categoryService.GetAllCategoriesAsync(0, false))
            model.AvailableCategories.Add(new SelectListItem { Text = category.Name, Value = category.Id.ToString(), Selected = installment.CategoryId == category.Id });
        return View("~/Plugins/Payments.GarantiPos/Views/Category/Edit.cshtml", model);
    }

    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin(false)]
    [HttpPost]
    [ParameterBasedOnFormName("save-continue", "continueEditing")]
    public async Task<IActionResult> CategoryInstallmentEditAsync(CategoryInstallmentModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmailAccounts))
            return AccessDeniedView();
        var installment = await _paymentPosService.GetBankInstallmentCategoryId(model.Id);
        if (ModelState.IsValid)
        {
            installment.Installment = model.Installment;
            installment.Rate = model.Rate;
            installment.CategoryName = (await _categoryService.GetCategoryByIdAsync(model.CategoryId)).Name;
            installment.CategoryId = model.CategoryId;
            await _paymentPosService.UpdateBankInstallmentCategory(installment);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Payments.GarantiPos.CategoryInstallment.Updated"), true);
            return continueEditing ? RedirectToAction("CategoryInstallmentEdit", new { id = installment.Id }) : RedirectToAction("CategoryInstallmentList");
        }
        foreach (var category in await _categoryService.GetAllCategoriesAsync(0, false))
            model.AvailableCategories.Add(new SelectListItem { Text = category.Name, Value = category.Id.ToString(), Selected = installment.CategoryId == category.Id });
        return View("~/Plugins/Payments.GarantiPos/Views/Category/Edit.cshtml", model);
    }

    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin(false)]
    public async Task<IActionResult> CategoryInstallmentDeleteAsync(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();
        var installment = await _paymentPosService.GetBankInstallmentCategoryId(id);
        if (installment != null)
            await _paymentPosService.DeleteBankInstallmentCategory(installment);
        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Payments.GarantiPos.CategoryInstallment.Deleted"), true);
        return RedirectToAction("CategoryInstallmentList");
    }
    #endregion

    #region Bank BIN CRUD
    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin(false)]
    public async Task<IActionResult> ListAsync()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();
        var model = new BankBinSearchModel();
        model.SetGridPageSize();
        return View("~/Plugins/Payments.GarantiPos/Views/BankBin/List.cshtml", model);
    }

    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin(false)]
    [HttpPost]
    public async Task<IActionResult> BankBinListAsync(BankBinSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return await AccessDeniedDataTablesJson();

        var bins = await _bankBinService.GetBankBinPageList(
            searchModel.BinNumber,
            searchModel.BankCode,
            searchModel.CardType,
            searchModel.Product,
            searchModel.CardAssociation,
            searchModel.BankName,
            searchModel.InstallmentInd,
            searchModel.Page - 1,
            searchModel.PageSize);

        var listModel = new BankBinListModel();
        var dataProjection = bins.Select(bin => new BankBinModel
        {
            BankCode = bin.BankCode,
            BinNumber = bin.BinNumber,
            CardAssociation = bin.CardAssociation,
            CardType = bin.CardType,
            BankName = bin.BankName,
            Product = bin.Product,
            InstallmentInd = bin.InstallmentInd,
            Id = bin.Id
        });

        var gridModel = listModel.PrepareToGrid(searchModel, bins, () => dataProjection);
        return Json(gridModel);
    }

    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin(false)]
    public async Task<IActionResult> CreateAsync()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();
        return View("~/Plugins/Payments.GarantiPos/Views/BankBin/Create.cshtml", new BankBinModel());
    }

    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin(false)]
    [HttpPost]
    [ParameterBasedOnFormName("save-continue", "continueEditing")]
    public async Task<IActionResult> CreateAsync(BankBinModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();
        if (ModelState.IsValid)
        {
            var bin = new PaymentGarantiBin
            {
                BankCode = model.BankCode,
                BinNumber = model.BinNumber,
                CardType = model.CardType,
                CardAssociation = model.CardAssociation,
                BankName = model.BankName,
                InstallmentInd = model.InstallmentInd,
                Product = model.Product
            };
            await _bankBinService.InsertBankBin(bin);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Payments.GarantiPos.BankBin.Added"), true);
            return continueEditing ? RedirectToAction("Edit", new { id = bin.Id }) : RedirectToAction("List");
        }
        return View("~/Plugins/Payments.GarantiPos/Views/BankBin/Create.cshtml", model);
    }

    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin(false)]
    public async Task<IActionResult> EditAsync(int id)
    {
        var bin = await _bankBinService.GetBankBinId(id);
        if (bin == null)
            return RedirectToAction("List");
        var model = new BankBinModel
        {
            Id = bin.Id,
            CardAssociation = bin.CardAssociation,
            BankCode = bin.BankCode,
            BinNumber = bin.BinNumber,
            CardType = bin.CardType,
            BankName = bin.BankName,
            Product = bin.Product,
            InstallmentInd = bin.InstallmentInd
        };
        return View("~/Plugins/Payments.GarantiPos/Views/BankBin/Edit.cshtml", model);
    }

    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin(false)]
    [HttpPost]
    [ParameterBasedOnFormName("save-continue", "continueEditing")]
    public async Task<IActionResult> EditAsync(BankBinModel model, bool continueEditing)
    {
        var bin = await _bankBinService.GetBankBinId(model.Id);
        if (bin == null)
            return RedirectToAction("List");
        if (ModelState.IsValid)
        {
            bin.CardAssociation = model.CardAssociation;
            bin.BankCode = model.BankCode;
            bin.BinNumber = model.BinNumber;
            bin.CardType = model.CardType;
            bin.BankName = model.BankName;
            bin.InstallmentInd = model.InstallmentInd;
            bin.Product = model.Product;
            await _bankBinService.UpdateBankBin(bin);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Payments.GarantiPos.BankBin.Updated"), true);
            return continueEditing ? RedirectToAction("Edit", new { id = bin.Id }) : RedirectToAction("List");
        }
        return View("~/Plugins/Payments.GarantiPos/Views/BankBin/Edit.cshtml", model);
    }

    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin(false)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();
        var bin = await _bankBinService.GetBankBinId(id);
        if (bin == null)
            return RedirectToAction("List");
        try
        {
            await _bankBinService.DeleteBankBin(bin);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Payments.GarantiPos.BankBin.Deleted"), true);
            return RedirectToAction("List");
        }
        catch (Exception ex)
        {
            await _notificationService.ErrorNotificationAsync(ex, true);
            return RedirectToAction("Edit", new { id = bin.Id });
        }
    }
    #endregion
}
