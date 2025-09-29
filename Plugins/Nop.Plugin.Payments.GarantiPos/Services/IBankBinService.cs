
using Nop.Core;
using Nop.Plugin.Payments.GarantiPos.Domains;

namespace Nop.Plugin.Payments.GarantiPos.Services;

public interface IBankBinService
{
	Task InsertBankBin(PaymentGarantiBin bankBin);

	Task InsertBankBin(IEnumerable<PaymentGarantiBin> bankBins);

	Task UpdateBankBin(PaymentGarantiBin bankBin);

	Task UpdateBankBin(IEnumerable<PaymentGarantiBin> bankBins);

	Task DeleteBankBin(PaymentGarantiBin bankBin);

	Task DeleteBankBin(IEnumerable<PaymentGarantiBin> bankBins);

	Task<PaymentGarantiBin> GetBankBinId(int id);

	Task<PaymentGarantiBin> GetBankBin(string prefix);

	Task<IList<PaymentGarantiBin>> GetBankBinList();

	Task<IPagedList<PaymentGarantiBin>> GetBankBinPageList(int pageIndex = 0, int pageSize = int.MaxValue);
}
