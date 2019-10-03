using System.Threading.Tasks;
using Abp.Application.Services;
using dc.Haiyakj.Authorization.Accounts.Dto;

namespace dc.Haiyakj.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
    }
}
