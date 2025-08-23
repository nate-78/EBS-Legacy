using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EbsClassPkg.Models {
    public class StatusUpdate {
        /// <summary>
        /// The actual status: "Accepted, Rejected, Etc"
        /// </summary>
        public string StatusType { get; set; }
        public string StatusMessage { get; set; }
        public string StatusCode { get; set; }
        public string ReceiptId { get; set; }
        public DateTime Timestamp { get; set; }
        public int SubmissionID { get; set; }
        public bool Has1094Errors { get; set; }
        public bool Has1095Errors { get; set; }
        public Int32 ID { get; set; }
    }
}