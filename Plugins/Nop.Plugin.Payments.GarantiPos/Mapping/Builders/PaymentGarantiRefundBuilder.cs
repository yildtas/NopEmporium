using FluentMigrator.Builders;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Payments.GarantiPos.Domains;

namespace Nop.Plugin.Payments.GarantiPos.Mapping.Builders;

public class PaymentGarantiRefundBuilder : NopEntityBuilder<PaymentGarantiRefund>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(PaymentGarantiRefund.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(PaymentGarantiRefund.Amount)).AsDecimal(18, 2).NotNullable()
            .WithColumn(nameof(PaymentGarantiRefund.CustomerId)).AsInt32().NotNullable()
            .WithColumn(nameof(PaymentGarantiRefund.OrderId)).AsInt32().NotNullable()
            .WithColumn(nameof(PaymentGarantiRefund.PaymentTransactionId)).AsString().Nullable()
            .WithColumn(nameof(PaymentGarantiRefund.PaymentId)).AsString().NotNullable()
            .WithColumn(nameof(PaymentGarantiRefund.CreatedOnUtc)).AsDateTime();
    }
}
