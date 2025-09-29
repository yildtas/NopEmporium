using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Data;
using Nop.Plugin.Payments.GarantiPos.Domains;
using Nop.Plugin.Payments.GarantiPos.Helpers;
using Nop.Plugin.Payments.GarantiPos.Services;
using Nop.Plugin.Payments.GarantiPos.Models; // added
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Services.ScheduleTasks;
using Nop.Web.Framework.Components;
using SessionExt = Nop.Core.Http.Extensions.SessionExtensions;

namespace Nop.Plugin.Payments.GarantiPos.Components
{
    /// <summary>
    /// Ödeme ekranında kart formunu oluşturan ViewComponent.
    /// Sepet verisini okuyup geçici PaymentGarantiOrder kaydı oluşturur/günceller.
    /// </summary>
    public class GarantiPosViewComponent : NopViewComponent
    {
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPaymentPosService _paymentPosService;
        private readonly IProductService _productService;
        private readonly IRepository<PaymentGarantiOrderItem> _paymentGarantiOrderItemRepository;
        private readonly IScheduleTaskService _scheduleTaskService;

        public GarantiPosViewComponent(
            ISettingService settingService,
            IWorkContext workContext,
            ICurrencyService currencyService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IShoppingCartService shoppingCartService,
            ICustomerService customerService,
            IStoreContext storeContext,
            IHttpContextAccessor httpContextAccessor,
            IPaymentPosService paymentPosService,
            IProductService productService,
            IRepository<PaymentGarantiOrderItem> paymentGarantiOrderItemRepository,
            IScheduleTaskService scheduleTaskService)
        {
            _settingService = settingService;
            _workContext = workContext;
            _currencyService = currencyService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _shoppingCartService = shoppingCartService;
            _customerService = customerService;
            _storeContext = storeContext;
            _httpContextAccessor = httpContextAccessor;
            _paymentPosService = paymentPosService;
            _productService = productService;
            _paymentGarantiOrderItemRepository = paymentGarantiOrderItemRepository;
            _scheduleTaskService = scheduleTaskService;
        }

        /// <summary>
        /// Senkronizasyon görevini yoksa ekler, pasifse aktifleştirir.
        /// Ödeme ekranına her girişte çalışması garanti eder (basit güvence mekanizması).
        /// </summary>
        private async Task EnsureSyncTaskEnabledAsync()
        {
            var task = await _scheduleTaskService.GetTaskByTypeAsync(GarantiPosDefault.SynchronizationTask);
            if (task == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
                {
                    Enabled = true,
                    Name = GarantiPosDefault.SynchronizationTaskName,
                    Seconds = GarantiPosDefault.DefaultSynchronizationPeriod * 60 * 60,
                    Type = GarantiPosDefault.SynchronizationTask
                });
                return;
            }

            if (!task.Enabled)
            {
                task.Enabled = true;
                task.Seconds = GarantiPosDefault.DefaultSynchronizationPeriod * 60 * 60;
                await _scheduleTaskService.UpdateTaskAsync(task);
            }
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            await EnsureSyncTaskEnabledAsync();

            var settings = await _settingService.LoadSettingAsync<GarantiPosSettings>(0);
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            var cartItems = (await _shoppingCartService.GetShoppingCartAsync(
                currentCustomer,
                ShoppingCartType.ShoppingCart,
                store.Id)).ToList();

            // Komisyon dahil toplam (Helper üzerinden taksit senaryolarında doğru hesaplama)
            var orderTotal = await HelperOptions.GetOrderTotalAsync(
                _shoppingCartService,
                _workContext,
                _storeContext,
                _orderTotalCalculationService,
                _currencyService,
                withCommission: true,
                convertCurrency: true);

            var currency = await _workContext.GetWorkingCurrencyAsync();

            var model = new PaymentInfoModel
            {
                Installment = settings.Installment,
                NumberOfInstallment = 1,
                Total = orderTotal,
                Currency = currency.CurrencyCode
            };

            var session = _httpContextAccessor.HttpContext.Session;
            var paymentOrder = await SessionExt.GetAsync<PaymentGarantiOrder>(session, "PaymentOrder");

            if (paymentOrder != null)
            {
                // Var olan geçici kayıt güncellenir.
                paymentOrder.CustomerId = currentCustomer.Id;
                paymentOrder.Email = currentCustomer.Email;
                paymentOrder.PaidPrice = orderTotal;
                paymentOrder.Price = orderTotal;
                paymentOrder.BasketId = currency.CurrencyCode;

                await _paymentPosService.UpdatePosOrderAsync(paymentOrder);
                await SessionExt.SetAsync(session, "PaymentOrder", paymentOrder);
            }
            else
            {
                // Yeni geçici ödeme kaydı oluşturulur.
                paymentOrder = new PaymentGarantiOrder
                {
                    CustomerId = currentCustomer.Id,
                    Email = currentCustomer.Email,
                    PaidPrice = orderTotal,
                    Price = orderTotal,
                    BasketId = currency.CurrencyCode
                };
                paymentOrder.MarkAsCreated();

                await _paymentPosService.InsertPosOrderAsync(paymentOrder);

                // Sepet kalemleri PaymentGarantiOrderItem olarak saklanır (raporlama / hata ayıklama için).
                foreach (var item in cartItems)
                {
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    var paymentItem = new PaymentGarantiOrderItem
                    {
                        PaymentOrderId = paymentOrder.Id,
                        CreatedOnUtc = DateTime.UtcNow,
                        PaidPrice = product.Price,
                        Price = product.Price,
                        ProductId = product.Id,
                        PaymentTransactionId = string.Empty,
                        Type = string.Empty
                    };
                    await _paymentGarantiOrderItemRepository.InsertAsync(paymentItem, true);
                }

                await SessionExt.SetAsync(session, "PaymentOrder", paymentOrder);
            }

            // POST isteğinde form verilerini modele al – validation controller aşamasında yapılır.
            if (Request.Method != HttpMethods.Get)
            {
                var form = Request.Form;
                model.CardholderName = form["CardholderName"];
                model.CardNumber = form["CardNumber"];
                model.CardCode = form["CardCode"];
                model.ExpireYear = form["ExpirationYear"];
                model.ExpireMonth = form["ExpirationMonth"];
            }

            return View<PaymentInfoModel>("~/Plugins/Payments.GarantiPos/Views/PaymentInfo.cshtml", model);
        }
    }
}
