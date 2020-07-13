using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.Delta;
using Nop.Plugin.Api.Factories;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Api.Services;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Stripe;
using System.Net;
using Nop.Plugin.Api.Models.Payment;
using Nop.Plugin.Api.JSON.Serializers;

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

        public PaymentController(ICategoryApiService categoryApiService,
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
            IDTOHelper dtoHelper) : base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService, localizationService,pictureService)
        {
            _categoryApiService = categoryApiService;
            _categoryService = categoryService;
            _urlRecordService = urlRecordService;
            _factory = factory;
            _dtoHelper = dtoHelper;
        }

        [HttpPost]
        [Route("/api/place_payment_by_customer_id")]
        public virtual IActionResult PlacePaymentByCustomerId([ModelBinder(typeof(JsonModelBinder<PaymentModel>))] Delta<PaymentModel> paymentModel)
        {
            StripeConfiguration.ApiKey = "sk_test_jqqXw7n0PjEISJ9gbu0YadNx";

            string OrderId = "abcorder";

            ChargeCreateOptions chargeOptions = new ChargeCreateOptions
            {
                Amount = paymentModel.Dto.Amount * 100,
                Currency = paymentModel.Dto.Currency,
                Source = paymentModel.Dto.StripeToken,
                Description = paymentModel.Dto.Description,
                ReceiptEmail = paymentModel.Dto.ReceiptEmail,
                //Customer = paymentModel.Dto.CustomerId
            };

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            ChargeService service = new ChargeService();

            try
            {
                var stripeCharge = service.Create(chargeOptions);
            }
            catch (StripeException stripeException)
            {
                return BadRequest(stripeException.Message);
            }

            return Ok(OrderId);
        }
    }
}