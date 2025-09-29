
using Nop.Core;
using Nop.Plugin.Payments.GarantiPos.Domains;

namespace Nop.Plugin.Payments.GarantiPos.Services;

public interface IPaymentPosService
{
	Task InsertPosOrderAsync(PaymentGarantiOrder order);

	Task UpdatePosOrderAsync(PaymentGarantiOrder order);

	Task DeletePosOrderAsync(PaymentGarantiOrder order);

	Task<PaymentGarantiOrder> GetPosOrder(int paymentGarantiOrderId);

	Task<PaymentGarantiOrder> GetPosOrderGuid(Guid orderGuid);

	Task<IList<PaymentGarantiOrder>> GetPosOrderList();

	Task<PagedList<PaymentGarantiOrder>> GetPosOrderPageList(int pageIndex = 0, int pageSize = int.MaxValue);

	Task InsertBankPosInstallment(PaymentGarantiInstallment bankInstallment);

	Task UpdateBankPosInstallment(PaymentGarantiInstallment bankInstallment);

	Task DeleteBankPosInstallment(PaymentGarantiInstallment bankInstallment);

	Task<PaymentGarantiInstallment> GetBankInstallmentId(int id);

	Task<IPagedList<PaymentGarantiInstallment>> GetBankInstallmentList(int pageIndex = 0, int pageSize = int.MaxValue);

	Task InsertBankInstallmentCategory(PaymentGarantiCategoryInstallment bankInstallmentCategory);

	Task UpdateBankInstallmentCategory(PaymentGarantiCategoryInstallment bankInstallmentCategory);

	Task DeleteBankInstallmentCategory(PaymentGarantiCategoryInstallment bankInstallmentCategory);

	Task<PaymentGarantiCategoryInstallment> GetBankInstallmentCategoryId(int id);

	Task<PaymentGarantiCategoryInstallment> GetBankInstallmentCategoryName(string bankInstallmentCategoryName);

	Task<IPagedList<PaymentGarantiCategoryInstallment>> GetBankInstallmentCategoryList(int pageIndex = 0, int pageSize = int.MaxValue);
}
