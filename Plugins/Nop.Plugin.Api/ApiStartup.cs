﻿using Nop.Plugin.Api.Data;
using Nop.Web.Framework.Infrastructure.Extensions;

namespace Nop.Plugin.Api
{
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;
    using Nop.Core.Data;
    using Nop.Core.Infrastructure;
    using Nop.Plugin.Api.Helpers;
    using Nop.Web.Framework.Infrastructure;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerUI;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    public class ApiStartup : INopStartup
    {
        private const string ObjectContextName = "nop_object_context_web_api";

        // TODO: extract all methods into extensions.
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApiObjectContext>(optionsBuilder =>
            {
                optionsBuilder.UseSqlServerWithLazyLoading(services);
            });

            services.AddHttpContextAccessor();

            AddRequiredConfiguration();

            AddTokenGenerationPipeline(services);

            //AddAuthorizationPipeline(services);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Nop API", Version = "v1" });

                // Swagger 2.+ support
                var security = new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] { }},
                };

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
                c.AddSecurityRequirement(security);
            });
        }

        public void Configure(IApplicationBuilder app)
        {

            app.UseHttpsRedirection();

            app.UseAuthentication();

            // During a clean install we should not register any middlewares i.e IdentityServer as it won't be able to create its  
            // tables without a connection string and will throw an exception
            //var dataSettings = DataSettingsManager.LoadSettings();
            //if (!dataSettings?.IsValid ?? true)
            //    return;

            // The default route templates for the Swagger docs and swagger - ui are "swagger/docs/{apiVersion}" and "swagger/ui/index#/{assetPath}" respectively.
            app.UseSwagger(option => { option.RouteTemplate = "swagger/{documentName}/swagger.json"; });
            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = "swagger";// add your virtual path here.
                options.SwaggerEndpoint("../swagger/v1/swagger.json", "Nop API");
                //options.DocExpansion(DocExpansion.None);
            });

            // This needs to be called here because in the plugin install method identity server is not yet registered.
            //ApplyIdentityServerMigrations(app);

            //SeedData(app);

            //var rewriteOptions = new RewriteOptions()
            //    .AddRewrite("api/token", "api/token", true);

            //app.UseRewriter(rewriteOptions);

            //app.UseMiddleware<IdentityServerScopeParameterMiddleware>();

            ////uncomment only if the client is an angular application that directly calls the oauth endpoint
            //app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            //UseIdentityServer(app);

            //need to enable rewind so we can read the request body multiple times (this should eventually be refactored, but both JsonModelBinder and all of the DTO validators need to read this stream)
            app.Use(async (context, next) =>
            {
                context.Request.EnableBuffering();
                await next();
            });

            app.UseHttpsRedirection();
            //app.UseCors("AllowMyOrigin");
            app.UseAuthentication();

            //app.UseAuthorization();

        }

        private void AddRequiredConfiguration()
        {

            var configManagerHelper = new NopConfigManagerHelper();

            // some of third party libaries that we use for WebHooks and Swagger use older versions
            // of certain assemblies so we need to redirect them to the once that nopCommerce uses
            //TODO: Upgrade 4.10 check this!
            //configManagerHelper.AddBindingRedirects();

            // required by the WebHooks support
            //TODO: Upgrade 4.10 check this!
            //configManagerHelper.AddConnectionString();           

            // This is required only in development.
            // It it is required only when you want to send a web hook to an https address with an invalid SSL certificate. (self-signed)
            // The code marks all certificates as valid.
            // We may want to extract this as a setting in the future.

            // NOTE: If this code is commented the certificates will be validated.
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        }

        //private void AddAuthorizationPipeline(IServiceCollection services)
        //{
        //    services.AddAuthorization(options =>
        //    {
        //        options.AddPolicy(JwtBearerDefaults.AuthenticationScheme,
        //            policy =>
        //            {
        //                policy.Requirements.Add(new ActiveApiPluginRequirement());
        //                policy.Requirements.Add(new AuthorizationSchemeRequirement());
        //                policy.Requirements.Add(new ActiveClientRequirement());
        //                policy.Requirements.Add(new RequestFromSwaggerOptional());
        //                policy.RequireAuthenticatedUser();
        //            });
        //    });

        //    services.AddSingleton<IAuthorizationHandler, ActiveApiPluginAuthorizationPolicy>();
        //    services.AddSingleton<IAuthorizationHandler, ValidSchemeAuthorizationPolicy>();
        //    services.AddSingleton<IAuthorizationHandler, ActiveClientAuthorizationPolicy>();
        //    services.AddSingleton<IAuthorizationHandler, RequestsFromSwaggerAuthorizationPolicy>();
        //}

        private void AddTokenGenerationPipeline(IServiceCollection services)
        {
            RsaSecurityKey signingKey = CryptoHelper.CreateRsaSecurityKey();

            DataSettings dataSettings = DataSettingsManager.LoadSettings();
            if (!dataSettings?.IsValid ?? true)
                return;

            string connectionStringFromNop = dataSettings.DataConnectionString;

            var migrationsAssembly = typeof(ApiStartup).GetTypeInfo().Assembly.GetName().Name;

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(cfg =>
            {
                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("JJ}jrw,L/[`H:[nF)(-Lp#z8?L%2zGBz")),
                    ValidateIssuer = false,
                    ValidIssuer = "",
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RequireExpirationTime = true
                };
            });

        }

        //private void SeedData(IApplicationBuilder app)
        //{
        //    using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
        //    {
        //        var configurationContext = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

        //        if (!configurationContext.ApiResources.Any())
        //        {
        //            // In the simple case an API has exactly one scope. But there are cases where you might want to sub-divide the functionality of an API, and give different clients access to different parts. 
        //            configurationContext.ApiResources.Add(new ApiResource()
        //            {
        //                Enabled = true,
        //                Scopes = new List<ApiScope>()
        //                {
        //                    new ApiScope()
        //                    {
        //                        Name = "nop_api",
        //                        DisplayName = "nop_api"
        //                    }
        //                },
        //                Name = "nop_api"
        //            });

        //            configurationContext.SaveChanges();

        //            TryRunUpgradeScript(configurationContext);
        //        }
        //    }
        //}

        private string LoadUpgradeScript()
        {
            var fileProvider = EngineContext.Current.Resolve<INopFileProvider>();
            string path = fileProvider.MapPath("~/Plugins/Nop.Plugin.Api/upgrade_script.sql");
            string script = File.ReadAllText(path);

            return script;
        }

        public void AddBindingRedirectsFallbacks()
        {
            // If no binding redirects are present in the config file then this will perform the binding redirect
            RedirectAssembly("Microsoft.AspNetCore.DataProtection.Abstractions", new Version(2, 0, 0, 0), "adb9793829ddae60");
        }

        ///<summary>Adds an AssemblyResolve handler to redirect all attempts to load a specific assembly name to the specified version.</summary>
        public static void RedirectAssembly(string shortName, Version targetVersion, string publicKeyToken)
        {
            ResolveEventHandler handler = null;

            handler = (sender, args) =>
            {
                // Use latest strong name & version when trying to load SDK assemblies
                var requestedAssembly = new AssemblyName(args.Name);
                if (requestedAssembly.Name != shortName)
                    return null;

                requestedAssembly.Version = targetVersion;
                requestedAssembly.SetPublicKeyToken(new AssemblyName("x, PublicKeyToken=" + publicKeyToken).GetPublicKeyToken());
                requestedAssembly.CultureInfo = CultureInfo.InvariantCulture;

                AppDomain.CurrentDomain.AssemblyResolve -= handler;

                return Assembly.Load(requestedAssembly);
            };
            AppDomain.CurrentDomain.AssemblyResolve += handler;
        }

        public int Order => new AuthenticationStartup().Order + 1;
    }
}
