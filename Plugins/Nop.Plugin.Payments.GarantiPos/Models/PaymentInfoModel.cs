using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.GarantiPos.Models;

/// <summary>
/// GarantiPos eklentisi ödeme bilgisi modeli
/// </summary>
public record PaymentInfoModel : BaseNopModel
{
	[NopResourceDisplayName("Payment.SelectCreditCard")]
	public string CreditCardType { get; set; }

	[NopResourceDisplayName("Payment.CardholderName")]
	public string CardholderName { get; set; }

	[NopResourceDisplayName("Payment.CardNumber")]
	public string CardNumber { get; set; }

	[NopResourceDisplayName("Payment.ExpirationDate")]
	public string ExpireMonth { get; set; }

	[NopResourceDisplayName("Payment.ExpirationDate")]
	public string ExpireYear { get; set; }

	[NopResourceDisplayName("Payment.CardCode")]
	public string CardCode { get; set; }

	public int NumberOfInstallment { get; set; }

	public bool Installment { get; set; }

	public decimal Total { get; set; }

	public string Currency { get; set; }
}
