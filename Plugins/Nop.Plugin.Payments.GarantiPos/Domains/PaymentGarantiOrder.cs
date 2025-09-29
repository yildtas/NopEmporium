using Nop.Core;
using Nop.Core.Domain.Payments;

namespace Nop.Plugin.Payments.GarantiPos.Domains;

/// <summary>
/// Geçici Garanti POS ödeme kaydýný temsil eder; Nop sipariþi tamamlanmadan önce veya sýrasýnda toplanan verileri tutar.
/// Gerçek Nop Order oluþana kadar kart & taksit bilgileri burada tutulur. Baþarýlý 3D sonrasýnda temizlenebilir.
/// </summary>
public class PaymentGarantiOrder : BaseEntity
{
    /// <summary>Müþteri Id (Customer.Id)</summary>
    public int CustomerId { get; set; }

    /// <summary>Ödeme anýndaki çalýþma para birimi kodu (örn: EUR, TRY)</summary>
    public string BasketId { get; set; }

    public string Email { get; set; }

    /// <summary>Ýliþkili Nop Order Guid deðeri (Order.OrderGuid).</summary>
    public Guid OrderNumber { get; set; }

    /// <summary>Seçilen taksit sayýsý. 1 veya 0 ise peþin gibi yorumlanýr.</summary>
    public int NumberOfInstallment { get; set; }

    /// <summary>Sepet toplamý (baþlangýç).</summary>
    public decimal? Price { get; set; }

    /// <summary>Taksit veya vade farký sonrasý nihai ödenecek tutar.</summary>
    public decimal? PaidPrice { get; set; }

    /// <summary>Serileþtirilmiþ ProcessPaymentRequest (JSON).</summary>
    public string PaymentInfo { get; set; }

    public string RefundRequest { get; set; }
    public string RefundResponse { get; set; }
    public string BankErrorMessage { get; set; }

    /// <summary>Ödeme durum Id (PaymentStatus cast edilir). 10 = Pending/Custom, 30 = Paid.</summary>
    public int StatusId { get; set; }

    /// <summary>Bankaya yönlendirilecek 3D isteði (log amaçlý).
    /// Ýçerik hassas veri içeriyorsa maskeleme düþünülebilir.</summary>
    public string BankRequest { get; set; }

    /// <summary>Banka dönüþü veya gönderilen HTML form (3D yönlendirme formu burada saklanýyor).</summary>
    public string BankResponse { get; set; }

    public DateTime? PaidDate { get; set; }
    public DateTime CreateDate { get; set; }

    private PaymentStatus Status
    {
        get => (PaymentStatus)StatusId;
        set => StatusId = (int)value;
    }

    /// <summary>Kaydý oluþturulmuþ (beklemede) olarak iþaretler.</summary>
    public void MarkAsCreated()
    {
        CreateDate = DateTime.UtcNow;
        Status = (PaymentStatus)10; // beklemede (custom)
    }

    /// <summary>Kaydý ödendi olarak iþaretler.</summary>
    public void MarkAsPaid(DateTime paidDate)
    {
        PaidDate = paidDate;
        Status = (PaymentStatus)30; // ödendi (custom mapping)
    }

    /// <summary>Kaydý baþarýsýz olarak iþaretler ve banka hata bilgisini kaydeder.
    /// Not: Þu an Status tekrar 10'a yani beklemeye çekiliyor; istenirse ayrý Failed kodu tanýmlanabilir.</summary>
    public void MarkAsFailed(string bankErrorMessage, string bankResponse)
    {
        Status = (PaymentStatus)10; // beklemede / baþarýsýz geçiþi
        BankErrorMessage = bankErrorMessage;
        BankResponse = bankResponse;
    }
}
