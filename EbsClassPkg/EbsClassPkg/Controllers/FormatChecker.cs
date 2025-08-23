using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EbsClassPkg.Models;
using OfficeOpenXml;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;

namespace EbsClassPkg.Controllers {
    public class FormatChecker : Controller {
        public static string[] States = { "AA", "AE", "AK", "AL", "AP", "AR", "AS", "AZ", "CA", "CO", "CT", "DC", "DE", "FL", "FM", "GA", "GU", "HI",
            "IA", "ID", "IL", "IN", "KS", "KY", "LA", "MA", "MD", "ME", "MH", "MI", "MN", "MO", "MP", "MS", "MT", "NC", "ND", "NE", "NH", "NJ",
            "NM", "NV", "NY", "OH", "OK", "OR", "PA", "PR", "PW", "RI", "SC", "SD", "TN", "TX", "UT", "VA", "VI", "VT", "WA", "WI", "WV", "WY"
        };
        
        public static string[] GetStates() {
            return States;
        }

        public static float ReturnFloat(string field, Int32 row, Int32 column) {
            field = field.Replace("$", "").Trim();
            float number;
            bool result = float.TryParse(field, out number);

            // if fiels is empty, return 0
            if (String.IsNullOrEmpty(field)) {
                number = 0;
            } else if (!result) {
                // if it can't be turned into an int...
                number = 0;
                // add error
                CustomError.AddError("The field at column " + GetExcelColumnName(column) + " and row " + row +
                    " needs an integer, not text.");
            }

            return number;
        }

        public static Int32 ReturnInt(string field, Int32 row, Int32 column) {
            field = field.Replace("$", "").Trim();
            Int32 number;
            bool result = Int32.TryParse(field, out number);

            // if fiels is empty, return null
            if (String.IsNullOrEmpty(field)) {
                number = 0;
            } else if (!result) { 
                // if it can't be turned into an int...
                number = 0;
                // add error
                CustomError.AddError("The field at column " + GetExcelColumnName(column) + " and row " + row + 
                    " needs an integer, not text.");
            }

            return number;
        } // end ReturnInt function

        // return a nullable int
        public static int? ReturnNullableInt(string field, Int32 row, Int32 column) {
            field = field.Replace("$", "").Trim();
            int? returnValue = null;
            int number;
            bool result = Int32.TryParse(field, out number);

            // if fiels is empty, return null
            if (!String.IsNullOrEmpty(field) && !result) {
                // if it can't be turned into an int...
                number = 0;
                // add error
                CustomError.AddError("The field at column " + GetExcelColumnName(column) + " and row " + row +
                    " needs an integer, not text.");
            } else if (result) {
                returnValue = (int)Convert.ToInt32(field);
            }

            return returnValue;
        } // end ReturnInt function

        public static string ReturnEIN(string field, Int32 row, Int32 column) {
            return ReturnEIN(field, row, column, true);
        }

        // rework ReturnEIN to straight int (correct length -- no dash)

        public static string ReturnEIN(string field, Int32 row, Int32 column, bool isRequired) {
            if (!isRequired && String.IsNullOrEmpty(field)) {
                return "";
            }
            Int32 notNeededNum;
            field = field.Replace("-", "");
            // if field is an integer, and its length is 8 or 9 characters, then just return it
            if (Int32.TryParse(field, out notNeededNum)) {
                // if the length is 8 or 9 characters, then it's okay
                if (field.Length == 8 || field.Length == 9) {
                    return field;
                } else { // if not, keep the value, but throw error
                    CustomError.AddError("The EIN at column " + GetExcelColumnName(column) + " and row " + row + " does not have the " +
                        "correct number of digits.");
                    return field;
                }
            } else { // it's not a number, so throw error and return empty
                CustomError.AddError("The EIN at column " + GetExcelColumnName(column) + " and row " + row +
                    " is not in the correct 00-0000000 format.");
                return "";
            }
        }

