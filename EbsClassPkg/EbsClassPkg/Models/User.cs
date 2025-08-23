using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EbsClassPkg.Models {
    public class User {
        public Int32 ID { get; set; }
        public Person Person { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsClient { get; set; }
        public bool HasAppAccess { get; set; } // actually, this is for ACA
        public bool HasBenEnrollAccess { get; set; } // will need to add one of these every time we add a new app
        public string Role { get; set; }
        public Company Company { get; set; }
        public List<App> Apps { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastUpdated { get; set; }
        public int ImpersonatingAs { get; set; }
        public Person ImpersonatingAsPerson { get; set; }
        public string ImpersonatingAsRole { get; set; }
        public Company ImpersonatingAsComp { get; set; }
        public bool ImpersonatingAsAdmin { get; set; }
        public bool ImpersonatingAsClient { get; set; }
        public bool IsImpersonating { get; set; }
    }
}