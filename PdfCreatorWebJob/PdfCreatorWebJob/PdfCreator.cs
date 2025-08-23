using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Net.Http;
using EbsClassPkg.Models;
using EbsClassPkg.Controllers;

using Microsoft.Azure; // Namespace for Azure Configuration Manager
using Microsoft.WindowsAzure.Storage; // Namespace for Storage Client Library
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage
using Microsoft.WindowsAzure.Storage.File; // Namespace for File storage

namespace PdfCreatorWebJob {
    public class PdfCreator : Controller
    {

        public static bool Fill1095cToTrulyLocalFile(byte[] templateFile, Record rec, string dir, string email, bool isCorrection = false) {
            bool result = false;

            // so, need to 
            List<string> pdfFilePaths = new List<string>();

            if (rec != null && rec.IndividualReports != null && rec.IndividualReports.Count > 0) {
                int counter = 1; // this will ensure the files are named sequentially
                foreach (IndividualReport report in rec.IndividualReports) {
                    // create a coversheet for each employee and return its filepath
                    string coversheetPath = createAndSaveCoversheet(rec, report, counter, dir);
                    pdfFilePaths.Add(coversheetPath);
                    counter++;
                    // create a pdf for each employee record.  Save each of them in a directory and return filepath
                    string pdfFilePath = createAndSave1095pdf(templateFile, rec, report, isCorrection, counter, dir);
                    pdfFilePaths.Add(pdfFilePath);
                    counter++;
                }

                // pull the finished files into a function that will wrap them in one (or more) large file(s)
                result = consolidateFilesLocally(pdfFilePaths, dir, rec.Company.Name, email);

            } 

            return result;
        }


        // make sure the form has space for more than 13 covered individuals (some tax years don't)
        private static bool formHasMoreThan13Individuals(int taxYr)
        {
            if (taxYr < 2020)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private static string getName(Person person) {
            string name = person.FirstName;
            if (!String.IsNullOrEmpty(person.MiddleName)) {
                name += " " + person.MiddleName;
            }
            name += " " + person.LastName;
            if (!String.IsNullOrEmpty(person.Suffix)) {
                name += " " + person.Suffix;
            }

            return name;
        }


        private static string maskSSN(string ssn) {
            string mask = "";

            // get last 4 digits of ssn
            if (ssn.Length >= 4) {
                mask = "XXX-XX-" + ssn.Substring(ssn.Length - 4);
            }

            return mask;
        }

        private static string getAddress(Address add) {
            string addText = "";
            addText = add.Address1 + " " + add.Address2;

            return addText;
        }

        private static string getCountryZip(Address add) {
            string czip = "";

            if (!String.IsNullOrEmpty(add.Country)) {
                czip = add.Country + " ";
            } else {
                czip = "US ";
            }

            czip += add.Zip.ToString();

            if (!String.IsNullOrEmpty(add.ZipExt.ToString()) && add.ZipExt > 0) {
                czip += "-" + add.ZipExt.ToString();
            }

            return czip;
        }

        // consolidate the files into one (or more?) file(s) // THIS WRITES TO A LOCAL FILE!
        private static bool consolidateFilesLocally(List<string> filePaths, string directory, string compName, string email)
        {
            bool result = false;
            using (var output = new FileStream(directory + "/consolidated.pdf", FileMode.Create))
            {
                using (BufferedStream bs = new BufferedStream(output))
                {
                    Document doc = new Document(PageSize.A4); // create a document

                    var writer = new PdfSmartCopy(doc, output); // create a writer that listens to the doc
                    writer.CloseStream = false;
                    doc.Open();
                    foreach (string file in filePaths)
                    {
                        // create a reader for each file
                        var reader = new PdfReader(file);
                        for (int i = 1; i <= reader.NumberOfPages; i++)
                        { // the index is 1-based, not 0
                          //if (i == 1 && reader.NumberOfPages > 3) {
                          // don't add the page (it's a cover page from the IRS)
                          //} else {
                            PdfImportedPage page = writer.GetImportedPage(reader, i);
                            writer.AddPage(page);
                            //}
                        }
                        writer.FreeReader(reader);
                        reader.Close();
                        // delete the file
                        System.IO.File.Delete(file);
                    }
                    writer.Close();
                    doc.Close();

                    result = true;
                } // end using BufferedStream

            } // end using FileStream

            return result;
        }

        // DEPRECATED
        // consolidate the files into one (or more?) file(s) // THIS WRITES TO A FILE!
        private static string consolidateFiles(List<string> filePaths, string directory, string compName, string email) {
            string result = null;
            using (var output = new FileStream(directory + "/consolidated.pdf", FileMode.Create)) {
                using (BufferedStream bs = new BufferedStream(output)) {
                    Document doc = new Document(PageSize.A4); // create a document

                    var writer = new PdfSmartCopy(doc, output); // create a writer that listens to the doc
                    writer.CloseStream = false;
                    doc.Open();
                    foreach (string file in filePaths) {
                        // create a reader for each file
                        var reader = new PdfReader(file);
                        for (int i = 1; i <= reader.NumberOfPages; i++) { // the index is 1-based, not 0
                            //if (i == 1 && reader.NumberOfPages > 3) {
                                // don't add the page (it's a cover page from the IRS)
                            //} else {
                                PdfImportedPage page = writer.GetImportedPage(reader, i);
                                writer.AddPage(page);
                            //}
                        }
                        writer.FreeReader(reader);
                        reader.Close();
                        // delete the file
                        System.IO.File.Delete(file);
                    }
                    writer.Close();
                    doc.Close();

                    // get user's company and save file in that company's FormBuilder folder
                    VaultItem formBldrFolder = HubDataManager.GetFormBuilderFolderFromUserEmail(email);

                    // attempt to save file to file storage
                    // Parse the connection string and return a reference to the storage account.
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                        CloudConfigurationManager.GetSetting("StorageConnectionString"));

                    // Create a CloudFileClient object for credentialed access to File storage.
                    CloudFileClient fileClient = storageAccount.CreateCloudFileClient();

                    // Get a reference to the file share we created previously.
                    CloudFileShare share = fileClient.GetShareReference("hubshare");

                    // Ensure that the share exists.
                    if (share.Exists()) {
                        // Get a reference to the root directory for the share.
                        CloudFileDirectory rootDir = share.GetRootDirectoryReference();

                        string formBldrFolderName = "";
                        // Ensure that the directory exists.
                        if (rootDir.Exists()) {

                            List<IListFileItem> contents = rootDir.ListFilesAndDirectories().ToList();
                            foreach (var item in contents) {
                                if (item.Uri.ToString() == formBldrFolder.VaultURL) {
                                    CloudFileDirectory cfd = (CloudFileDirectory)item;
                                    formBldrFolderName = cfd.Name;
                                    break;
                                }
                            }

                            CloudFileDirectory formBuilderFolder = rootDir.GetDirectoryReference(formBldrFolderName);

                            if (formBuilderFolder.Exists()) {
                                string fileName = compName.Replace(' ', '_') + "_" + DateTime.Now.ToString("yyyy-MM-dd_H-mm-ss") + ".pdf";
                                CloudFile consolidatedFile = rootDir.GetFileReference(fileName);
                                output.Position = 0;
                                consolidatedFile.UploadFromStreamAsync(output).Wait();
                                result = consolidatedFile.Name;

                                // add a record of this new file to the db
                                User u = new EbsClassPkg.Models.User();
                                HubDataManager.InsertVaultItem(fileName, true, false, consolidatedFile.Uri.ToString(), "pdf", formBldrFolder.CompID, null, formBldrFolder.ID, u);
                            }
                        }
                    }
                } // end using BufferedStream

            } // end using FileStream

            //return "<div><a target='_blank' href='" + directory + "/consolidated.pdf" + "'>File</a></div>";
            //return directory + "/consolidated.pdf";
            return result;
        }

