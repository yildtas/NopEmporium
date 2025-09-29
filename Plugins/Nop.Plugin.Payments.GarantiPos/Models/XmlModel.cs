// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// Nop.Plugin.Payments.GarantiPos, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// Nop.Plugin.Payments.GarantiPos.Models.XmlModel
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

	public string MotoInd
	{
		get
		{
			return "N";
		}
		protected set
		{
			MotoInd = value;
		}
	}

	public string TimeStamp
	{
		get
		{
			return DateTime.Now.ToString();
		}
		protected set
		{
			TimeStamp = value;
		}
	}

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
