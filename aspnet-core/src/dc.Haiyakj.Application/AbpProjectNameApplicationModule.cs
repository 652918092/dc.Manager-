using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Threading.BackgroundWorkers;
using dc.Haiyakj.Authorization;
using dc.Haiyakj.Communication;

namespace dc.Haiyakj
{
    [DependsOn(
        typeof(AbpProjectNameCoreModule), 
        typeof(AbpAutoMapperModule))]
    public class AbpProjectNameApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<AbpProjectNameAuthorizationProvider>();
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(AbpProjectNameApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddProfiles(thisAssembly)
            );
        }
        public override void PostInitialize()
        {
            ////后台任务
            var workManager = IocManager.Resolve<IBackgroundWorkerManager>();
            workManager.Add(IocManager.Resolve<AsyncBackgroundTask>());
        }
    }
}
