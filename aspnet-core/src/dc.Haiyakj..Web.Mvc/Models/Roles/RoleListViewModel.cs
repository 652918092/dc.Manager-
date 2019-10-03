using System.Collections.Generic;
using dc.Haiyakj.Roles.Dto;

namespace dc.Haiyakj.Web.Models.Roles
{
    public class RoleListViewModel
    {
        public IReadOnlyList<RoleListDto> Roles { get; set; }

        public IReadOnlyList<PermissionDto> Permissions { get; set; }
    }
}
