using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EbsClassPkg.Models {
    public class PairedList {
        public List<Company> CompList { get; set; }
        public List<App> AppList { get; set; }
    }
}