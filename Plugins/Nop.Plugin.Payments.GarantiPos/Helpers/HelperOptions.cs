
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Services.Directory;
using Nop.Services.Orders;
using System.Security.Cryptography;
using System.Text;

namespace Nop.Plugin.Payments.GarantiPos.Helpers;

public class HelperOptions
{
	private static readonly string[] MdStatusCodes = new string[4] { "1", "2", "3", "4" };

	public static async Task<decimal> GetOrderTotalAsync(IShoppingCartService shoppingCartService, IWorkContext workContext, IStoreContext storeContext, IOrderTotalCalculationService orderTotalCalculationService, ICurrencyService currencyService, bool withCommission, bool convertCurrency)
	{
		Customer val = await workContext.GetCurrentCustomerAsync();
		ShoppingCartType? val2 = (ShoppingCartType)1;
		List<ShoppingCartItem> shoppingCart = (await shoppingCartService.GetShoppingCartAsync(val, val2, ((BaseEntity)(await storeContext.GetCurrentStoreAsync())).Id, (int?)null, (DateTime?)null, (DateTime?)null)).ToList();
		bool flag = withCommission;
		decimal orderTotal = Math.Round((await orderTotalCalculationService.GetShoppingCartTotalAsync((IList<ShoppingCartItem>)shoppingCart, (bool?)null, flag)).Item1.GetValueOrDefault(), 2);
		Currency currency = await workContext.GetWorkingCurrencyAsync();
		if (orderTotal != 0m && convertCurrency)
		{
			return Math.Round(await currencyService.ConvertFromPrimaryStoreCurrencyAsync(orderTotal, currency), 2);
		}
		return orderTotal;
	}

	public static string EncodeExpireMonth(int month)
	{
		string text = ((month.ToString().Length == 1) ? ("0" + month) : month.ToString());
		return text ?? "";
	}

	public static string EncodeExpireYear(int year)
	{
		if (year.ToString().Length != 4)
		{
			throw new ArgumentException("The length of the year is not 4.", "year");
		}
		string text = year.ToString().Substring(2);
		return text ?? "";
	}

	public static int GetCurrencyCode(string currencyCode)
	{
		if (currencyCode == null)
		{
			throw new ArgumentNullException("currencyCode");
		}
		int result = 949;
		switch (currencyCode)
		{
		case "TRY":
			result = 949;
			break;
		case "USD":
			result = 840;
			break;
		case "EUR":
			result = 978;
			break;
		case "GBP":
			result = 826;
			break;
		case "JPY":
			result = 392;
			break;
		case "CAD":
			result = 124;
			break;
		case "DKK":
			result = 208;
			break;
		}
		return result;
	}

	public static string Sha1(string text)
	{
		EncodingProvider ınstance = CodePagesEncodingProvider.Instance;
		Encoding.RegisterProvider(ınstance);
		SHA1CryptoServiceProvider sHA1CryptoServiceProvider = new SHA1CryptoServiceProvider();
		byte[] array = sHA1CryptoServiceProvider.ComputeHash(Encoding.GetEncoding("ISO-8859-9").GetBytes(text));
		StringBuilder stringBuilder = new StringBuilder();
		byte[] array2 = array;
		foreach (byte value in array2)
		{
			stringBuilder.Append($"{value,2:x}".Replace(" ", "0"));
		}
		return stringBuilder.ToString().ToUpper();
	}

	public static string Sha512(string text)
	{
		EncodingProvider ınstance = CodePagesEncodingProvider.Instance;
		Encoding.RegisterProvider(ınstance);
		SHA512CryptoServiceProvider sHA512CryptoServiceProvider = new SHA512CryptoServiceProvider();
		byte[] array = sHA512CryptoServiceProvider.ComputeHash(Encoding.GetEncoding("ISO-8859-9").GetBytes(text));
		StringBuilder stringBuilder = new StringBuilder();
		byte[] array2 = array;
		foreach (byte value in array2)
		{
			stringBuilder.Append($"{value,2:x}".Replace(" ", "0"));
		}
		return stringBuilder.ToString().ToUpper();
	}

	public static string GetHashData(string provisionPassword, string terminalId, string orderId, int installmentCount, string storeKey, ulong amount, int currencyCode, string successUrl, string type, string errorUrl)
	{
		string text = Sha1(provisionPassword + "0" + terminalId);
		return Sha512(terminalId + orderId + amount + currencyCode + successUrl + errorUrl + type + installmentCount + storeKey + text).ToUpper();
	}

	public static string IsRequireZero(string id, int complete)
	{
		string text = id.Trim();
		if (text.Length < complete)
		{
			for (int i = 0; i < complete - text.Length; i++)
			{
				id = id.Insert(0, "0");
			}
		}
		return id;
	}
}
