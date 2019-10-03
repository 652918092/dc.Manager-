using Abp.Application.Services;
using Abp.Application.Services.Dto;
using dc.Haiyakj.MultiTenancy.Dto;

namespace dc.Haiyakj.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}

