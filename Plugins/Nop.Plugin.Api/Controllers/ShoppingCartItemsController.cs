using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.Constants;
using Nop.Plugin.Api.Delta;
using Nop.Plugin.Api.DTOs.ShoppingCarts;
using Nop.Plugin.Api.Factories;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Api.Models.ShoppingCartsParameters;
using Nop.Plugin.Api.Services;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
namespace Nop.Plugin.Api.Controllers
{
    [ApiAuthorize(Policy = JwtBearerDefaults.AuthenticationScheme, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ShoppingCartItemsController : BaseApiController
    {
        private readonly IShoppingCartItemApiService _shoppingCartItemApiService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductService _productService;
        private readonly IFactory<ShoppingCartItem> _factory;
        private readonly IProductAttributeConverter _productAttributeConverter;
        private readonly IDTOHelper _dtoHelper;
        private readonly IStoreContext _storeContext;

        public ShoppingCartItemsController(IShoppingCartItemApiService shoppingCartItemApiService,
            IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IShoppingCartService shoppingCartService,
            IProductService productService,
            IFactory<ShoppingCartItem> factory,
            IPictureService pictureService,
            IProductAttributeConverter productAttributeConverter,
            IDTOHelper dtoHelper,
            IStoreContext storeContext)
            : base(jsonFieldsSerializer,
                 aclService,
                 customerService,
                 storeMappingService,
                 storeService,
                 discountService,
                 customerActivityService,
                 localizationService,
                 pictureService)
        {
            _shoppingCartItemApiService = shoppingCartItemApiService;
            _shoppingCartService = shoppingCartService;
            _productService = productService;
            _factory = factory;
            _productAttributeConverter = productAttributeConverter;
            _dtoHelper = dtoHelper;
            _storeContext = storeContext;
        }

        [HttpPost]
        [Route("/api/add_shopping_cart_items")]
        public IActionResult CreateShoppingCartItem([ModelBinder(typeof(JsonModelBinder<ShoppingCartItemDto>))] Delta<ShoppingCartItemDto> shoppingCartItem)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }

            var newShoppingCartItem = _factory.Initialize();
            shoppingCartItem.Merge(newShoppingCartItem);

            // We know that the product id and customer id will be provided because they are required by the validator.
            // TODO: validate
            var product = _productService.GetProductById(newShoppingCartItem.ProductId);

            if (product == null)
            {
                return Error(HttpStatusCode.NotFound, "product", "not found");
            }

            var customer = CustomerService.GetCustomerById(newShoppingCartItem.CustomerId);

            if (customer == null)
            {
                return Error(HttpStatusCode.NotFound, "customer", "not found");
            }

            var shoppingCartType = (ShoppingCartType)Enum.Parse(typeof(ShoppingCartType), shoppingCartItem.Dto.ShoppingCartType);

            if (!product.IsRental)
            {
                newShoppingCartItem.RentalStartDateUtc = null;
                newShoppingCartItem.RentalEndDateUtc = null;
            }

            var attributesXml = _productAttributeConverter.ConvertToXml(shoppingCartItem.Dto.Attributes, product.Id);

            var currentStoreId = _storeContext.CurrentStore.Id;

            var warnings = _shoppingCartService.AddToCart(customer, product, shoppingCartType, currentStoreId, attributesXml, 0M,
                                        newShoppingCartItem.RentalStartDateUtc, newShoppingCartItem.RentalEndDateUtc,
                                        shoppingCartItem.Dto.Quantity ?? 1);

            if (warnings.Count > 0)
            {
                foreach (var warning in warnings)
                {
                    ModelState.AddModelError("shopping cart item", warning);
                }

                return Error(HttpStatusCode.BadRequest);
            }
            else
            {
                // the newly added shopping cart item should be the last one
                newShoppingCartItem = customer.ShoppingCartItems.LastOrDefault();
            }

            string json = JsonConvert.SerializeObject(shoppingCartItem.Dto, Formatting.Indented,
                            new JsonSerializerSettings()
                            {
                                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                            });

            return new RawJsonActionResult(json);
        }
    }
}