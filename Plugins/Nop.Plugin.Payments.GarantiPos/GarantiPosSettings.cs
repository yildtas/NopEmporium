using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.GarantiPos
{
    /// <summary>
    /// Garanti POS yapılandırma ayarları.
    /// Üretim ortamında gizlenmesi gereken kritik alanlar (Password, StoreKey vb.) için gizli saklama stratejileri değerlendirilebilir.
    /// </summary>
    public class GarantiPosSettings : ISettings
    {
        /// <summary>Terminal numarası (9 haneli, bankadan verilen).</summary>
        public string TerminalId { get; set; }

        /// <summary>İşyeri (Merchant) Id.</summary>
        public string MerchantId { get; set; }

        /// <summary>3D Secure hash hesaplamasında kullanılan Store Key.</summary>
        public string StoreKey { get; set; }

        /// <summary>Provizyon şifresi (banka panelinden alınır).</summary>
        public string Password { get; set; }

        /// <summary>Firma adı (isteğe bağlı gösterim için).</summary>
        public string CompanyName { get; set; }

        /// <summary>Taksit özelliği açık mı?</summary>
        public bool Installment { get; set; }

        /// <summary>Bilgilendirme e-postası (opsiyonel).</summary>
        public string Email { get; set; }

        /// <summary>Sabit ek komisyon tutarı.</summary>
        public decimal AdditionalFee { get; set; }

        /// <summary>Komisyon yüzdesel mi? true ise AdditionalFee yüzde olarak yorumlanır.</summary>
        public bool AdditionalFeePercentage { get; set; }

        /// <summary>Banka kullanıcı adı (3D işlemlerindeki terminal user).</summary>
        public string TerminalUserId { get; set; }

        /// <summary>Provizyon kullanıcı Id (banka sağlıyor).</summary>
        public string TerminalProvUserId { get; set; }

        /// <summary>Test modu açık mı? true ise bankanın test endpoint'leri kullanılır.</summary>
        public bool TestMode { get; set; }

        /// <summary>Banka 3D gateway URL (test / prod).</summary>
        public string BankUrl { get; set; }

        /// <summary>API sürümü (ör: v0.01)</summary>
        public string Version { get; set; }

        /// <summary>Güvenlik seviyesi (3D_FULL, 3D_PAY vb.)</summary>
        public string SecurityLevel { get; set; }

        /// <summary>NopCommerce lisans ya da referans numarası (varsa).</summary>
        public string NopCommerceNumber { get; set; }

        /// <summary>Ödeme yöntemi aktif mi?</summary>
        public bool Enable { get; set; }
    }
}
