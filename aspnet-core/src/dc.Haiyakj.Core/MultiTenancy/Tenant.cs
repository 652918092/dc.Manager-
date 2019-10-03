using Abp.MultiTenancy;
using dc.Haiyakj.Authorization.Users;

namespace dc.Haiyakj.MultiTenancy
{
    public class Tenant : AbpTenant<User>
    {
        public Tenant()
        {            
        }

        public Tenant(string tenancyName, string name)
            : base(tenancyName, name)
        {
        }
    }
}
