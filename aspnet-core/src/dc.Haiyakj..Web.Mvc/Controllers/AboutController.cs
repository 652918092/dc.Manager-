using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using dc.Haiyakj.Controllers;

namespace dc.Haiyakj.Web.Controllers
{
    [AbpMvcAuthorize]
    public class AboutController : AbpProjectNameControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }
	}
}
