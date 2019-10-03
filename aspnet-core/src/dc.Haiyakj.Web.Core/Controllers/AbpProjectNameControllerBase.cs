using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace dc.Haiyakj.Controllers
{
    public abstract class AbpProjectNameControllerBase: AbpController
    {
        protected AbpProjectNameControllerBase()
        {
            LocalizationSourceName = AbpProjectNameConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
