
using FluentMigrator.Builders;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Payments.GarantiPos.Domains;

namespace Nop.Plugin.Payments.GarantiPos.Mapping.Builders;

public class PaymentGarantiRefundBuilder : NopEntityBuilder<PaymentGarantiRefund>
{
	public override void MapEntity(CreateTableExpressionBuilder table)
	{
		((IColumnTypeSyntax<ICreateTableColumnOptionOrWithColumnSyntax>)(object)((ICreateTableWithColumnSyntax)((IColumnOptionSyntax<ICreateTableColumnOptionOrWithColumnSyntax, ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax>)(object)((IColumnTypeSyntax<ICreateTableColumnOptionOrWithColumnSyntax>)(object)((ICreateTableWithColumnSyntax)((IColumnOptionSyntax<ICreateTableColumnOptionOrWithColumnSyntax, ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax>)(object)((IColumnTypeSyntax<ICreateTableColumnOptionOrWithColumnSyntax>)(object)((ICreateTableWithColumnSyntax)((IColumnOptionSyntax<ICreateTableColumnOptionOrWithColumnSyntax, ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax>)(object)((IColumnTypeSyntax<ICreateTableColumnOptionOrWithColumnSyntax>)(object)((ICreateTableWithColumnSyntax)((IColumnOptionSyntax<ICreateTableColumnOptionOrWithColumnSyntax, ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax>)(object)((IColumnTypeSyntax<ICreateTableColumnOptionOrWithColumnSyntax>)(object)((ICreateTableWithColumnSyntax)((IColumnOptionSyntax<ICreateTableColumnOptionOrWithColumnSyntax, ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax>)(object)((IColumnTypeSyntax<ICreateTableColumnOptionOrWithColumnSyntax>)(object)((ICreateTableWithColumnSyntax)((IColumnOptionSyntax<ICreateTableColumnOptionOrWithColumnSyntax, ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax>)(object)((IColumnOptionSyntax<ICreateTableColumnOptionOrWithColumnSyntax, ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax>)(object)((IColumnTypeSyntax<ICreateTableColumnOptionOrWithColumnSyntax>)(object)table.WithColumn("Id")).AsInt32()).Identity()).PrimaryKey()).WithColumn("Amount")).AsDecimal()).NotNullable()).WithColumn("CustomerId")).AsInt32()).NotNullable()).WithColumn("OrderId")).AsInt32()).NotNullable()).WithColumn("PaymentTransactionId")).AsString()).Nullable()).WithColumn("PaymentId")).AsString()).NotNullable()).WithColumn("CreatedOnUtc")).AsDateTime();
	}
}
