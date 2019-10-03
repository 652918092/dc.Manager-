﻿using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using dc.Haiyakj.Controllers;

namespace dc.Haiyakj.Web.Controllers
{
    [AbpMvcAuthorize]
    public class HomeController : AbpProjectNameControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }
	}
}
