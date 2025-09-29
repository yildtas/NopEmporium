using Nop.Core;
using Nop.Data;
using Nop.Plugin.Payments.GarantiPos.Domains;

namespace Nop.Plugin.Payments.GarantiPos.Services;

/// <summary>
/// Garanti POS ödeme akýþý için sipariþ ve taksit verilerini yöneten servis.
/// </summary>
public class PaymentPosService : IPaymentPosService
{
    private readonly IRepository<PaymentGarantiOrder> _orderRepository;
    private readonly IRepository<PaymentGarantiInstallment> _installmentRepository;
    private readonly IRepository<PaymentGarantiCategoryInstallment> _categoryInstallmentRepository;

    public PaymentPosService(
        IRepository<PaymentGarantiOrder> orderRepository,
        IRepository<PaymentGarantiInstallment> installmentRepository,
        IRepository<PaymentGarantiCategoryInstallment> categoryInstallmentRepository)
    {
        _orderRepository = orderRepository;
        _installmentRepository = installmentRepository;
        _categoryInstallmentRepository = categoryInstallmentRepository;
    }

    #region Ödeme Sipariþleri
    public async Task InsertPosOrderAsync(PaymentGarantiOrder order)
    {
        if (order == null) throw new ArgumentNullException(nameof(order));
        await _orderRepository.InsertAsync(order, true);
    }

    public async Task UpdatePosOrderAsync(PaymentGarantiOrder order)
    {
        if (order == null) throw new ArgumentNullException(nameof(order));
        await _orderRepository.UpdateAsync(order, true);
    }

    public async Task DeletePosOrderAsync(PaymentGarantiOrder order)
    {
        if (order == null) throw new ArgumentNullException(nameof(order));
        await _orderRepository.DeleteAsync(order, true);
    }

    public async Task<PaymentGarantiOrder> GetPosOrder(int paymentGarantiOrderId)
    {
        if (paymentGarantiOrderId <= 0) throw new ArgumentOutOfRangeException(nameof(paymentGarantiOrderId));
        return await _orderRepository.GetByIdAsync(paymentGarantiOrderId);
    }

    public async Task<PaymentGarantiOrder> GetPosOrderGuid(Guid orderGuid)
    {
        if (orderGuid == Guid.Empty) throw new ArgumentOutOfRangeException(nameof(orderGuid));
        var result = await _orderRepository.GetAllAsync(query =>
        {
            return from o in query
                   where o.OrderNumber == orderGuid
                   select o;
        });
        return result.FirstOrDefault();
    }

    public async Task<IList<PaymentGarantiOrder>> GetPosOrderList()
    {
        return await _orderRepository.GetAllAsync(query =>
        {
            return from o in query
                   orderby o.Id
                   select o;
        });
    }

    public async Task<PagedList<PaymentGarantiOrder>> GetPosOrderPageList(int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var paged = await _orderRepository.GetAllPagedAsync(query =>
        {
            return from o in query
                   orderby o.Id
                   select o;
        }, pageIndex, pageSize);
        return (PagedList<PaymentGarantiOrder>)paged;
    }
    #endregion

    #region Banka Taksitleri
    public async Task InsertBankPosInstallment(PaymentGarantiInstallment bankInstallment)
    {
        if (bankInstallment == null) throw new ArgumentNullException(nameof(bankInstallment));
        await _installmentRepository.InsertAsync(bankInstallment, true);
    }

    public async Task UpdateBankPosInstallment(PaymentGarantiInstallment bankInstallment)
    {
        if (bankInstallment == null) throw new ArgumentNullException(nameof(bankInstallment));
        await _installmentRepository.UpdateAsync(bankInstallment, true);
    }

    public async Task DeleteBankPosInstallment(PaymentGarantiInstallment bankInstallment)
    {
        if (bankInstallment == null) throw new ArgumentNullException(nameof(bankInstallment));
        await _installmentRepository.DeleteAsync(bankInstallment, true);
    }

    public async Task<PaymentGarantiInstallment> GetBankInstallmentId(int id)
    {
        return await _installmentRepository.GetByIdAsync(id);
    }

    public async Task<IPagedList<PaymentGarantiInstallment>> GetBankInstallmentList(int pageIndex = 0, int pageSize = int.MaxValue)
    {
        return await _installmentRepository.GetAllPagedAsync(query =>
        {
            return from i in query
                   orderby i.Installment
                   select i;
        }, pageIndex, pageSize);
    }
    #endregion

    #region Kategori Taksitleri
    public async Task InsertBankInstallmentCategory(PaymentGarantiCategoryInstallment bankInstallmentCategory)
    {
        if (bankInstallmentCategory == null) throw new ArgumentNullException(nameof(bankInstallmentCategory));
        await _categoryInstallmentRepository.InsertAsync(bankInstallmentCategory, true);
    }

    public async Task UpdateBankInstallmentCategory(PaymentGarantiCategoryInstallment bankInstallmentCategory)
    {
        if (bankInstallmentCategory == null) throw new ArgumentNullException(nameof(bankInstallmentCategory));
        await _categoryInstallmentRepository.UpdateAsync(bankInstallmentCategory, true);
    }

    public async Task DeleteBankInstallmentCategory(PaymentGarantiCategoryInstallment bankInstallmentCategory)
    {
        if (bankInstallmentCategory == null) throw new ArgumentNullException(nameof(bankInstallmentCategory));
        await _categoryInstallmentRepository.DeleteAsync(bankInstallmentCategory, true);
    }

    public async Task<PaymentGarantiCategoryInstallment> GetBankInstallmentCategoryId(int id)
    {
        return await _categoryInstallmentRepository.GetByIdAsync(id);
    }

    public async Task<PaymentGarantiCategoryInstallment> GetBankInstallmentCategoryName(string bankInstallmentCategoryName)
    {
        if (string.IsNullOrWhiteSpace(bankInstallmentCategoryName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(bankInstallmentCategoryName));
        var result = await _categoryInstallmentRepository.GetAllAsync(query =>
        {
            return from c in query
                   where c.CategoryName.Contains(bankInstallmentCategoryName)
                   select c;
        });
        return result.FirstOrDefault();
    }

    public async Task<IPagedList<PaymentGarantiCategoryInstallment>> GetBankInstallmentCategoryList(int pageIndex = 0, int pageSize = int.MaxValue)
    {
        return await _categoryInstallmentRepository.GetAllPagedAsync(query =>
        {
            return from c in query
                   orderby c.CategoryId, c.Installment
                   select c;
        }, pageIndex, pageSize);
    }
    #endregion
}