        // get compiled address line 1
        private static string getCompiledAddressLine1(Address addy) {
            string add1 = addy.Address1;
            if (!String.IsNullOrEmpty(addy.Address2)) {
                add1 += ", " + addy.Address2;
            }

            return add1;
        }

        // compiled address line 2
        private static string getCompiledAddressLine2(Address addy) {
            string add2 = addy.City + ", " + addy.State + " " + addy.Zip.ToString();
            if (!String.IsNullOrEmpty(addy.ZipExt.ToString()) && addy.ZipExt > 0) {
                add2 += "-" + addy.ZipExt.ToString();
            }
            return add2;
        }

        private static string getCompiledPersonsName(Person p) {
            string name = p.FirstName;
            if (!String.IsNullOrEmpty(p.MiddleName)) {
                name += " " + p.MiddleName;
            }
            name += " " + p.LastName;
            if (!String.IsNullOrEmpty(p.Suffix)) {
                name += " " + p.Suffix;
            }
            return name;
        }

        // if value is 0, return empty string
        private static string floatToString(float val) {
            string result = "";
            if (val != 0) {
                result = val.ToString("0.00");
            }

            return result;
        }


        // create a coversheet for the employee, save it, and return filepath
        private static string createAndSaveCoversheet(Record rec, IndividualReport report, int fileNum, string dir) {
            // good tutorial: http://www.4guysfromrolla.com/articles/030911-1.aspx
            string coversheetFilepath = dir + "/file_" + fileNum.ToString() + ".pdf";

            // create the coversheet
            Document coversheet = new Document(PageSize.LETTER, 70, 70, 40, 65); // these values confirmed by the printer 10/4/16

            using (var output = new FileStream(coversheetFilepath, FileMode.Create)) {
                var writer = PdfWriter.GetInstance(coversheet, output);
                // open the document for writing
                coversheet.Open();
                // create the fonts
                var headerFooterFont = FontFactory.GetFont("Calibri", 7, Font.NORMAL);
                var bodyFont = FontFactory.GetFont("Calibri", 8, Font.NORMAL);

                // add some space at the top (for envelope window)
                coversheet.Add(new Paragraph(" "));

                // create company address text
                coversheet.Add(new Paragraph(rec.Company.Name + rec.Company.Name2, headerFooterFont));
                coversheet.Add(new Paragraph(getCompiledAddressLine1(rec.Company.Address), headerFooterFont));
                coversheet.Add(new Paragraph(getCompiledAddressLine2(rec.Company.Address), headerFooterFont));

                // get some space in the doc
                coversheet.Add(Chunk.NEWLINE);
                coversheet.Add(Chunk.NEWLINE);
                coversheet.Add(Chunk.NEWLINE);
                coversheet.Add(Chunk.NEWLINE);
                coversheet.Add(Chunk.NEWLINE);
                coversheet.Add(Chunk.NEWLINE);
                coversheet.Add(Chunk.NEWLINE);
                coversheet.Add(Chunk.NEWLINE);
                coversheet.Add(Chunk.NEWLINE);
                coversheet.Add(Chunk.NEWLINE);
                coversheet.Add(Chunk.NEWLINE);

                // get individual's info
                coversheet.Add(new Paragraph(getCompiledPersonsName(report.Person), bodyFont));
                coversheet.Add(new Paragraph(getCompiledAddressLine1(report.Person.Address), bodyFont));
                coversheet.Add(new Paragraph(getCompiledAddressLine2(report.Person.Address), bodyFont));

                // add "Cover Page" footer
                var bottomPos = coversheet.Bottom;
                var rightPos = coversheet.Right;
                PdfContentByte footer = writer.DirectContent;
                footer.BeginText(); // tells the object that we'll be drawing text
                Font f = new Font();
                footer.SetFontAndSize(f.GetCalculatedBaseFont(false), 7);
                footer.SetTextMatrix(rightPos - 50, bottomPos);
                footer.ShowText("Cover Page");
                footer.EndText(); // done

                // add a second (blank) page
                coversheet.NewPage();
                coversheet.Add(Chunk.NEWLINE);

                coversheet.Close(); // finished
            }

            return coversheetFilepath;
        }


        // fill pdf
        private static string createAndSave1095pdf(byte[] templateFile, Record rec, IndividualReport report, bool isCorrection, 
            int fileNum, string dir) {
            // good tutorial: http://www.4guysfromrolla.com/articles/030211-1.aspx
            string savedFilePath = dir + "/file_" + fileNum.ToString() + ".pdf";

            var reader = new PdfReader(templateFile);

            using (var output = new FileStream(savedFilePath, FileMode.Create)) {
                var stamper = new PdfStamper(reader, output);
                
                var i = FieldManager.GetFieldId((int)Structures.FormFields.FirstName, 2017);
                
                // SET THE FIELDS

                //
                // PAGE 1
                //
                // PART 1
                // void flag // in the 1095 spreadsheet, column OO might contain an "IS_VOID" flag...
                if (report.IsVoidedForm) {
                    stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.Void, rec.TaxYr), "1");
                }
                
