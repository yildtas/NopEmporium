using FluentMigrator.Builders;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Payments.GarantiPos.Domains;

namespace Nop.Plugin.Payments.GarantiPos.Mapping.Builders;

public class PaymentGarantiOrderBuild : NopEntityBuilder<PaymentGarantiOrder>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(PaymentGarantiOrder.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(PaymentGarantiOrder.CustomerId)).AsInt32().NotNullable()
            .WithColumn(nameof(PaymentGarantiOrder.BasketId)).AsString(50).Nullable()
            .WithColumn(nameof(PaymentGarantiOrder.OrderNumber)).AsGuid()
            .WithColumn(nameof(PaymentGarantiOrder.CreateDate)).AsDateTime().NotNullable()
            .WithColumn(nameof(PaymentGarantiOrder.BankRequest)).AsString(4000).Nullable()
            .WithColumn(nameof(PaymentGarantiOrder.BankResponse)).AsString(4000).Nullable()
            .WithColumn(nameof(PaymentGarantiOrder.BankErrorMessage)).AsString().Nullable()
            .WithColumn(nameof(PaymentGarantiOrder.Email)).AsString().Nullable()
            .WithColumn(nameof(PaymentGarantiOrder.NumberOfInstallment)).AsInt32()
            .WithColumn(nameof(PaymentGarantiOrder.PaidDate)).AsDateTime().Nullable()
            .WithColumn(nameof(PaymentGarantiOrder.PaidPrice)).AsDecimal(18, 2).Nullable()
            .WithColumn(nameof(PaymentGarantiOrder.Price)).AsDecimal(18, 2).Nullable()
            .WithColumn(nameof(PaymentGarantiOrder.RefundRequest)).AsString(4000).Nullable()
            .WithColumn(nameof(PaymentGarantiOrder.StatusId)).AsInt32()
            .WithColumn(nameof(PaymentGarantiOrder.RefundResponse)).AsString(4000).Nullable()
            .WithColumn(nameof(PaymentGarantiOrder.PaymentInfo)).AsString(4000).Nullable();
    }
}
