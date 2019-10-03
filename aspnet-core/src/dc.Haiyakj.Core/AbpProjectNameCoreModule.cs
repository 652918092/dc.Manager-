using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Timing;
using Abp.Zero;
using Abp.Zero.Configuration;
using dc.Haiyakj.Authorization.Roles;
using dc.Haiyakj.Authorization.Users;
using dc.Haiyakj.Configuration;
using dc.Haiyakj.Localization;
using dc.Haiyakj.MultiTenancy;
using dc.Haiyakj.Timing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace dc.Haiyakj
{
    [DependsOn(typeof(AbpZeroCoreModule))]
    public class AbpProjectNameCoreModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Auditing.IsEnabledForAnonymousUsers = true;

            // Declare entity types
            Configuration.Modules.Zero().EntityTypes.Tenant = typeof(Tenant);
            Configuration.Modules.Zero().EntityTypes.Role = typeof(Role);
            Configuration.Modules.Zero().EntityTypes.User = typeof(User);

            AbpProjectNameLocalizationConfigurer.Configure(Configuration.Localization);

            // Enable this line to create a multi-tenant application.
            Configuration.MultiTenancy.IsEnabled = AbpProjectNameConsts.MultiTenancyEnabled;

            // Configure roles
            AppRoleConfig.Configure(Configuration.Modules.Zero().RoleManagement);

            Configuration.Settings.Providers.Add<AppSettingProvider>();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(AbpProjectNameCoreModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            IocManager.Resolve<AppTimes>().StartupTime = Clock.Now;
        }
    }
}
