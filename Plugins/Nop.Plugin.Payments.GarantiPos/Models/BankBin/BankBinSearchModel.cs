using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.GarantiPos.Models.BankBin;

public record BankBinSearchModel : BaseSearchModel
{
    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.BinNumber")]
    public string BinNumber { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.BankCode")]
    public string BankCode { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.CardType")]
    public string CardType { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.Product")]
    public string Product { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.CardAssociation")]
    public string CardAssociation { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.BankName")]
    public string BankName { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.InstallmentInd")]
    public string InstallmentInd { get; set; }
}
