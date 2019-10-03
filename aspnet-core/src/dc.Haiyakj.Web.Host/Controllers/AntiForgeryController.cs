using Microsoft.AspNetCore.Antiforgery;
using dc.Haiyakj.Controllers;

namespace dc.Haiyakj.Web.Host.Controllers
{
    public class AntiForgeryController : AbpProjectNameControllerBase
    {
        private readonly IAntiforgery _antiforgery;

        public AntiForgeryController(IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
        }

        public void GetToken()
        {
            _antiforgery.SetCookieTokenAndHeader(HttpContext);
        }
    }
}
