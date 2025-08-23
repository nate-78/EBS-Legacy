using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EbsClassPkg.Models {
    public class CustomResponse {
        public string StatusCode { get; set; }
        public string StatusReason { get; set; }
        public string StatusMessage { get; set; }
        public bool Successful { get; set; }
        public string ReceiptId { get; set; }
        public DateTime TimeStamp { get; set; }
        public string FormDataFilePath { get; set; }
        public string ManifestFilePath { get; set; }
        public string FormDataFileName { get; set; }
    }
}