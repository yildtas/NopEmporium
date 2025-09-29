using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Payments.GarantiPos.Domains;

namespace Nop.Plugin.Payments.GarantiPos.Services;

/// <summary>
/// Garanti POS �deme ak��� i�in sipari� ve taksit verilerini y�neten servis.
/// Not: �u an i�in sorgularda ek filtre yok; ileride performans i�in indeks ve filtre iyile�tirmeleri gerekebilir.
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

    #region �deme Sipari�leri
    /// <summary>Ge�ici POS sipari� kayd� ekler.</summary>
    public async Task InsertPosOrderAsync(PaymentGarantiOrder order)
    {
        if (order == null) throw new ArgumentNullException(nameof(order));
        await _orderRepository.InsertAsync(order, true);
    }

    /// <summary>Ge�ici POS sipari� kayd� g�nceller.</summary>
    public async Task UpdatePosOrderAsync(PaymentGarantiOrder order)
    {
        if (order == null) throw new ArgumentNullException(nameof(order));
        await _orderRepository.UpdateAsync(order, true);
    }

    /// <summary>Ge�ici POS sipari� kayd� siler.</summary>
    public async Task DeletePosOrderAsync(PaymentGarantiOrder order)
    {
        if (order == null) throw new ArgumentNullException(nameof(order));
        await _orderRepository.DeleteAsync(order, true);
    }

    /// <summary>Id ile �deme sipari�i getirir.</summary>
    public async Task<PaymentGarantiOrder> GetPosOrder(int paymentGarantiOrderId)
    {
        if (paymentGarantiOrderId == 0) throw new ArgumentNullException(nameof(paymentGarantiOrderId));
        return await _orderRepository.GetByIdAsync((int?)paymentGarantiOrderId, (Func<ICacheKeyService, CacheKey>)null, true, false);
    }

    /// <summary>Sipari� GUID (OrderNumber) de�eri ile ge�ici kay�t getirir.</summary>
    public async Task<PaymentGarantiOrder> GetPosOrderGuid(Guid orderGuid)
    {
        if (orderGuid == Guid.Empty) throw new ArgumentNullException(nameof(orderGuid));
        return await Task.FromResult(_orderRepository.Table.FirstOrDefault(o => o.OrderNumber == orderGuid));
    }

    /// <summary>T�m kay�tlar� listeler. (Not: B�y�k tablolar i�in paging tercih edilmeli)</summary>
    public async Task<IList<PaymentGarantiOrder>> GetPosOrderList() => await _orderRepository.Table.ToListAsync();

    /// <summary>Basit paging uygular (bellek i�i). �leride IQueryable �zerinden direkt sayfalama yap�labilir.</summary>
    public async Task<PagedList<PaymentGarantiOrder>> GetPosOrderPageList(int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var list = _orderRepository.Table.ToList();
        return await Task.FromResult(new PagedList<PaymentGarantiOrder>(list, pageIndex, pageSize));
    }
    #endregion

    #region Banka Taksitleri
    /// <summary>Banka genel taksit oran� ekler.</summary>
    public async Task InsertBankPosInstallment(PaymentGarantiInstallment bankInstallment)
    {
        if (bankInstallment == null) throw new ArgumentNullException(nameof(bankInstallment));
        await _installmentRepository.InsertAsync(bankInstallment, true);
    }

    /// <summary>Banka genel taksit oran� g�nceller.</summary>
    public async Task UpdateBankPosInstallment(PaymentGarantiInstallment bankInstallment)
    {
        if (bankInstallment == null) throw new ArgumentNullException(nameof(bankInstallment));
        await _installmentRepository.UpdateAsync(bankInstallment, true);
    }

    /// <summary>Banka genel taksit oran� siler.</summary>
    public async Task DeleteBankPosInstallment(PaymentGarantiInstallment bankInstallment)
    {
        if (bankInstallment == null) throw new ArgumentNullException(nameof(bankInstallment));
        await _installmentRepository.DeleteAsync(bankInstallment, true);
    }

    /// <summary>Id ile taksit oran� getirir.</summary>
    public async Task<PaymentGarantiInstallment> GetBankInstallmentId(int id) => await _installmentRepository.GetByIdAsync((int?)id, (Func<ICacheKeyService, CacheKey>)null, true, false);

    /// <summary>T�m banka taksit oranlar�n� listeler. (Not: bellek i�i paging)</summary>
    public async Task<IPagedList<PaymentGarantiInstallment>> GetBankInstallmentList(int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var list = _installmentRepository.Table.ToList();
        return await Task.FromResult<IPagedList<PaymentGarantiInstallment>>(new PagedList<PaymentGarantiInstallment>(list, pageIndex, pageSize));
    }
    #endregion

    #region Kategori Taksitleri
    /// <summary>Kategoriye �zel taksit tan�m� ekler.</summary>
    public async Task InsertBankInstallmentCategory(PaymentGarantiCategoryInstallment bankInstallmentCategory)
    {
        if (bankInstallmentCategory == null) throw new ArgumentNullException(nameof(bankInstallmentCategory));
        await _categoryInstallmentRepository.InsertAsync(bankInstallmentCategory, true);
    }

    /// <summary>Kategoriye �zel taksit tan�m� g�nceller.</summary>
    public async Task UpdateBankInstallmentCategory(PaymentGarantiCategoryInstallment bankInstallmentCategory)
    {
        if (bankInstallmentCategory == null) throw new ArgumentNullException(nameof(bankInstallmentCategory));
        await _categoryInstallmentRepository.UpdateAsync(bankInstallmentCategory, true);
    }

    /// <summary>Kategoriye �zel taksit tan�m� siler.</summary>
    public async Task DeleteBankInstallmentCategory(PaymentGarantiCategoryInstallment bankInstallmentCategory)
    {
        if (bankInstallmentCategory == null) throw new ArgumentNullException(nameof(bankInstallmentCategory));
        await _categoryInstallmentRepository.DeleteAsync(bankInstallmentCategory, true);
    }

    /// <summary>Id ile kategori taksit kayd� getirir.</summary>
    public async Task<PaymentGarantiCategoryInstallment> GetBankInstallmentCategoryId(int id) => await _categoryInstallmentRepository.GetByIdAsync((int?)id, (Func<ICacheKeyService, CacheKey>)null, true, false);

    /// <summary>�sim aramas� (Contains) ile tek kategori taksit kayd� getirir.</summary>
    public async Task<PaymentGarantiCategoryInstallment> GetBankInstallmentCategoryName(string bankInstallmentCategoryName)
    {
        if (bankInstallmentCategoryName == null) throw new ArgumentNullException(nameof(bankInstallmentCategoryName));
        return await Task.FromResult(_categoryInstallmentRepository.Table.FirstOrDefault(c => c.CategoryName.Contains(bankInstallmentCategoryName)));
    }

    /// <summary>Kategori taksit listesini d�ner (bellek i�i paging).</summary>
    public async Task<IPagedList<PaymentGarantiCategoryInstallment>> GetBankInstallmentCategoryList(int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var list = _categoryInstallmentRepository.Table.ToList();
        return await Task.FromResult<IPagedList<PaymentGarantiCategoryInstallment>>(new PagedList<PaymentGarantiCategoryInstallment>(list, pageIndex, pageSize));
    }
    #endregion
}