        // this function  has been replaced with the one above
        /*public static string ReturnEIN(string field, Int32 row, Int32 column, bool isRequired) {
            if (!isRequired && String.IsNullOrEmpty(field)) {
                return "";
            }
            Int32 notNeededNum;
            System.Text.RegularExpressions.Regex pattern = new System.Text.RegularExpressions.Regex(@"^[1-9]\d?-\d{7}$");
            System.Text.RegularExpressions.Match match = pattern.Match(field);
            if (match.Success) {
                return field;
            } else if (field.Length < 10 && Int32.TryParse(field, out notNeededNum)) {
                if (field.Length == 9) {
                    return field.Insert(2, "-");
                } else if (field.Length == 8) {
                    return field.Insert(1, "-");
                } else {
                    return field;
                }
            } else {
                // throw error...
                CustomError.AddError("The field at column " + GetExcelColumnName(column) + " and row " + row + 
                    " is not in the correct 00-0000000 format.");
                return "";
            }
        } // end ReturnEIN function */

        public static string ReturnSSN(string field, Int32 row, Int32 column) {
            return ReturnSSN(field, row, column, true);
        }

        // the SSN can be entered in the Excel spreadsheet in traditional format, but dashes must be removed prior to submission
        public static string ReturnSSN(string field, Int32 row, Int32 column, bool isRequired) {
            if (!isRequired && String.IsNullOrEmpty(field)) {
                return "";
            } else {
                Int32 numForIntCheck;
                System.Text.RegularExpressions.Regex pattern = new System.Text.RegularExpressions.Regex(@"^\d{3}-\d{2}-\d{4}$");
                System.Text.RegularExpressions.Match match = pattern.Match(field);
                if (match.Success) {
                    field = field.Replace("-", "");
                    return field;
                } else if (field.Length == 9 && Int32.TryParse(field, out numForIntCheck)) {
                    //field = field.Insert(3, "-");
                    //field = field.Insert(6, "-");
                    return field;
                } else {
                    // throw error
                    //CustomError.AddError("The field at column " + GetExcelColumnName(column) + " and row " + row +
                     //   " is not in the correct 000-00-0000 format.");
                    return "";
                }
            }
        } // end ReturnSSN function

        public static string ReturnLastFour(string ssn) {
            string last = ssn;
            if (!String.IsNullOrEmpty(ssn) && ssn.Length >= 4) {
                last = ssn.Substring(ssn.Length - 4); // gets last 4
                //last = PasswordStorage.CreateHash(last); // encrypts value // do encryption right before storing in db
            }
            return last;
        }

        // MIGHT REINSTATE THIS IF WE GO BACK TO USING A2A SOAP SERVICE
        // return boolean object as a "DigitBooleanType"
        //public static ACASvc.DigitBooleanType BoolToDigitBoolType(bool value) {
        //    var dig = new ACASvc.DigitBooleanType();
        //    dig = ACASvc.DigitBooleanType.Item0;
        //    if (value) {
        //        dig = ACASvc.DigitBooleanType.Item1;
        //    }

        //    return dig;
        //}

        // return boolean object as a "DigitCodeType"
        // options are 0 (unanswered), 1 (yes), 2 (no), and 3 (both)
        public static string BoolToDigitCodeType(string field) {
            string result = "0";

            if (!String.IsNullOrEmpty(field)) { // if the field has a value, check it
                bool conditional = false;
                if (field.Trim().ToLower() == "x" || field.Trim().ToLower() == "yes") {
                    conditional = true;
                }

                if (conditional) {
                    result = "1";
                } else {
                    result = "2";
                }
            }

            return result;
        }

        // return boolean object as an int-to-string
        public static string BoolToDigitBool(bool value) {
            var dig = 0;
            if (value) {
                dig = 1;
            }

            return dig.ToString();
        }

        public static string ReturnSubmissionTypeCd(string field) {
            if (!String.IsNullOrEmpty(field) && (field.ToLower() == "o" || field.ToLower() == "c" || field.ToLower() == "r")) {
                // everything's good
            } else {
                // throw error
                CustomError.AddError("Please ensure that an appropriate Submission Type is selected before uploading the spreadsheet.");
            }
            return field;
        }

