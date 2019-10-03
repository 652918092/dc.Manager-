using Microsoft.Extensions.Configuration;
using Castle.MicroKernel.Registration;
using Abp.Events.Bus;
using Abp.Modules;
using Abp.Reflection.Extensions;
using dc.Haiyakj.Configuration;
using dc.Haiyakj.EntityFrameworkCore;
using dc.Haiyakj.Migrator.DependencyInjection;

namespace dc.Haiyakj.Migrator
{
    [DependsOn(typeof(AbpProjectNameEntityFrameworkModule))]
    public class AbpProjectNameMigratorModule : AbpModule
    {
        private readonly IConfigurationRoot _appConfiguration;

        public AbpProjectNameMigratorModule(AbpProjectNameEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbSeed = true;

            _appConfiguration = AppConfigurations.Get(
                typeof(AbpProjectNameMigratorModule).GetAssembly().GetDirectoryPathOrNull()
            );
        }

        public override void PreInitialize()
        {
            Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
                AbpProjectNameConsts.ConnectionStringName
            );

            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
            Configuration.ReplaceService(
                typeof(IEventBus), 
                () => IocManager.IocContainer.Register(
                    Component.For<IEventBus>().Instance(NullEventBus.Instance)
                )
            );
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(AbpProjectNameMigratorModule).GetAssembly());
            ServiceCollectionRegistrar.Register(IocManager);
        }
    }
}
