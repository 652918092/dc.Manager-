using System.Threading.Tasks;
using dc.Haiyakj.Configuration.Dto;

namespace dc.Haiyakj.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
