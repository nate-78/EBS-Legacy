using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EbsClassPkg.Models {
    // this class is a Singleton to track all errors with one object 
    // and do so without stopping the application
    public class CustomError {
        // use a static variable to hold the single instance
        private static CustomError uniqueInstance;
        // variable to instantiate ErrorCount property
        private static Int32 errorCount = 0;

        // private constructor, so only the class can create one
        private CustomError () { }

        // the public function that will return the CustomError object
        public static CustomError GetInstance() {
            if (uniqueInstance == null) {
                uniqueInstance = new CustomError();
            }
            return uniqueInstance;
        }

        // need an error counter and a message builder
        public static Int32 ErrorCount {
            get { return errorCount; }
            set { errorCount = value; }
        }
        public static string ErrorMessage { get; set; }

        // method to add an error
        public static void AddError(string message) {
            // increment ErrorCount
            ErrorCount++;
            // add to ErrorMessage
            ErrorMessage += "<div class='error-message'>" + message + "</div>"; // wrap each in its own div
        }
    }
}