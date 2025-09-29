using Nop.Web.Framework.Models;

public record InstallmentViewModel : BaseNopModel
{
	public class InstallmentItem
	{
		public string Text { get; set; }

		public int Installment { get; set; }

		public decimal Rate { get; set; }

		public string Amount { get; set; }

		public decimal AmountValue { get; set; }

		public string TotalAmount { get; set; }

		public decimal TotalAmountValue { get; set; }
	}

	public decimal TotalAmount { get; set; }

	public string CardAssociation { get; set; }

	public string BinNumber { get; set; }

	public string CardFamily { get; set; }

	public string CardType { get; set; }

	public string ErrorMessage { get; set; }

	public List<InstallmentItem> InstallmentItems { get; set; } = new();

	public string InfoMessage { get; set; }

	public string Message { get; set; }

	public void AddCashRate(decimal totalAmount, string text)
	{
		InstallmentItems.Add(new InstallmentItem
		{
			Text = text,
			Installment = 1,
			Amount = totalAmount.ToString("N2"),
			AmountValue = totalAmount,
			TotalAmount = totalAmount.ToString("N2"),
			TotalAmountValue = totalAmount
		});
	}
}
