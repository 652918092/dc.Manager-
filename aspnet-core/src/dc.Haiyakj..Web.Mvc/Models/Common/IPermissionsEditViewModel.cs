using System.Collections.Generic;
using dc.Haiyakj.Roles.Dto;

namespace dc.Haiyakj.Web.Models.Common
{
    public interface IPermissionsEditViewModel
    {
        List<FlatPermissionDto> Permissions { get; set; }
    }
}