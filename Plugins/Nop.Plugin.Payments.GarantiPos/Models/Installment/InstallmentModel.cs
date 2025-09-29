using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.GarantiPos.Models.Installment;

public record InstallmentModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.Installment")]
    public int Installment { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.Rate")]
    public decimal Rate { get; set; }
}
