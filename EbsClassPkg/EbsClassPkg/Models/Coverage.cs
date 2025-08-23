using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EbsClassPkg.Models {
    public class Coverage {
        public string IsMinEssentialCovOffer { get; set; }
        public int? FTEmployeeCt { get; set; }
        public int? TotEmployeeCt { get; set; }
        public bool IsAggregatedGroup { get; set; }
        public string Sec4980HTransReliefIndicator { get; set; }
    }
}