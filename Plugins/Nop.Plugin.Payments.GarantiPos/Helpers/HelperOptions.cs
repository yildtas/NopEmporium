using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Directory;
using Nop.Services.Orders;
using System.Security.Cryptography;
using System.Text;

namespace Nop.Plugin.Payments.GarantiPos.Helpers;

public class HelperOptions
{
    //private static readonly string[] MdStatusCodes = new[] { "1", "2", "3", "4" };
    public static async Task<decimal> GetOrderTotalAsync(IShoppingCartService shoppingCartService, IWorkContext workContext, IStoreContext storeContext, IOrderTotalCalculationService orderTotalCalculationService, ICurrencyService currencyService, bool withCommission, bool convertCurrency)
    {
        var customer = await workContext.GetCurrentCustomerAsync();
        var cartType = ShoppingCartType.ShoppingCart;
        var storeId = (await storeContext.GetCurrentStoreAsync()).Id;
        var shoppingCart = (await shoppingCartService.GetShoppingCartAsync(customer, cartType, storeId)).ToList();

        var totalResult = await orderTotalCalculationService.GetShoppingCartTotalAsync(shoppingCart);
        var roundedBase = Math.Round(totalResult.Item1.GetValueOrDefault(), 2);

        // withCommission true ise payment method additional fee'yi manuel ekleyelim
        if (withCommission)
        {
            var fee = await orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(shoppingCart, 0m, false);
            roundedBase = Math.Round(roundedBase + fee, 2);
        }

        var currency = await workContext.GetWorkingCurrencyAsync();
        if (roundedBase != 0m && convertCurrency)
            return Math.Round(await currencyService.ConvertFromPrimaryStoreCurrencyAsync(roundedBase, currency), 2);

        return roundedBase;
    }

    public static string EncodeExpireMonth(int month)
    {
        var s = month.ToString().Length == 1 ? $"0{month}" : month.ToString();
        return s ?? string.Empty;
    }

    public static string EncodeExpireYear(int year)
    {
        if (year.ToString().Length != 4)
            throw new ArgumentException("The length of the year is not 4.", nameof(year));

        var s = year.ToString()[2..];
        return s ?? string.Empty;
    }

    public static int GetCurrencyCode(string currencyCode)
    {
        ArgumentNullException.ThrowIfNull(currencyCode);
        return currencyCode switch
        {
            "TRY" => 949,
            "USD" => 840,
            "EUR" => 978,
            "GBP" => 826,
            "JPY" => 392,
            "CAD" => 124,
            "DKK" => 208,
            _ => 949
        };
    }

    public static string Sha1Upper(string text)
    {
        using var sha1 = SHA1.Create();
        var bytes = Encoding.UTF8.GetBytes(text);// ASCII girdilerde UTF-8 ≡ ISO-8859-9
        var hash = sha1.ComputeHash(bytes);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash) sb.Append(b.ToString("X2")); // direkt uppercase hex
        return sb.ToString();
    }

    public static string Sha1(string text)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using var sha1 = SHA1.Create();
        var bytes = sha1.ComputeHash(Encoding.GetEncoding("ISO-8859-9").GetBytes(text));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
            sb.Append($"{b,2:x}".Replace(" ", "0"));
        return sb.ToString().ToUpperInvariant();
    }

    public static string Sha512(string text)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using var sha512 = SHA512.Create();
        var bytes = sha512.ComputeHash(Encoding.GetEncoding("ISO-8859-9").GetBytes(text));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
            sb.Append($"{b,2:x}".Replace(" ", "0"));
        return sb.ToString().ToUpperInvariant();
    }

    public static string GetHashData(string provisionPassword, string terminalId, string orderId, string installmentCount, string storeKey, string amount, int currencyCode, string successUrl, string type, string errorUrl)
    {
        var securityData = Sha1Upper(provisionPassword + "0" + terminalId);
       
        return Sha512(terminalId + orderId + amount + currencyCode + successUrl + errorUrl + type + installmentCount + storeKey + securityData).ToUpperInvariant();
    }

    public static string IsRequireZero(string id, int complete)
    {
        var s = id?.Trim() ?? string.Empty;
        if (s.Length < complete)
            s = s.PadLeft(complete, '0');
        return s;
    }
}
