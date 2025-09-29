using Nop.Core;

namespace Nop.Plugin.Payments.GarantiPos.Domains;

/// <summary>
/// Belirli bir kategoriye özel taksit oraný tanýmý. Ürün sepetinde birden fazla kategori varsa öncelik/çakýþma kurallarý controller içinde yönetilir.
/// </summary>
public class PaymentGarantiCategoryInstallment : BaseEntity
{
	/// <summary>Kategori Id.</summary>
	public int CategoryId { get; set; }

	/// <summary>Kategori adý (görüntüleme için kopyalanmýþ).</summary>
	public string CategoryName { get; set; }

	/// <summary>Taksit sayýsý.</summary>
	public int Installment { get; set; }

	/// <summary>Komisyon oraný (%).</summary>
	public decimal Rate { get; set; }
}
