using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.GarantiPos.Models.Category;

public record CategoryInstallmentModel : BaseNopEntityModel
{
	[NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.Category")]
	public int CategoryId { get; set; }

	public IList<SelectListItem> AvailableCategories { get; set; } = new List<SelectListItem>();

	[NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.CategoryName")]
	public string CategoryName { get; set; }

	[NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.Installment")]
	public int Installment { get; set; }

	[NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.Rate")]
	public decimal Rate { get; set; }
}
