
using System;
using Nop.Core;

namespace Nop.Plugin.Payments.GarantiPos.Domains;

public class PaymentGarantiOrderItem : BaseEntity
{
	public int PaymentOrderId { get; set; }

	public string PaymentTransactionId { get; set; }

	public int ProductId { get; set; }

	public decimal? Price { get; set; }

	public decimal? PaidPrice { get; set; }

	public string Type { get; set; }

	public DateTime CreatedOnUtc { get; set; }
}
