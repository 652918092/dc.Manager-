using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using dc.Haiyakj.Configuration;
using dc.Haiyakj.Web;

namespace dc.Haiyakj.EntityFrameworkCore
{
    /* This class is needed to run "dotnet ef ..." commands from command line on development. Not used anywhere else */
    public class AbpProjectNameDbContextFactory : IDesignTimeDbContextFactory<AbpProjectNameDbContext>
    {
        public AbpProjectNameDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<AbpProjectNameDbContext>();
            var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder());

            AbpProjectNameDbContextConfigurer.Configure(builder, configuration.GetConnectionString(AbpProjectNameConsts.ConnectionStringName));

            return new AbpProjectNameDbContext(builder.Options);
        }
    }
}
