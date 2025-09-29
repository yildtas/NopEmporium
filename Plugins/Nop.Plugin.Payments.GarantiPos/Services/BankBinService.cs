
using Nop.Core;
using Nop.Core.Caching;
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
		if (bankBin == null)
		{
			throw new ArgumentNullException("bankBin");
		}
		await _bankBinRepository.InsertAsync(bankBin, true);
	}

	public async Task InsertBankBin(IEnumerable<PaymentGarantiBin> bankBins)
	{
		if (bankBins == null)
		{
			throw new ArgumentNullException("bankBins");
		}
		foreach (PaymentGarantiBin bankBin in bankBins)
		{
			await InsertBankBin(bankBin);
		}
	}

	public async Task UpdateBankBin(PaymentGarantiBin bankBin)
	{
		if (bankBin == null)
		{
			throw new ArgumentNullException("bankBin");
		}
		await _bankBinRepository.UpdateAsync(bankBin, true);
	}

	public async Task UpdateBankBin(IEnumerable<PaymentGarantiBin> bankBins)
	{
		if (bankBins == null)
		{
			throw new ArgumentNullException("bankBins");
		}
		foreach (PaymentGarantiBin bin in bankBins)
		{
			await UpdateBankBin(bin);
		}
	}

	public async Task DeleteBankBin(PaymentGarantiBin bankBin)
	{
		if (bankBin == null)
		{
			throw new ArgumentNullException("bankBin");
		}
		await _bankBinRepository.DeleteAsync(bankBin, true);
	}

	public async Task DeleteBankBin(IEnumerable<PaymentGarantiBin> bankBins)
	{
		if (bankBins == null)
		{
			throw new ArgumentNullException("bankBins");
		}
		foreach (PaymentGarantiBin bin in bankBins)
		{
			await DeleteBankBin(bin);
		}
	}

	public async Task<PaymentGarantiBin> GetBankBinId(int id)
	{
		return await _bankBinRepository.GetByIdAsync((int?)id, (Func<ICacheKeyService, CacheKey>)null, true, false);
	}

	public async Task<PaymentGarantiBin> GetBankBin(string prefix)
	{
		IQueryable<PaymentGarantiBin> query = _bankBinRepository.Table.Where((PaymentGarantiBin p) => p.BinNumber == prefix);
		return await Task.FromResult(query.FirstOrDefault());
	}

	public async Task<IList<PaymentGarantiBin>> GetBankBinList()
	{
		IQueryable<PaymentGarantiBin> query = _bankBinRepository.Table.Select((PaymentGarantiBin p) => p);
		return await AsyncIQueryableExtensions.ToListAsync<PaymentGarantiBin>(query);
	}

	public async Task<IPagedList<PaymentGarantiBin>> GetBankBinPageList(int pageIndex = 0, int pageSize = int.MaxValue)
	{
		IQueryable<PaymentGarantiBin> query = _bankBinRepository.Table.Select((PaymentGarantiBin p) => p);
		return (IPagedList<PaymentGarantiBin>)(object)new PagedList<PaymentGarantiBin>((IList<PaymentGarantiBin>)(await AsyncIQueryableExtensions.ToListAsync<PaymentGarantiBin>(query)), pageIndex, pageSize, (int?)null);
	}
}
