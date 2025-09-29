using Nop.Core;

namespace Nop.Plugin.Payments.GarantiPos.Domains;

/// <summary>
/// Banka genel taksit oran� tan�m�. Kategori �zel oran yoksa bu tablo kullan�l�r.
/// </summary>
public class PaymentGarantiInstallment : BaseEntity
{
	/// <summary>Taksit say�s�.</summary>
	public int Installment { get; set; }

	/// <summary>Komisyon oran� (%). 0 ise vade fark� yok.</summary>
	public decimal Rate { get; set; }
}
