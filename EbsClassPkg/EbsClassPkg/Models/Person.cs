using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EbsClassPkg.Models {
    public class Person {
        public Int32 ID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public string SSN { get; set; }
        public DateTime DOB { get; set; }
        public Address Address { get; set; }
        public string Title { get; set; }
        public string LastFourSSN { get; set; }
    }
}