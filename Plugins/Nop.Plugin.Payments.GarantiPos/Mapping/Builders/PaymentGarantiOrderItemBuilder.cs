
using FluentMigrator.Builders;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Payments.GarantiPos.Domains;

namespace Nop.Plugin.Payments.GarantiPos.Mapping.Builders;

public class PaymentGarantiOrderItemBuilder : NopEntityBuilder<PaymentGarantiOrderItem>
{
	public override void MapEntity(CreateTableExpressionBuilder table)
	{
		((IColumnOptionSyntax<ICreateTableColumnOptionOrWithColumnSyntax, ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax>)(object)((IColumnTypeSyntax<ICreateTableColumnOptionOrWithColumnSyntax>)(object)((ICreateTableWithColumnSyntax)((IColumnTypeSyntax<ICreateTableColumnOptionOrWithColumnSyntax>)(object)((ICreateTableWithColumnSyntax)((IColumnTypeSyntax<ICreateTableColumnOptionOrWithColumnSyntax>)(object)((ICreateTableWithColumnSyntax)((IColumnTypeSyntax<ICreateTableColumnOptionOrWithColumnSyntax>)(object)((ICreateTableWithColumnSyntax)((IColumnOptionSyntax<ICreateTableColumnOptionOrWithColumnSyntax, ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax>)(object)((IColumnTypeSyntax<ICreateTableColumnOptionOrWithColumnSyntax>)(object)((ICreateTableWithColumnSyntax)((IColumnTypeSyntax<ICreateTableColumnOptionOrWithColumnSyntax>)(object)((ICreateTableWithColumnSyntax)((IColumnTypeSyntax<ICreateTableColumnOptionOrWithColumnSyntax>)(object)((ICreateTableWithColumnSyntax)((IColumnOptionSyntax<ICreateTableColumnOptionOrWithColumnSyntax, ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax>)(object)((IColumnOptionSyntax<ICreateTableColumnOptionOrWithColumnSyntax, ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax>)(object)((IColumnTypeSyntax<ICreateTableColumnOptionOrWithColumnSyntax>)(object)table.WithColumn("Id")).AsInt32()).Identity()).PrimaryKey()).WithColumn("Price")).AsDecimal()).WithColumn("PaidPrice")).AsDecimal()).WithColumn("PaymentTransactionId")).AsString()).Nullable()).WithColumn("PaymentOrderId")).AsInt32()).WithColumn("ProductId")).AsInt32()).WithColumn("CreatedOnUtc")).AsDateTime()).WithColumn("Type")).AsString()).Nullable();
	}
}
