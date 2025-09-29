using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Payments.GarantiPos.Domains;

namespace Nop.Plugin.Payments.GarantiPos.Migrations;

/// <summary>
/// Garanti POS temel �ema olu�turma migrasyonu.
/// Tablolar:
/// - PaymentGarantiBin: Kart BIN bilgileri
/// - PaymentGarantiOrder: Ge�ici �deme kay�tlar�
/// - PaymentGarantiOrderItem: Ge�ici sipari� kalemleri
/// - PaymentGarantiRefund: �ade talepleri/yan�tlar�
/// - PaymentGarantiCategoryInstallment: Kategori bazl� taksit oranlar�
/// - PaymentGarantiInstallment: Genel taksit oranlar�
/// </summary>
[NopMigration("2024/09/01 00:00:00", "Payments.GarantiPos base schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        // Attribute ile i�aretle � FluentMigrator �al��ma zaman�nda otomatik tetikler.
        Create.TableFor<PaymentGarantiBin>();
        Create.TableFor<PaymentGarantiOrder>();
        Create.TableFor<PaymentGarantiOrderItem>();
        Create.TableFor<PaymentGarantiRefund>();
        Create.TableFor<PaymentGarantiCategoryInstallment>();
        Create.TableFor<PaymentGarantiInstallment>();
    }
}
