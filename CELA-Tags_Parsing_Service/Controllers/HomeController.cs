using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CELA_Tags_Parsing_Service.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "CELA Productivity Hackers-#Tagulous Proof of Concept Implmentation";

            return View();
        }
    }
}
