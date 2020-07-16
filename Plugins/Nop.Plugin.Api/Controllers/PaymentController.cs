using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Http.Extensions;
using Nop.Plugin.Api.Delta;
using Nop.Plugin.Api.Factories;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Api.Models.Payment;
using Nop.Plugin.Api.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Stripe;
using System;
using System.Net;

namespace Nop.Plugin.Api.Controllers
{
    //[ApiAuthorize(Policy = JwtBearerDefaults.AuthenticationScheme, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PaymentController : BaseApiController
    {
        private readonly ICategoryApiService _categoryApiService;
        private readonly ICategoryService _categoryService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IFactory<Category> _factory;
        private readonly IDTOHelper _dtoHelper;
        private readonly ICustomerService _customerService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IPaymentOrderProcessingService _paymentOrderProcessingService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly PaymentSettings _paymentSettings;


        public PaymentController(
            PaymentSettings paymentSettings,
            IPaymentOrderProcessingService paymentOrderProcessingService,
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext,
            IShoppingCartService shoppingCartService,
            ICategoryApiService categoryApiService,
            IJsonFieldsSerializer jsonFieldsSerializer,
            ICategoryService categoryService,
            IUrlRecordService urlRecordService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            IAclService aclService,
            ICustomerService customerService,
            IFactory<Category> factory,
            IDTOHelper dtoHelper) :
            base(
                jsonFieldsSerializer, aclService,
                customerService,
                storeMappingService,
                storeService,
                discountService,
                customerActivityService,
                localizationService,
                pictureService)
        {
            _paymentSettings = paymentSettings;
            _categoryApiService = categoryApiService;
            _categoryService = categoryService;
            _urlRecordService = urlRecordService;
            _factory = factory;
            _dtoHelper = dtoHelper;
            _customerService = customerService;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _paymentOrderProcessingService = paymentOrderProcessingService;
            _genericAttributeService = genericAttributeService;
        }

        [HttpPost]
        [Route("/api/place_payment_by_customer_id")]
        public virtual IActionResult PlacePaymentByCustomerId([ModelBinder(typeof(JsonModelBinder<PaymentModel>))] Delta<PaymentModel> paymentModel)
        {
            StripeConfiguration.ApiKey = "sk_test_jqqXw7n0PjEISJ9gbu0YadNx";

            PlaceOrderResult orderResult = new PlaceOrderResult();
            PlaceOrderModalResult orderModalResult = new PlaceOrderModalResult();

            ChargeCreateOptions chargeOptions = new ChargeCreateOptions
            {
                Amount = paymentModel.Dto.Amount * 100,
                Currency = paymentModel.Dto.Currency,
                Source = paymentModel.Dto.StripeToken,
                Description = paymentModel.Dto.Description,
                ReceiptEmail = paymentModel.Dto.ReceiptEmail,
            };

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            ChargeService service = new ChargeService();

            try
            {
                Charge stripeCharge = service.Create(chargeOptions);

                if (stripeCharge.Paid)
                {
                    Nop.Core.Domain.Customers.Customer customer = _customerService.GetCustomerById(paymentModel.Dto.CustomerId);

                    var cart = _shoppingCartService.GetShoppingCart(customer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

                    ProcessPaymentRequest processPaymentRequest = new ProcessPaymentRequest();

                    GenerateOrderGuid(processPaymentRequest);
                    processPaymentRequest.StoreId = _storeContext.CurrentStore.Id;
                    processPaymentRequest.CustomerId = customer.Id;
                    processPaymentRequest.PaymentMethodSystemName = _genericAttributeService.GetAttribute<string>(customer,
                        NopCustomerDefaults.SelectedPaymentMethodAttribute, _storeContext.CurrentStore.Id);
                    HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", processPaymentRequest);

                    orderResult = _paymentOrderProcessingService.PlaceOrder(processPaymentRequest);

                    if (orderResult.Errors != null && orderResult.Errors.Count > 0)
                    {
                        orderModalResult.Errors = orderResult.Errors;
                        return BadRequest(orderModalResult);
                    }

                    orderModalResult.Id = orderResult.PlacedOrder.Id;
                    orderModalResult.OrderGuid = orderResult.PlacedOrder.OrderGuid.ToString();
                    orderModalResult.OrderSubtotalInclTax = orderResult.PlacedOrder.OrderSubtotalInclTax;
                    orderModalResult.OrderSubtotalExclTax = orderResult.PlacedOrder.OrderSubtotalExclTax;
                    orderModalResult.Email = orderResult.PlacedOrder.Customer.Email;
                    orderModalResult.OrderTotal = orderResult.PlacedOrder.OrderTotal;

                    //string json = JsonConvert.SerializeObject(orderResult, Formatting.Indented, new JsonSerializerSettings
                    //{
                    //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    //});

                    return Ok(orderModalResult);
                }
                else
                {
                    orderModalResult.AddError(stripeCharge.FailureMessage);
                    return BadRequest(orderModalResult);
                }

            }
            catch (StripeException stripeException)
            {
                orderModalResult.AddError(stripeException.Message);
                return BadRequest(orderModalResult);
            }
        }

        private void GenerateOrderGuid(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest == null)
                return;

            //we should use the same GUID for multiple payment attempts
            //this way a payment gateway can prevent security issues such as credit card brute-force attacks
            //in order to avoid any possible limitations by payment gateway we reset GUID periodically
            var previousPaymentRequest = HttpContext.Session.Get<ProcessPaymentRequest>("OrderPaymentInfo");
            if (_paymentSettings.RegenerateOrderGuidInterval > 0 &&
                previousPaymentRequest != null &&
                previousPaymentRequest.OrderGuidGeneratedOnUtc.HasValue)
            {
                var interval = DateTime.UtcNow - previousPaymentRequest.OrderGuidGeneratedOnUtc.Value;
                if (interval.TotalSeconds < _paymentSettings.RegenerateOrderGuidInterval)
                {
                    processPaymentRequest.OrderGuid = previousPaymentRequest.OrderGuid;
                    processPaymentRequest.OrderGuidGeneratedOnUtc = previousPaymentRequest.OrderGuidGeneratedOnUtc;
                }
            }

            if (processPaymentRequest.OrderGuid == Guid.Empty)
            {
                processPaymentRequest.OrderGuid = Guid.NewGuid();
                processPaymentRequest.OrderGuidGeneratedOnUtc = DateTime.UtcNow;
            }
        }

    }
}