        public static string ReturnPhone(string field, Int32 row, Int32 column) {
            Int64 numForIntCheck;
            // 000-000-0000
            System.Text.RegularExpressions.Regex pattern1 = new System.Text.RegularExpressions.Regex(@"^[1-9]\d{2}-\d{3}-\d{4}");
            // (000) 000-0000
            System.Text.RegularExpressions.Regex pattern2 = new System.Text.RegularExpressions.Regex(@"^\(\d{3}\)\s\d{3}-\d{4}");
            // 000 000 0000
            System.Text.RegularExpressions.Regex pattern3 = new System.Text.RegularExpressions.Regex(@"^[1-9]\d{2}\s\d{3}\s\d{4}");
            // 000.000.0000
            System.Text.RegularExpressions.Regex pattern4 = new System.Text.RegularExpressions.Regex(@"^[1-9]\d{2}\.\d{3}\.\d{4}");
            // (000)000-0000
            System.Text.RegularExpressions.Regex pattern5 = new System.Text.RegularExpressions.Regex(@"^\(\d{3}\)\d{3}-\d{4}");

            System.Text.RegularExpressions.Match match1 = pattern1.Match(field);
            System.Text.RegularExpressions.Match match2 = pattern2.Match(field);
            System.Text.RegularExpressions.Match match3 = pattern3.Match(field);
            System.Text.RegularExpressions.Match match4 = pattern4.Match(field);
            System.Text.RegularExpressions.Match match5 = pattern5.Match(field);

            if (String.IsNullOrEmpty(field)) {
                return field;
            } else if (match1.Success) { // correct format
                field = field.Replace("-", "");
                return field;
            } else if (match2.Success) { // (000) 000-0000
                field = field.Replace("(", "");
                field = field.Replace(") ", "-");
                field = field.Replace("-", "");
                return field;
            } else if (match3.Success) { // 000 000 0000
                field = field.Replace(" ", "");
                return field;
            } else if (match4.Success) { // 000.000.0000
                field = field.Replace(".", "");
                return field;
            } else if (match5.Success) { // (000)000-0000
                field = field.Replace("(", "");
                field = field.Replace(")", "");
                return field;
            } else if (field.Length == 10 && Int64.TryParse(field, out numForIntCheck)) { // if 0000000000
                //field.Insert(3, "-");
                //field.Insert(7, "-");
                return field;
            } else {
                // throw error
                CustomError.AddError("The field at column " + GetExcelColumnName(column) + " and row " + row + 
                    " is not in the correct 000-000-0000 format.");
                return "";
            }
        } // end ReturnPhone function

        public static string ReturnPhoneReadable(string field) {
            string phone = field;
            string seg1 = "";
            string seg2 = "";
            string seg3 = "";
            if (field.Length == 10) {
                seg1 = field.Substring(0, 3);
                seg2 = field.Substring(3, 3);
                seg3 = field.Substring(6, 4);
                phone = seg1 + "-" + seg2 + "-" + seg3;
            } else if (field.Length > 10) {
                var extra = field.Length - 10;
                string preSeg = field.Substring(0, extra);
                field = field.Substring(field.Length - 10, field.Length - extra);
                seg1 = field.Substring(0, 3);
                seg2 = field.Substring(3, 3);
                seg3 = field.Substring(6, 4);
                phone = preSeg + "-" + seg1 + "-" + seg2 + "-" + seg3;
            }

            return phone;
        }

        public static Int32? ReturnZip(string field, Int32 row, Int32 column) {
            if (String.IsNullOrEmpty(field)) {
                return null;
            } else {
                Int32 numForIntCheck;
                if (field.Length == 5 && Int32.TryParse(field, out numForIntCheck)) { // field is correct
                    return numForIntCheck;
                } else if (Int32.TryParse(field, out numForIntCheck)) { // not 5 digits
                                                                        // throw error
                    return numForIntCheck;
                } else {
                    // throw error
                    CustomError.AddError("The field at column " + GetExcelColumnName(column) + " and row " + row +
                        " is not in the correct 00000 format.");
                    return 0;
                }
            }
        } // end ReturnZip function

