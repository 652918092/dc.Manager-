using Abp.AspNetCore.Mvc.ViewComponents;

namespace dc.Haiyakj.Web.Views
{
    public abstract class AbpProjectNameViewComponent : AbpViewComponent
    {
        protected AbpProjectNameViewComponent()
        {
            LocalizationSourceName = AbpProjectNameConsts.LocalizationSourceName;
        }
    }
}
