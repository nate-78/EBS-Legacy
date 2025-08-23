using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EbsClassPkg.Models {
    public class CoveredIndividual {
        public Person Person { get; set; }
        public bool IsCoveredYr { get; set; }
        public bool IsCoveredJan { get; set; }
        public bool IsCoveredFeb { get; set; }
        public bool IsCoveredMar { get; set; }
        public bool IsCoveredApr { get; set; }
        public bool IsCoveredMay { get; set; }
        public bool IsCoveredJun { get; set; }
        public bool IsCoveredJul { get; set; }
        public bool IsCoveredAug { get; set; }
        public bool IsCoveredSep { get; set; }
        public bool IsCoveredOct { get; set; }
        public bool IsCoveredNov { get; set; }
        public bool IsCoveredDec { get; set; }
    }
}