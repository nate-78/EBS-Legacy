using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EbsClassPkg.Models {
    public class IndividualReport {
        public string RecordId { get; set; }
        public Person Person { get; set; }
        public string EmployerName { get; set; }
        public string EmployerEIN { get; set; }
        public Address EmployerAddress { get; set; }
        public string EmployerPhone { get; set; }
        public string EmplyeeAgeOnJan1 { get; set; } // new for TY 2020
        public string PlanStartMonth { get; set; }
        public string CoverageCodeYr { get; set; }
        public string CoverageCodeJan { get; set; }
        public string CoverageCodeFeb { get; set; }
        public string CoverageCodeMar { get; set; }
        public string CoverageCodeApr { get; set; }
        public string CoverageCodeMay { get; set; }
        public string CoverageCodeJun { get; set; }
        public string CoverageCodeJul { get; set; }
        public string CoverageCodeAug { get; set; }
        public string CoverageCodeSep { get; set; }
        public string CoverageCodeOct { get; set; }
        public string CoverageCodeNov { get; set; }
        public string CoverageCodeDec { get; set; }
        // does this field accept blank? If so, should it be a string, or just do the conversion when converting to SOAP?
        public float LowCostYr { get; set; } 
        public float LowCostJan { get; set; }
        public float LowCostFeb { get; set; }
        public float LowCostMar { get; set; }
        public float LowCostApr { get; set; }
        public float LowCostMay { get; set; }
        public float LowCostJun { get; set; }
        public float LowCostJul { get; set; }
        public float LowCostAug { get; set; }
        public float LowCostSep { get; set; }
        public float LowCostOct { get; set; }
        public float LowCostNov { get; set; }
        public float LowCostDec { get; set; }
        public string SafeHarborYr { get; set; }
        public string SafeHarborJan { get; set; }
        public string SafeHarborFeb { get; set; }
        public string SafeHarborMar { get; set; }
        public string SafeHarborApr { get; set; }
        public string SafeHarborMay { get; set; }
        public string SafeHarborJun { get; set; }
        public string SafeHarborJul { get; set; }
        public string SafeHarborAug { get; set; }
        public string SafeHarborSep { get; set; }
        public string SafeHarborOct { get; set; }
        public string SafeHarborNov { get; set; }
        public string SafeHarborDec { get; set; }

        // edits for 2020 and beyond
        public string IchraZipCdYr { get; set; }
        public string IchraZipCdJan { get; set; }
        public string IchraZipCdFeb { get; set; }
        public string IchraZipCdMar { get; set; }
        public string IchraZipCdApr { get; set; }
        public string IchraZipCdMay { get; set; }
        public string IchraZipCdJun { get; set; }
        public string IchraZipCdJul { get; set; }
        public string IchraZipCdAug { get; set; }
        public string IchraZipCdSep { get; set; }
        public string IchraZipCdOct { get; set; }
        public string IchraZipCdNov { get; set; }
        public string IchraZipCdDec { get; set; }

        public bool EmployerProvidedSelfInsured { get; set; }
        public string TestScenarioId { get; set; }
        // TODO how to do "correctedInd"? Can I base it off the "O,C,R" thing for the 1094 form? OR do I actually need individual flags too?
        public bool CorrectedInd { get; set; }
        public string IdOfRecordBeingReplaced { get; set; }
        public bool HasCoveredIndividuals { get; set; }
        public List<CoveredIndividual> CoveredIndividuals { get; set; }
        // these 2 fields only apply to the FormBuilder
        public bool IsVoidedForm { get; set; }
        public bool IsCorrectedForm { get; set; }

    }
}