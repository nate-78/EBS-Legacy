using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ACAfiling_Web.Models;

namespace ACAfiling_Web.Controllers
{
    public class UserMgmtController : Controller
    {
        // LOG OUT USER
        [RequireHttps]
        public ActionResult LogOutUser() {
            FormsAuthentication.SignOut();
            ViewBag.JustLoggedOut = true;
            CurrentUserSingleton.LogOutUser(); // clears the singleton
            //return View("../Home/Login");
            return new EmptyResult();
        }
    }
}