        public static Int32? ReturnZipExt(string field, Int32 row, Int32 column) {
            if (String.IsNullOrEmpty(field)) {
                return null;
            } else {
                Int32 numForIntCheck;
                if (String.IsNullOrEmpty(field)) {
                    return 0;
                } else if (field.Length == 4 && Int32.TryParse(field, out numForIntCheck)) { // field is correct
                    return numForIntCheck;
                } else if (Int32.TryParse(field, out numForIntCheck)) { // not 4 digits
                                                                        // throw error
                    return numForIntCheck;
                } else {
                    // throw error
                    CustomError.AddError("The field at column " + GetExcelColumnName(column) + " and row " + row +
                        " is not in the correct 0000 format.");
                    return 0;
                }
            }
        } // end ReturnZipExt function

        public static string ReturnZipToString(int zip) {
            string result = "";
            if (!String.IsNullOrEmpty(zip.ToString()) && zip > 0) {
                result = zip.ToString();
                int length = result.Length;
                while (length < 5) {
                    result = "0" + result;
                    length++;
                }
            }

            return result;
        }

        public static string ReturnZipExtToString(int ext) {
            string result = "";
            if (!String.IsNullOrEmpty(ext.ToString()) && ext > 0) {
                result = ext.ToString();
                int length = result.Length;
                while (length < 4) {
                    result = "0" + result;
                    length++;
                }
            }

            return result;
        }

        public static string ReturnReliefIndicator(string field) {
            // TODO: should there be an error thrown if something was entered in the field that isn't A or B?
            if (field == "A" || field == "B") {
                return field;
            } else { // leave blank if invalid
                return "";
            }
        } // end ReturnReliefIndicator function

        public static Int32 ReturnMonthAsInt(string field, Int32 row, Int32 column) {
            Int32 numForIntCheck;
            if (Int32.TryParse(field, out numForIntCheck)) { // if it's an int...
                if (numForIntCheck < 1 || numForIntCheck > 12) { // if it's not within 1-12, throw error
                    // throw error
                    CustomError.AddError("The field at column " + column + " and row " + row + " should be an integer between 1 and 12.");
                    return 0;
                } else { // otherwise, return it as an int
                    return numForIntCheck;
                }
            } else {
                switch (field.ToLower()) {
                    case "jan":
                    case "january":
                        return 1;
                    case "feb":
                    case "february":
                        return 2;
                    case "mar":
                    case "march":
                        return 3;
                    case "apr":
                    case "april":
                        return 4;
                    case "may":
                        return 5;
                    case "jun":
                    case "june":
                        return 6;
                    case "jul":
                    case "july":
                        return 7;
                    case "aug":
                    case "august":
                        return 8;
                    case "sep":
                    case "sept":
                    case "september":
                        return 9;
                    case "oct":
                    case "october":
                        return 10;
                    case "nov":
                    case "november":
                        return 11;
                    case "dec":
                    case "december":
                        return 12;
                    default:
                        // throw error
                        CustomError.AddError("The field at column " + GetExcelColumnName(column) + " and row " + row + 
                            " should be an integer between 1 and 12.");
                        return 1;
                } // end switch case
            } // end if else
        } // end ReturnMonthAsInt function


        public static DateTime ReturnDateTime(string field, Int32 row, Int32 column) { // I could probably streamline this function...
            DateTime date = new DateTime();
            if (!String.IsNullOrEmpty(field)) {
                // try to convert the string into a DateTime object
                if (DateTime.TryParse(field, out date)) {
                    return date; // this will be an updated value 
                } else {
                    return date;
                }
            } else { // field is blank
                // throw error
                //CustomError.AddError("The field at column " + GetExcelColumnName(column) + " and row " + row + " should be a date (MM/DD/YYYY).");
                return date;
            }
        } // end ReturnDateTime function

        
        public static string GetStateAbb(string field, Int32 row, Int32 column) {
            if (!String.IsNullOrEmpty(field) && (field.Length != 2 || !isState(field))) {
                // throw error
                CustomError.AddError("The field at column " + GetExcelColumnName(column) + " and row " + row + " should be a valid state " +
                    "code like AL, CA, FL, etc.");
            }
            return field; // field is returned even if invalid
        }


