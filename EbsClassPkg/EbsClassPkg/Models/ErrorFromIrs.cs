using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EbsClassPkg.Models {
    public class ErrorFromIrs {
        public int ID { get; set; }
        public string ReceiptId { get; set; }
        public int SubmissionId { get; set; }
        /// <summary>
        /// The IRS's ID for this record, not the DB's
        /// </summary>
        public int RecordId { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string XpathContent { get; set; }
        public bool Is1095Error { get; set; }
        public Person Person { get; set; }
    }
}