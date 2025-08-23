using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ACAfiling_Web.Controllers;
using EbsClassPkg.Models;
using EbsClassPkg.Controllers;

namespace ACAfiling_Web.Models {
    public class CurrentUserSingleton {
        // this static variable holds the SINGLE INSTANCE of the class
        private static User uniqueInstance;

        // constructor
        private CurrentUserSingleton() { }

        // public access method
        public static User GetUser() {
            string email = HttpContext.Current.User.Identity.Name;
            if (uniqueInstance == null || uniqueInstance.Email != email) {
                if (String.IsNullOrEmpty(email)) {
                    email = "nathan@owensdev.com"; // should only fire when testing
                }
                uniqueInstance = HubDataManager.GetUser(email);
            }
            return uniqueInstance;
        }

        public static void LogOutUser() {
            uniqueInstance = null;
        }
    }
}