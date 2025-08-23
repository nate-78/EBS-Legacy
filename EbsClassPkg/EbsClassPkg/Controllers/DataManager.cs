using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using EbsClassPkg.Models;


namespace EbsClassPkg.Controllers {
    // a basic Data Manager class for interacting with db(s)
    public class DataManager {
        // establish data connection
        public static SqlConnection BuildConnection() {
            SqlConnectionStringBuilder connBuilder = new SqlConnectionStringBuilder();
            connBuilder["Server"] = ConfigurationManager.AppSettings["DbServer"].ToString();
            connBuilder["User ID"] = ConfigurationManager.AppSettings["DbUserName"].ToString();
            connBuilder["Password"] = ConfigurationManager.AppSettings["DbPword"].ToString();
            connBuilder["Database"] = ConfigurationManager.AppSettings["DbName"].ToString();
            connBuilder["Integrated Security"] = false; // this means credentials are being passed in the string

            SqlConnection conn = new SqlConnection(connBuilder.ToString());

            return conn;
        } // end BuildConnection

        public static SqlConnection BuildUserMgmtConnection() {
            SqlConnectionStringBuilder connBuilder = new SqlConnectionStringBuilder();
            connBuilder["Server"] = ConfigurationManager.AppSettings["DbServer"].ToString();
            connBuilder["User ID"] = ConfigurationManager.AppSettings["DbUserName"].ToString();
            connBuilder["Password"] = ConfigurationManager.AppSettings["DbPword"].ToString();
            connBuilder["Database"] = ConfigurationManager.AppSettings["UserMgmtDbName"].ToString();
            connBuilder["Integrated Security"] = false; // this means credentials are being passed in the string

            SqlConnection conn = new SqlConnection(connBuilder.ToString());

            return conn;
        }

        public static SqlCommand PrepareCommand(string procedureName, SqlConnection connection) {
            SqlCommand command = new SqlCommand();

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = procedureName;
            command.Connection = connection;

            return command;
        }

        public static void IssueStatement(string sql, bool isForUserMgmt = false) {
            // determine which connection string to use
            SqlConnection conn;
            if (isForUserMgmt) {
                conn = BuildUserMgmtConnection();
            } else {
                conn = BuildConnection();
            }

            conn.Open();

            SqlCommand command = new SqlCommand(sql, conn);
            command.ExecuteNonQuery();

            conn.Close(); // cleanup
        }

        public static string SqlSafe(string inputString) {
            if (!String.IsNullOrEmpty(inputString)) {
                return inputString.Replace("'", "''").Trim();
            } else {
                return String.Empty;
            }
        }

        public static string CamelCase(string text) {
            if (!String.IsNullOrEmpty(text) && text == text.ToLower()) { // we only change text that was entered completely lower case
                string firstLetter = text.Substring(0, 1);
                string remainder = text.Substring(1);
                text = firstLetter.ToUpper() + remainder.ToLower();
            }
            return text;

        }


        public static string LimitName(string name) {
            if (!String.IsNullOrEmpty(name) && name.Length > 22) {
                name = name.Substring(0, 22);
            }
            return name;
        }
    }
}