        public static string FormatCityName(string city) {
            city = city.Replace("-", " ");
            // city name can only have 1 space.  Check for more
            int count = city.Count(c => c == ' ');
            if (count > 1) {
                var arr = city.Split(' ');
                var counter = 0;
                city = "";
                foreach (string s in arr) {
                    city += s;
                    if (counter == 0) {
                        city += " ";
                    }
                    counter++;
                }
            }

            // remove periods, extra spaces, etc
            city = removeSpecialChars(city, true);
            
            if (!String.IsNullOrEmpty(city)) {
                var len = 22;
                if (city.Length < 22) {
                    len = city.Length;
                }
                city = city.Substring(0, len);
            }
            
            return city;
        }


        // convert column numbers to Excel column names
        public static string GetExcelColumnName(int columnNumber) {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0) {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }


        // build name control for an individual
        public static string GetNameControlIndividual(string lastName) {
            lastName.Replace("'", "").Replace(" ", "").Replace("-", "");
            lastName = NameCtrlSpecChar(lastName);
            int endingIndex = 4;
            if (lastName.Length < endingIndex) {
                endingIndex = lastName.Length;
            }
            string ctrl = lastName.ToUpper().Substring(0, endingIndex);

            return ctrl;
        }


        // build name code
        public static string GetNameControlCode(string compName) {
            if (String.IsNullOrEmpty(compName)) {
                return "";
            } else {
                string nameControl = "";
                // the rules for this method come from Pub 4163 (https://www.irs.gov/pub/irs-pdf/p4163.pdf) Sec 3.11.
                // in the version I had access to, the rules started on page 40

                // find number of words in name
                string[] nameArray = compName.Split(' ');
                int wordCount = nameArray.Length;

                // is "the" the first word?
                bool theIsFirst = false;
                if (compName.Substring(0, 4).ToLower() == "the ") {
                    theIsFirst = true;
                }

                // if "the" is first word, and there are more than 2 words in name, remove "the" from consideration
                if (theIsFirst && wordCount > 2) {
                    compName = compName.Substring(4);
                }

                // does the name contain "dba"?
                if (compName.ToLower().IndexOf(" dba ") > -1) {
                    // trim off " dba " and everything before it
                    compName = compName.Substring(compName.ToLower().IndexOf(" dba "));
                    compName = compName.Substring(5);
                }

                // check for partnerships that don't include "partners" in the name
                if ((compName.ToLower().IndexOf("partners") == -1 || compName.ToLower().IndexOf("ptr") == -1) &&
                    (compName.IndexOf(" & ") > -1 || compName.ToLower().IndexOf(" and ") > -1)) {
                    // "and" or "&" is in the comp name, which means it could be a partnership
                    // check for "FName LName & FName LName" kind of pattern
                    string[] newNameArray = compName.Split(' ');
                    int newWordCount = newNameArray.Length;
                    if (newWordCount == 5 && (newNameArray[2].ToLower() == "and" || newNameArray[2].ToLower() == "&")) {
                        // remove first firstName
                        // NOTE: we would normally subtract 1 since this is an index, but this way, we pick up the trailing space
                        compName = compName.Substring(newNameArray[0].Length);
                    }
                }

                // if text contains HTML escape character (like &apos;) remove it
                // unless it's actually for ampersand -- just change that back to an actual ampersand
                compName = NameCtrlSpecChar(compName);

                // if first word is "VFW", replace with "Vete"
                if (compName.ToLower().IndexOf("vfw") == 0) {
                    compName = compName.ToLower().Replace("vfw", "vete");
                }

                // replace "Parent Teacher's Association" with "PTA"
                if (compName.ToLower().IndexOf("parent teacher's association") > -1) {
                    compName = compName.ToLower().Replace("parent teacher's association", "pta");
                }

                // remove special characters
                Regex regex = new Regex(@"/|\ |\-|\&|[0-9]|[a-z]|[A-Z]|/g", RegexOptions.IgnoreCase);
                char[] badCharArray = compName.Where(c => !regex.IsMatch(c.ToString())).ToArray();
                foreach (char c in badCharArray) {
                    compName = compName.Replace(c.ToString(), "");
                }

                // create name control from remaining company name
                int nameCtrlLength = 4;
                if (compName.Length < nameCtrlLength) {
                    nameCtrlLength = compName.Length;
                }
                nameControl = compName.Trim().ToUpper().Replace(" ", "").Substring(0, nameCtrlLength);


                return nameControl;
            }
        }


