using System.Collections.Generic;
using dc.Haiyakj.Roles.Dto;
using dc.Haiyakj.Users.Dto;

namespace dc.Haiyakj.Web.Models.Users
{
    public class UserListViewModel
    {
        public IReadOnlyList<UserDto> Users { get; set; }

        public IReadOnlyList<RoleDto> Roles { get; set; }
    }
}
