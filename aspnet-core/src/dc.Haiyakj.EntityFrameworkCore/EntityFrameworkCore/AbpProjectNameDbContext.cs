using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using dc.Haiyakj.Authorization.Roles;
using dc.Haiyakj.Authorization.Users;
using dc.Haiyakj.MultiTenancy;
using dc.Haiyakj.Communication;

namespace dc.Haiyakj.EntityFrameworkCore
{
    public class AbpProjectNameDbContext : AbpZeroDbContext<Tenant, Role, User, AbpProjectNameDbContext>
    {
        /* Define a DbSet for each entity of the application */
        public virtual DbSet<Command_Log> Command_Log { get; set; }
        public virtual DbSet<Device_CheckParam> DeviceCheckParam { get; set; }
        public virtual DbSet<Device_Info> DeviceInformation { get; set; }
        public virtual DbSet<Heartbeat_Log> Heartbeat { get; set; }
        public virtual DbSet<SensorWave_Log> SensorWave { get; set; }
        public virtual DbSet<Device_IPEndPort> DeviceIPEndPort { get; set; }

        public AbpProjectNameDbContext(DbContextOptions<AbpProjectNameDbContext> options)
            : base(options)
        {
        }
    }
}
