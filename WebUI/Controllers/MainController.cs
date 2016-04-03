using Banana.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebUI.Controllers
{
    public class MainController : Controller
    {

        

        //
        // GET: /Main/

        public ActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public void GetImg()
        {
            CreateImg.GetImg();
        }

    }
}
