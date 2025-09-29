using FluentMigrator.Builders;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Payments.GarantiPos.Domains;

namespace Nop.Plugin.Payments.GarantiPos.Mapping.Builders;

public class PaymentGarantiOrderItemBuilder : NopEntityBuilder<PaymentGarantiOrderItem>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(PaymentGarantiOrderItem.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(PaymentGarantiOrderItem.Price)).AsDecimal(18, 2).Nullable()
            .WithColumn(nameof(PaymentGarantiOrderItem.PaidPrice)).AsDecimal(18, 2).Nullable()
            .WithColumn(nameof(PaymentGarantiOrderItem.PaymentTransactionId)).AsString().Nullable()
            .WithColumn(nameof(PaymentGarantiOrderItem.PaymentOrderId)).AsInt32()
            .WithColumn(nameof(PaymentGarantiOrderItem.ProductId)).AsInt32()
            .WithColumn(nameof(PaymentGarantiOrderItem.CreatedOnUtc)).AsDateTime()
            .WithColumn(nameof(PaymentGarantiOrderItem.Type)).AsString().Nullable();
    }
}
