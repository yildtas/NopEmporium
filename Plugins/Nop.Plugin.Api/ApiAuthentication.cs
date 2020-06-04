namespace Nop.Plugin.Api
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.Extensions.DependencyInjection;
    using Nop.Plugin.Api.Helpers;
    using Nop.Services.Authentication.External;
    using System.Collections.Generic;

    public class ApiAuthentication : IExternalAuthenticationRegistrar
    {
        public void Configure(AuthenticationBuilder builder)
        {
            //RsaSecurityKey signingKey = CryptoHelper.CreateRsaSecurityKey();

            //builder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwt =>
            //    {
            //        jwt.Audience = "nop_api";
            //        jwt.TokenValidationParameters = new TokenValidationParameters
            //        {
            //            ValidateActor = false,
            //            ValidateIssuer = false,
            //            NameClaimType = JwtClaimTypes.Name,
            //            RoleClaimType = JwtClaimTypes.Role,
            //            // Uncomment this if you are using an certificate to sign your tokens.
            //            // IssuerSigningKey = new X509SecurityKey(cert),
            //            IssuerSigningKeyResolver = (string token, SecurityToken securityToken, string kid,
            //                    TokenValidationParameters validationParameters) =>
            //                   new List<RsaSecurityKey> { signingKey }
            //        };
            //    });
        }
    }
}