using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

public record ConfigurationModel : BaseNopModel
{
    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.TerminalId")]
    public string TerminalId { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.MerchantId")]
    public string MerchantId { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.StoreKey")]
    public string StoreKey { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.Password")]
    public string Password { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.CompanyName")]
    public string CompanyName { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.Email")]
    public string Email { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.TestMode")]
    public bool TestMode { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.Bank3DUrl")]
    public string Bank3DUrl { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.BankNone3DUrl")]
    public string BankNone3DUrl { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.Version")]
    public string Version { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.TerminalUserId")]
    public string TerminalUserId { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.TerminalProvUserId")]
    public string TerminalProvUserId { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.Installment")]
    public bool Installment { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.SecurityLevel")]
    public string SecurityLevel { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.AdditionalFee")]
    public decimal AdditionalFee { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.AdditionalFeePercentage")]
    public bool AdditionalFeePercentage { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.NopCommerceNumber")]
    public string NopCommerceNumber { get; set; }

    [NopResourceDisplayName("Plugins.Payments.GarantiPos.Field.Enable")]
    public bool Enable { get; set; }
}
