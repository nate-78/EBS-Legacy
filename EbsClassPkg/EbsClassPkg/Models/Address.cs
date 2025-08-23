using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EbsClassPkg.Models {
    public class Address {
        public Int32 ID { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int? Zip { get; set; }
        public int? ZipExt { get; set; }
        public string Country { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsPhysical { get; set; }
        public bool IsBilling { get; set; }
        public string BranchId { get; set; }
    }
}