        private static string NameCtrlSpecChar(string compName) {
            // if text contains HTML escape character (like &apos;) remove it
            // unless it's actually for ampersand -- just change that back to an actual ampersand
            if (compName.Contains("&") && compName.Contains(";")) {
                if (compName.ToLower().Contains("&amp;")) {
                    compName.ToLower().Replace("&amp;", "&");
                } else {
                    int iStart = compName.IndexOf("&");
                    int iEnd = compName.IndexOf(";");
                    string beginning = compName.Substring(0, iStart);
                    string ending = compName.Substring(iEnd + 1);
                    compName = beginning + ending;
                }
            }
            return compName;
        }


        // handle string conversion, even if null
        public static string NullToString(ExcelRangeBase val, bool stripPeriods) {
            string result = "";
            if (val.Text != null) {
                result = val.Text;
                result = removeSpecialChars(result, stripPeriods);
                result = result.Trim();
            }

            return result;
        }

        
        // remove periods and commas
        public static string StripCommasAndPeriods(string text) {
            // remove diacritics (accented letters, etc)
            text = removeDiacritics(text);
            text = text.Replace(".", "").Replace(",", "");

            return text;
        }


        // get submission timestamp
        //public static DateTime 


        // HELPERS
        private static bool isState(string field) {
            bool result = false;
            string[] states = GetStates();
            if (states.Contains(field.ToUpper())) { // if the field is in the state list, return true
                result = true;
            }
            
            return result;
        }

        private static string removeSpecialChars(string text, bool stripPeriods) {
            // only make changes if 'text' is not a date
            DateTime testDate = new DateTime();
            if (!DateTime.TryParse(text, out testDate)) {
                if (text.Contains("&") && !hasHtmlEnts(text)) { // remove ampersand
                    text = text.Replace("&", "&amp;");
                }
                // remove other prohibited characters
                text = text.Replace("--", "-").Replace("#", "").Replace("'", "&apos;").Replace("\"", "&quot;").Replace("<", "&lt;")
                    .Replace(">", "&gt;").Replace("/", "").Replace("\\", "").Replace("  ", " ").Replace("  ", " ").Replace(":", "")
                    .Replace("(", "").Replace(")", "");

                // remove diacritics (accented letters, etc)
                text = removeDiacritics(text);

                // remove whitespace
                text = Regex.Replace(text, @"\s+", " ");

                if (stripPeriods) {
                    text = text.Replace(".", "");
                }
            }

            return text;
        }

        public static string RemoveEscapedChars(string text) {
            if (hasHtmlEnts(text)) {
                text = text.Replace("&amp;", "").Replace("&apos;", "").Replace("&quot;", "").Replace("&lt;", "").Replace("&gt;", "");
                // remove whitespace
                text = Regex.Replace(text, @"\s+", " ");
            }
            return text;
        }

        private static string removeDiacritics(string text) {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString) {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark) {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        private static bool hasHtmlEnts(string text) {
            bool result = false;
            if (text.Contains("&amp;") || text.Contains("&apos;") || text.Contains("&quot;") || text.Contains("&lt;") || text.Contains("&gt;")) {
                result = true;
            }

            return result;
        }

    } // end class
} // end namespace