using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Delta;
using Nop.Plugin.Api.Factories;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Api.Models;
using Nop.Plugin.Api.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace Nop.Plugin.Api.Controllers
{
    [ApiAuthorize(Policy = JwtBearerDefaults.AuthenticationScheme, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TokenController : BaseApiController
    {
        private readonly ICustomerApiService _customerApiService;
        private readonly ICustomerRolesHelper _customerRolesHelper;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IEncryptionService _encryptionService;
        private readonly ICountryService _countryService;
        private readonly IMappingHelper _mappingHelper;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly ILanguageService _languageService;
        private readonly IFactory<Customer> _factory;
        private readonly ICustomerService _customerService;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IAuthApiService _authApiService;
        private readonly ISettingService _settingService;

        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        private readonly SecurityKey _securityKey;
        private readonly SigningCredentials _signingCredentials;
        private readonly JwtHeader _jwtHeader;


        public TokenController(
            ICustomerApiService customerApiService,
            IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            ICustomerRolesHelper customerRolesHelper,
            IGenericAttributeService genericAttributeService,
            IEncryptionService encryptionService,
            IFactory<Customer> factory,
            ICountryService countryService,
            IMappingHelper mappingHelper,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IPictureService pictureService,
            ILanguageService languageService,
            IRepository<Customer> customerRepository,
            IAuthApiService authApiService,
            ISettingService settingService) :
            base(jsonFieldsSerializer,
                aclService,
                customerService,
                storeMappingService,
                storeService,
                discountService,
                customerActivityService,
                localizationService,
                pictureService)
        {
            _customerApiService = customerApiService;
            _customerService = customerService;
            _factory = factory;
            _countryService = countryService;
            _mappingHelper = mappingHelper;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _languageService = languageService;
            _encryptionService = encryptionService;
            _genericAttributeService = genericAttributeService;
            _customerRolesHelper = customerRolesHelper;
            _customerRepository = customerRepository;
            _authApiService = authApiService;
            _settingService = settingService;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("/api/guest_login")]
        public IActionResult GuestLogin([ModelBinder(typeof(JsonModelBinder<TokenModel>))] Delta<TokenModel> tokenModel)
        {
            try
            {
                string apiUserName = _settingService.GetSettingByKey<string>("mobileapisettings.username");
                string apiPassword = _settingService.GetSettingByKey<string>("mobileapisettings.password");

                TokenModel model = new TokenModel();

                if (!(tokenModel.Dto.ApiUserName == apiUserName && tokenModel.Dto.ApiPassword == apiPassword))
                {
                    return Error(HttpStatusCode.Unauthorized, "Token", "invalid username or password");
                }

                Customer guestCustomer = _customerService.InsertGuestCustomer();
                string token = _authApiService.CreateAccessToken(guestCustomer);

                model.CustomerId = guestCustomer.Id;
                model.CustomerGuid = guestCustomer.CustomerGuid.ToString();
                model.Token = token;

                return Ok(model);
            }
            catch (System.Exception ex)
            {
                return Error(HttpStatusCode.BadRequest, "Exception", ex.Message);
            }
        }
    }
}