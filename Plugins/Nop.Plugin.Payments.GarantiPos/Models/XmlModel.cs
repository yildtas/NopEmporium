using System;

public class XmlModel
{
	public string ApiVersion { get; set; }

	public string TerminalProvUserId { get; set; }

	public string TxnType { get; set; }

	public string Amount { get; set; }

	public int CurrencyCode { get; set; }

	public string ModeEnum { get; set; }

	public string InstallmentCount { get; set; }

	public string TerminalUserId { get; set; }

	public string OrderId { get; set; }

	public string CustomerIp { get; set; }

	public string CustomerEmail { get; set; }

	public string TerminalId { get; set; }

	public string TerminalMerchantId { get; set; }

	public string StoreKey { get; set; }

	public string ProvisionPassword { get; set; }

	public string SuccessUrl { get; set; }

	public string ErrorrUrl { get; set; }

	public string CompanyName { get; set; }

	public string Language { get; set; }

	public string RefreshTime { get; set; }

	// Garanti dokümantasyonuna göre MOTO işlemi değil -> her zaman 'N'
	public string MotoInd => "N";

	// Banka tarafında timestamp genellikle string olarak kullanılır
	public string TimeStamp => DateTime.Now.ToString();

	public string SecurityData { get; set; }

	public string HashData { get; set; }

	public string PaymentUrl { get; set; }

	public string SecurityLevel { get; set; }

	public string Html { get; set; }

	public string CardholderName { get; set; }

	public string CardNumber { get; set; }

	public string ExpireMonth { get; set; }

	public string ExpireYear { get; set; }

	public string CardCode { get; set; }
}
