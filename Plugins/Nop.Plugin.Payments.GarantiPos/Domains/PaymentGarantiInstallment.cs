using Nop.Core;

namespace Nop.Plugin.Payments.GarantiPos.Domains;

/// <summary>
/// Banka genel taksit oraný tanýmý. Kategori özel oran yoksa bu tablo kullanýlýr.
/// </summary>
public class PaymentGarantiInstallment : BaseEntity
{
	/// <summary>Taksit sayýsý.</summary>
	public int Installment { get; set; }

	/// <summary>Komisyon oraný (%). 0 ise vade farký yok.</summary>
	public decimal Rate { get; set; }
}
