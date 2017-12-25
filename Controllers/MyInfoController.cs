using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SCE.Controllers
{
    public class MyInfoController : Controller
    {
        //View
        public IActionResult Index()
        {
            return View();
        }

        //Edit
        public IActionResult Edit()
        {
            return View();
        }
    }
}