using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Castle.Facilities.Logging;
using Swashbuckle.AspNetCore.Swagger;
using Abp.AspNetCore;
using Abp.Castle.Logging.Log4Net;
using Abp.Extensions;
using dc.Haiyakj.Configuration;
using dc.Haiyakj.Identity;

using Abp.AspNetCore.SignalR.Hubs;
using Microsoft.AspNetCore.Mvc;

namespace dc.Haiyakj.Web.Host.Startup
{
    public class Startup
    {
        private const string _defaultCorsPolicyName = "localhost";
        private readonly IConfigurationRoot _appConfiguration;

        public Startup(IHostingEnvironment env)
        {
            _appConfiguration = env.GetAppConfiguration();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // MVC
            services.AddMvc(
                options => options.Filters.Add(new CorsAuthorizationFilterFactory(_defaultCorsPolicyName))
            ).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            IdentityRegistrar.Register(services);
            AuthConfigurer.Configure(services, _appConfiguration);

            services.AddSignalR();

            // Configure CORS for angular2 UI
            //=======================> 20190930配置跨域问题
            services.AddCors(
                options => options.AddPolicy(
                    _defaultCorsPolicyName,
                    builder => builder
                        .WithOrigins(
                            // App:CorsOrigins in appsettings.json can contain more than one address separated by comma.
                            _appConfiguration["App:CorsOrigins"]
                                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                                .Select(o => o.RemovePostFix("/"))
                                .ToArray()
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                )
            );
            //services.AddCors(options =>
            //{
            //    options.AddPolicy(_defaultCorsPolicyName, builder =>
            //    {
            //        //App:CorsOrigins in appsettings.json can contain more than one address with splitted by comma.
            //        builder
            //            //.WithOrigins(_appConfiguration["App:CorsOrigins"].Split(",", StringSplitOptions.RemoveEmptyEntries).Select(o => o.RemovePostFix("/")).ToArray())
            //            .AllowAnyOrigin() //TODO: Will be replaced by above when Microsoft releases microsoft.aspnetcore.cors 2.0 - https://github.com/aspnet/CORS/pull/94
            //            .AllowAnyHeader()
            //            .AllowAnyMethod();
            //    });
            //});

            // Swagger - Enable this line and the related lines in Configure method to enable swagger UI
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info { Title = "AbpProjectName API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);

                // Define the BearerAuth scheme that's in use
                options.AddSecurityDefinition("bearerAuth", new ApiKeyScheme()
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
            });

            // Configure Abp and Dependency Injection
            return services.AddAbp<AbpProjectNameWebHostModule>(
                // Configure Log4Net logging
                options => options.IocManager.IocContainer.AddFacility<LoggingFacility>(
                    f => f.UseAbpLog4Net().WithConfig("log4net.config")
                )
            );
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseAbp(options => { options.UseAbpRequestLocalization = false; }); // Initializes ABP framework.

            app.UseCors(_defaultCorsPolicyName); // Enable CORS!

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseAbpRequestLocalization();

            app.UseSignalR(routes =>
            {
                routes.MapHub<AbpCommonHub>("/signalr");
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "defaultWithArea",
                    template: "{area}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger();
            // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)

            //----20190930===>>>访问接口跨域问题
            //app.UseSwaggerUI(options =>
            //{
            //    options.SwaggerEndpoint(_appConfiguration["App:ServerRootAddress"].EnsureEndsWith('/') + "swagger/v1/swagger.json", "Dc.Haiyakj API V1");
            //    options.IndexStream = () => Assembly.GetExecutingAssembly()
            //        .GetManifestResourceStream("dc.Haiyakj.Web.Host.wwwroot.swagger.ui.index.html");
            //}); // URL: /swagger

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Dc.Haiyakj API V1");
                options.IndexStream = () => Assembly.GetExecutingAssembly()
                   .GetManifestResourceStream("dc.Haiyakj.Web.Host.wwwroot.swagger.ui.index.html");
            }); //URL: /swagger
        }
    }
}
