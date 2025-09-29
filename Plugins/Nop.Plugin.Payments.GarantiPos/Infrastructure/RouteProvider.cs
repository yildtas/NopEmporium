
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Plugin.Payments.GarantiPos;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.GarantiPos.Infrastructure;

public class RouteProvider : IRouteProvider
{
	public int Priority => 1000;

	public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
	{
		endpointRouteBuilder.MapControllerRoute(GarantiPosDefault.ConfigurationRouteName, "Plugins/PaymentGarantiPos/Configure", new
		{
			controller = "PaymentGarantiPos",
			action = "Configure",
			area = "Admin"
		});
		endpointRouteBuilder.MapControllerRoute("GetInstallment", "Plugins/PaymentGarantiPos/GetInstallment", new
		{
			controller = "PaymentGarantiPos",
			action = "GetInstallment"
		});
		endpointRouteBuilder.MapControllerRoute("Plugins.Payments.GarantiPos.Success", "Plugins/PaymentGarantiPos/Success", new
		{
			controller = "PaymentGarantiPos",
			action = "Success"
		});
		endpointRouteBuilder.MapControllerRoute("Plugins.Payments.GarantiPos.Cancel", "Plugins/PaymentGarantiPos/Cancel", new
		{
			controller = "PaymentGarantiPos",
			action = "Cancel"
		});
		endpointRouteBuilder.MapControllerRoute(GarantiPosDefault.CreateRouteName, "Admin/PaymentGarantiPos/Create", new
		{
			controller = "PaymentGarantiPos",
			action = "Create",
			area = "Admin"
		});
		endpointRouteBuilder.MapControllerRoute(GarantiPosDefault.EditRouteName, "Admin/PaymentGarantiPos/Edit/{id?}", new
		{
			controller = "PaymentGarantiPos",
			action = "Edit",
			area = "Admin"
		});
		endpointRouteBuilder.MapControllerRoute(GarantiPosDefault.DeleteRouteName, "Admin/PaymentGarantiPos/Delete/{id?}", new
		{
			controller = "PaymentGarantiPos",
			action = "Delete",
			area = "Admin"
		});
		endpointRouteBuilder.MapControllerRoute(GarantiPosDefault.ListRouteName, "Admin/PaymentGarantiPos/List", new
		{
			controller = "PaymentGarantiPos",
			action = "List",
			area = "Admin"
		});
		endpointRouteBuilder.MapControllerRoute(GarantiPosDefault.InstallmentCreate, "Admin/PaymentGarantiPos/InstallmentCreate", new
		{
			controller = "PaymentGarantiPos",
			action = "InstallmentCreate",
			area = "Admin"
		});
		endpointRouteBuilder.MapControllerRoute(GarantiPosDefault.InstallmentEdit, "Admin/PaymentGarantiPos/InstallmentEdit/{id?}", new
		{
			controller = "PaymentGarantiPos",
			action = "InstallmentEdit",
			area = "Admin"
		});
		endpointRouteBuilder.MapControllerRoute(GarantiPosDefault.InstallmentDelete, "Admin/PaymentGarantiPos/InstallmentDelete/{id?}", new
		{
			controller = "PaymentGarantiPos",
			action = "InstallmentDelete",
			area = "Admin"
		});
		endpointRouteBuilder.MapControllerRoute(GarantiPosDefault.InstallmentList, "Admin/PaymentGarantiPos/InstallmentList", new
		{
			controller = "PaymentGarantiPos",
			action = "InstallmentList",
			area = "Admin"
		});
		endpointRouteBuilder.MapControllerRoute(GarantiPosDefault.CategoryInstallmentCreate, "Admin/PaymentGarantiPos/CategoryInstallmentCreate", new
		{
			controller = "PaymentGarantiPos",
			action = "CategoryInstallmentCreate",
			area = "Admin"
		});
		endpointRouteBuilder.MapControllerRoute(GarantiPosDefault.CategoryInstallmentEdit, "Admin/PaymentGarantiPos/CategoryInstallmentEdit/{id?}", new
		{
			controller = "PaymentGarantiPos",
			action = "CategoryInstallmentEdit",
			area = "Admin"
		});
		endpointRouteBuilder.MapControllerRoute(GarantiPosDefault.CategoryInstallmentDelete, "Admin/PaymentGarantiPos/CategoryInstallmentDelete/{id?}", new
		{
			controller = "PaymentGarantiPos",
			action = "CategoryInstallmentDelete",
			area = "Admin"
		});
		endpointRouteBuilder.MapControllerRoute(GarantiPosDefault.CategoryInstallmentList, "Admin/PaymentGarantiPos/CategoryInstallmentList", new
		{
			controller = "PaymentGarantiPos",
			action = "CategoryInstallmentList",
			area = "Admin"
		});
	}
}
