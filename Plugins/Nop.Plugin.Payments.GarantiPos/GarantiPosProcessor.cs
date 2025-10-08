using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Data;
using Nop.Plugin.Payments.GarantiPos.Components;
using Nop.Plugin.Payments.GarantiPos.Domains;
using Nop.Plugin.Payments.GarantiPos.Helpers;
using Nop.Plugin.Payments.GarantiPos.Models;
using Nop.Plugin.Payments.GarantiPos.Services;
using Nop.Plugin.Payments.GarantiPos.Validators;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Web.Framework.Menu;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using SessionExt = Nop.Core.Http.Extensions.SessionExtensions;

namespace Nop.Plugin.Payments.GarantiPos;

/// <summary>
/// GarantiPos ana ödeme işleyici sınıfı.
/// IPaymentMethod implementasyonu ile Nop ödeme pipeline'ına entegre olur.
/// 3D doğrulama senaryosunda ProcessPayment -> PostProcessPayment -> (Bank form POST) -> Success/Cancel akışı izlenir.
/// </summary>
public class GarantiPosProcessor : BasePlugin, IPaymentMethod, IAdminMenuPlugin
{
    private static readonly string PluginSystemName = GarantiPosDefault.SystemName;

    #region Bağımlılıklar
    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;
    private readonly GarantiPosSettings _garantiPosSettings;
    private readonly IWebHelper _webHelper;
    private readonly IQueuedEmailService _queuedEmailService;
    private readonly EmailAccountSettings _emailAccountSettings;
    private readonly IEmailAccountService _emailAccountService;
    private readonly IStoreContext _storeContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IOrderTotalCalculationService _orderTotalCalculationService;
    private readonly ILanguageService _languageService;
    private readonly IRepository<PaymentGarantiOrder> _paymentGarantiPosRepository;
    private readonly ILogger _logger;
    private readonly IWorkContext _workContext;
    private readonly IPaymentPosService _paymentPosService;
    private readonly IScheduleTaskService _scheduleTaskService;
    #endregion

    #region Ctor
    public GarantiPosProcessor(
        ILocalizationService localizationService,
        ISettingService settingService,
        GarantiPosSettings garantiPosSettings,
        IWebHelper webHelper,
        IQueuedEmailService queuedEmailService,
        EmailAccountSettings emailAccountSettings,
        IEmailAccountService emailAccountService,
        IStoreContext storeContext,
        IHttpContextAccessor httpContextAccessor,
        IActionContextAccessor actionContextAccessor,
        IUrlHelperFactory urlHelperFactory,
        IOrderTotalCalculationService orderTotalCalculationService,
        ILanguageService languageService,
        IRepository<PaymentGarantiOrder> paymentGarantiPosRepository,
        ILogger logger,
        IWorkContext workContext,
        IPaymentPosService paymentPosService,
        IScheduleTaskService scheduleTaskService)
    {
        _localizationService = localizationService;
        _settingService = settingService;
        _garantiPosSettings = garantiPosSettings;
        _webHelper = webHelper;
        _queuedEmailService = queuedEmailService;
        _emailAccountSettings = emailAccountSettings;
        _emailAccountService = emailAccountService;
        _storeContext = storeContext;
        _httpContextAccessor = httpContextAccessor;
        _actionContextAccessor = actionContextAccessor;
        _urlHelperFactory = urlHelperFactory;
        _orderTotalCalculationService = orderTotalCalculationService;
        _languageService = languageService;
        _paymentGarantiPosRepository = paymentGarantiPosRepository;
        _logger = logger;
        _workContext = workContext;
        _paymentPosService = paymentPosService;
        _scheduleTaskService = scheduleTaskService;
    }
    #endregion

    #region IPaymentMethod Meta Bilgisi
    // Garanti pos için desteklenmeyen işlemler false olarak işaretlendi
    public bool SupportCapture => false;
    public bool SupportPartiallyRefund => false;
    public bool SupportRefund => false;
    public bool SupportVoid => false;
    public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;
    public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;
    public bool SkipPaymentInfo => false; // ödeme sayfasında kart formu gösterilecek
    #endregion

