using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.GarantiPos.Services;

namespace Nop.Plugin.Payments.GarantiPos.Infrastructure;

public class NopStartup : INopStartup
{
	public int Order => 11;

	public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddScoped<IBankBinService, BankBinService>();
		services.AddScoped<IPaymentPosService, PaymentPosService>();
		// Removed ILicenseService registration after deleting LicenceProtected folder
	}

	public void Configure(IApplicationBuilder application)
	{
	}
}
