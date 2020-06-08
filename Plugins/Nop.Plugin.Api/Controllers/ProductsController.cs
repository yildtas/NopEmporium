using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Constants;
using Nop.Plugin.Api.Delta;
using Nop.Plugin.Api.DTOs.Images;
using Nop.Plugin.Api.DTOs.Products;
using Nop.Plugin.Api.Factories;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Api.Models.ProductsParameters;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Nop.Plugin.Api.Controllers
{
    using DTOs.Errors;
    using JSON.Serializers;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Nop.Core;
    using Nop.Core.Domain.Media;
    using Nop.Plugin.Api.Models.Catalog;

    [ApiAuthorize(Policy = JwtBearerDefaults.AuthenticationScheme, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProductsController : BaseApiController
    {
        private readonly IProductApiService _productApiService;
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IFactory<Product> _factory;
        private readonly IProductTagService _productTagService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IDTOHelper _dtoHelper;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ICategoryService _categoryService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly CatalogSettings _catalogSettings;
        private readonly MediaSettings _mediaSettings;

        public ProductsController(IProductApiService productApiService,
                                  IJsonFieldsSerializer jsonFieldsSerializer,
                                  IProductService productService,
                                  IUrlRecordService urlRecordService,
                                  ICustomerActivityService customerActivityService,
                                  ILocalizationService localizationService,
                                  IFactory<Product> factory,
                                  IAclService aclService,
                                  IStoreMappingService storeMappingService,
                                  IStoreService storeService,
                                  ICustomerService customerService,
                                  IDiscountService discountService,
                                  IPictureService pictureService,
                                  IManufacturerService manufacturerService,
                                  IProductTagService productTagService,
                                  IProductAttributeService productAttributeService,
                                  IDTOHelper dtoHelper,
                                  ICategoryService categoryService,
                                  IHttpContextAccessor httpContextAccessor,
                                  IStoreContext storeContext,
                                  IWorkContext workContext,
                                  CatalogSettings catalogSettings,
                                  MediaSettings mediaSettings) : base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService, localizationService, pictureService)
        {
            _productApiService = productApiService;
            _factory = factory;
            _manufacturerService = manufacturerService;
            _productTagService = productTagService;
            _urlRecordService = urlRecordService;
            _productService = productService;
            _productAttributeService = productAttributeService;
            _dtoHelper = dtoHelper;
            _storeMappingService = storeMappingService;
            _aclService = aclService;
            _categoryService = categoryService;
            _httpContextAccessor = httpContextAccessor;
            _storeContext = storeContext;
            _catalogSettings = catalogSettings;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
        }

        [HttpPost]
        [Route("/api/products_search")]
        public IActionResult GetProductsByTerm([ModelBinder(typeof(JsonModelBinder<ProductDetailsModel>))] Delta<ProductDetailsModel> command)
        {
            if (string.IsNullOrWhiteSpace(command.Dto.Term) || command.Dto.Term.Length < _catalogSettings.ProductSearchTermMinimumLength)
                return Content("");

            //products
            var productNumber = _catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0 ?
                _catalogSettings.ProductSearchAutoCompleteNumberOfProducts : 10;

            var products = _productService.SearchProducts(
                storeId: _storeContext.CurrentStore.Id,
                keywords: command.Dto.Term,
                languageId: _workContext.WorkingLanguage.Id,
                visibleIndividuallyOnly: true,
                pageSize: productNumber);

            var showLinkToResultSearch = _catalogSettings.ShowLinkToAllResultInSearchAutoComplete && (products.TotalCount > productNumber);

            var models = _productApiService.PrepareProductOverviewModels(products, true, _catalogSettings.ShowProductImagesInSearchAutoComplete, _mediaSettings.AutoCompleteSearchThumbPictureSize).ToList();
            var result = (from p in models
                          select new
                          {
                              Id = p.Id,
                              Label = p.Name,
                              ShortDescription = p.ShortDescription,
                              FullDescription = p.FullDescription,
                              SeName = p.SeName,
                              Sku = p.Sku,
                              ProductUrl = Url.RouteUrl("Product", new { SeName = p.SeName }),
                              ShowLinkToResultSearch = showLinkToResultSearch,
                              ProductPrice = p.ProductPrice,
                              ProductPicture = p.DefaultPictureModel,
                              ProductTags = p.ProductTags,
                              ThumbImageUrl = p.DefaultPictureModel == null ? string.Empty : p.DefaultPictureModel.ThumbImageUrl
                          })
                .ToList();

            string json = JsonConvert.SerializeObject(result);

            return new RawJsonActionResult(json);
        }

        [HttpPost]
        [Route("/api/products_by_id")]
        public IActionResult GetProductById([ModelBinder(typeof(JsonModelBinder<ProductDetailsModel>))] Delta<ProductDetailsModel> command)
        {
            if (command.Dto.ProductId <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "productId", "invalid productId");
            }

            var product = _productService.GetProductById(command.Dto.ProductId);
            if (product == null || product.Deleted)
            {
                return NotFound();
            }

            //model
            var model = _productApiService.PrepareProductDetailsModel(product);
           
            //template

            string json = JsonConvert.SerializeObject(model);

            return new RawJsonActionResult(json);
        }

        [HttpPost]
        [Route("/api/products_by_category_id")]
        public IActionResult GetProductsByCategoryId([ModelBinder(typeof(JsonModelBinder<CatalogPagingFilteringModel>))] Delta<CatalogPagingFilteringModel> command)
        {
            var category = _categoryService.GetCategoryById(command.Dto.CategoryId);
            if (category == null || category.Deleted)
                return NotFound();

            //model
            var model = _productApiService.PrepareCategoryModel(category, command.Dto);

            string json = JsonConvert.SerializeObject(model);

            return new RawJsonActionResult(json);
        }

        [HttpGet]
        [Route("/api/home_page_products")]
        public IActionResult GetHomePageProducts()
        {
            var products = _productService.GetAllProductsDisplayedOnHomepage();
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => _productService.ProductIsAvailable(p)).ToList();

            products = products.Where(p => p.VisibleIndividually).ToList();

            if (!products.Any())
                return Content("");

            var model = _productApiService.PrepareProductOverviewModels(products, true, true, null).ToList();

            var json = JsonConvert.SerializeObject(model);

            return new RawJsonActionResult(json);
        }

        /// <summary>
        /// Receive a list of all products
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/products")]
        [ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult GetProducts(ProductsParametersModel parameters)
        {
            if (parameters.Limit < Configurations.MinLimit || parameters.Limit > Configurations.MaxLimit)
            {
                return Error(HttpStatusCode.BadRequest, "limit", "invalid limit parameter");
            }

            if (parameters.Page < Configurations.DefaultPageValue)
            {
                return Error(HttpStatusCode.BadRequest, "page", "invalid page parameter");
            }

            var allProducts = _productApiService.GetProducts(parameters.Ids, parameters.CreatedAtMin, parameters.CreatedAtMax, parameters.UpdatedAtMin,
                                                                        parameters.UpdatedAtMax, parameters.Limit, parameters.Page, parameters.SinceId, parameters.CategoryId,
                                                                        parameters.VendorName, parameters.PublishedStatus)
                                                .Where(p => StoreMappingService.Authorize(p));

            IList<ProductDto> productsAsDtos = allProducts.Select(product => _dtoHelper.PrepareProductDTO(product)).ToList();

            var productsRootObject = new ProductsRootObjectDto()
            {
                Products = productsAsDtos
            };

            var json = JsonFieldsSerializer.Serialize(productsRootObject, parameters.Fields);

            return new RawJsonActionResult(json);
        }

    }
}