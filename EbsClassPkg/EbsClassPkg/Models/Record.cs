using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EbsClassPkg.Models {
    public class Record {
        public int ID { get; set; }
        public string TCC { get; set; }
        public string Directory { get; set; }
        public string CurrentSubDirectory { get; set; }
        public string FileName { get; set; }
        public string IrsFileName { get; set; }
        public string XmlFilePath { get; set; }
        public string TimeStampGMT { get; set; }
        public int TaxYr { get; set; }
        public bool PriorYrDataInd { get; set; }
        /// <summary>
        /// Use "O", "C", or "R"
        /// </summary>
        public string SubmissionType { get; set; }
        public bool Is1094Correction { get; set; }
        public string UniqueId { get; set; }
        public string ReceiptId { get; set; }
        public int ParentSubId { get; set; } // these should probably be ints...
        public string RootParentSubId { get; set; } // ...maybe in a future update?
        public Company Company { get; set; }
        public int Ct1095Transmittal { get; set; }
        public bool IsAuthoritative { get; set; }
        public int? Ct1095TotalALE { get; set; }
        public string IsMemberOfAggregatedGroup { get; set; }
        public bool IsQualOfferMethod { get; set; }
        public bool IsQualOfferMethodTransitionRelief { get; set; }
        public bool IsSec4980HTransitionRelief { get; set; }
        public bool Is98PercentOfferMethod { get; set; }
        public string SignaturePIN { get; set; }
        public string SignatureDate { get; set; }
        public bool HadMinEssentialCoverageForTheYr { get; set; }
        public bool IsArchived { get; set; }
        public bool IsTestData { get; set; }
        public string Current1094Status { get; set; }
        public string Current1095Status { get; set; }
        /// <summary>
        /// I know this is clunky. Min Essential Covg can be either "Yes" "No" "Unanswered" or "Both"
        /// We don't actually account for "Both" because it's stupid. But we need to know if the year
        /// had yearly coverage or didn't, and "unanswered" keeps us from making this one bool variable
        /// </summary>
        public bool DidNotHaveMinEssentialCoverageForTheYr { get; set; }
        public Coverage CoverageYr { get; set; }
        public Coverage CoverageJan { get; set; }
        public Coverage CoverageFeb { get; set; }
        public Coverage CoverageMar { get; set; }
        public Coverage CoverageApr { get; set; }
        public Coverage CoverageMay { get; set; }
        public Coverage CoverageJun { get; set; }
        public Coverage CoverageJul { get; set; }
        public Coverage CoverageAug { get; set; }
        public Coverage CoverageSep { get; set; }
        public Coverage CoverageOct { get; set; }
        public Coverage CoverageNov { get; set; }
        public Coverage CoverageDec { get; set; }
        public List<OtherMember> OtherMembers { get; set; }
        public List<IndividualReport> IndividualReports { get; set; }
        public List<StatusUpdate> StatusUpdates { get; set; }
        public Int32 ByteSz { get; set; }
        public string TestScenarioId { get; set; }
        public int Num1095RecordsProcessed { get; set; }
        public int RecordStartedWith { get; set; }
        public int RecordEndedWith { get; set; }
        public bool HasRecordsToSubmit { get { return hasRecordsToSubmit; } set { hasRecordsToSubmit = value; } }

        private bool hasRecordsToSubmit = true;

        
    }

    
}