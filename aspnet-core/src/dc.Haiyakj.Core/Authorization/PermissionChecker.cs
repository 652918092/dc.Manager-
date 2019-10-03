using Abp.Authorization;
using dc.Haiyakj.Authorization.Roles;
using dc.Haiyakj.Authorization.Users;

namespace dc.Haiyakj.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
