using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using dc.Haiyakj.Roles.Dto;
using dc.Haiyakj.Users.Dto;

namespace dc.Haiyakj.Users
{
    public interface IUserAppService : IAsyncCrudAppService<UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>
    {
        Task<ListResultDto<RoleDto>> GetRoles();

        Task ChangeLanguage(ChangeUserLanguageDto input);
    }
}
