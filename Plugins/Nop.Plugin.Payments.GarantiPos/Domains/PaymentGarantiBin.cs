
using Nop.Core;

namespace Nop.Plugin.Payments.GarantiPos.Domains;

public class PaymentGarantiBin : BaseEntity
{
	public string BinNumber { get; set; }

	public string CardType { get; set; }

	public string CardAssociation { get; set; }

	public string Product { get; set; }

	public string BankCode { get; set; }

	public string InstallmentInd { get; set; }

	public string BankName { get; set; }
}
