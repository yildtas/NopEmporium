using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Payments.GarantiPos.Domains;

namespace Nop.Plugin.Payments.GarantiPos.Migrations;

/// <summary>
/// Garanti POS temel þema oluþturma migrasyonu.
/// Tablolar:
/// - PaymentGarantiBin: Kart BIN bilgileri
/// - PaymentGarantiOrder: Geçici ödeme kayýtlarý
/// - PaymentGarantiOrderItem: Geçici sipariþ kalemleri
/// - PaymentGarantiRefund: Ýade talepleri/yanýtlarý
/// - PaymentGarantiCategoryInstallment: Kategori bazlý taksit oranlarý
/// - PaymentGarantiInstallment: Genel taksit oranlarý
/// </summary>
[NopMigration("2024/09/01 00:00:00", "Payments.GarantiPos base schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        // Attribute ile iþaretle – FluentMigrator çalýþma zamanýnda otomatik tetikler.
        Create.TableFor<PaymentGarantiBin>();
        Create.TableFor<PaymentGarantiOrder>();
        Create.TableFor<PaymentGarantiOrderItem>();
        Create.TableFor<PaymentGarantiRefund>();
        Create.TableFor<PaymentGarantiCategoryInstallment>();
        Create.TableFor<PaymentGarantiInstallment>();
    }
}
