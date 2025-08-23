using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EbsClassPkg.Models {
    public class Activity {
        public User User { get; set; }
        public string ActivitySlug { get; set; }
        public string ActivityDesc { get; set; }
        public string FullText { get; set; }
        public DateTime Timestamp { get; set; }
        public User ImpersonatingAsUser { get; set; }
        public string Notes { get; set; }
        public string ObjId { get; set; }
        public string ObjType { get; set; }
    }
}