                // corrected flag
                if (report.IsCorrectedForm) {
                    stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.Corrected, rec.TaxYr), "2");
                } 

                // name
                if (rec.TaxYr == 2016 || rec.TaxYr == 2017) { // name was just one field
                    stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.EmployeeName, rec.TaxYr), getName(report.Person));
                } else { // name is 3 fields
                    stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.FirstName, rec.TaxYr), report.Person.FirstName);
                    if (!String.IsNullOrEmpty(report.Person.MiddleName))
                    {
                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.MiddleInitial, rec.TaxYr), report.Person.MiddleName.Substring(0, 1));
                    }
                    stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.LastName, rec.TaxYr), report.Person.LastName);
                }
                
                // other personal details
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.SSN, rec.TaxYr), maskSSN(report.Person.SSN)); // ssn
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.Street, rec.TaxYr), getAddress(report.Person.Address)); // address
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.City, rec.TaxYr), report.Person.Address.City); // city
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.State, rec.TaxYr), report.Person.Address.State); // state
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.Zip, rec.TaxYr), getCountryZip(report.Person.Address)); // country / zip
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.EmployerName, rec.TaxYr), report.EmployerName); // employer
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.EmployerEIN, rec.TaxYr), report.EmployerEIN); // ein
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.EmployerStreet, rec.TaxYr), getAddress(report.EmployerAddress)); // addy
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.ContactPhone, rec.TaxYr), report.EmployerPhone); // contact phone
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.EmployerCity, rec.TaxYr), report.EmployerAddress.City);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.EmployerState, rec.TaxYr), report.EmployerAddress.State); // state
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.EmployerZip, rec.TaxYr), getCountryZip(report.EmployerAddress)); // country and zip

                // PART 2
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.AgeOnJan1, rec.TaxYr), report.EmplyeeAgeOnJan1); // age on Jan 1

                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.PlanStartMonth, rec.TaxYr), report.PlanStartMonth); // plan start month

                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferOfCoverageYr, rec.TaxYr), report.CoverageCodeYr); // offer yr
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferOfCoverageJan, rec.TaxYr), report.CoverageCodeJan);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferOfCoverageFeb, rec.TaxYr), report.CoverageCodeFeb);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferOfCoverageMar, rec.TaxYr), report.CoverageCodeMar);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferOfCoverageApr, rec.TaxYr), report.CoverageCodeApr);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferOfCoverageMay, rec.TaxYr), report.CoverageCodeMay);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferOfCoverageJun, rec.TaxYr), report.CoverageCodeJun);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferOfCoverageJul, rec.TaxYr), report.CoverageCodeJul);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferOfCoverageAug, rec.TaxYr), report.CoverageCodeAug);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferOfCoverageSep, rec.TaxYr), report.CoverageCodeSep);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferOfCoverageOct, rec.TaxYr), report.CoverageCodeOct);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferOfCoverageNov, rec.TaxYr), report.CoverageCodeNov);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferOfCoverageDec, rec.TaxYr), report.CoverageCodeDec);

                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.EmployeeReqContribYr, rec.TaxYr), floatToString(report.LowCostYr));
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.EmployeeReqContribJan, rec.TaxYr), floatToString(report.LowCostJan));
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.EmployeeReqContribFeb, rec.TaxYr), floatToString(report.LowCostFeb));
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.EmployeeReqContribMar, rec.TaxYr), floatToString(report.LowCostMar));
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.EmployeeReqContribApr, rec.TaxYr), floatToString(report.LowCostApr));
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.EmployeeReqContribMay, rec.TaxYr), floatToString(report.LowCostMay));
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.EmployeeReqContribJun, rec.TaxYr), floatToString(report.LowCostJun));
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.EmployeeReqContribJul, rec.TaxYr), floatToString(report.LowCostJul));
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.EmployeeReqContribAug, rec.TaxYr), floatToString(report.LowCostAug));
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.EmployeeReqContribSep, rec.TaxYr), floatToString(report.LowCostSep));
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.EmployeeReqContribOct, rec.TaxYr), floatToString(report.LowCostOct));
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.EmployeeReqContribNov, rec.TaxYr), floatToString(report.LowCostNov));
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.EmployeeReqContribDec, rec.TaxYr), floatToString(report.LowCostDec));

                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.Sec4980Yr, rec.TaxYr), report.SafeHarborYr);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.Sec4980Jan, rec.TaxYr), report.SafeHarborJan);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.Sec4980Feb, rec.TaxYr), report.SafeHarborFeb);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.Sec4980Mar, rec.TaxYr), report.SafeHarborMar);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.Sec4980Apr, rec.TaxYr), report.SafeHarborApr);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.Sec4980May, rec.TaxYr), report.SafeHarborMay);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.Sec4980Jun, rec.TaxYr), report.SafeHarborJun);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.Sec4980Jul, rec.TaxYr), report.SafeHarborJul);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.Sec4980Aug, rec.TaxYr), report.SafeHarborAug);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.Sec4980Sep, rec.TaxYr), report.SafeHarborSep);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.Sec4980Oct, rec.TaxYr), report.SafeHarborOct);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.Sec4980Nov, rec.TaxYr), report.SafeHarborNov);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.Sec4980Dec, rec.TaxYr), report.SafeHarborDec);

                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferZipCdAnn, rec.TaxYr), report.IchraZipCdYr);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferZipCdJan, rec.TaxYr), report.IchraZipCdJan);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferZipCdFeb, rec.TaxYr), report.IchraZipCdFeb);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferZipCdMar, rec.TaxYr), report.IchraZipCdMar);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferZipCdApr, rec.TaxYr), report.IchraZipCdApr);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferZipCdMay, rec.TaxYr), report.IchraZipCdMay);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferZipCdJun, rec.TaxYr), report.IchraZipCdJun);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferZipCdJul, rec.TaxYr), report.IchraZipCdJul);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferZipCdAug, rec.TaxYr), report.IchraZipCdAug);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferZipCdSep, rec.TaxYr), report.IchraZipCdSep);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferZipCdOct, rec.TaxYr), report.IchraZipCdOct);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferZipCdNov, rec.TaxYr), report.IchraZipCdNov);
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.OfferZipCdDec, rec.TaxYr), report.IchraZipCdDec);

                // PART 3
                if (report.EmployerProvidedSelfInsured) {
                    // covered individuals check box
                    stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.EmployerProvidedSelfInsured, rec.TaxYr), "1");
                }
                
                if (report.CoveredIndividuals != null && report.CoveredIndividuals.Count > 0) {
                    // fill in covered individuals' info
                    // can't do a loop here, because the fields have specific IDs
                    if (rec.TaxYr == 2016 || rec.TaxYr == 2017)
                    {
                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndName_1, rec.TaxYr),
                            getName(report.CoveredIndividuals.First().Person));
                    }
                    else {
                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFirstName_1, rec.TaxYr),
                            report.CoveredIndividuals.First().Person.FirstName);
                        if (!String.IsNullOrEmpty(report.CoveredIndividuals.First().Person.MiddleName))
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMiddleInitial_1, rec.TaxYr),
                                report.CoveredIndividuals.First().Person.MiddleName.Substring(0, 1));
                        }
                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndLastName_1, rec.TaxYr),
                            report.CoveredIndividuals.First().Person.LastName);
                    }
                    
                    stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSSN_1, rec.TaxYr),
                        maskSSN(report.CoveredIndividuals.First().Person.SSN));
                    if (String.IsNullOrEmpty(report.CoveredIndividuals.First().Person.SSN) &&
                            !String.IsNullOrEmpty(report.CoveredIndividuals.First().Person.DOB.ToShortDateString())) {
                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDOB_1, rec.TaxYr),
                            report.CoveredIndividuals.First().Person.DOB.ToShortDateString());
                    }
                    // all the check boxes...
                    if (report.CoveredIndividuals.First().IsCoveredYr) {
                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndYr_1, rec.TaxYr), "1");
                    }
                    if (report.CoveredIndividuals.First().IsCoveredJan) {
                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJan_1, rec.TaxYr), "1");
                    }
                    if (report.CoveredIndividuals.First().IsCoveredFeb) {
                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFeb_1, rec.TaxYr), "1");
                    }
                    if (report.CoveredIndividuals.First().IsCoveredMar) {
                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMar_1, rec.TaxYr), "1");
                    }
                    if (report.CoveredIndividuals.First().IsCoveredApr) {
                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndApr_1, rec.TaxYr), "1");
                    }
                    if (report.CoveredIndividuals.First().IsCoveredMay) {
                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMay_1, rec.TaxYr), "1");
                    }
                    if (report.CoveredIndividuals.First().IsCoveredJun) {
                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJun_1, rec.TaxYr), "1");
                    }
                    if (report.CoveredIndividuals.First().IsCoveredJul) {
                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJul_1, rec.TaxYr), "1");
                    }
                    if (report.CoveredIndividuals.First().IsCoveredAug) {
                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndAug_1, rec.TaxYr), "1");
                    }
                    if (report.CoveredIndividuals.First().IsCoveredSep) {
                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSep_1, rec.TaxYr), "1");
                    }
                    if (report.CoveredIndividuals.First().IsCoveredOct) {
                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndOct_1, rec.TaxYr), "1");
                    }
                    if (report.CoveredIndividuals.First().IsCoveredNov) {
                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndNov_1, rec.TaxYr), "1");
                    }
                    if (report.CoveredIndividuals.First().IsCoveredDec) {
                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDec_1, rec.TaxYr), "1");
                    }

                    // 2nd individual
                    if (report.CoveredIndividuals.Count > 1) {
                        if (rec.TaxYr == 2016 || rec.TaxYr == 2017)
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndName_2, rec.TaxYr),
                                getName(report.CoveredIndividuals.ElementAt(1).Person));
                        }
                        else
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFirstName_2, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(1).Person.FirstName);
                            if (!String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(1).Person.MiddleName))
                            {
                                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMiddleInitial_2, rec.TaxYr),
                                    report.CoveredIndividuals.ElementAt(1).Person.MiddleName.Substring(0, 1));
                            }
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndLastName_2, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(1).Person.LastName);
                        }
                        
                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSSN_2, rec.TaxYr),
                            maskSSN(report.CoveredIndividuals.ElementAt(1).Person.SSN));
                        if (String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(1).Person.SSN) &&
                                !String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(1).Person.DOB.ToShortDateString())) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDOB_2, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(1).Person.DOB.ToShortDateString());
                        }
                        // all the check boxes...
                        if (report.CoveredIndividuals.ElementAt(1).IsCoveredYr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndYr_2, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(1).IsCoveredJan) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJan_2, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(1).IsCoveredFeb) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFeb_2, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(1).IsCoveredMar) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMar_2, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(1).IsCoveredApr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndApr_2, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(1).IsCoveredMay) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMay_2, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(1).IsCoveredJun) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJun_2, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(1).IsCoveredJul) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJul_2, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(1).IsCoveredAug) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndAug_2, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(1).IsCoveredSep) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSep_2, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(1).IsCoveredOct) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndOct_2, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(1).IsCoveredNov) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndNov_2, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(1).IsCoveredDec) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDec_2, rec.TaxYr), "1");
                        }
                    }

                    // 3rd individual
                    if (report.CoveredIndividuals.Count > 2) {
                        if (rec.TaxYr == 2016 || rec.TaxYr == 2017)
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndName_3, rec.TaxYr),
                                getName(report.CoveredIndividuals.ElementAt(2).Person));
                        }
                        else
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFirstName_3, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(2).Person.FirstName);
                            if (!String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(2).Person.MiddleName))
                            {
                                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMiddleInitial_3, rec.TaxYr),
                                    report.CoveredIndividuals.ElementAt(2).Person.MiddleName.Substring(0, 1));
                            }
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndLastName_3, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(2).Person.LastName);
                        }
                        
                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSSN_3, rec.TaxYr),
                            maskSSN(report.CoveredIndividuals.ElementAt(2).Person.SSN));
                        if (String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(2).Person.SSN) &&
                                !String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(2).Person.DOB.ToShortDateString())) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDOB_3, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(2).Person.DOB.ToShortDateString());
                        }
                        // all the check boxes...
                        if (report.CoveredIndividuals.ElementAt(2).IsCoveredYr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndYr_3, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(2).IsCoveredJan) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJan_3, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(2).IsCoveredFeb) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFeb_3, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(2).IsCoveredMar) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMar_3, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(2).IsCoveredApr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndApr_3, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(2).IsCoveredMay) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMay_3, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(2).IsCoveredJun) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJun_3, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(2).IsCoveredJul) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJul_3, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(2).IsCoveredAug) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndAug_3, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(2).IsCoveredSep) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSep_3, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(2).IsCoveredOct) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndOct_3, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(2).IsCoveredNov) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndNov_3, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(2).IsCoveredDec) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDec_3, rec.TaxYr), "1");
                        }
                    }

                    // 4th individual
                    if (report.CoveredIndividuals.Count > 3) {
                        if (rec.TaxYr == 2016 || rec.TaxYr == 2017)
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndName_4, rec.TaxYr),
                                getName(report.CoveredIndividuals.ElementAt(3).Person));
                        }
                        else
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFirstName_4, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(3).Person.FirstName);
                            if (!String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(3).Person.MiddleName))
                            {
                                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMiddleInitial_4, rec.TaxYr),
                                    report.CoveredIndividuals.ElementAt(3).Person.MiddleName.Substring(0, 1));
                            }
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndLastName_4, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(3).Person.LastName);
                        }
                        
                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSSN_4, rec.TaxYr),
                            maskSSN(report.CoveredIndividuals.ElementAt(3).Person.SSN));
                        if (String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(3).Person.SSN) &&
                                !String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(3).Person.DOB.ToShortDateString())) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDOB_4, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(3).Person.DOB.ToShortDateString());
                        }
                        // all the check boxes...
                        if (report.CoveredIndividuals.ElementAt(3).IsCoveredYr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndYr_4, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(3).IsCoveredJan) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJan_4, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(3).IsCoveredFeb) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFeb_4, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(3).IsCoveredMar) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMar_4, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(3).IsCoveredApr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndApr_4, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(3).IsCoveredMay) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMay_4, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(3).IsCoveredJun) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJun_4, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(3).IsCoveredJul) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJul_4, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(3).IsCoveredAug) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndAug_4, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(3).IsCoveredSep) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSep_4, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(3).IsCoveredOct) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndOct_4, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(3).IsCoveredNov) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndNov_4, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(3).IsCoveredDec) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDec_4, rec.TaxYr), "1");
                        }
                    }

                    // 5th individual
                    if (report.CoveredIndividuals.Count > 4) {
                        if (rec.TaxYr == 2016 || rec.TaxYr == 2017)
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndName_5, rec.TaxYr),
                                getName(report.CoveredIndividuals.ElementAt(4).Person));
                        }
                        else
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFirstName_5, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(4).Person.FirstName);
                            if (!String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(4).Person.MiddleName))
                            {
                                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMiddleInitial_5, rec.TaxYr),
                                    report.CoveredIndividuals.ElementAt(4).Person.MiddleName.Substring(0, 1));
                            }
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndLastName_5, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(4).Person.LastName);
                        }
                        
                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSSN_5, rec.TaxYr),
                            maskSSN(report.CoveredIndividuals.ElementAt(4).Person.SSN));
                        if (String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(4).Person.SSN) &&
                                !String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(4).Person.DOB.ToShortDateString())) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDOB_5, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(4).Person.DOB.ToShortDateString());
                        }
                        // all the check boxes...
                        if (report.CoveredIndividuals.ElementAt(4).IsCoveredYr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndYr_5, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(4).IsCoveredJan) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJan_5, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(4).IsCoveredFeb) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFeb_5, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(4).IsCoveredMar) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMar_5, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(4).IsCoveredApr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndApr_5, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(4).IsCoveredMay) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMay_5, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(4).IsCoveredJun) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJun_5, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(4).IsCoveredJul) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJul_5, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(4).IsCoveredAug) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndAug_5, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(4).IsCoveredSep) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSep_5, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(4).IsCoveredOct) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndOct_5, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(4).IsCoveredNov) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndNov_5, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(4).IsCoveredDec) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDec_5, rec.TaxYr), "1");
                        }
                    }

                    // 6th individual
                    if (report.CoveredIndividuals.Count > 5) {
                        if (rec.TaxYr == 2016 || rec.TaxYr == 2017)
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndName_6, rec.TaxYr),
                                getName(report.CoveredIndividuals.ElementAt(5).Person));
                        }
                        else
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFirstName_6, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(5).Person.FirstName);
                            if (!String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(5).Person.MiddleName))
                            {
                                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMiddleInitial_6, rec.TaxYr),
                                    report.CoveredIndividuals.ElementAt(5).Person.MiddleName.Substring(0, 1));
                            }
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndLastName_6, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(5).Person.LastName);
                        }
                        
                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSSN_6, rec.TaxYr),
                            maskSSN(report.CoveredIndividuals.ElementAt(5).Person.SSN));
                        if (String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(5).Person.SSN) &&
                                !String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(5).Person.DOB.ToShortDateString())) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDOB_6, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(5).Person.DOB.ToShortDateString());
                        }
                        // all the check boxes...
                        if (report.CoveredIndividuals.ElementAt(5).IsCoveredYr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndYr_6, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(5).IsCoveredJan) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJan_6, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(5).IsCoveredFeb) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFeb_6, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(5).IsCoveredMar) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMar_6, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(5).IsCoveredApr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndApr_6, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(5).IsCoveredMay) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMay_6, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(5).IsCoveredJun) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJun_6, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(5).IsCoveredJul) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJul_6, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(5).IsCoveredAug) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndAug_6, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(5).IsCoveredSep) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSep_6, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(5).IsCoveredOct) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndOct_6, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(5).IsCoveredNov) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndNov_6, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(5).IsCoveredDec) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDec_6, rec.TaxYr), "1");
                        }
                    }

                    // (end of the first page...)

                    // 7th individual (row 1 on page 2; line 23)
                    if (report.CoveredIndividuals.Count > 6) {
                        if (rec.TaxYr == 2016 || rec.TaxYr == 2017)
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndName_7, rec.TaxYr),
                                getName(report.CoveredIndividuals.ElementAt(6).Person));
                        }
                        else
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFirstName_7, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(6).Person.FirstName);
                            if (!String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(6).Person.MiddleName))
                            {
                                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMiddleInitial_7, rec.TaxYr),
                                    report.CoveredIndividuals.ElementAt(6).Person.MiddleName.Substring(0, 1));
                            }
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndLastName_7, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(6).Person.LastName);
                        }

                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSSN_7, rec.TaxYr),
                            maskSSN(report.CoveredIndividuals.ElementAt(6).Person.SSN));
                        if (String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(6).Person.SSN) &&
                                !String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(6).Person.DOB.ToShortDateString())) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDOB_7, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(6).Person.DOB.ToShortDateString());
                        }
                        // all the check boxes...
                        if (report.CoveredIndividuals.ElementAt(6).IsCoveredYr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndYr_7, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(6).IsCoveredJan) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJan_7, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(6).IsCoveredFeb) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFeb_7, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(6).IsCoveredMar) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMar_7, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(6).IsCoveredApr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndApr_7, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(6).IsCoveredMay) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMay_7, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(6).IsCoveredJun) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJun_7, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(6).IsCoveredJul) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJul_7, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(6).IsCoveredAug) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndAug_7, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(6).IsCoveredSep) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSep_7, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(6).IsCoveredOct) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndOct_7, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(6).IsCoveredNov) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndNov_7, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(6).IsCoveredDec) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDec_7, rec.TaxYr), "1");
                        }
                    }

                    // 8th individual (row 2 on page 2; line 24)
                    if (report.CoveredIndividuals.Count > 7) {
                        if (rec.TaxYr == 2016 || rec.TaxYr == 2017)
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndName_8, rec.TaxYr),
                                getName(report.CoveredIndividuals.ElementAt(7).Person));
                        }
                        else
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFirstName_8, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(7).Person.FirstName);
                            if (!String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(7).Person.MiddleName))
                            {
                                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMiddleInitial_8, rec.TaxYr),
                                    report.CoveredIndividuals.ElementAt(7).Person.MiddleName.Substring(0, 1));
                            }
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndLastName_8, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(7).Person.LastName);
                        }

                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSSN_8, rec.TaxYr),
                            maskSSN(report.CoveredIndividuals.ElementAt(7).Person.SSN));
                        if (String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(7).Person.SSN) &&
                                !String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(7).Person.DOB.ToShortDateString())) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDOB_8, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(7).Person.DOB.ToShortDateString());
                        }
                        // all the check boxes...
                        if (report.CoveredIndividuals.ElementAt(7).IsCoveredYr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndYr_8, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(7).IsCoveredJan) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJan_8, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(7).IsCoveredFeb) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFeb_8, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(7).IsCoveredMar) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMar_8, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(7).IsCoveredApr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndApr_8, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(7).IsCoveredMay) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMay_8, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(7).IsCoveredJun) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJun_8, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(7).IsCoveredJul) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJul_8, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(7).IsCoveredAug) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndAug_8, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(7).IsCoveredSep) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSep_8, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(7).IsCoveredOct) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndOct_8, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(7).IsCoveredNov) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndNov_8, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(7).IsCoveredDec) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDec_8, rec.TaxYr), "1");
                        }
                    }

                    // 9th individual (row 3 on page 2; line 25)
                    if (report.CoveredIndividuals.Count > 8) {
                        if (rec.TaxYr == 2016 || rec.TaxYr == 2017)
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndName_9, rec.TaxYr),
                                getName(report.CoveredIndividuals.ElementAt(8).Person));
                        }
                        else
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFirstName_9, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(8).Person.FirstName);
                            if (!String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(8).Person.MiddleName))
                            {
                                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMiddleInitial_9, rec.TaxYr),
                                    report.CoveredIndividuals.ElementAt(8).Person.MiddleName.Substring(0, 1));
                            }
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndLastName_9, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(8).Person.LastName);
                        }

                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSSN_9, rec.TaxYr),
                            maskSSN(report.CoveredIndividuals.ElementAt(8).Person.SSN));
                        if (String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(8).Person.SSN) &&
                                !String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(8).Person.DOB.ToShortDateString())) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDOB_9, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(8).Person.DOB.ToShortDateString());
                        }
                        // all the check boxes...
                        if (report.CoveredIndividuals.ElementAt(8).IsCoveredYr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndYr_9, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(8).IsCoveredJan) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJan_9, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(8).IsCoveredFeb) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFeb_9, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(8).IsCoveredMar) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMar_9, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(8).IsCoveredApr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndApr_9, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(8).IsCoveredMay) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMay_9, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(8).IsCoveredJun) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJun_9, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(8).IsCoveredJul) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJul_9, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(8).IsCoveredAug) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndAug_9, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(8).IsCoveredSep) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSep_9, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(8).IsCoveredOct) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndOct_9, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(8).IsCoveredNov) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndNov_9, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(8).IsCoveredDec) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDec_9, rec.TaxYr), "1");
                        }
                    }

                    // 10th individual (row 4 on page 2; line 26)
                    if (report.CoveredIndividuals.Count > 9) {
                        if (rec.TaxYr == 2016 || rec.TaxYr == 2017)
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndName_10, rec.TaxYr),
                                getName(report.CoveredIndividuals.ElementAt(9).Person));
                        }
                        else
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFirstName_10, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(9).Person.FirstName);
                            if (!String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(9).Person.MiddleName))
                            {
                                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMiddleInitial_10, rec.TaxYr),
                                    report.CoveredIndividuals.ElementAt(9).Person.MiddleName.Substring(0, 1));
                            }
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndLastName_10, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(9).Person.LastName);
                        }

                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSSN_10, rec.TaxYr),
                            maskSSN(report.CoveredIndividuals.ElementAt(9).Person.SSN));
                        if (String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(9).Person.SSN) &&
                                !String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(9).Person.DOB.ToShortDateString())) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDOB_10, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(9).Person.DOB.ToShortDateString());
                        }
                        // all the check boxes...
                        if (report.CoveredIndividuals.ElementAt(9).IsCoveredYr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndYr_10, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(9).IsCoveredJan) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJan_10, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(9).IsCoveredFeb) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFeb_10, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(9).IsCoveredMar) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMar_10, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(9).IsCoveredApr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndApr_10, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(9).IsCoveredMay) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMay_10, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(9).IsCoveredJun) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJun_10, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(9).IsCoveredJul) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJul_10, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(9).IsCoveredAug) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndAug_10, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(9).IsCoveredSep) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSep_10, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(9).IsCoveredOct) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndOct_10, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(9).IsCoveredNov) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndNov_10, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(9).IsCoveredDec) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDec_10, rec.TaxYr), "1");
                        }
                    }

                    // 11th individual (row 5 on page 2; line 27)
                    if (report.CoveredIndividuals.Count > 10) {
                        if (rec.TaxYr == 2016 || rec.TaxYr == 2017)
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndName_11, rec.TaxYr),
                                getName(report.CoveredIndividuals.ElementAt(10).Person));
                        }
                        else
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFirstName_11, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(10).Person.FirstName);
                            if (!String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(10).Person.MiddleName))
                            {
                                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMiddleInitial_11, rec.TaxYr),
                                    report.CoveredIndividuals.ElementAt(10).Person.MiddleName.Substring(0, 1));
                            }
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndLastName_11, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(10).Person.LastName);
                        }

                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSSN_11, rec.TaxYr),
                            maskSSN(report.CoveredIndividuals.ElementAt(10).Person.SSN));
                        if (String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(10).Person.SSN) &&
                                !String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(10).Person.DOB.ToShortDateString())) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDOB_11, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(10).Person.DOB.ToShortDateString());
                        }
                        // all the check boxes...
                        if (report.CoveredIndividuals.ElementAt(10).IsCoveredYr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndYr_11, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(10).IsCoveredJan) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJan_11, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(10).IsCoveredFeb) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFeb_11, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(10).IsCoveredMar) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMar_11, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(10).IsCoveredApr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndApr_11, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(10).IsCoveredMay) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMay_11, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(10).IsCoveredJun) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJun_11, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(10).IsCoveredJul) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJul_11, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(10).IsCoveredAug) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndAug_11, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(10).IsCoveredSep) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSep_11, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(10).IsCoveredOct) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndOct_11, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(10).IsCoveredNov) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndNov_11, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(10).IsCoveredDec) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDec_11, rec.TaxYr), "1");
                        }
                    }

                    // 12th individual (row 6 on page 2; line 28)
                    if (report.CoveredIndividuals.Count > 11) {
                        if (rec.TaxYr == 2016 || rec.TaxYr == 2017)
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndName_12, rec.TaxYr),
                                getName(report.CoveredIndividuals.ElementAt(11).Person));
                        }
                        else
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFirstName_12, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(11).Person.FirstName);
                            if (!String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(11).Person.MiddleName))
                            {
                                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMiddleInitial_12, rec.TaxYr),
                                    report.CoveredIndividuals.ElementAt(11).Person.MiddleName.Substring(0, 1));
                            }
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndLastName_12, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(11).Person.LastName);
                        }

                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSSN_12, rec.TaxYr),
                            maskSSN(report.CoveredIndividuals.ElementAt(11).Person.SSN));
                        if (String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(11).Person.SSN) &&
                                !String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(11).Person.DOB.ToShortDateString())) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDOB_12, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(11).Person.DOB.ToShortDateString());
                        }
                        // all the check boxes...
                        if (report.CoveredIndividuals.ElementAt(11).IsCoveredYr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndYr_12, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(11).IsCoveredJan) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJan_12, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(11).IsCoveredFeb) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFeb_12, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(11).IsCoveredMar) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMar_12, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(11).IsCoveredApr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndApr_12, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(11).IsCoveredMay) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMay_12, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(11).IsCoveredJun) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJun_12, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(11).IsCoveredJul) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJul_12, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(11).IsCoveredAug) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndAug_12, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(11).IsCoveredSep) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSep_12, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(11).IsCoveredOct) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndOct_12, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(11).IsCoveredNov) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndNov_12, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(11).IsCoveredDec) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDec_12, rec.TaxYr), "1");
                        }
                    }

                    // 13th individual (row 7 on page 2; line 29)
                    if (report.CoveredIndividuals.Count > 12) {
                        if (rec.TaxYr == 2016 || rec.TaxYr == 2017)
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndName_13, rec.TaxYr),
                                getName(report.CoveredIndividuals.ElementAt(12).Person));
                        }
                        else
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFirstName_13, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(12).Person.FirstName);
                            if (!String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(12).Person.MiddleName))
                            {
                                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMiddleInitial_13, rec.TaxYr),
                                    report.CoveredIndividuals.ElementAt(12).Person.MiddleName.Substring(0, 1));
                            }
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndLastName_13, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(12).Person.LastName);
                        }

                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSSN_13, rec.TaxYr),
                            maskSSN(report.CoveredIndividuals.ElementAt(12).Person.SSN));
                        if (String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(12).Person.SSN) &&
                                !String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(12).Person.DOB.ToShortDateString())) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDOB_13, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(12).Person.DOB.ToShortDateString());
                        }
                        // all the check boxes...
                        if (report.CoveredIndividuals.ElementAt(12).IsCoveredYr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndYr_13, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(12).IsCoveredJan) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJan_13, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(12).IsCoveredFeb) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFeb_13, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(12).IsCoveredMar) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMar_13, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(12).IsCoveredApr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndApr_13, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(12).IsCoveredMay) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMay_13, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(12).IsCoveredJun) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJun_13, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(12).IsCoveredJul) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJul_13, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(12).IsCoveredAug) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndAug_13, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(12).IsCoveredSep) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSep_13, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(12).IsCoveredOct) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndOct_13, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(12).IsCoveredNov) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndNov_13, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(12).IsCoveredDec) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDec_13, rec.TaxYr), "1");
                        }
                    }

                    // 14th individual (row 8 on page 2; line 30)
                    if (report.CoveredIndividuals.Count > 13 && formHasMoreThan13Individuals(rec.TaxYr)) {
                        if (rec.TaxYr == 2016 || rec.TaxYr == 2017)
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndName_14, rec.TaxYr),
                                getName(report.CoveredIndividuals.ElementAt(13).Person));
                        }
                        else
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFirstName_14, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(13).Person.FirstName);
                            if (!String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(13).Person.MiddleName))
                            {
                                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMiddleInitial_14, rec.TaxYr),
                                    report.CoveredIndividuals.ElementAt(13).Person.MiddleName.Substring(0, 1));
                            }
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndLastName_14, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(13).Person.LastName);
                        }

                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSSN_14, rec.TaxYr),
                            maskSSN(report.CoveredIndividuals.ElementAt(13).Person.SSN));
                        if (String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(13).Person.SSN) &&
                                !String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(13).Person.DOB.ToShortDateString())) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDOB_14, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(13).Person.DOB.ToShortDateString());
                        }
                        // all the check boxes...
                        if (report.CoveredIndividuals.ElementAt(13).IsCoveredYr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndYr_14, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(13).IsCoveredJan) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJan_14, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(13).IsCoveredFeb) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFeb_14, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(13).IsCoveredMar) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMar_14, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(13).IsCoveredApr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndApr_14, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(13).IsCoveredMay) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMay_14, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(13).IsCoveredJun) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJun_14, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(13).IsCoveredJul) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJul_14, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(13).IsCoveredAug) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndAug_14, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(13).IsCoveredSep) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSep_14, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(13).IsCoveredOct) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndOct_14, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(13).IsCoveredNov) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndNov_14, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(13).IsCoveredDec) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDec_14, rec.TaxYr), "1");
                        }
                    }

                    // 15th individual (row 9 on page 2; line 31)
                    if (report.CoveredIndividuals.Count > 14 && formHasMoreThan13Individuals(rec.TaxYr)) {
                        if (rec.TaxYr == 2016 || rec.TaxYr == 2017)
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndName_15, rec.TaxYr),
                                getName(report.CoveredIndividuals.ElementAt(14).Person));
                        }
                        else
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFirstName_15, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(14).Person.FirstName);
                            if (!String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(14).Person.MiddleName))
                            {
                                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMiddleInitial_15, rec.TaxYr),
                                    report.CoveredIndividuals.ElementAt(14).Person.MiddleName.Substring(0, 1));
                            }
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndLastName_15, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(14).Person.LastName);
                        }

                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSSN_15, rec.TaxYr),
                            maskSSN(report.CoveredIndividuals.ElementAt(14).Person.SSN));
                        if (String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(14).Person.SSN) &&
                                !String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(14).Person.DOB.ToShortDateString())) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDOB_15, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(14).Person.DOB.ToShortDateString());
                        }
                        // all the check boxes...
                        if (report.CoveredIndividuals.ElementAt(14).IsCoveredYr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndYr_15, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(14).IsCoveredJan) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJan_15, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(14).IsCoveredFeb) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFeb_15, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(14).IsCoveredMar) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMar_15, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(14).IsCoveredApr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndApr_15, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(14).IsCoveredMay) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMay_15, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(14).IsCoveredJun) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJun_15, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(14).IsCoveredJul) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJul_15, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(14).IsCoveredAug) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndAug_15, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(14).IsCoveredSep) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSep_15, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(14).IsCoveredOct) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndOct_15, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(14).IsCoveredNov) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndNov_15, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(14).IsCoveredDec) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDec_15, rec.TaxYr), "1");
                        }
                    }

                    // 16th individual (row 10 on page 2; line 32)
                    if (report.CoveredIndividuals.Count > 15 && formHasMoreThan13Individuals(rec.TaxYr)) {
                        if (rec.TaxYr == 2016 || rec.TaxYr == 2017)
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndName_16, rec.TaxYr),
                                getName(report.CoveredIndividuals.ElementAt(15).Person));
                        }
                        else
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFirstName_16, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(15).Person.FirstName);
                            if (!String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(15).Person.MiddleName))
                            {
                                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMiddleInitial_16, rec.TaxYr),
                                    report.CoveredIndividuals.ElementAt(15).Person.MiddleName.Substring(0, 1));
                            }
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndLastName_16, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(15).Person.LastName);
                        }

                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSSN_16, rec.TaxYr),
                            maskSSN(report.CoveredIndividuals.ElementAt(15).Person.SSN));
                        if (String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(15).Person.SSN) &&
                                !String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(15).Person.DOB.ToShortDateString())) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDOB_16, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(15).Person.DOB.ToShortDateString());
                        }
                        // all the check boxes...
                        if (report.CoveredIndividuals.ElementAt(15).IsCoveredYr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndYr_16, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(15).IsCoveredJan) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJan_16, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(15).IsCoveredFeb) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFeb_16, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(15).IsCoveredMar) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMar_16, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(15).IsCoveredApr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndApr_16, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(15).IsCoveredMay) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMay_16, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(15).IsCoveredJun) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJun_16, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(15).IsCoveredJul) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJul_16, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(15).IsCoveredAug) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndAug_16, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(15).IsCoveredSep) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSep_16, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(15).IsCoveredOct) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndOct_16, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(15).IsCoveredNov) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndNov_16, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(15).IsCoveredDec) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDec_16, rec.TaxYr), "1");
                        }
                    }

                    // 17th individual (row 11 on page 2; line 33)
                    if (report.CoveredIndividuals.Count > 16 && formHasMoreThan13Individuals(rec.TaxYr)) {
                        if (rec.TaxYr == 2016 || rec.TaxYr == 2017)
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndName_17, rec.TaxYr),
                                getName(report.CoveredIndividuals.ElementAt(16).Person));
                        }
                        else
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFirstName_17, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(16).Person.FirstName);
                            if (!String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(16).Person.MiddleName))
                            {
                                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMiddleInitial_17, rec.TaxYr),
                                    report.CoveredIndividuals.ElementAt(16).Person.MiddleName.Substring(0, 1));
                            }
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndLastName_17, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(16).Person.LastName);
                        }

                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSSN_17, rec.TaxYr),
                            maskSSN(report.CoveredIndividuals.ElementAt(16).Person.SSN));
                        if (String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(16).Person.SSN) &&
                                !String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(16).Person.DOB.ToShortDateString())) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDOB_17, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(16).Person.DOB.ToShortDateString());
                        }
                        // all the check boxes...
                        if (report.CoveredIndividuals.ElementAt(16).IsCoveredYr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndYr_17, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(16).IsCoveredJan) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJan_17, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(16).IsCoveredFeb) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFeb_17, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(16).IsCoveredMar) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMar_17, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(16).IsCoveredApr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndApr_17, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(16).IsCoveredMay) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMay_17, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(16).IsCoveredJun) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJun_17, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(16).IsCoveredJul) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJul_17, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(16).IsCoveredAug) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndAug_17, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(16).IsCoveredSep) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSep_17, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(16).IsCoveredOct) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndOct_17, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(16).IsCoveredNov) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndNov_17, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(16).IsCoveredDec) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDec_17, rec.TaxYr), "1");
                        }
                    }

                    // 18th individual (row 12 on page 2; line 34)
                    if (report.CoveredIndividuals.Count > 17 && formHasMoreThan13Individuals(rec.TaxYr)) {
                        if (rec.TaxYr == 2016 || rec.TaxYr == 2017)
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndName_18, rec.TaxYr),
                                getName(report.CoveredIndividuals.ElementAt(17).Person));
                        }
                        else
                        {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFirstName_18, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(17).Person.FirstName);
                            if (!String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(17).Person.MiddleName))
                            {
                                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMiddleInitial_18, rec.TaxYr),
                                    report.CoveredIndividuals.ElementAt(17).Person.MiddleName.Substring(0, 1));
                            }
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndLastName_18, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(17).Person.LastName);
                        }

                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSSN_18, rec.TaxYr),
                            maskSSN(report.CoveredIndividuals.ElementAt(17).Person.SSN));
                        if (String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(17).Person.SSN) &&
                                !String.IsNullOrEmpty(report.CoveredIndividuals.ElementAt(17).Person.DOB.ToShortDateString())) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDOB_18, rec.TaxYr),
                                report.CoveredIndividuals.ElementAt(17).Person.DOB.ToShortDateString());
                        }
                        // all the check boxes...
                        if (report.CoveredIndividuals.ElementAt(17).IsCoveredYr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndYr_18, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(17).IsCoveredJan) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJan_18, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(17).IsCoveredFeb) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndFeb_18, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(17).IsCoveredMar) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMar_18, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(17).IsCoveredApr) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndApr_18, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(17).IsCoveredMay) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndMay_18, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(17).IsCoveredJun) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJun_18, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(17).IsCoveredJul) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndJul_18, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(17).IsCoveredAug) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndAug_18, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(17).IsCoveredSep) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndSep_18, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(17).IsCoveredOct) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndOct_18, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(17).IsCoveredNov) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndNov_18, rec.TaxYr), "1");
                        }
                        if (report.CoveredIndividuals.ElementAt(17).IsCoveredDec) {
                            stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.CovIndDec_18, rec.TaxYr), "1");
                        }
                    }


                } // end if covered individuals

                //
                // PAGE 2 (heading only -- covered individuals have already been processed)
                //
                if (rec.TaxYr == 2016 || rec.TaxYr == 2017)
                {
                    stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.Page3HeadingName, rec.TaxYr), getName(report.Person)); // employee name
                }
                else
                {
                    stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.Page3HeadingFirstName, rec.TaxYr), report.Person.FirstName); // first name
                    if (!String.IsNullOrEmpty(report.Person.MiddleName))
                    {
                        stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.Page3HeadingMiddleInitial, rec.TaxYr), report.Person.MiddleName.Substring(0, 1)); // middle name
                    }
                    stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.Page3HeadingLastName, rec.TaxYr), report.Person.LastName); // last name
                }
                stamper.AcroFields.SetField(FieldManager.GetFieldId((int)Structures.FormFields.Page3HeadingSSN, rec.TaxYr), maskSSN(report.Person.SSN)); // SSN

                // DONE!

                
                // fields should no longer be editable
                stamper.FormFlattening = true;
                stamper.Close();
                
            } // end using
            reader.Close();

            return savedFilePath;
        }

    }
}