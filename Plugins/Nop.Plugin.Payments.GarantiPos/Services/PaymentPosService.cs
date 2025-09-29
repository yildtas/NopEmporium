using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Payments.GarantiPos.Domains;

namespace Nop.Plugin.Payments.GarantiPos.Services;

/// <summary>
/// Garanti POS ödeme akýþý için sipariþ ve taksit verilerini yöneten servis.
/// Not: Þu an için sorgularda ek filtre yok; ileride performans için indeks ve filtre iyileþtirmeleri gerekebilir.
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
    /// <summary>Geçici POS sipariþ kaydý ekler.</summary>
    public async Task InsertPosOrderAsync(PaymentGarantiOrder order)
    {
        if (order == null) throw new ArgumentNullException(nameof(order));
        await _orderRepository.InsertAsync(order, true);
    }

    /// <summary>Geçici POS sipariþ kaydý günceller.</summary>
    public async Task UpdatePosOrderAsync(PaymentGarantiOrder order)
    {
        if (order == null) throw new ArgumentNullException(nameof(order));
        await _orderRepository.UpdateAsync(order, true);
    }

    /// <summary>Geçici POS sipariþ kaydý siler.</summary>
    public async Task DeletePosOrderAsync(PaymentGarantiOrder order)
    {
        if (order == null) throw new ArgumentNullException(nameof(order));
        await _orderRepository.DeleteAsync(order, true);
    }

    /// <summary>Id ile ödeme sipariþi getirir.</summary>
    public async Task<PaymentGarantiOrder> GetPosOrder(int paymentGarantiOrderId)
    {
        if (paymentGarantiOrderId == 0) throw new ArgumentNullException(nameof(paymentGarantiOrderId));
        return await _orderRepository.GetByIdAsync((int?)paymentGarantiOrderId, (Func<ICacheKeyService, CacheKey>)null, true, false);
    }

    /// <summary>Sipariþ GUID (OrderNumber) deðeri ile geçici kayýt getirir.</summary>
    public async Task<PaymentGarantiOrder> GetPosOrderGuid(Guid orderGuid)
    {
        if (orderGuid == Guid.Empty) throw new ArgumentNullException(nameof(orderGuid));
        return await Task.FromResult(_orderRepository.Table.FirstOrDefault(o => o.OrderNumber == orderGuid));
    }

    /// <summary>Tüm kayýtlarý listeler. (Not: Büyük tablolar için paging tercih edilmeli)</summary>
    public async Task<IList<PaymentGarantiOrder>> GetPosOrderList() => await _orderRepository.Table.ToListAsync();

    /// <summary>Basit paging uygular (bellek içi). Ýleride IQueryable üzerinden direkt sayfalama yapýlabilir.</summary>
    public async Task<PagedList<PaymentGarantiOrder>> GetPosOrderPageList(int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var list = _orderRepository.Table.ToList();
        return await Task.FromResult(new PagedList<PaymentGarantiOrder>(list, pageIndex, pageSize));
    }
    #endregion

    #region Banka Taksitleri
    /// <summary>Banka genel taksit oraný ekler.</summary>
    public async Task InsertBankPosInstallment(PaymentGarantiInstallment bankInstallment)
    {
        if (bankInstallment == null) throw new ArgumentNullException(nameof(bankInstallment));
        await _installmentRepository.InsertAsync(bankInstallment, true);
    }

    /// <summary>Banka genel taksit oraný günceller.</summary>
    public async Task UpdateBankPosInstallment(PaymentGarantiInstallment bankInstallment)
    {
        if (bankInstallment == null) throw new ArgumentNullException(nameof(bankInstallment));
        await _installmentRepository.UpdateAsync(bankInstallment, true);
    }

    /// <summary>Banka genel taksit oraný siler.</summary>
    public async Task DeleteBankPosInstallment(PaymentGarantiInstallment bankInstallment)
    {
        if (bankInstallment == null) throw new ArgumentNullException(nameof(bankInstallment));
        await _installmentRepository.DeleteAsync(bankInstallment, true);
    }

    /// <summary>Id ile taksit oraný getirir.</summary>
    public async Task<PaymentGarantiInstallment> GetBankInstallmentId(int id) => await _installmentRepository.GetByIdAsync((int?)id, (Func<ICacheKeyService, CacheKey>)null, true, false);

    /// <summary>Tüm banka taksit oranlarýný listeler. (Not: bellek içi paging)</summary>
    public async Task<IPagedList<PaymentGarantiInstallment>> GetBankInstallmentList(int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var list = _installmentRepository.Table.ToList();
        return await Task.FromResult<IPagedList<PaymentGarantiInstallment>>(new PagedList<PaymentGarantiInstallment>(list, pageIndex, pageSize));
    }
    #endregion

    #region Kategori Taksitleri
    /// <summary>Kategoriye özel taksit tanýmý ekler.</summary>
    public async Task InsertBankInstallmentCategory(PaymentGarantiCategoryInstallment bankInstallmentCategory)
    {
        if (bankInstallmentCategory == null) throw new ArgumentNullException(nameof(bankInstallmentCategory));
        await _categoryInstallmentRepository.InsertAsync(bankInstallmentCategory, true);
    }

    /// <summary>Kategoriye özel taksit tanýmý günceller.</summary>
    public async Task UpdateBankInstallmentCategory(PaymentGarantiCategoryInstallment bankInstallmentCategory)
    {
        if (bankInstallmentCategory == null) throw new ArgumentNullException(nameof(bankInstallmentCategory));
        await _categoryInstallmentRepository.UpdateAsync(bankInstallmentCategory, true);
    }

    /// <summary>Kategoriye özel taksit tanýmý siler.</summary>
    public async Task DeleteBankInstallmentCategory(PaymentGarantiCategoryInstallment bankInstallmentCategory)
    {
        if (bankInstallmentCategory == null) throw new ArgumentNullException(nameof(bankInstallmentCategory));
        await _categoryInstallmentRepository.DeleteAsync(bankInstallmentCategory, true);
    }

    /// <summary>Id ile kategori taksit kaydý getirir.</summary>
    public async Task<PaymentGarantiCategoryInstallment> GetBankInstallmentCategoryId(int id) => await _categoryInstallmentRepository.GetByIdAsync((int?)id, (Func<ICacheKeyService, CacheKey>)null, true, false);

    /// <summary>Ýsim aramasý (Contains) ile tek kategori taksit kaydý getirir.</summary>
    public async Task<PaymentGarantiCategoryInstallment> GetBankInstallmentCategoryName(string bankInstallmentCategoryName)
    {
        if (bankInstallmentCategoryName == null) throw new ArgumentNullException(nameof(bankInstallmentCategoryName));
        return await Task.FromResult(_categoryInstallmentRepository.Table.FirstOrDefault(c => c.CategoryName.Contains(bankInstallmentCategoryName)));
    }

    /// <summary>Kategori taksit listesini döner (bellek içi paging).</summary>
    public async Task<IPagedList<PaymentGarantiCategoryInstallment>> GetBankInstallmentCategoryList(int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var list = _categoryInstallmentRepository.Table.ToList();
        return await Task.FromResult<IPagedList<PaymentGarantiCategoryInstallment>>(new PagedList<PaymentGarantiCategoryInstallment>(list, pageIndex, pageSize));
    }
    #endregion
}
