using Abp.Application.Services.Dto;

namespace dc.Haiyakj.Roles.Dto
{
    public class PagedRoleResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
    }
}

