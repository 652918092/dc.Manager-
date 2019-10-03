using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using dc.Haiyakj.Configuration.Dto;

namespace dc.Haiyakj.Configuration
{
    [AbpAuthorize]
    public class ConfigurationAppService : AbpProjectNameAppServiceBase, IConfigurationAppService
    {
        public async Task ChangeUiTheme(ChangeUiThemeInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
        }
    }
}
