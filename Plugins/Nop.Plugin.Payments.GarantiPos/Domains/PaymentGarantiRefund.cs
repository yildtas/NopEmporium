
using System;
using Nop.Core;

namespace Nop.Plugin.Payments.GarantiPos.Domains;

public class PaymentGarantiRefund : BaseEntity
{
	public int CustomerId { get; set; }

	public int OrderId { get; set; }

	public string PaymentTransactionId { get; set; }

	public string PaymentId { get; set; }

	public decimal Amount { get; set; }

	public DateTime CreatedOnUtc { get; set; }
}
