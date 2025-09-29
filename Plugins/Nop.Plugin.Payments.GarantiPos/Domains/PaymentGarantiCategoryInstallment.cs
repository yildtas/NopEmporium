using Nop.Core;

namespace Nop.Plugin.Payments.GarantiPos.Domains;

/// <summary>
/// Belirli bir kategoriye �zel taksit oran� tan�m�. �r�n sepetinde birden fazla kategori varsa �ncelik/�ak��ma kurallar� controller i�inde y�netilir.
/// </summary>
public class PaymentGarantiCategoryInstallment : BaseEntity
{
	/// <summary>Kategori Id.</summary>
	public int CategoryId { get; set; }

	/// <summary>Kategori ad� (g�r�nt�leme i�in kopyalanm��).</summary>
	public string CategoryName { get; set; }

	/// <summary>Taksit say�s�.</summary>
	public int Installment { get; set; }

	/// <summary>Komisyon oran� (%).</summary>
	public decimal Rate { get; set; }
}
