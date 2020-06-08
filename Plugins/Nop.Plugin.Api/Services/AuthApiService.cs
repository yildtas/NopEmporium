using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.Constants;
using Nop.Plugin.Api.DataStructures;
using Nop.Plugin.Api.Extensions;
using Nop.Services.Stores;

namespace Nop.Plugin.Api.Services
{
    public class AuthApiService : IAuthApiService
    {
        private readonly IStoreMappingService _storeMappingService;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<ProductCategory> _productCategoryMappingRepository;
        private DateTime _accessTokenExpiration;

        public AuthApiService(IRepository<Category> categoryRepository,
            IRepository<ProductCategory> productCategoryMappingRepository,
            IStoreMappingService storeMappingService)
        {
            _categoryRepository = categoryRepository;
            _productCategoryMappingRepository = productCategoryMappingRepository;
            _storeMappingService = storeMappingService;
        }

        public string CreateAccessToken(Customer customer)
        {
            List<string> claims = new List<string> { "Supplier" };
            _accessTokenExpiration = DateTime.Now.AddMinutes(180);
            var securityKey = CreateSecurityKey("JJ}jrw,L/[`H:[nF)(-Lp#z8?L%2zGBz");
            var signinCredentials = CreateSigningCredentials(securityKey);

            var jwt = CreateJwtSecurityToken(customer, signinCredentials, claims);
            var jwtSecurityTokenHanler = new JwtSecurityTokenHandler();
            var token = jwtSecurityTokenHanler.WriteToken(jwt);

            return token;
        }


        private SecurityKey CreateSecurityKey(string key)
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        }

        private SigningCredentials CreateSigningCredentials(SecurityKey securityKey)
        {
            return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
        }

        private JwtSecurityToken CreateJwtSecurityToken(Customer customer, SigningCredentials signingCredentials, List<string> operationClaims)
        {
            var jwt = new JwtSecurityToken(
                issuer: "Issuer",
                audience: "Audience",
                expires: _accessTokenExpiration,
                notBefore: DateTime.Now,
                claims: SetClaims(customer, operationClaims),
                signingCredentials: signingCredentials
                );
            return jwt;
        }

        private IEnumerable<Claim> SetClaims(Customer customer, List<string> operationClaims)
        {
            List<Claim> claims = new List<Claim>();
            claims.AddNameIdentifier(customer.Id.ToString());
            claims.AddGuid(customer.CustomerGuid.ToString());
            claims.AddRoles(operationClaims.ToArray());
            return claims;
        }
    }
}