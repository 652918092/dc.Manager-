using Abp.AutoMapper;
using dc.Haiyakj.Sessions.Dto;

namespace dc.Haiyakj.Web.Views.Shared.Components.TenantChange
{
    [AutoMapFrom(typeof(GetCurrentLoginInformationsOutput))]
    public class TenantChangeViewModel
    {
        public TenantLoginInfoDto Tenant { get; set; }
    }
}
