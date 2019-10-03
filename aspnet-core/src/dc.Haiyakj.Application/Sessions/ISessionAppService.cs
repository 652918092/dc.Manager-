using System.Threading.Tasks;
using Abp.Application.Services;
using dc.Haiyakj.Sessions.Dto;

namespace dc.Haiyakj.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
