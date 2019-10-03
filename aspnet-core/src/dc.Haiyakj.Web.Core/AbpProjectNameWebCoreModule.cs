using System;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Abp.AspNetCore;
using Abp.AspNetCore.Configuration;
using Abp.AspNetCore.SignalR;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Zero.Configuration;
using dc.Haiyakj.Authentication.JwtBearer;
using dc.Haiyakj.Configuration;
using dc.Haiyakj.EntityFrameworkCore;
using dc.Haiyakj.Communication;

namespace dc.Haiyakj
{
    [DependsOn(
         typeof(AbpProjectNameApplicationModule),
         typeof(AbpProjectNameEntityFrameworkModule),
         typeof(AbpAspNetCoreModule)
        ,typeof(AbpAspNetCoreSignalRModule)
     )]
    public class AbpProjectNameWebCoreModule : AbpModule
    {
        private readonly IHostingEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public AbpProjectNameWebCoreModule(IHostingEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void PreInitialize()
        {
            Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
                AbpProjectNameConsts.ConnectionStringName
            );

            // Use database for language management
            Configuration.Modules.Zero().LanguageManagement.EnableDbLocalization();

            Configuration.Modules.AbpAspNetCore()
                 .CreateControllersForAppServices(
                     typeof(AbpProjectNameApplicationModule).GetAssembly()
                 );

            ConfigureTokenAuth();

            InitUpdConfig();
            InitFolder();
        }

        private void ConfigureTokenAuth()
        {
            IocManager.Register<TokenAuthConfiguration>();
            var tokenAuthConfig = IocManager.Resolve<TokenAuthConfiguration>();

            tokenAuthConfig.SecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appConfiguration["Authentication:JwtBearer:SecurityKey"]));
            tokenAuthConfig.Issuer = _appConfiguration["Authentication:JwtBearer:Issuer"];
            tokenAuthConfig.Audience = _appConfiguration["Authentication:JwtBearer:Audience"];
            tokenAuthConfig.SigningCredentials = new SigningCredentials(tokenAuthConfig.SecurityKey, SecurityAlgorithms.HmacSha256);
            tokenAuthConfig.Expiration = TimeSpan.FromDays(1);
        }
        /// <summary>
        /// UPD通讯参数配置
        /// </summary>
        private void InitUpdConfig()
        {
            IocManager.Register<UdpParamConfig>();
            var udpConfig = IocManager.Resolve<UdpParamConfig>();
            udpConfig.IpAddress = _appConfiguration["UdpParam:IpAddress"];
            udpConfig.Port = _appConfiguration["UdpParam:Port"];
        }
        /// <summary>
        /// 文件夹初始化
        /// </summary>
        private void InitFolder()
        {
            IocManager.Register<AppFolders>();
            var folderConfig = IocManager.Resolve<AppFolders>();
            folderConfig.ExcelFolder = _appConfiguration["AppFolders:ExcelFolder"];

        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(AbpProjectNameWebCoreModule).GetAssembly());
        }
    }
}
