using Nop.Core;
using Nop.Data;
using Nop.Plugin.Payments.GarantiPos.Domains;

namespace Nop.Plugin.Payments.GarantiPos.Services;

public class BankBinService : IBankBinService
{
    private readonly IRepository<PaymentGarantiBin> _bankBinRepository;

    public BankBinService(IRepository<PaymentGarantiBin> bankBinRepository)
    {
        _bankBinRepository = bankBinRepository;
    }

    public async Task InsertBankBin(PaymentGarantiBin bankBin)
    {
        if (bankBin == null) throw new ArgumentNullException(nameof(bankBin));
        await _bankBinRepository.InsertAsync(bankBin, true);
    }

    public async Task InsertBankBin(IEnumerable<PaymentGarantiBin> bankBins)
    {
        if (bankBins == null) throw new ArgumentNullException(nameof(bankBins));
        var list = bankBins as IList<PaymentGarantiBin> ?? bankBins.ToList();
        if (list.Count == 0) return;
        await _bankBinRepository.InsertAsync(list, true);
    }

    public async Task UpdateBankBin(PaymentGarantiBin bankBin)
    {
        if (bankBin == null) throw new ArgumentNullException(nameof(bankBin));
        await _bankBinRepository.UpdateAsync(bankBin, true);
    }

    public async Task UpdateBankBin(IEnumerable<PaymentGarantiBin> bankBins)
    {
        if (bankBins == null) throw new ArgumentNullException(nameof(bankBins));
        var list = bankBins as IList<PaymentGarantiBin> ?? bankBins.ToList();
        if (list.Count == 0) return;
        await _bankBinRepository.UpdateAsync(list, true);
    }

    public async Task DeleteBankBin(PaymentGarantiBin bankBin)
    {
        if (bankBin == null) throw new ArgumentNullException(nameof(bankBin));
        await _bankBinRepository.DeleteAsync(bankBin, true);
    }

    public async Task DeleteBankBin(IEnumerable<PaymentGarantiBin> bankBins)
    {
        if (bankBins == null) throw new ArgumentNullException(nameof(bankBins));
        var list = bankBins as IList<PaymentGarantiBin> ?? bankBins.ToList();
        if (list.Count == 0) return;
        await _bankBinRepository.DeleteAsync(list, true);
    }

    public async Task<PaymentGarantiBin> GetBankBinId(int id)
    {
        return await _bankBinRepository.GetByIdAsync(id);
    }

    public async Task<PaymentGarantiBin> GetBankBin(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(prefix));
        return await Task.FromResult(_bankBinRepository.Table.FirstOrDefault(p => p.BinNumber == prefix));
    }

    public async Task<IList<PaymentGarantiBin>> GetBankBinList()
    {
        return await _bankBinRepository.GetAllAsync((Func<IQueryable<PaymentGarantiBin>, IQueryable<PaymentGarantiBin>>)(q => q));
    }

    public async Task<IPagedList<PaymentGarantiBin>> GetBankBinPageList(int pageIndex = 0, int pageSize = int.MaxValue)
    {
        return await _bankBinRepository.GetAllPagedAsync((Func<IQueryable<PaymentGarantiBin>, IQueryable<PaymentGarantiBin>>)(q => q), pageIndex: pageIndex, pageSize: pageSize);
    }
}
