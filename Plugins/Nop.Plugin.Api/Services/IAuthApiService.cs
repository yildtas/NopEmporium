using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Api.Services
{
    public interface IAuthApiService
    {
        string CreateAccessToken(Customer customer);
    }
}