    #region Yardımcılar
    // Artık kurulum/kaldırma için e-posta gönderilmiyor; yer tutucu.
    private Task CreateMessageAsync(bool install = true) => Task.CompletedTask;

    /// <summary>
    /// Bankaya gönderilecek veya tarayıcıya otomatik gönderim yapılacak HTML formunu üretir.
    /// 3D Secure yönlendirmesi için kullanılır.
    /// </summary>
    private static string PreparePostForm(string url, NameValueCollection data)
    {
        const string formId = "PostForm";
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html lang=\"tr\"><head><meta charset=\"text/html;charset=UTF-8\" /></head><body><div>");
        sb.Append($"<form id=\"{formId}\" name=\"{formId}\" action=\"{url}\" method=\"POST\">");
        foreach (string key in data)
            sb.Append($"<input type=\"hidden\" name=\"{key}\" id=\"{key}\" value=\"{data[key]}\">");
        sb.Append("</form>");
        // Otomatik submit – kullanıcı ara ekran görmez.
        sb.Append("<script type=\"text/javascript\">document.getElementById('PostForm').submit();</script></div></body></html>");
        return sb.ToString();
    }
    #endregion

    #region Ödeme Akışı
    /// <summary>
    /// Sipariş oluşturulması aşamasında çağrılır. Henüz banka ile konuşulmaz.
    /// Banka yönlendirmesi PostProcessPayment içinde yapılır.
    /// </summary>
    public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest request)
    {
        var result = new ProcessPaymentResult
        {
            NewPaymentStatus = (PaymentStatus)10, // eklentiye özel başlangıç statüsü (Pending benzeri)
            AllowStoringCreditCardNumber = true
        };

        // Session'da tutulan geçici pos kaydını siparişin GUID'i ile ilişkilendir.
        var paymentOrder = await SessionExt.GetAsync<PaymentGarantiOrder>(_httpContextAccessor.HttpContext.Session, "PaymentOrder");
        paymentOrder.OrderNumber = request.OrderGuid;
        await _paymentPosService.UpdatePosOrderAsync(paymentOrder);
        await SessionExt.SetAsync(_httpContextAccessor.HttpContext.Session, "PaymentOrder", paymentOrder);
        return result;
    }

    /// <summary>
    /// Nop siparişi oluşturduktan sonra bankaya yönlendirme için 3D form HTML üretimi.
    /// Burada hash hesaplanır ve müşteri banka sayfasına (veya iframe) yönlendirilir.
    /// </summary>
    public async Task PostProcessPaymentAsync(PostProcessPaymentRequest ctx)
    {
        var settings = await _settingService.LoadSettingAsync<GarantiPosSettings>(0);
        var paymentOrder = await SessionExt.GetAsync<PaymentGarantiOrder>(_httpContextAccessor.HttpContext.Session, "PaymentOrder");
        var order = ctx.Order;
        await _paymentPosService.UpdatePosOrderAsync(paymentOrder);
        await SessionExt.SetAsync(_httpContextAccessor.HttpContext.Session, "PaymentOrder", paymentOrder);
        if (paymentOrder.StatusId == 30)
            return; // zaten ödendi (StatusId = 30 -> Paid)

        var total = paymentOrder.PaidPrice.HasValue ? Math.Round(paymentOrder.PaidPrice.Value, 2) : 0m;
        var processPayment = JsonConvert.DeserializeObject<ProcessPaymentRequest>(paymentOrder.PaymentInfo);
        var cardNumber = processPayment.CreditCardNumber;
        var currencyCode = HelperOptions.GetCurrencyCode((await _workContext.GetWorkingCurrencyAsync()).CurrencyCode);

        // Banka isteği modeli hazırlama
        var model = new XmlModel
        {
            ModeEnum = settings.TestMode ? "TEST" : "PROD",
            ApiVersion = settings.Version,
            TerminalProvUserId = settings.TerminalProvUserId,
            CurrencyCode = currencyCode,
            InstallmentCount = paymentOrder.NumberOfInstallment < 2 ? string.Empty : paymentOrder.NumberOfInstallment.ToString(),
            TerminalUserId = settings.TerminalUserId,
            OrderId = order.OrderGuid.ToString("N"),
            CustomerIp = _webHelper.GetCurrentIpAddress(),
            CustomerEmail = (await _workContext.GetCurrentCustomerAsync()).Email,
            TerminalId = settings.TerminalId,
            TerminalMerchantId = settings.MerchantId,
            StoreKey = settings.StoreKey,
            ProvisionPassword = settings.Password,
            SuccessUrl = _webHelper.GetStoreLocation(true) + "PaymentGarantiPos/Success",
            ErrorrUrl = _webHelper.GetStoreLocation(true) + "PaymentGarantiPos/Cancel",
            CompanyName = settings.CompanyName,
            SecurityLevel = settings.SecurityLevel,
            Language = (await _workContext.GetWorkingLanguageAsync()).UniqueSeoCode,
            RefreshTime = "10",
            SecurityData = HelperOptions.Sha1(settings.Password + HelperOptions.IsRequireZero(settings.TerminalId, 9)),
            CardNumber = cardNumber,
            CardholderName = processPayment.CreditCardName,
            ExpireMonth = HelperOptions.EncodeExpireMonth(processPayment.CreditCardExpireMonth),
            ExpireYear = HelperOptions.EncodeExpireYear(processPayment.CreditCardExpireYear),
            CardCode = processPayment.CreditCardCvv2,
            TxnType = "sales",
            PaymentUrl = settings.Bank3DUrl
        };
        // Banka genelde tutarı kuruş *100 formatında ister.
        model.Amount = (total * 100m).ToString("0.##", new CultureInfo("en-US"));
        // Hash veri bütünlüğü ve sahtecilik önleme için.
        try
        {
            int.TryParse(model.InstallmentCount, out var installmentCount);

            if (installmentCount == 0)
            {
                model.InstallmentCount = "";
            }
            model.HashData = HelperOptions.GetHashData(settings.Password,
                model.TerminalId,
                model.OrderId,
                model.InstallmentCount,
                model.StoreKey,
                model.Amount,
                currencyCode,
                model.SuccessUrl,
                model.TxnType,
                model.ErrorrUrl);
        }
        catch (Exception ex)
        {

        }

        // Otomatik post edilecek form HTML
        var strForm = PreparePostForm(model.PaymentUrl, new NameValueCollection
        {
            { "secure3dsecuritylevel", model.SecurityLevel },
            { "mode", model.ModeEnum },
            { "apiversion", model.ApiVersion },
            { "terminalprovuserid", model.TerminalProvUserId },
            { "terminaluserid", model.TerminalUserId },
            { "terminalmerchantid", model.TerminalMerchantId },
            { "txntype", model.TxnType },
            { "txnamount", model.Amount },
            { "txninstallmentcount", model.InstallmentCount },
            { "txncurrencycode", model.CurrencyCode.ToString() },
            { "orderid", model.OrderId },
            { "terminalid", model.TerminalId },
            { "successurl", model.SuccessUrl },
            { "errorurl", model.ErrorrUrl },
            { "customeremailaddress", model.CustomerEmail },
            { "customeripaddress", model.CustomerIp },
            { "secure3dhash", model.HashData },
            { "lang", model.Language },
            { "txntimestamp", model.TimeStamp },
            { "cardnumber", model.CardNumber },
            { "cardexpiredatemonth", model.ExpireMonth },
            { "cardexpiredateyear", model.ExpireYear },
            { "cardcvv2", model.CardCode }
        });

        // Banka isteği ve response (hazırlanan form) loglanıyor
        paymentOrder.BankRequest = JsonConvert.SerializeObject(model);
        paymentOrder.BankResponse = strForm; // burada fiilen banka yanıtı değil form html'i tutuluyor.
        await _paymentPosService.UpdatePosOrderAsync(paymentOrder);
        await _logger.InsertLogAsync(LogLevel.Debug, "htmlContent", strForm, null);
        _httpContextAccessor.HttpContext.Session.SetString("htmlContent", strForm);
        var uri = _webHelper.GetStoreLocation(null) + "PaymentGarantiPos/TreeDContent"; // form gösterim endpoint'i
        _httpContextAccessor.HttpContext.Response.Redirect(uri);
    }

    public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart) => Task.FromResult(false);
    public Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart) => _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart, _garantiPosSettings.AdditionalFee, _garantiPosSettings.AdditionalFeePercentage);
    public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest request) => Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture yöntemi desteklenmiyor" } });
    public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest request) => Task.FromResult(new RefundPaymentResult()); // manuel süreç varsayıldı
    public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest request) => Task.FromResult(new VoidPaymentResult { Errors = new[] { "Void (iptal) desteklenmiyor" } });
    public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest request) => Task.FromResult(new ProcessPaymentResult { Errors = new[] { "Tekrarlayan ödeme desteklenmiyor" } });
    public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest request) => Task.FromResult(new CancelRecurringPaymentResult { Errors = new[] { "Tekrarlayan ödeme desteklenmiyor" } });
    public Task<bool> CanRePostProcessPaymentAsync(Order order) => Task.FromResult(true); // 3D formu yeniden denenebilir

    /// <summary>
    /// Kart form doğrulaması. FluentValidation kullanır.
    /// </summary>
    public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
    {
        if (form == null) throw new ArgumentNullException(nameof(form));
        var validator = new PaymentInfoValidator(_localizationService);
        var model = new PaymentInfoModel
        {
            CardholderName = form["cardName"],
            CardNumber = form["cardNumber"],
            CardCode = form["cardCvv"],
            ExpireMonth = form["cardMonth"],
            ExpireYear = form["cardYear"]
        };
        var validation = validator.Validate(model);
        return Task.FromResult(validation.IsValid ? new List<string>() as IList<string> : validation.Errors.Select(e => e.ErrorMessage).ToList());
    }

    /// <summary>
    /// Ödeme formundan ProcessPaymentRequest modelini üretir ve taksit bilgilerini ilişkilendirir.
    /// </summary>
    public async Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
    {
        var req = new ProcessPaymentRequest
        {
            CreditCardType = form["CreditCardType"],
            CreditCardName = form["cardName"],
            CreditCardNumber = form["cardNumber"].ToString().Replace(" ", string.Empty).Trim(),
            CreditCardCvv2 = form["cardCvv"]
        };
        var paymentOrder = await SessionExt.GetAsync<PaymentGarantiOrder>(_httpContextAccessor.HttpContext.Session, "PaymentOrder");
        var installmentInfo = await SessionExt.GetAsync<InstallmentViewModel>(_httpContextAccessor.HttpContext.Session, "InstallmentViewModel");

        if (int.TryParse(form["cardMonth"], out var em)) req.CreditCardExpireMonth = em;
        if (int.TryParse(form["cardYear"], out var ey)) req.CreditCardExpireYear = ey;
        if (int.TryParse(form["NumberOfInstallment"], out var inst)) { paymentOrder.NumberOfInstallment = inst; req.CustomValues.Add("Taksit", inst); }
        if (decimal.TryParse(form["Total"], out var total))
        {
            req.CustomValues.Add("Ödeme Tutarı", Math.Round(total, 2));
            // Seçilen taksit satırı üzerinden vade farkı hesaplama
            foreach (var item in installmentInfo.InstallmentItems.Where(i => i.Installment == paymentOrder.NumberOfInstallment))
            {
                var diff = item.TotalAmountValue - total;
                req.CustomValues.Add("Vade Farkı", diff);
                paymentOrder.PaidPrice = Math.Round(item.TotalAmountValue, 2);
            }
        }
        // Ödeme isteğini JSON olarak geçici kayda yaz.
        paymentOrder.PaymentInfo = JsonConvert.SerializeObject(req);
        await _paymentGarantiPosRepository.UpdateAsync(paymentOrder, true);
        await SessionExt.SetAsync(_httpContextAccessor.HttpContext.Session, "PaymentOrder", paymentOrder);
        return req;
    }
    #endregion

    #region Arayüz / Açıklama
    public Type GetPublicViewComponent() => typeof(GarantiPosViewComponent);
    public Task<string> GetPaymentMethodDescriptionAsync() => _localizationService.GetResourceAsync("Plugins.Payments.GarantiPos.PaymentMethodDescription");
    #endregion

    #region Kurulum / Kaldırma
    /// <summary>
    /// Varsayılan ayarları ve yerelleştirme kaynaklarını ekler; senkronizasyon görevi planlanır.
    /// </summary>
    public override async Task InstallAsync()
    {
        var setting = new GarantiPosSettings
        {
            MerchantId = "7000679",
            Installment = true,
            Email = string.Empty,
            AdditionalFee = 0m,
            AdditionalFeePercentage = false,
            CompanyName = string.Empty,
            Password = "123qweASD/",
            SecurityLevel = "3D",
            StoreKey = "12345678",
            TerminalId = "30691297",
            TerminalProvUserId = "PROVAUT",
            TerminalUserId = "SANALUSER",
            TestMode = true,
            Version = "512",
            Bank3DUrl = "https://sanalposprovtest.garantibbva.com.tr/servlet/gt3dengine",
            BankNone3DUrl = "https://sanalposprovtest.garantibbva.com.tr/VPServlet"
        };
        await _settingService.SaveSettingAsync(setting, 0);

        var allLanguages = await _languageService.GetAllLanguagesAsync(false, 0);
        var trLanguage = allLanguages.FirstOrDefault(x => x.LanguageCulture == "tr-TR");
        if (trLanguage != null)
        {
            var res = new Dictionary<string, string>
            {
                // (Kaynak listesi kesilmedi – mevcut içerik korunuyor)
                ["Plugins.Payments.GarantiPos.Field.TerminalId"] = "Terminal Id",
                ["Plugins.Payments.GarantiPos.Field.TerminalId.hint"] = "Terminal Id",
                ["Plugins.Payments.GarantiPos.Field.MerchantId."] = "Merchant(İşyeri) Id",
                ["Plugins.Payments.GarantiPos.Field.MerchantId.hint"] = "Merchant(İşyeri) Id",
                ["Plugins.Payments.GarantiPos.Field.StoreKey"] = "Store Key",
                ["Plugins.Payments.GarantiPos.Field.StoreKey.hint"] = "Store Key",
                ["Plugins.Payments.GarantiPos.Field.Password."] = "Password",
                ["Plugins.Payments.GarantiPos.Field.Password.hint"] = "Password",
                ["Plugins.Payments.GarantiPos.Field.CompanyName"] = "CompanyName",
                ["Plugins.Payments.GarantiPos.Field.CompanyName.hint"] = "CompanyName",
                ["Plugins.Payments.GarantiPos.Field.Email"] = "Email",
                ["Plugins.Payments.GarantiPos.Field.Email.hint"] = "Email",
                ["Plugins.Payments.GarantiPos.Field.TestUrl"] = "TestUrl",
                ["Plugins.Payments.GarantiPos.Field.TestUrl.hint"] = "TestUrl",
                ["Plugins.Payments.GarantiPos.Field.Version"] = "Version",
                ["Plugins.Payments.GarantiPos.Field.Version.hint"] = "Version",
                ["Plugins.Payments.GarantiPos.Field.TerminalUserId"] = "TerminalUserId",
                ["Plugins.Payments.GarantiPos.Field.TerminalUserId.hint"] = "TerminalUserId",
                ["Plugins.Payments.GarantiPos.Field.TerminalProvUserId"] = "TerminalProvUserId",
                ["Plugins.Payments.GarantiPos.Field.TerminalProvUserId.hint"] = "TerminalProvUserId",
                ["Plugins.Payments.GarantiPos.Field.SecurityLevel"] = "SecurityLevel",
                ["Plugins.Payments.GarantiPos.Field.SecurityLevel.hint"] = "SecurityLevel",
                ["Plugins.Payments.GarantiPos.Field.Installment"] = "Installment Count",
                ["Plugins.Payments.GarantiPos.Field.Installment.hint"] = "Installment Count",
                ["Plugins.Payments.GarantiPos.Field.AdditionalFee"] = "AdditionalFee",
                ["Plugins.Payments.GarantiPos.Field.AdditionalFee.hint"] = "AdditionalFee",
                ["Plugins.Payments.GarantiPos.Field.AdditionalFeePercentage"] = "AdditionalFeePercentage",
                ["Plugins.Payments.GarantiPos.Field.AdditionalFeePercentage.hint"] = "AdditionalFeePercentage",
                // Yeni alan etiketleri
                ["Plugins.Payments.GarantiPos.Field.Bank3DUrl"] = "3D Ödeme URL",
                ["Plugins.Payments.GarantiPos.Field.Bank3DUrl.hint"] = "Bankanın 3D ödeme geçidi URL'si",
                ["Plugins.Payments.GarantiPos.Field.BankNone3DUrl"] = "3D Olmayan İşlem URL",
                ["Plugins.Payments.GarantiPos.Field.BankNone3DUrl.hint"] = "BIN sorgu vb. 3D olmayan servis URL'si",
                // Grid ve CRUD metinleri
                ["Plugins.Payments.GarantiPos.BankBin.Added"] = "Bin Kodu Eklendi",
                ["Plugins.Payments.GarantiPos.BankBin.Updated"] = "Bin Kodu Güncellendi",
                ["Plugins.Payments.GarantiPos.BankBin.Deleted"] = "Bin Kodu Silindi",
                ["Plugins.Payments.GarantiPos.Installment.Added"] = "Taksit Eklendi",
                ["Plugins.Payments.GarantiPos.Installment.Updated"] = "Taksit Güncellendi",
                ["Plugins.Payments.GarantiPos.Installment.Deleted"] = "Taksit Silindi",
                ["Plugins.Payments.GarantiPos.CategoryInstallment.Added"] = "Kategori Taksit Eklendi",
                ["Plugins.Payments.GarantiPos.CategoryInstallment.Updated"] = "Kategori Taksit Güncellendi",
                ["Plugins.Payments.GarantiPos.CategoryInstallment.Deleted"] = "Kategori Taksit Silindi",
                ["Plugins.Payments.GarantiPos.BankBin"] = "Banka Bin Listesi",
                ["Plugins.Payments.GarantiPos.BankBin.BankName"] = "Banka Adı",
                ["Plugins.Payments.GarantiPos.BankBin.BinNumber"] = "Bin Numarası",
                ["Plugins.Payments.GarantiPos.BankBin.CardAssociation"] = "CardAssociation",
                ["Plugins.Payments.GarantiPos.BankBin.EditDetails"] = "Banka Bin Detay",
                ["Plugins.Payments.GarantiPos.BackToList"] = "Listeye Dön",
                ["Plugins.Payments.GarantiPos.BankBin.AddNew"] = "Ekle",
                ["Plugins.Payments.GarantiPos.Category.Installment"] = "Taksitler",
                ["Plugins.Payments.GarantiPos.Category.Installment.CategoryName"] = "Kategori Adı",
                ["Plugins.Payments.GarantiPos.Category.Installment.Installment"] = "Taskit Sayısı",
                ["Plugins.Payments.GarantiPos.Category.Installment.Rate"] = "Taskit Oranı",
                ["Plugins.Payments.GarantiPos.Installment.Installment"] = "Taskit Sayısı",
                ["Plugins.Payments.GarantiPos.Installment.Rate"] = "Taskit Oranı",
                ["Plugins.Payments.GarantiPos.Installment.EditDetails"] = "Taskit Detay",
                ["Plugins.Payments.GarantiPosr.Required"] = "Bin Numarası Gereklidir",
                ["Plugins.Payments.GarantiPos.BankName.Required"] = "Banka Adı Gereklidir",
                ["Plugins.Payments.GarantiPos.BankCode.Required"] = "BankCode  Gereklidir",
                ["Plugins.Payments.GarantiPos.CardAssociation.Required"] = "CardAssociation  Gereklidir",
                ["Plugins.Payments.GarantiPos.CardFamilyName.Required"] = "CardFamilyName  Gereklidir",
                ["Plugins.Payments.GarantiPos.Installment.Required"] = "Taksit Sayısı  Gereklidir",
                ["Plugins.Payments.GarantiPos.Rate.Required"] = "Taksit Oranı  Gereklidir",
                ["Plugins.Payments.GarantiPos.Admin.Menu.Pos"] = "Garanti Bank Pos Ayarları",
                ["Plugins.Payments.GarantiPos.Admin.Menu.PaymentGarantiPos.Installment.InstallmentList"] = "Taksitler",
                ["Plugins.Payments.GarantiPos.Admin.Menu.PaymentGarantiPos.Installment.CategoryInstallmentList"] = "Kategori Taksitler",
                ["Plugins.Payments.GarantiPos.Admin.Menu.PaymentGarantiPos.Installment.List"] = "Bin Kodları",
                ["plugins.payments.GarantiPos.admin.menu.title "] = "Garanti Bank Pos",
                ["plugins.payments.GarantiPos.admin.menu.PaymentGarantiPos.bincode.list "] = "Bin Kodları",
                ["plugins.payments.GarantiPos.installmentnumber "] = "Taksit Sayısı",
                ["plugins.payments.paymentgarantipos.bankbin.binnumber "] = "Bin Numarası",
                ["plugins.payments.paymentgarantipos.bankbin.cardassociation "] = "Card Association",
                ["Plugins.Payments.GarantiPos.Field.CardType"] = "Card Type",
                ["Plugins.Payments.GarantiPos.Field.BankCode"] = "Bank Kodu",
                ["Plugins.Payments.GarantiPos.Field.BinNumber"] = "Bin Number",
                ["Plugins.Payments.GarantiPos.Field.Product"] = "Product",
                ["Plugins.Payments.GarantiPos.Field.CardAssociation"] = "Card Association",
                ["Plugins.Payments.GarantiPos.Field.BankName"] = "Bank Name",
                ["Plugins.Payments.GarantiPos.Field.InstallmentInd"] = "InstallmentInd",
                ["plugins.payments.garantipos.installment"] = "installment",
                ["plugins.payments.garantipos.installment.addnew"] = "AddNew",
                ["Plugins.Payments.GarantiPos.Field.Rate"] = "Rate",
                ["Plugins.Payments.GarantiPos.Field.MerchantId"] = "MerchantId",
                ["Plugins.Payments.GarantiPos.Field.Password"] = "Password",
                ["Plugins.Payments.GarantiPos.Field.TestMode"] = "TestMode",
                ["Plugins.Payments.GarantiPos.Field.Force3D"] = "Force3D",
                ["plugins.payments.garantipos.code"] = "CVV",
                ["plugins.payments.garantipos.expirationdate"] = "Expiration Date",
                ["plugins.payments.garantipos.price"] = "Price",
                ["plugins.payments.garantipos.totalprice"] = "Total Price",
                ["plugins.payments.garantipos.month"] = "Ay",
                ["plugins.payments.garantipos.year"] = "Yıl",
                ["Plugins.Payments.GarantiPos.InstallmentEmpty"] = "Peşin",
                ["Plugins.Payments.GarantiPos.installmentdisplay"] = "Taksit",
                ["plugins.payments.garantipos.paymentmethoddescription"] = "Kredi Kartı"
            };
            await _localizationService.AddOrUpdateLocaleResourceAsync(res, trLanguage.Id);
        }
        if (trLanguage != null)
        {
            var resTr = new Dictionary<string, string> { ["Plugins.Payments.GarantiPos.InstallmentEmpty"] = "Peşin" };
            await _localizationService.AddOrUpdateLocaleResourceAsync(resTr, trLanguage.Id);
        }

        // Senkronizasyon görevi aktifleştirme
        var scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync(GarantiPosDefault.SynchronizationTask);
        if (scheduleTask == null)
        {
            await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
            {
                Enabled = true,
                Name = GarantiPosDefault.SynchronizationTaskName,
                Seconds = GarantiPosDefault.DefaultSynchronizationPeriod * 60 * 60,
                Type = GarantiPosDefault.SynchronizationTask
            });
        }
        else if (!scheduleTask.Enabled)
        {
            scheduleTask.Enabled = true;
            scheduleTask.Seconds = GarantiPosDefault.DefaultSynchronizationPeriod * 60 * 60;
            await _scheduleTaskService.UpdateTaskAsync(scheduleTask);
        }
        await CreateMessageAsync();
        await base.InstallAsync();
    }

    /// <summary>
    /// Kaynakları, ayarları ve görevleri temizler.
    /// </summary>
    public override async Task UninstallAsync()
    {
        var scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync(GarantiPosDefault.SynchronizationTask);
        if (scheduleTask != null)
        {
            scheduleTask.Enabled = false;
            await _scheduleTaskService.UpdateTaskAsync(scheduleTask);
        }
        await _localizationService.DeleteLocaleResourcesAsync("plugins.Payments.GarantiPos", null);
        await _settingService.DeleteSettingAsync<GarantiPosSettings>();
        await CreateMessageAsync(false);
        await base.UninstallAsync();
    }
    #endregion

    #region Yönetim Menüsü
    /// <summary>
    /// Yönetim menüsüne pos alt menülerini ekler.
    /// </summary>
    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var paymentGarantiPos = new SiteMapNode
        {
            SystemName = GarantiPosDefault.SystemName,
            Title = await _localizationService.GetResourceAsync("Plugins.Payments.GarantiPos.Admin.Menu.Title"),
            Visible = true,
            IconClass = "far fa-credit-card"
        };
        paymentGarantiPos.ChildNodes = new List<SiteMapNode>
        {
            new()
            {
                Title = await _localizationService.GetResourceAsync("Plugins.Payments.GarantiPos.Admin.Menu.PaymentGarantiPos.BinCode.List"),
                Visible = true,
                ActionName = "List",
                ControllerName = "PaymentGarantiPos",
                SystemName = "BankBin",
                IconClass = "far fa-dot-circle",
                RouteValues = new RouteValueDictionary { { "area", "Admin" } }
            },
            new()
            {
                Title = await _localizationService.GetResourceAsync("Plugins.Payments.GarantiPos.Admin.Menu.PaymentGarantiPos.Installment.InstallmentList"),
                Visible = true,
                ActionName = "InstallmentList",
                ControllerName = "PaymentGarantiPos",
                SystemName = "Installments",
                IconClass = "far fa-dot-circle",
                RouteValues = new RouteValueDictionary { { "area", "Admin" } }
            },
            new()
            {
                Title = await _localizationService.GetResourceAsync("Plugins.Payments.GarantiPos.Admin.Menu.GarantiPos.Installment.CategoryInstallmentList"),
                Visible = true,
                ActionName = "CategoryInstallmentList",
                ControllerName = "PaymentGarantiPos",
                SystemName = "CategoryInstallment",
                IconClass = "far fa-dot-circle",
                RouteValues = new RouteValueDictionary { { "area", "Admin" } }
            }
        };
        rootNode.ChildNodes.Add(paymentGarantiPos);
    }

    public override string GetConfigurationPageUrl() =>
        _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).RouteUrl(new UrlRouteContext { RouteName = GarantiPosDefault.ConfigurationRouteName });
    #endregion
}
