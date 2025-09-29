using Nop.Core;
using Nop.Core.Domain.Payments;

namespace Nop.Plugin.Payments.GarantiPos.Domains;

/// <summary>
/// Ge�ici Garanti POS �deme kayd�n� temsil eder; Nop sipari�i tamamlanmadan �nce veya s�ras�nda toplanan verileri tutar.
/// Ger�ek Nop Order olu�ana kadar kart & taksit bilgileri burada tutulur. Ba�ar�l� 3D sonras�nda temizlenebilir.
/// </summary>
public class PaymentGarantiOrder : BaseEntity
{
    /// <summary>M��teri Id (Customer.Id)</summary>
    public int CustomerId { get; set; }

    /// <summary>�deme an�ndaki �al��ma para birimi kodu (�rn: EUR, TRY)</summary>
    public string BasketId { get; set; }

    public string Email { get; set; }

    /// <summary>�li�kili Nop Order Guid de�eri (Order.OrderGuid).</summary>
    public Guid OrderNumber { get; set; }

    /// <summary>Se�ilen taksit say�s�. 1 veya 0 ise pe�in gibi yorumlan�r.</summary>
    public int NumberOfInstallment { get; set; }

    /// <summary>Sepet toplam� (ba�lang��).</summary>
    public decimal? Price { get; set; }

    /// <summary>Taksit veya vade fark� sonras� nihai �denecek tutar.</summary>
    public decimal? PaidPrice { get; set; }

    /// <summary>Serile�tirilmi� ProcessPaymentRequest (JSON).</summary>
    public string PaymentInfo { get; set; }

    public string RefundRequest { get; set; }
    public string RefundResponse { get; set; }
    public string BankErrorMessage { get; set; }

    /// <summary>�deme durum Id (PaymentStatus cast edilir). 10 = Pending/Custom, 30 = Paid.</summary>
    public int StatusId { get; set; }

    /// <summary>Bankaya y�nlendirilecek 3D iste�i (log ama�l�).
    /// ��erik hassas veri i�eriyorsa maskeleme d���n�lebilir.</summary>
    public string BankRequest { get; set; }

    /// <summary>Banka d�n��� veya g�nderilen HTML form (3D y�nlendirme formu burada saklan�yor).</summary>
    public string BankResponse { get; set; }

    public DateTime? PaidDate { get; set; }
    public DateTime CreateDate { get; set; }

    private PaymentStatus Status
    {
        get => (PaymentStatus)StatusId;
        set => StatusId = (int)value;
    }

    /// <summary>Kayd� olu�turulmu� (beklemede) olarak i�aretler.</summary>
    public void MarkAsCreated()
    {
        CreateDate = DateTime.UtcNow;
        Status = (PaymentStatus)10; // beklemede (custom)
    }

    /// <summary>Kayd� �dendi olarak i�aretler.</summary>
    public void MarkAsPaid(DateTime paidDate)
    {
        PaidDate = paidDate;
        Status = (PaymentStatus)30; // �dendi (custom mapping)
    }

    /// <summary>Kayd� ba�ar�s�z olarak i�aretler ve banka hata bilgisini kaydeder.
    /// Not: �u an Status tekrar 10'a yani beklemeye �ekiliyor; istenirse ayr� Failed kodu tan�mlanabilir.</summary>
    public void MarkAsFailed(string bankErrorMessage, string bankResponse)
    {
        Status = (PaymentStatus)10; // beklemede / ba�ar�s�z ge�i�i
        BankErrorMessage = bankErrorMessage;
        BankResponse = bankResponse;
    }
}
