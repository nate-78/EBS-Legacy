using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfCreatorWebJob
{
    class FieldManager
    {
        public static string GetFieldId(int fieldKey, int taxYr)
        {
            string id = "";

            switch (fieldKey) {
                case (int)Structures.FormFields.Void:
                    if (taxYr == 2016 || taxYr == 2017) {
                        return "topmostSubform[0].Page1[0].c1_01[0]";
                    } else if (taxYr == 2018) {
                        return "topmostSubform[0].Page1[0].PgHeader[0].c1_1[0]";
                    } else if (taxYr >= 2019)
                    {
                        return "topmostSubform[0].Page1[0].Pg1Header[0].c1_1[0]";
                    }
                    break;

                case (int)Structures.FormFields.Corrected:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].c1_01[1]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].PgHeader[0].c1_1[1]";
                    }
                    else if (taxYr >= 2019)
                    {
                        return "topmostSubform[0].Page1[0].Pg1Header[0].c1_1[1]";
                    }
                    break;

                case (int)Structures.FormFields.EmployeeName:
                    return "topmostSubform[0].Page1[0].EmployeeName[0].f1_002[0]";

                case (int)Structures.FormFields.FirstName:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].EmployeeName[0].EmployeeName[0].f1_1[0]";
                    }
                    else if (taxYr >= 2019)
                    {
                        return "topmostSubform[0].Page1[0].EmployeeName[0].f1_1[0]";
                    }
                    break;

                case (int)Structures.FormFields.MiddleInitial:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].EmployeeName[0].EmployeeName[0].f1_2[0]";
                    }
                    else if (taxYr >= 2019)
                    {
                        return "topmostSubform[0].Page1[0].EmployeeName[0].f1_2[0]";
                    }
                    break;

                case (int)Structures.FormFields.LastName:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].EmployeeName[0].EmployeeName[0].f1_3[0]";
                    }
                    else if (taxYr >= 2019)
                    {
                        return "topmostSubform[0].Page1[0].EmployeeName[0].f1_3[0]";
                    }
                    break;

                case (int)Structures.FormFields.SSN:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].EmployeeName[0].f1_003[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].EmployeeName[0].f1_4[0]";
                    }
                    break;

                case (int)Structures.FormFields.Street:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].EmployeeName[0].f1_004[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].EmployeeName[0].f1_5[0]";
                    }
                    break;

                case (int)Structures.FormFields.City:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].EmployeeName[0].f1_005[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].EmployeeName[0].f1_6[0]";
                    }
                    break;

                case (int)Structures.FormFields.State:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].EmployeeName[0].f1_006[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].EmployeeName[0].f1_7[0]";
                    }
                    break;

                case (int)Structures.FormFields.Zip:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].EmployeeName[0].f1_007[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].EmployeeName[0].f1_8[0]";
                    }
                    break;

                case (int)Structures.FormFields.EmployerName:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].EmployerIssuer[0].f1_008[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].EmployerIssuer[0].f1_9[0]";
                    }
                    break;

                case (int)Structures.FormFields.EmployerEIN:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].EmployerIssuer[0].f1_009[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].EmployerIssuer[0].f1_10[0]";
                    }
                    break;

                case (int)Structures.FormFields.EmployerStreet:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].EmployerIssuer[0].f1_010[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].EmployerIssuer[0].f1_11[0]";
                    }
                    break;

                case (int)Structures.FormFields.ContactPhone:
                    if (taxYr == 2016 || taxYr == 2017) {
                        return "topmostSubform[0].Page1[0].EmployerIssuer[0].f1_011[0]";
                    } else if (taxYr >= 2018) {
                        return "topmostSubform[0].Page1[0].EmployerIssuer[0].f1_12[0]";
                    }
                    break;

                case (int)Structures.FormFields.EmployerCity:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].EmployerIssuer[0].f1_012[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].EmployerIssuer[0].f1_13[0]";
                    }
                    break;

                case (int)Structures.FormFields.EmployerState:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].EmployerIssuer[0].f1_013[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].EmployerIssuer[0].f1_14[0]";
                    }
                    break;

                case (int)Structures.FormFields.EmployerZip:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].EmployerIssuer[0].f1_014[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].EmployerIssuer[0].f1_15[0]";
                    }
                    break;


                case (int)Structures.FormFields.AgeOnJan1:
                    if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page1[0].PartII[0].f1_17[0]";
                    }
                    break;


                case (int)Structures.FormFields.PlanStartMonth:
                    if (taxYr == 2016 || taxYr == 2017) {
                        return "topmostSubform[0].Page1[0].PlanStartMonth[0]";
                    } else if (taxYr == 2018 || taxYr == 2019) {
                        return "topmostSubform[0].Page1[0].PartII[0].f1_16[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page1[0].PartII[0].f1_16[0]";
                    }
                    break;


                case (int)Structures.FormFields.OfferOfCoverageYr:
                    if (taxYr == 2016)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow1[0].f1_011[0]";
                    }
                    else if (taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow1[0].f1_400[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row1[0].f1_17[0]";
                    }
                    break;

                case (int)Structures.FormFields.OfferOfCoverageJan:
                    if (taxYr == 2016)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow1[0].f1_012[0]";
                    }
                    else if (taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow1[0].f1_401[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row1[0].f1_18[0]";
                    }
                    break;

                case (int)Structures.FormFields.OfferOfCoverageFeb:
                    if (taxYr == 2016)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow1[0].f1_013[0]";
                    }
                    else if (taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow1[0].f1_402[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row1[0].f1_19[0]";
                    }
                    break;

                case (int)Structures.FormFields.OfferOfCoverageMar:
                    if (taxYr == 2016)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow1[0].f1_014[0]";
                    }
                    else if (taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow1[0].f1_403[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row1[0].f1_20[0]";
                    }
                    break;

                case (int)Structures.FormFields.OfferOfCoverageApr:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow1[0].f1_015[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row1[0].f1_21[0]";
                    }
                    break;

                case (int)Structures.FormFields.OfferOfCoverageMay:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow1[0].f1_016[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row1[0].f1_22[0]";
                    }
                    break;

                case (int)Structures.FormFields.OfferOfCoverageJun:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow1[0].f1_017[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row1[0].f1_23[0]";
                    }
                    break;

                case (int)Structures.FormFields.OfferOfCoverageJul:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow1[0].f1_018[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row1[0].f1_24[0]";
                    }
                    break;

                case (int)Structures.FormFields.OfferOfCoverageAug:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow1[0].f1_019[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row1[0].f1_25[0]";
                    }
                    break;

                case (int)Structures.FormFields.OfferOfCoverageSep:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow1[0].f1_020[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row1[0].f1_26[0]";
                    }
                    break;

                case (int)Structures.FormFields.OfferOfCoverageOct:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow1[0].f1_021[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row1[0].f1_27[0]";
                    }
                    break;

                case (int)Structures.FormFields.OfferOfCoverageNov:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow1[0].f1_022[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row1[0].f1_28[0]";
                    }
                    break;

                case (int)Structures.FormFields.OfferOfCoverageDec:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow1[0].f1_023[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row1[0].f1_29[0]";
                    }
                    break;


                case (int)Structures.FormFields.EmployeeReqContribYr:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow2[0].f1_025[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row2[0].f1_30[0]";
                    }
                    break;

                case (int)Structures.FormFields.EmployeeReqContribJan:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow2[0].f1_026[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row2[0].f1_31[0]";
                    }
                    break;

                case (int)Structures.FormFields.EmployeeReqContribFeb:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow2[0].f1_027[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row2[0].f1_32[0]";
                    }
                    break;

                case (int)Structures.FormFields.EmployeeReqContribMar:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow2[0].f1_028[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row2[0].f1_33[0]";
                    }
                    break;

                case (int)Structures.FormFields.EmployeeReqContribApr:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow2[0].f1_029[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row2[0].f1_34[0]";
                    }
                    break;

                case (int)Structures.FormFields.EmployeeReqContribMay:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow2[0].f1_030[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row2[0].f1_35[0]";
                    }
                    break;

                case (int)Structures.FormFields.EmployeeReqContribJun:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow2[0].f1_031[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row2[0].f1_36[0]";
                    }
                    break;

                case (int)Structures.FormFields.EmployeeReqContribJul:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow2[0].f1_032[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row2[0].f1_37[0]";
                    }
                    break;

                case (int)Structures.FormFields.EmployeeReqContribAug:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow2[0].f1_033[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row2[0].f1_38[0]";
                    }
                    break;

                case (int)Structures.FormFields.EmployeeReqContribSep:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow2[0].f1_034[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row2[0].f1_39[0]";
                    }
                    break;

                case (int)Structures.FormFields.EmployeeReqContribOct:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow2[0].f1_035[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row2[0].f1_40[0]";
                    }
                    break;

                case (int)Structures.FormFields.EmployeeReqContribNov:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow2[0].f1_036[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row2[0].f1_41[0]";
                    }
                    break;

                case (int)Structures.FormFields.EmployeeReqContribDec:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow2[0].f1_300[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row2[0].f1_42[0]";
                    }
                    break;


                case (int)Structures.FormFields.Sec4980Yr:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow3[0].f1_050[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row3[0].f1_43[0]";
                    }
                    break;

                case (int)Structures.FormFields.Sec4980Jan:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow3[0].f1_051[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row3[0].f1_44[0]";
                    }
                    break;

                case (int)Structures.FormFields.Sec4980Feb:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow3[0].f1_052[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row3[0].f1_45[0]";
                    }
                    break;

                case (int)Structures.FormFields.Sec4980Mar:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow3[0].f1_053[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row3[0].f1_46[0]";
                    }
                    break;

                case (int)Structures.FormFields.Sec4980Apr:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow3[0].f1_054[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row3[0].f1_47[0]";
                    }
                    break;

                case (int)Structures.FormFields.Sec4980May:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow3[0].f1_055[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row3[0].f1_48[0]";
                    }
                    break;

                case (int)Structures.FormFields.Sec4980Jun:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow3[0].f1_056[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row3[0].f1_49[0]";
                    }
                    break;

                case (int)Structures.FormFields.Sec4980Jul:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow3[0].f1_057[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row3[0].f1_50[0]";
                    }
                    break;

                case (int)Structures.FormFields.Sec4980Aug:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow3[0].f1_058[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row3[0].f1_51[0]";
                    }
                    break;

                case (int)Structures.FormFields.Sec4980Sep:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow3[0].f1_059[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row3[0].f1_52[0]";
                    }
                    break;

                case (int)Structures.FormFields.Sec4980Oct:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow3[0].f1_060[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row3[0].f1_53[0]";
                    }
                    break;

                case (int)Structures.FormFields.Sec4980Nov:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow3[0].f1_061[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row3[0].f1_54[0]";
                    }
                    break;

                case (int)Structures.FormFields.Sec4980Dec:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Part2Table[0].BodyRow3[0].f1_062[0]";
                    }
                    else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row3[0].f1_55[0]";
                    }
                    break;

                // employee offer zip code (as of 2020, page 1, part 2 
                case (int)Structures.FormFields.OfferZipCdAnn:
                    if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row4[0].f1_56[0]";
                    }
                    break;
                case (int)Structures.FormFields.OfferZipCdJan:
                    if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row4[0].f1_57[0]";
                    }
                    break;
                case (int)Structures.FormFields.OfferZipCdFeb:
                    if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row4[0].f1_58[0]";
                    }
                    break;
                case (int)Structures.FormFields.OfferZipCdMar:
                    if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row4[0].f1_59[0]";
                    }
                    break;
                case (int)Structures.FormFields.OfferZipCdApr:
                    if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row4[0].f1_60[0]";
                    }
                    break;
                case (int)Structures.FormFields.OfferZipCdMay:
                    if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row4[0].f1_61[0]";
                    }
                    break;
                case (int)Structures.FormFields.OfferZipCdJun:
                    if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row4[0].f1_62[0]";
                    }
                    break;
                case (int)Structures.FormFields.OfferZipCdJul:
                    if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row4[0].f1_63[0]";
                    }
                    break;
                case (int)Structures.FormFields.OfferZipCdAug:
                    if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row4[0].f1_64[0]";
                    }
                    break;
                case (int)Structures.FormFields.OfferZipCdSep:
                    if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row4[0].f1_65[0]";
                    }
                    break;
                case (int)Structures.FormFields.OfferZipCdOct:
                    if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row4[0].f1_66[0]";
                    }
                    break;
                case (int)Structures.FormFields.OfferZipCdNov:
                    if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row4[0].f1_67[0]";
                    }
                    break;
                case (int)Structures.FormFields.OfferZipCdDec:
                    if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page1[0].Table1[0].Row4[0].f1_68[0]";
                    }
                    break;


                case (int)Structures.FormFields.EmployerProvidedSelfInsured:
                    if (taxYr == 2016 || taxYr == 2017) {
                        return "topmostSubform[0].Page1[0].PartIII[0].c1_02[0]";
                    } else if (taxYr == 2018 || taxYr == 2019) {
                        return "topmostSubform[0].Page1[0].PartIII[0].c1_2[0]";
                    } else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].PartIII[0].c1_2[0]";
                    }
                    break;


                case (int)Structures.FormFields.CovIndName_1: // EDIT 1/3/2021: I don't remember what this field is for
                    return "topmostSubform[0].Page1[0].Line17[0].Ln17[0].Name1[0]";
                case (int)Structures.FormFields.CovIndName_2:
                    return "topmostSubform[0].Page1[0].Line18[0].#subform[0].Name2[0]";
                case (int)Structures.FormFields.CovIndName_3:
                    return "topmostSubform[0].Page1[0].Line19[0].#subform[0].Name3[0]";
                case (int)Structures.FormFields.CovIndName_4:
                    return "topmostSubform[0].Page1[0].Line20[0].Ln20[0].Name4[0]";
                case (int)Structures.FormFields.CovIndName_5:
                    return "topmostSubform[0].Page1[0].Line21[0].Ln21[0].Name5[0]";
                case (int)Structures.FormFields.CovIndName_6:
                    return "topmostSubform[0].Page1[0].Line22[0].Ln22[0].Name6[0]";
                case (int)Structures.FormFields.CovIndName_7:
                    return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow1[0].f2_01[0]";
                case (int)Structures.FormFields.CovIndName_8:
                    return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow2[0].f2_04[0]";
                case (int)Structures.FormFields.CovIndName_9:
                    return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow3[0].f2_07[0]";
                case (int)Structures.FormFields.CovIndName_10:
                    return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow4[0].f2_10[0]";
                case (int)Structures.FormFields.CovIndName_11:
                    return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow5[0].f2_12[0]";
                case (int)Structures.FormFields.CovIndName_12:
                    return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow6[0].f1_15[0]";
                case (int)Structures.FormFields.CovIndName_13:
                    return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow7[0].f1_18[0]";
                case (int)Structures.FormFields.CovIndName_14:
                    return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow8[0].f1_21[0]";
                case (int)Structures.FormFields.CovIndName_15:
                    return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow9[0].f1_24[0]";
                case (int)Structures.FormFields.CovIndName_16:
                    return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow10[0].f1_27[0]";
                case (int)Structures.FormFields.CovIndName_17:
                    return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow11[0].f1_30[0]";
                case (int)Structures.FormFields.CovIndName_18:
                    return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow12[0].f1_33[0]";

                case (int)Structures.FormFields.CovIndFirstName_1:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row1[0].f1_56[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row1[0].f1_56[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].f3_56[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFirstName_2:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row2[0].f1_61[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row2[0].f1_61[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].f3_61[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFirstName_3:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row3[0].f1_66[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row3[0].f1_66[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].f3_66[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFirstName_4:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row4[0].f1_71[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row4[0].f1_71[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].f3_71[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFirstName_5:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row5[0].f1_76[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row5[0].f1_76[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].f3_76[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFirstName_6:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row6[0].f1_81[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row6[0].f1_81[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].f3_81[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFirstName_7:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row1[0].f3_5[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].f3_5[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].f3_86[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFirstName_8:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row2[0].f3_10[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].f3_10[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].f3_91[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFirstName_9:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row3[0].f3_15[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].f3_15[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].f3_97[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFirstName_10:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row4[0].f3_20[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].f3_20[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].f3_102[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFirstName_11:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row5[0].f3_25[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].f3_25[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].f3_107[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFirstName_12:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row6[0].f3_30[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].f3_30[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].f3_113[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFirstName_13:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row7[0].f3_35[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].f3_35[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row13[0].f3_118[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFirstName_14:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row8[0].f3_40[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].f3_40[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFirstName_15:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row9[0].f3_45[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].f3_45[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFirstName_16:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row10[0].f3_50[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].f3_50[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFirstName_17:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row11[0].f3_55[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].f3_55[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFirstName_18:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row12[0].f3_60[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].f3_60[0]";
                    }
                    break;


                // middle initial
                case (int)Structures.FormFields.CovIndMiddleInitial_1:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row1[0].f1_57[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row1[0].f1_57[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].f3_57[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMiddleInitial_2:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row2[0].f1_62[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row2[0].f1_62[0]";
                    } else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].f3_62[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMiddleInitial_3:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row3[0].f1_67[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row3[0].f1_67[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].f3_67[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMiddleInitial_4:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row4[0].f1_72[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row4[0].f1_72[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].f3_72[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMiddleInitial_5:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row5[0].f1_77[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row5[0].f1_77[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].f3_77[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMiddleInitial_6:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row6[0].f1_82[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row6[0].f1_82[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].f3_82[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMiddleInitial_7:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row1[0].f3_6[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].f3_6[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].f3_87[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMiddleInitial_8:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row2[0].f3_11[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].f3_11[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].f3_92[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMiddleInitial_9:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row3[0].f3_16[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].f3_16[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].f3_98[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMiddleInitial_10:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row4[0].f3_21[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].f3_21[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].f3_103[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMiddleInitial_11:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row5[0].f3_26[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].f3_26[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].f3_108[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMiddleInitial_12:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row6[0].f3_31[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].f3_31[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].f3_114[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMiddleInitial_13:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row7[0].f3_36[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].f3_36[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row13[0].f3_119[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMiddleInitial_14:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row8[0].f3_41[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].f3_41[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMiddleInitial_15:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row9[0].f3_46[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].f3_46[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMiddleInitial_16:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row10[0].f3_51[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].f3_51[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMiddleInitial_17:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row11[0].f3_56[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].f3_56[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMiddleInitial_18:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row12[0].f3_61[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].f3_61[0]";
                    }
                    break;


                // last name
                case (int)Structures.FormFields.CovIndLastName_1:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row1[0].f1_58[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row1[0].f1_58[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].f3_58[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndLastName_2:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row2[0].f1_63[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row2[0].f1_63[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].f3_63[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndLastName_3:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row3[0].f1_68[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row3[0].f1_68[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].f3_68[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndLastName_4:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row4[0].f1_73[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row4[0].f1_73[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].f3_73[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndLastName_5:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row5[0].f1_78[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row5[0].f1_78[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].f3_78[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndLastName_6:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row6[0].f1_83[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row6[0].f1_83[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].f3_83[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndLastName_7:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row1[0].f3_7[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].f3_7[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].f3_88[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndLastName_8:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row2[0].f3_12[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].f3_12[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].f3_93[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndLastName_9:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row3[0].f3_17[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].f3_17[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].f3_99[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndLastName_10:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row4[0].f3_22[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].f3_22[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].f3_104[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndLastName_11:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row5[0].f3_27[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].f3_27[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].f3_109[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndLastName_12:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row6[0].f3_32[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].f3_32[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].f3_115[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndLastName_13:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row7[0].f3_37[0]";
                    } 
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].f3_37[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row13[0].f3_120[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndLastName_14:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row8[0].f3_42[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].f3_42[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndLastName_15:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row9[0].f3_47[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].f3_47[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndLastName_16:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row10[0].f3_52[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].f3_52[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndLastName_17:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row11[0].f3_57[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].f3_57[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndLastName_18:
                    if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row12[0].f3_62[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].f3_62[0]";
                    }
                    break;


                // ssn
                case (int)Structures.FormFields.CovIndSSN_1:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line17[0].Ln17[0].SSN1[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row1[0].f1_59[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row1[0].f1_59[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].f3_59[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSSN_2:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line18[0].SSN2[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row2[0].f1_64[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row2[0].f1_64[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].f3_64[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSSN_3:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line19[0].SSN3[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row3[0].f1_69[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row3[0].f1_69[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].f3_69[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSSN_4:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line20[0].SSN4[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row4[0].f1_74[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row4[0].f1_74[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].f3_74[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSSN_5:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line21[0].SSN5[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row5[0].f1_79[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row5[0].f1_79[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].f3_79[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSSN_6:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line22[0].SSN6[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row6[0].f1_84[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row6[0].f1_84[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].f3_84[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSSN_7:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow1[0].f2_02[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row1[0].f3_8[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].f3_8[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].f3_89[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSSN_8:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow2[0].f2_05[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row2[0].f3_13[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].f3_13[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].f3_95[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSSN_9:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow3[0].f2_08[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row3[0].f3_18[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].f3_18[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].f3_100[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSSN_10:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow4[0].f2_11[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row4[0].f3_23[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].f3_23[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].f3_105[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSSN_11:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow5[0].f2_13[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row5[0].f3_28[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].f3_28[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].f3_110[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSSN_12:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow6[0].f1_16[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row6[0].f3_33[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].f3_33[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].f3_116[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSSN_13:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow7[0].f1_19[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row7[0].f3_38[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].f3_38[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row13[0].f3_121[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSSN_14:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow8[0].f1_22[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row8[0].f3_43[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].f3_43[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSSN_15:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow9[0].f1_25[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row9[0].f3_48[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].f3_48[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSSN_16:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow10[0].f1_28[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row10[0].f3_53[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].f3_53[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSSN_17:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow11[0].f1_31[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row11[0].f3_58[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].f3_58[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSSN_18:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow12[0].f1_34[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row12[0].f3_63[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].f3_63[0]";
                    }
                    break;


                // dob
                case (int)Structures.FormFields.CovIndDOB_1:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line17[0].DOB1[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row1[0].f1_60[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row1[0].f1_60[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].f3_60[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDOB_2:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line18[0].DOB2[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row3[0].f1_65[0]"; // TODO: check the row number for this field
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row2[0].f1_65[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].f3_65[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDOB_3:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line19[0].DOB3[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row3[0].f1_70[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row3[0].f1_70[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].f3_70[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDOB_4:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line20[0].DOB4[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row4[0].f1_75[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row4[0].f1_75[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].f3_75[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDOB_5:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line21[0].DOB5[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row5[0].f1_80[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row5[0].f1_80[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].f3_80[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDOB_6:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line22[0].DOB6[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row6[0].f1_85[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row6[0].f1_85[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].f3_85[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDOB_7:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow1[0].f2_03[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row1[0].f3_9[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].f3_9[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].f3_90[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDOB_8:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow2[0].f2_06[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row2[0].f3_14[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].f3_14[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].f3_96[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDOB_9:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow3[0].f2_09[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row3[0].f3_19[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].f3_19[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].f3_101[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDOB_10:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow4[0].f2_12[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row4[0].f3_24[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].f3_24[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].f3_106[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDOB_11:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow5[0].f2_14[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row5[0].f3_29[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].f3_29[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].f3_111[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDOB_12:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow6[0].f1_17[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row6[0].f3_34[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].f3_34[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].f3_117[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDOB_13:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow7[0].f1_20[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row7[0].f3_39[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].f3_39[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row13[0].f3_122[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDOB_14:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow8[0].f1_23[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row8[0].f3_44[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].f3_44[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDOB_15:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow9[0].f1_26[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row9[0].f3_49[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].f3_49[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDOB_16:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow10[0].f1_29[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row10[0].f3_54[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].f3_54[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDOB_17:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow11[0].f1_32[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row11[0].f3_59[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].f3_59[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDOB_18:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow12[0].f1_35[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row12[0].f3_64[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].f3_64[0]";
                    }
                    break;


                // covered year
                case (int)Structures.FormFields.CovIndYr_1:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line17[0].c1_011[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row1[0].c1_3[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row1[0].c1_3[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_3[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndYr_2:
                    if (taxYr == 2016 || taxYr == 2017) {
                        return "topmostSubform[0].Page1[0].Line18[0].c1_024[0]";
                    } else if (taxYr == 2018) {
                        return "topmostSubform[0].Page1[0].Table2[0].Row2[0].c1_16[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row2[0].c1_16[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_16[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndYr_3:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line19[0].c1_037[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row3[0].c1_29[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row3[0].c1_29[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_29[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndYr_4:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line20[0].c1_050[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row4[0].c1_42[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row4[0].c1_42[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_42[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndYr_5:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line21[0].c1_063[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row5[0].c1_55[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row5[0].c1_55[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_55[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndYr_6:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line22[0].c1_076[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row6[0].c1_68[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row6[0].c1_68[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_68[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndYr_7:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow1[0].c2_01[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row1[0].c3_1[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_1[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_81[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndYr_8:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow2[0].c2_14[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row2[0].c3_14[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_14[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_94[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndYr_9:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow3[0].c2_27[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row3[0].c3_27[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_27[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_107[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndYr_10:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow4[0].c2_40[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row4[0].c3_40[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_40[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_120[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndYr_11:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow5[0].c2_53[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row5[0].c3_53[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_53[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_112[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndYr_12:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow6[0].c2_66[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row6[0].c3_66[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_66[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_125[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndYr_13:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow7[0].c2_79[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row7[0].c3_79[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_79[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row13[0].c3_138[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndYr_14:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow8[0].c2_92[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row8[0].c3_92[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_92[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndYr_15:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow9[0].c2_105[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row9[0].c3_105[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_105[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndYr_16:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow10[0].c2_118[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row10[0].c3_118[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_118[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndYr_17:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow11[0].c2_131[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row11[0].c3_131[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_131[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndYr_18:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow12[0].c2_144[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row12[0].c3_144[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_144[0]";
                    }
                    break;


                // covered January
                case (int)Structures.FormFields.CovIndJan_1:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line17[0].c1_012[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row1[0].c1_4[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row1[0].c1_4[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_4[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJan_2:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line18[0].c1_025[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row2[0].c1_17[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row2[0].c1_17[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_17[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJan_3:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line19[0].c1_038[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row3[0].c1_30[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row3[0].c1_30[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_30[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJan_4:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line20[0].c1_051[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row4[0].c1_43[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row4[0].c1_43[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_43[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJan_5:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line21[0].c1_064[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row5[0].c1_56[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row5[0].c1_56[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_56[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJan_6:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line22[0].c1_077[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row6[0].c1_69[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row6[0].c1_69[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_69[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJan_7:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow1[0].c2_02[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row1[0].c3_2[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_2[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_82[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJan_8:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow2[0].c2_15[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row2[0].c3_15[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_15[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_95[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJan_9:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow3[0].c2_28[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row3[0].c3_28[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_28[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_108[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJan_10:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow4[0].c2_41[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row4[0].c3_41[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_41[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_121[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJan_11:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow5[0].c2_54[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row5[0].c3_54[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_54[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_113[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJan_12:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow6[0].c2_67[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row6[0].c3_67[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_67[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_126[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJan_13:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow7[0].c2_80[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row7[0].c3_80[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_80[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row13[0].c3_139[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJan_14:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow8[0].c2_93[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row8[0].c3_93[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_93[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJan_15:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow9[0].c2_106[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row9[0].c3_106[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_106[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJan_16:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow10[0].c2_119[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row10[0].c3_119[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_119[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJan_17:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow11[0].c2_132[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row11[0].c3_132[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_132[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJan_18:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow12[0].c2_145[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row12[0].c3_145[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_145[0]";
                    }
                    break;


                // covered February
                case (int)Structures.FormFields.CovIndFeb_1:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line19[0].c1_013[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row1[0].c1_5[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row1[0].c1_5[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_5[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFeb_2:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line18[0].c1_026[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row2[0].c1_18[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row2[0].c1_18[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_18[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFeb_3:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line19[0].c1_039[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row3[0].c1_31[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row3[0].c1_31[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_31[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFeb_4:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line20[0].c1_052[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row4[0].c1_44[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row4[0].c1_44[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_44[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFeb_5:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line21[0].c1_065[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row5[0].c1_57[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row5[0].c1_57[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_57[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFeb_6:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line22[0].c1_078[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row6[0].c1_70[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row6[0].c1_70[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_70[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFeb_7:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow1[0].c2_03[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row1[0].c3_3[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_3[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_83[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFeb_8:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow2[0].c2_16[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row2[0].c3_16[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_16[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_96[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFeb_9:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow3[0].c2_29[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row3[0].c3_29[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_29[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_109[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFeb_10:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow4[0].c2_42[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row4[0].c3_42[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_42[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_122[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFeb_11:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow5[0].c2_55[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row5[0].c3_55[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_55[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_114[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFeb_12:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow6[0].c2_68[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row6[0].c3_68[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_68[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_127[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFeb_13:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow7[0].c2_81[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row7[0].c3_81[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_81[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row13[0].c3_140[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFeb_14:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow8[0].c2_94[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row8[0].c3_94[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_94[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFeb_15:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow9[0].c2_107[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row9[0].c3_107[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_107[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFeb_16:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow10[0].c2_120[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row10[0].c3_120[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_120[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFeb_17:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow11[0].c2_133[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row11[0].c3_133[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_133[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndFeb_18:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow12[0].c2_146[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row12[0].c3_146[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_146[0]";
                    }
                    break;


                // covered March
                case (int)Structures.FormFields.CovIndMar_1:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line17[0].c1_014[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row1[0].c1_6[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row1[0].c1_6[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_6[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMar_2:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line18[0].c1_027[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row2[0].c1_19[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row2[0].c1_19[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_19[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMar_3:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line19[0].c1_040[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row3[0].c1_32[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row3[0].c1_32[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_32[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMar_4:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line20[0].c1_053[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row4[0].c1_45[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row4[0].c1_45[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_45[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMar_5:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line21[0].c1_066[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row5[0].c1_58[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row5[0].c1_58[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_58[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMar_6:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line22[0].c1_079[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row6[0].c1_71[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row6[0].c1_71[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_71[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMar_7:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow1[0].c2_04[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row1[0].c3_4[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_4[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_84[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMar_8:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow2[0].c2_17[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row2[0].c3_17[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_17[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_97[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMar_9:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow3[0].c2_30[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row3[0].c3_30[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_30[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_110[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMar_10:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow4[0].c2_43[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row4[0].c3_43[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_43[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_123[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMar_11:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow5[0].c2_56[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row5[0].c3_56[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_56[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_115[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMar_12:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow6[0].c2_69[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row6[0].c3_69[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_69[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_128[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMar_13:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow7[0].c2_82[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row7[0].c3_82[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_82[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row13[0].c3_141[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMar_14:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow8[0].c2_95[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row8[0].c3_95[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_95[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMar_15:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow9[0].c2_108[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row9[0].c3_108[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_108[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMar_16:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow10[0].c2_121[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row10[0].c3_121[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_121[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMar_17:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow11[0].c2_134[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row11[0].c3_134[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_134[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMar_18:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow12[0].c2_147[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row12[0].c3_147[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_147[0]";
                    }
                    break;


                // covered April
                case (int)Structures.FormFields.CovIndApr_1:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line17[0].c1_015[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row1[0].c1_7[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row1[0].c1_7[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_7[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndApr_2:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line18[0].c1_028[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row2[0].c1_20[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row2[0].c1_20[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_20[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndApr_3:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line19[0].c1_041[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row3[0].c1_33[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row3[0].c1_33[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_33[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndApr_4:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line20[0].c1_054[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row4[0].c1_46[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row4[0].c1_46[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_46[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndApr_5:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line21[0].c1_067[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row5[0].c1_59[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row5[0].c1_59[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_59[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndApr_6:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line22[0].c1_080[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row6[0].c1_72[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row6[0].c1_72[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_72[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndApr_7:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow1[0].c2_05[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row1[0].c3_5[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_5[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_85[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndApr_8:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow2[0].c2_18[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row2[0].c3_18[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_18[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_98[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndApr_9:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow3[0].c2_31[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row3[0].c3_31[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_31[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_111[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndApr_10:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow4[0].c2_44[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row4[0].c3_44[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_44[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_124[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndApr_11:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow5[0].c2_57[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row5[0].c3_57[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_57[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_116[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndApr_12:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow6[0].c2_70[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row6[0].c3_70[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_70[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_129[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndApr_13:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow7[0].c2_83[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row7[0].c3_83[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_83[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row13[0].c3_142[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndApr_14:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow8[0].c2_96[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row8[0].c3_96[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_96[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndApr_15:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow9[0].c2_109[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row9[0].c3_109[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_109[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndApr_16:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow10[0].c2_122[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row10[0].c3_122[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_122[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndApr_17:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow11[0].c2_135[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row11[0].c3_135[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_135[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndApr_18:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow12[0].c2_148[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row12[0].c3_148[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_148[0]";
                    }
                    break;


                // covered May
                case (int)Structures.FormFields.CovIndMay_1:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line17[0].c1_016[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row1[0].c1_8[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row1[0].c1_8[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_8[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMay_2:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line18[0].c1_029[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row2[0].c1_21[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row2[0].c1_21[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_21[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMay_3:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line19[0].c1_042[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row3[0].c1_34[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row3[0].c1_34[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_34[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMay_4:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line20[0].c1_055[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row4[0].c1_47[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row4[0].c1_47[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_47[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMay_5:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line21[0].c1_068[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row5[0].c1_60[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row5[0].c1_60[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_60[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMay_6:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line22[0].c1_081[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row6[0].c1_73[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row6[0].c1_73[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_73[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMay_7:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow1[0].c2_06[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row1[0].c3_6[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_6[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_86[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMay_8:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow2[0].c2_19[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row2[0].c3_19[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_19[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_99[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMay_9:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow3[0].c2_32[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row3[0].c3_32[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_32[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_112[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMay_10:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow4[0].c2_45[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row4[0].c3_45[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_45[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_125[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMay_11:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow5[0].c2_58[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row5[0].c3_58[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_58[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_117[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMay_12:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow6[0].c2_71[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row6[0].c3_71[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_71[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_117[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMay_13:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow7[0].c2_84[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row7[0].c3_84[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_84[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_130[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMay_14:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow8[0].c2_97[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row8[0].c3_97[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_97[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row13[0].c3_143[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMay_15:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow9[0].c2_110[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row9[0].c3_110[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_110[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMay_16:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow10[0].c2_123[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row10[0].c3_123[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_123[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMay_17:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow11[0].c2_136[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row11[0].c3_136[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_136[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndMay_18:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow12[0].c2_149[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row12[0].c3_149[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_149[0]";
                    }
                    break;


                // covered June
                case (int)Structures.FormFields.CovIndJun_1:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line17[0].c1_017[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row1[0].c1_9[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row1[0].c1_9[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_9[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJun_2:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line18[0].c1_030[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row2[0].c1_22[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row2[0].c1_22[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_22[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJun_3:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line19[0].c1_043[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row3[0].c1_35[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row3[0].c1_35[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_35[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJun_4:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line20[0].c1_056[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row4[0].c1_48[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row4[0].c1_48[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_48[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJun_5:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line21[0].c1_069[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row5[0].c1_61[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row5[0].c1_61[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_61[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJun_6:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line22[0].c1_082[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row6[0].c1_74[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row6[0].c1_74[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_74[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJun_7:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow1[0].c2_07[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row1[0].c3_7[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_7[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_87[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJun_8:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow2[0].c2_20[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row2[0].c3_20[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_20[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_100[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJun_9:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow3[0].c2_33[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row3[0].c3_33[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_33[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_113[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJun_10:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow4[0].c2_46[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row4[0].c3_46[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_46[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_126[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJun_11:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow5[0].c2_59[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row5[0].c3_59[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_59[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_118[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJun_12:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow6[0].c2_72[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row6[0].c3_72[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_72[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_131[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJun_13:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow7[0].c2_85[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row7[0].c3_85[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_85[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row13[0].c3_144[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJun_14:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow8[0].c2_98[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row8[0].c3_98[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_98[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJun_15:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow9[0].c2_111[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row9[0].c3_111[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_111[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJun_16:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow10[0].c2_124[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row10[0].c3_124[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_124[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJun_17:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow11[0].c2_137[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row11[0].c3_137[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_137[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJun_18:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow12[0].c2_150[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row12[0].c3_150[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_150[0]";
                    }
                    break;


                // covered July
                case (int)Structures.FormFields.CovIndJul_1:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line17[0].c1_018[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row1[0].c1_10[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row1[0].c1_10[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_10[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJul_2:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line18[0].c1_031[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row2[0].c1_23[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row2[0].c1_23[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_23[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJul_3:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line19[0].c1_044[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row3[0].c1_36[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row3[0].c1_36[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_36[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJul_4:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line20[0].c1_057[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row4[0].c1_49[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row4[0].c1_49[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_49[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJul_5:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line21[0].c1_070[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row5[0].c1_62[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row5[0].c1_62[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_62[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJul_6:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line22[0].c1_083[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row6[0].c1_75[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row6[0].c1_75[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_75[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJul_7:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow1[0].c2_08[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row1[0].c3_8[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_8[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_88[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJul_8:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow2[0].c2_21[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row2[0].c3_21[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_21[0]";
                    }else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_101[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJul_9:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow3[0].c2_34[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row3[0].c3_34[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_34[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_114[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJul_10:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow4[0].c2_47[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row4[0].c3_47[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_47[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_127[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJul_11:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow5[0].c2_60[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row5[0].c3_60[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_60[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_119[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJul_12:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow6[0].c2_73[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row6[0].c3_73[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_73[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_132[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJul_13:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow7[0].c2_86[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row7[0].c3_86[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_86[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row13[0].c3_145[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJul_14:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow8[0].c2_99[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row8[0].c3_99[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_99[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJul_15:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow9[0].c2_112[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row9[0].c3_112[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_112[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJul_16:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow10[0].c2_125[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row10[0].c3_125[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_125[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJul_17:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow11[0].c2_138[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row11[0].c3_138[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_138[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndJul_18:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow12[0].c2_151[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row12[0].c3_151[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_151[0]";
                    }
                    break;


                // covered August
                case (int)Structures.FormFields.CovIndAug_1:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line17[0].c1_019[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row1[0].c1_11[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row1[0].c1_11[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_11[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndAug_2:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line18[0].c1_032[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row2[0].c1_24[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row2[0].c1_24[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_24[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndAug_3:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line19[0].c1_045[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row3[0].c1_37[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row3[0].c1_37[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_37[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndAug_4:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line20[0].c1_058[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row4[0].c1_50[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row4[0].c1_50[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_50[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndAug_5:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line21[0].c1_071[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row5[0].c1_63[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row5[0].c1_63[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_63[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndAug_6:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line22[0].c1_084[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row6[0].c1_76[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row6[0].c1_76[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_76[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndAug_7:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow1[0].c2_09[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row1[0].c3_9[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_9[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_89[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndAug_8:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow2[0].c2_22[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row2[0].c3_22[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_22[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_102[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndAug_9:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow3[0].c2_35[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row3[0].c3_35[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_35[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "opmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_115[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndAug_10:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow4[0].c2_48[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row4[0].c3_48[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_48[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_128[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndAug_11:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow5[0].c2_61[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row5[0].c3_61[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_61[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_120[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndAug_12:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow6[0].c2_74[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row6[0].c3_74[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_74[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_133[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndAug_13:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow7[0].c2_87[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row7[0].c3_87[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_87[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row13[0].c3_146[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndAug_14:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow8[0].c2_100[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row8[0].c3_100[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_100[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndAug_15:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow9[0].c2_113[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row9[0].c3_113[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_113[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndAug_16:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow10[0].c2_126[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row10[0].c3_126[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_126[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndAug_17:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow11[0].c2_139[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row11[0].c3_139[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_139[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndAug_18:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow12[0].c2_152[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row12[0].c3_152[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_152[0]";
                    }
                    break;


                // covered September
                case (int)Structures.FormFields.CovIndSep_1:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line17[0].c1_020[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row1[0].c1_12[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row1[0].c1_12[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_12[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSep_2:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line18[0].c1_033[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row2[0].c1_25[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row2[0].c1_25[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_25[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSep_3:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line19[0].c1_046[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row3[0].c1_38[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row3[0].c1_38[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_38[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSep_4:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line20[0].c1_059[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row4[0].c1_51[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row4[0].c1_51[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_51[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSep_5:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line21[0].c1_072[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row5[0].c1_64[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row5[0].c1_64[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_64[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSep_6:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line22[0].c1_085[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row6[0].c1_77[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row6[0].c1_77[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_77[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSep_7:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow1[0].c2_10[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row1[0].c3_10[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_10[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_90[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSep_8:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow2[0].c2_23[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row2[0].c3_23[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_23[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_103[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSep_9:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow3[0].c2_36[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row3[0].c3_36[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_36[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_116[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSep_10:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow4[0].c2_49[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row4[0].c3_49[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_49[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_129[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSep_11:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow5[0].c2_62[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row5[0].c3_62[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_62[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_121[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSep_12:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow6[0].c2_75[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row6[0].c3_75[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_75[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_134[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSep_13:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow7[0].c2_88[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row7[0].c3_88[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_88[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row13[0].c3_148[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSep_14:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow8[0].c2_101[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row8[0].c3_101[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_101[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSep_15:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow9[0].c2_114[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row9[0].c3_114[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_114[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSep_16:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow10[0].c2_127[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row10[0].c3_127[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_127[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSep_17:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow11[0].c2_140[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row11[0].c3_140[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_140[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndSep_18:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow12[0].c2_153[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row12[0].c3_153[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_153[0]";
                    }
                    break;


                // covered October
                case (int)Structures.FormFields.CovIndOct_1:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line17[0].c1_021[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row1[0].c1_13[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row1[0].c1_13[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_13[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndOct_2:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line18[0].c1_034[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row2[0].c1_26[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row2[0].c1_26[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_26[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndOct_3:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line19[0].c1_047[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row3[0].c1_39[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row3[0].c1_39[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_39[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndOct_4:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line20[0].c1_060[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row4[0].c1_52[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row4[0].c1_52[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_52[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndOct_5:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line21[0].c1_073[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row5[0].c1_65[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row5[0].c1_65[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_65[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndOct_6:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line22[0].c1_086[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row6[0].c1_78[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row6[0].c1_78[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_78[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndOct_7:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow1[0].c2_11[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row1[0].c3_11[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_11[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_91[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndOct_8:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow2[0].c2_24[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row2[0].c3_24[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_24[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_104[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndOct_9:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow3[0].c2_37[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row3[0].c3_37[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_37[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_117[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndOct_10:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow4[0].c2_50[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row4[0].c3_50[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_50[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_130[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndOct_11:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow5[0].c2_63[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row5[0].c3_63[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_63[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_122[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndOct_12:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow6[0].c2_76[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row6[0].c3_76[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_76[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_135[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndOct_13:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow7[0].c2_89[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row7[0].c3_89[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_89[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row13[0].c3_149[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndOct_14:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow8[0].c2_102[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row8[0].c3_102[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_102[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndOct_15:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow9[0].c2_115[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row9[0].c3_115[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_115[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndOct_16:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow10[0].c2_128[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row10[0].c3_128[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_128[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndOct_17:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow11[0].c2_141[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row11[0].c3_141[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_141[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndOct_18:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow12[0].c2_154[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row12[0].c3_154[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_154[0]";
                    }
                    break;


                // covered November
                case (int)Structures.FormFields.CovIndNov_1:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line17[0].c1_022[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row1[0].c1_14[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row1[0].c1_14[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_14[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndNov_2:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line18[0].c1_035[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row2[0].c1_27[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row2[0].c1_27[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_27[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndNov_3:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line19[0].c1_048[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row3[0].c1_40[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row3[0].c1_40[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_40[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndNov_4:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line20[0].c1_061[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row4[0].c1_53[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row4[0].c1_53[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_53[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndNov_5:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line21[0].c1_074[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row5[0].c1_66[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row5[0].c1_66[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_66[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndNov_6:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line22[0].c1_087[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row6[0].c1_79[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row6[0].c1_79[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_79[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndNov_7:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow1[0].c2_12[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row1[0].c3_12[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_12[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_92[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndNov_8:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow2[0].c2_25[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row2[0].c3_25[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_25[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_105[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndNov_9:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow3[0].c2_38[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row3[0].c3_38[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_38[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_118[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndNov_10:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow4[0].c2_51[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row4[0].c3_51[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_51[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_131[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndNov_11:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow5[0].c2_64[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row5[0].c3_64[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_64[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_123[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndNov_12:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow6[0].c2_77[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row6[0].c3_77[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_77[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_136[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndNov_13:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow7[0].c2_90[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row7[0].c3_90[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_90[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row13[0].c3_150[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndNov_14:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow8[0].c2_103[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row8[0].c3_103[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_103[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndNov_15:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow9[0].c2_116[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row9[0].c3_116[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_116[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndNov_16:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow10[0].c2_129[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row10[0].c3_129[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_129[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndNov_17:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow11[0].c2_142[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row11[0].c3_142[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_142[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndNov_18:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow12[0].c2_155[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row12[0].c3_155[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_155[0]";
                    }
                    break;


                // covered December
                case (int)Structures.FormFields.CovIndDec_1:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line17[0].c1_023[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row1[0].c1_15[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row1[0].c1_15[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_15[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDec_2:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line18[0].c1_036[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row2[0].c1_28[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row2[0].c1_28[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_28[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDec_3:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line19[0].c1_049[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row3[0].c1_41[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row3[0].c1_41[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_41[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDec_4:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line20[0].c1_062[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row4[0].c1_54[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row4[0].c1_54[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_54[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDec_5:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line21[0].c1_075[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row5[0].c1_67[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row5[0].c1_67[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_67[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDec_6:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page1[0].Line22[0].c1_088[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page1[0].Table2[0].Row6[0].c1_80[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page1[0].Table_Part3[0].Row6[0].c1_80[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_80[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDec_7:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow1[0].c2_13[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row1[0].c3_13[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row1[0].c3_13[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_93[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDec_8:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow2[0].c2_26[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row2[0].c3_26[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row2[0].c3_26[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_106[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDec_9:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow3[0].c2_39[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row3[0].c3_39[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row3[0].c3_39[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_119[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDec_10:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow4[0].c2_52[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row4[0].c1_52[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row4[0].c3_52[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_132[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDec_11:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow5[0].c2_65[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row5[0].c3_65[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row5[0].c3_65[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_124[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDec_12:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow6[0].c2_78[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row6[0].c3_78[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row6[0].c3_78[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_137[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDec_13:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow7[0].c2_91[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row7[0].c3_91[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row7[0].c3_91[0]";
                    }
                    else if (taxYr >= 2020)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row13[0].c3_151[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDec_14:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow8[0].c2_104[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row8[0].c3_104[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row8[0].c3_104[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDec_15:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow9[0].c2_117[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row9[0].c3_117[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row9[0].c3_117[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDec_16:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow10[0].c2_130[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row10[0].c3_130[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row10[0].c3_130[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDec_17:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow11[0].c2_143[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row11[0].c3_143[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row11[0].c3_143[0]";
                    }
                    break;

                case (int)Structures.FormFields.CovIndDec_18:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow12[0].c2_156[0]";
                    }
                    else if (taxYr == 2018)
                    {
                        return "topmostSubform[0].Page3[0].Table1[0].Row12[0].c3_156[0]";
                    }
                    else if (taxYr == 2019)
                    {
                        return "topmostSubform[0].Page3[0].Table_Part3[0].Row12[0].c3_156[0]";
                    }
                    break;



                // page 3 heading 
                case (int)Structures.FormFields.Page3HeadingName:
                    //return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow1[0].f2_01[0]";
                    return "topmostSubform[0].Page3[0].f3_001[0]";

                case (int)Structures.FormFields.Page3HeadingFirstName:
                    if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page3[0].f3_1[0]";
                    }
                    break;

                case (int)Structures.FormFields.Page3HeadingMiddleInitial:
                    if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page3[0].f3_2[0]";
                    }
                    break;

                case (int)Structures.FormFields.Page3HeadingLastName:
                    if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page3[0].f3_3[0]";
                    }
                    break;

                case (int)Structures.FormFields.Page3HeadingSSN:
                    if (taxYr == 2016 || taxYr == 2017)
                    {
                        //return "topmostSubform[0].Page3[0].Table_Part4[0].BodyRow1[0].f2_02[0]";
                        return "topmostSubform[0].Page3[0].f3_002[0]";
                    } else if (taxYr >= 2018)
                    {
                        return "topmostSubform[0].Page3[0].f3_4[0]";
                    }
                    break;

            }

            return id;
        }
    }
}
