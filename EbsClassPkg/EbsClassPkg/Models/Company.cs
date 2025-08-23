using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EbsClassPkg.Models {
    public class Company {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Name2 { get; set; }
        public string DisplayName { get; set; }
        public Address Address { get; set; }
        public string EIN { get; set; } // maybe this should be an int?
        public Person ContactPerson { get; set; }
        public string ContactPhone { get; set; }
        public string GovEntityName { get; set; }
        public string GovEntityName2 { get; set; }
        public string GovEntityEIN { get; set; }
        public Address GovEntityAddress { get; set; }
        public Person GovContact { get; set; }
        public string GovContactPhone { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<App> Apps { get; set; }
        public bool HasAcaAccess { get; set; }
        public bool HasBenEnrollAccess { get; set; }
        public int LastAcaTaxYr { get; set; }
    }
}