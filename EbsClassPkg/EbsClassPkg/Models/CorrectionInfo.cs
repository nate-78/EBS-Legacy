using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EbsClassPkg.Models {
    public class CorrectionInfo {
        /// <summary>
        /// If false, then it's a correction
        /// </summary>
        public bool IsReplacement { get; set; }
        /// <summary>
        /// If IsReplacement is true, then this property will distinguish 
        /// between an entire Transmission replacement or a Submission replacement. 
        /// Effects the kind of ReceiptID we return -- if the problem was at the 
        /// transmission level, then we only return the Receipt ID with no submission ID included.
        /// But if it was a Submission error (because a Transmission can contain multiple
        /// Submissions), then we'll return a Receipt ID and a Submission ID. Example: 
        /// 1095C-16-00001234|1
        /// </summary>
        public bool IsWholeTransReplacement { get; set; }
        /// <summary>
        /// Receipt ID of the submission that's being corrected/replaced
        /// </summary>
        public string BadSubmissionReceiptId { get; set; }
        /// <summary>
        /// Not sure if I'll need this or not. List of the record ids that are being replaced.
        /// Would probably need more than this... Like how to link these to the record info.
        /// However, could get that from the DB. Just search on the record id and the original submission.
        /// </summary>
        public List<string> RecordIds { get; set; }
        /// <summary>
        /// Which form is being replaced/corrected?
        /// </summary>
        public FormType FormType { get; set; } // this is cool and all, but would a bool be better?
        
    }

    public enum FormType { Form1094, Form1095 }

}