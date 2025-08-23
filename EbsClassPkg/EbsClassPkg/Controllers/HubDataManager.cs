using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using EbsClassPkg.Models;

namespace EbsClassPkg.Controllers {
    // functions to interact with all the data that the Hub functions need
    public class HubDataManager {
        // EVENT LOGGING
        /// <summary>
        /// Log an activity
        /// </summary>
        /// <param name="activitySlug">A list of slugs is in the db: submission, status_update, error,
        /// user_edited, user_deactivated, company_edited, company_deactivated, report_run, archived_submission,
        /// login_success, login_failure, logout, form-generated, vault_item_added, vault_item_moved,
        /// vault_item_deleted, vault_item_downloaded</param>
        /// <param name="notes">Any notes needed</param>
        /// <param name="objId">If the activity dealt with an object (a user, submission, status update, etc), you can include
        ///  the object's id here.</param>
        /// <param name="objType">Specify the type of ID you used in the previous arg: user-id, company-id, receipt-id, 
        /// status-update-id, user-id, email, ein, vault-url, etc</param>
        public static void LogActivity(int currentUserId, int? impersonatingAsID, string activitySlug, string notes, string objId,
            string objType) {

            using (SqlConnection conn = DataManager.BuildUserMgmtConnection()) {
                conn.Open();
                SqlDataReader reader;
                SqlCommand cmd = DataManager.PrepareCommand("p_LogActivity", conn);
                // assign parameters
                if (cmd != null) {
                    cmd.Parameters.Add("@user_id", SqlDbType.Int).Value = String.IsNullOrEmpty(currentUserId.ToString()) ?
                        DBNull.Value : (object)currentUserId;
                    cmd.Parameters.Add("@activity_slug", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(activitySlug) ?
                        DBNull.Value : (object)activitySlug;
                    cmd.Parameters.Add("@notes", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(notes) ?
                        DBNull.Value : (object)notes;
                    cmd.Parameters.Add("@object_id", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(objId) ?
                        DBNull.Value : (object)objId;
                    cmd.Parameters.Add("@object_type", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(objType) ?
                        DBNull.Value : (object)objType;
                    cmd.Parameters.Add("@impersonatingAs", SqlDbType.Int).Value = String.IsNullOrEmpty(impersonatingAsID.ToString()) ?
                        DBNull.Value : (object)impersonatingAsID;
                }
                reader = cmd.ExecuteReader();
            }
        }


        // USER MANAGEMENT
        // insert user
        // will return list of users
        public static List<User> InsertUser(Person person, string email, string pword, Int32 userId, Int32 clientId, Int32 roleId, string[] appSlugs) {
            // prep params
            person.FirstName = DataManager.CamelCase(person.FirstName);
            person.MiddleName = DataManager.CamelCase(person.MiddleName);
            person.LastName = DataManager.CamelCase(person.LastName);
            person.Suffix = DataManager.CamelCase(person.Suffix);
            // do it!
            List<User> users = new List<User>();
            SqlDataReader reader;
            DataSet userInfo = new DataSet();
            using (SqlConnection conn = DataManager.BuildUserMgmtConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_InsertUser", conn);
                // assign parameters
                if (cmd != null) {
                    cmd.Parameters.Add("@email", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(email) ? DBNull.Value : (object)email;
                    cmd.Parameters.Add("@password", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(pword) ? DBNull.Value : (object)pword;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = userId;
                    cmd.Parameters.Add("@fName", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(person.FirstName) ? DBNull.Value : (object)person.FirstName;
                    cmd.Parameters.Add("@mName", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(person.MiddleName) ? DBNull.Value : (object)person.MiddleName;
                    cmd.Parameters.Add("@lName", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(person.LastName) ? DBNull.Value : (object)person.LastName;
                    cmd.Parameters.Add("@clientId", SqlDbType.Int).Value = String.IsNullOrEmpty(clientId.ToString()) ? DBNull.Value : (object)clientId.ToString();
                    cmd.Parameters.Add("@roleId", SqlDbType.Int).Value = String.IsNullOrEmpty(roleId.ToString()) ? DBNull.Value : (object)roleId.ToString();
                    //cmd.Parameters.Add("@appSlug", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(appSlug) ? DBNull.Value : (object)appSlug;
                }
                reader = cmd.ExecuteReader();
                userInfo.Load(reader, LoadOption.OverwriteChanges, "users"); // this overwrites the name of the datatable(s)
            }
            // add data to User object
            if (userInfo.Tables[0].Rows.Count > 0) {
                users = BuildUserList(userInfo.Tables[0]);
            }

            return users;
        }


        // get user (for log in purposes)
        public static User GetUser(string email) {
            List<User> userList = new List<User>();
            SqlDataReader reader;
            DataSet userInfo = new DataSet();
            using (SqlConnection conn = DataManager.BuildUserMgmtConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetUser", conn);
                // now assign parameters
                cmd.Parameters.Add("@email", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(email) ? DBNull.Value : (object)email;
                // execute it
                reader = cmd.ExecuteReader();
                userInfo.Load(reader, LoadOption.OverwriteChanges, "users"); // this overwrites the name of the datatable(s)
            }
            // add data to User object
            if (userInfo.Tables[0].Rows.Count > 0) {
                userList = BuildUserList(userInfo.Tables[0]);
            }

            if (userList == null || userList.Count == 0) {
                return getDummyUserObj(); // should only happen when testing
            } else {
                return userList.First();
            } // end if-else
        }

        // get user (for log in purposes)
        public static User GetUser(int userId) {
            List<User> userList = new List<User>();
            SqlDataReader reader;
            DataSet userInfo = new DataSet();
            using (SqlConnection conn = DataManager.BuildUserMgmtConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetUserById", conn);
                // now assign parameters
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = String.IsNullOrEmpty(userId.ToString()) ? DBNull.Value : (object)userId;
                // execute it
                reader = cmd.ExecuteReader();
                userInfo.Load(reader, LoadOption.OverwriteChanges, "users"); // this overwrites the name of the datatable(s)
            }
            // add data to User object
            if (userInfo.Tables[0].Rows.Count > 0) {
                userList = BuildUserList(userInfo.Tables[0]);
            }

            if (userList == null || userList.Count == 0) {
                return getDummyUserObj(); // should only happen when testing
            } else {
                return userList.First();
            } // end if-else
        }


        // Deactivate User
        public static void DeactivateUser(string email) {
            SqlDataReader reader;
            using (SqlConnection conn = DataManager.BuildUserMgmtConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_DeactivateUser", conn);
                // now assign parameters
                cmd.Parameters.Add("@email", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(email) ? DBNull.Value : (object)email;
                // execute it
                reader = cmd.ExecuteReader();
            }
        }

        // Activate User
        public static void ActivateUser(string email) {
            SqlDataReader reader;
            using (SqlConnection conn = DataManager.BuildUserMgmtConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_ActivateUser", conn);
                // now assign parameters
                cmd.Parameters.Add("@email", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(email) ? DBNull.Value : (object)email;
                // execute it
                reader = cmd.ExecuteReader();
            }
        }


        // get dummy user object for testing, when db is not available
        private static User getDummyUserObj() {
            User u = new User();
            Person p = new Person();
            p.FirstName = "Nate";
            p.LastName = "Owens";
            u.Person = p;
            u.IsAdmin = true;
            u.IsClient = false;
            u.HasAppAccess = true;
            return u;
        }


        // get all users (admin stuff)
        /// <summary>
        /// This function can occasionally return multiple rows of the same user. This happens when 
        /// the user is assigned to more than one application. Receiving function should be smart
        /// enough to consolidate them.
        /// </summary>
        /// <returns></returns>
        public static List<User> GetAllUsers() {
            List<User> users = new List<User>();
            SqlDataReader reader;
            DataSet userInfo = new DataSet();
            using (SqlConnection conn = DataManager.BuildUserMgmtConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetAllUsers", conn);
                // execute it
                reader = cmd.ExecuteReader();
                userInfo.Load(reader, LoadOption.OverwriteChanges, "users"); // this overwrites the name of the datatable(s)
            }
            // add data to User object
            if (userInfo.Tables[0].Rows.Count > 0) {
                users = BuildUserList(userInfo.Tables[0]);
            }

            return users;
        }


        // get list of applications and list of clients (companies)
        public static PairedList GetClientsAndApps() {
            SqlDataReader reader;
            DataSet dataResults = new DataSet();
            using (SqlConnection conn = DataManager.BuildUserMgmtConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetClientsAndApps", conn);
                // execute it
                reader = cmd.ExecuteReader();
                string[] tableNames = { "clients", "apps" };
                dataResults.Load(reader, LoadOption.OverwriteChanges, tableNames); // this overwrites the name of the datatable(s)
            }

            // create variables to store the info
            List<Company> clients = new List<Company>();
            List<App> apps = new List<App>();

            if (dataResults.Tables.Count == 2) {
                // build client list
                clients = buildClientList(dataResults.Tables["clients"]);
                // build app list
                apps = buildAppList(dataResults.Tables["apps"]);
            }

            PairedList pairedList = new PairedList();
            pairedList.CompList = clients;
            pairedList.AppList = apps;
            return pairedList;
        }


        // upsert apps for user
        public static void UpsertApps_User(string email, string[] appSlugs) {
            string sql = "";
            // get existing apps for this user
            List<App> apps = getAppsForUser(email);
            // compare them to the appSlugs array
            if (apps != null && apps.Count > 0) {
                foreach (App a in apps) {
                    // if any apps are assigned but aren't in the array, remove them
                    if (appSlugs == null || !appSlugs.Contains(a.App_Slug)) {
                        sql += "delete from user_app where [user_id] = (select id from [users] where email = '" + email
                            + "') and app_id = (select id from apps where app_slug = '" + a.App_Slug + "');";
                    }
                } // end foreach app
            } // end if
              // if any are in the array, but not already assigned, add them
            if (appSlugs != null) {
                foreach (string s in appSlugs) {
                    bool matches = false;
                    foreach (App app in apps) {
                        if (s == app.App_Slug) {
                            matches = true;
                        }
                    }
                    if (!matches) { // if it doesn't match, add it
                        sql += "insert into user_app (user_id, app_id) values ((select id from [users] where email = '" + email
                            + "'), (select id from apps where app_slug = '" + s + "'));";
                    } // otherwise, don't do anything
                } // end foreach string
            }


            // submit the sql statement
            if (!String.IsNullOrEmpty(sql)) {
                DataManager.IssueStatement(sql, true);
            }
        }

        // upsert apps for company (client)
        public static void UpsertApps_Comp(string ein, string[] appSlugs) {
            string sql = "";
            // get existing apps for this user
            List<App> apps = getAppsForComp(ein);
            // compare them to the appSlugs array
            if (apps != null && apps.Count > 0) {
                foreach (App a in apps) {
                    // if any apps are assigned but aren't in the array, remove them and any users who are also assigned within that company
                    if (appSlugs == null || !appSlugs.Contains(a.App_Slug)) {
                        sql += "delete from client_app where [client_id] = (select id from [clients] where ein = '" + ein
                            + "') and app_id = (select id from apps where app_slug = '" + a.App_Slug + "');";
                        sql += removeAppAccessFromCompanyUsers(a, ein);
                    }
                } // end foreach app
            } // end if
              // if any are in the array, but not already assigned, add them
            if (appSlugs != null) {
                foreach (string s in appSlugs) {
                    bool matches = false;
                    foreach (App app in apps) {
                        if (s == app.App_Slug) {
                            matches = true;
                        }
                    }
                    if (!matches) { // if it doesn't match, add it
                        sql += "insert into client_app (client_id, app_id) values ((select id from [clients] where ein = '" + ein
                            + "'), (select id from apps where app_slug = '" + s + "'));";
                    } // otherwise, don't do anything
                } // end foreach string
            }


            // submit the sql statement
            if (!String.IsNullOrEmpty(sql)) {
                DataManager.IssueStatement(sql, true);
            }
        }


        // return a SQL string with commands to delete user_app associations
        private static string removeAppAccessFromCompanyUsers(App a, string ein) {
            // get all the users for a company that are assigned to a particular app
            List<User> users = getUsersAssignedToCompApp(a, ein);
            string sql = "";
            if (users != null && users.Count > 0) {
                foreach (User u in users) {
                    sql += "delete from user_app where [user_id]  = " + u.ID.ToString() + " and [app_id] = (select id from [apps] where app_slug = '" + a.App_Slug + "');";
                }
            }

            return sql;
        }


        private static List<User> getUsersAssignedToCompApp(App a, string ein) {
            List<User> users = new List<User>();
            SqlDataReader reader;
            DataSet dataResults = new DataSet();
            using (SqlConnection conn = DataManager.BuildUserMgmtConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetUsers_ClientApp", conn);
                cmd.Parameters.Add("@ein", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(ein) ? DBNull.Value : (object)ein;
                cmd.Parameters.Add("@appSlug", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(a.App_Slug) ? DBNull.Value : (object)a.App_Slug;
                // execute it
                reader = cmd.ExecuteReader();
                dataResults.Load(reader, LoadOption.OverwriteChanges, "users"); // this overwrites the name of the datatable(s)
            }
            if (dataResults.Tables != null && dataResults.Tables[0].Rows != null && dataResults.Tables[0].Rows.Count > 0) {
                users = BuildUserList(dataResults.Tables[0]);
            }
            return users;
        }


        private static List<App> getAppsForUser(string email) {
            List<App> apps = new List<App>();
            SqlDataReader reader;
            DataSet dataResults = new DataSet();
            using (SqlConnection conn = DataManager.BuildUserMgmtConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetApps_User", conn);
                cmd.Parameters.Add("@email", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(email) ? DBNull.Value : (object)email;
                // execute it
                reader = cmd.ExecuteReader();
                dataResults.Load(reader, LoadOption.OverwriteChanges, "apps"); // this overwrites the name of the datatable(s)
            }
            if (dataResults.Tables != null && dataResults.Tables[0].Rows != null && dataResults.Tables[0].Rows.Count > 0) {
                foreach (DataRow row in dataResults.Tables[0].Rows) {
                    App a = new App();
                    a.App_Name = row["app_name"].ToString();
                    a.App_Slug = row["app_slug"].ToString();
                    apps.Add(a);
                }
            }
            return apps;
        }

        private static List<App> getAppsForComp(string ein) {
            List<App> apps = new List<App>();
            SqlDataReader reader;
            DataSet dataResults = new DataSet();
            using (SqlConnection conn = DataManager.BuildUserMgmtConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetApps_Client", conn);
                cmd.Parameters.Add("@ein", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(ein) ? DBNull.Value : (object)ein;
                // execute it
                reader = cmd.ExecuteReader();
                dataResults.Load(reader, LoadOption.OverwriteChanges, "apps"); // this overwrites the name of the datatable(s)
            }
            if (dataResults.Tables != null && dataResults.Tables[0].Rows != null && dataResults.Tables[0].Rows.Count > 0) {
                foreach (DataRow row in dataResults.Tables[0].Rows) {
                    App a = new App();
                    a.App_Name = row["app_name"].ToString();
                    a.App_Slug = row["app_slug"].ToString();
                    apps.Add(a);
                }
            }
            return apps;
        }



        // COMPANY / CLIENT MANAGEMENT
        public static Int32 UpsertClient_Admin(string clientName1, string clientName2, string ein, string street1, string street2,
            string city, string state, int? zip, int? zipExt, string country, bool isBilling, bool isPhysical, bool isPrimary,
            string branchId, string displayName) {
            // branch ID is simply for client's purposes.  In case they have a location number that they'd like to use
            clientName1 = DataManager.CamelCase(clientName1);
            clientName2 = DataManager.CamelCase(clientName2);
            street1 = DataManager.CamelCase(street1);
            street2 = DataManager.CamelCase(street2);
            city = DataManager.CamelCase(city);
            state = state.ToUpper();
            displayName = DataManager.CamelCase(displayName);
            ein = ein.Replace("-", "");

            int clientId = 0; // instantiate the id. sql will return the real one

            using (SqlConnection conn = DataManager.BuildUserMgmtConnection()) {
                conn.Open();
                SqlDataReader reader;
                SqlCommand cmd = DataManager.PrepareCommand("p_InsertClient", conn);
                // assign parameters
                if (cmd != null) {
                    cmd.Parameters.Add("@clientName1", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(clientName1) ? DBNull.Value : (object)clientName1;
                    cmd.Parameters.Add("@clientName2", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(clientName2) ? DBNull.Value : (object)clientName2;
                    cmd.Parameters.Add("@ein", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(ein) ? DBNull.Value : (object)ein;
                    cmd.Parameters.Add("@street1", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(street1) ? DBNull.Value : (object)street1;
                    cmd.Parameters.Add("@street2", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(street2) ? DBNull.Value : (object)street2;
                    cmd.Parameters.Add("@city", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(city) ? DBNull.Value : (object)city;
                    cmd.Parameters.Add("@state", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(state) ? DBNull.Value : (object)state;
                    cmd.Parameters.Add("@zip", SqlDbType.Int).Value = String.IsNullOrEmpty(zip.ToString()) ? DBNull.Value : (object)zip.ToString();
                    cmd.Parameters.Add("@zipExt", SqlDbType.Int).Value = String.IsNullOrEmpty(zipExt.ToString()) ? DBNull.Value : (object)zipExt.ToString();
                    cmd.Parameters.Add("@country", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(country) ? DBNull.Value : (object)country;
                    cmd.Parameters.Add("@isBilling", SqlDbType.Bit).Value = String.IsNullOrEmpty(isBilling.ToString()) ? DBNull.Value : (object)isBilling; // toString?
                    cmd.Parameters.Add("@isPhysical", SqlDbType.Bit).Value = String.IsNullOrEmpty(isPhysical.ToString()) ? DBNull.Value : (object)isPhysical;
                    cmd.Parameters.Add("@isPrimary", SqlDbType.Bit).Value = String.IsNullOrEmpty(isPrimary.ToString()) ? DBNull.Value : (object)isPrimary;
                    cmd.Parameters.Add("@branchId", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(branchId) ? DBNull.Value : (object)branchId;
                    cmd.Parameters.Add("@displayName", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(displayName) ? DBNull.Value : (object)displayName;
                }
                reader = cmd.ExecuteReader();

                DataSet results = new DataSet();
                results.Load(reader, LoadOption.OverwriteChanges, "company-id");
                if (results != null && results.Tables != null && results.Tables.Count > 0 && results.Tables[0].Rows != null && results.Tables[0].Rows.Count > 0) {
                    foreach (DataRow row in results.Tables[0].Rows) { // there should only be one row
                        clientId = Convert.ToInt32(row[0].ToString());
                    }
                }
            }
            return clientId;
        }


        // get company by id
        public static Company GetCompany_Admin(Int32 companyId) {
            List<Company> comps = new List<Company>();
            SqlDataReader reader;
            DataSet compInfo = new DataSet();
            using (SqlConnection conn = DataManager.BuildUserMgmtConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetClient", conn);
                cmd.Parameters.Add("@clientId", SqlDbType.Int).Value = String.IsNullOrEmpty(companyId.ToString()) ? DBNull.Value : (object)companyId.ToString();
                // execute it
                reader = cmd.ExecuteReader();
                compInfo.Load(reader, LoadOption.OverwriteChanges, "company"); // this overwrites the name of the datatable(s)
            }
            // add data to User object
            if (compInfo.Tables[0].Rows.Count > 0) {
                comps = BuildCompanyList(compInfo.Tables[0]);
            }

            return comps.First();
        }


        public static Company GetCompanyByUser(User user) {
            List<Company> comps = new List<Company>();
            SqlDataReader reader;
            DataSet compInfo = new DataSet();
            using (SqlConnection conn = DataManager.BuildUserMgmtConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetClientByUser", conn);
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = String.IsNullOrEmpty(user.ID.ToString()) ? DBNull.Value : (object)user.ID;
                // execute it
                reader = cmd.ExecuteReader();
                compInfo.Load(reader, LoadOption.OverwriteChanges, "company"); // this overwrites the name of the datatable(s)
            }
            // add data to User object
            if (compInfo.Tables[0].Rows.Count > 0) {
                comps = BuildCompanyList(compInfo.Tables[0]);
            }

            return comps.First();
        }



        // get all companies
        public static List<Company> GetCompanyList_Admin(bool includeEBS = true) {
            List<Company> companies = new List<Company>();
            SqlDataReader reader;
            DataSet compInfo = new DataSet();
            using (SqlConnection conn = DataManager.BuildUserMgmtConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetClientList", conn);
                // execute it
                reader = cmd.ExecuteReader();
                compInfo.Load(reader, LoadOption.OverwriteChanges, "company"); // this overwrites the name of the datatable(s)
            }
            // add data to User object
            if (compInfo.Tables[0].Rows.Count > 0) {
                companies = BuildCompanyList(compInfo.Tables[0], includeEBS);
            }

            return companies;
        }


        // get client list (for acafiling)
        public static List<Company> GetClientList_Aca() {
            List<Company> companies = new List<Company>();
            SqlDataReader reader;
            DataSet compInfo = new DataSet();
            using (SqlConnection conn = DataManager.BuildConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetClientList", conn);
                // execute it
                reader = cmd.ExecuteReader();
                compInfo.Load(reader, LoadOption.OverwriteChanges, "companies"); // this overwrites the name of the datatable(s)
            }
            // add data to User object
            if (compInfo.Tables[0].Rows.Count > 0) {
                companies = BuildCompanyList(compInfo.Tables[0]);
            }

            return companies;
        }



        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // REPORTING 
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        // get User Activity Report
        public static List<Activity> GetUserActivityReport(int userId, string activity, bool allDates, DateTime startDate, DateTime endDate) {
            List<Activity> activityLog = new List<Activity>();
            SqlDataReader reader;
            DataSet activityData = new DataSet();
            using (SqlConnection conn = DataManager.BuildUserMgmtConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetUserActivityReport", conn);
                // assign params
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = String.IsNullOrEmpty(userId.ToString()) ? DBNull.Value : (object)userId;
                cmd.Parameters.Add("@activity", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(activity) ? DBNull.Value : (object)activity;
                cmd.Parameters.Add("@dateFrom", SqlDbType.DateTime2).Value = String.IsNullOrEmpty(startDate.ToString()) ?
                    DBNull.Value : (object)startDate;
                cmd.Parameters.Add("@dateTo", SqlDbType.DateTime2).Value = String.IsNullOrEmpty(endDate.ToString()) ?
                    DBNull.Value : (object)endDate;
                cmd.Parameters.Add("@allDates", SqlDbType.Bit).Value = String.IsNullOrEmpty(allDates.ToString()) ?
                    DBNull.Value : (object)allDates;

                // execute the call
                reader = cmd.ExecuteReader();
                activityData.Load(reader, LoadOption.OverwriteChanges, "activityLog");
            }

            // build the list
            if (activityData.Tables != null && activityData.Tables.Count > 0 &&
               activityData.Tables[0].Rows != null && activityData.Tables[0].Rows.Count > 0) {

                foreach (DataRow row in activityData.Tables[0].Rows) {
                    Activity a = new Activity();
                    a = buildActivityObj(row, activity);
                    activityLog.Add(a);
                }
            }

            return activityLog;
        }


        // get all companies that have done ACA submissions
        // used only HUB, despite its name
        public static List<Company> GetCompaniesWithAcaSubs() {
            List<Company> companies = new List<Company>();
            SqlDataReader reader;
            DataSet compInfo = new DataSet();
            using (SqlConnection conn = DataManager.BuildConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetCompaniesWithAcaSubs", conn);
                // execute it
                reader = cmd.ExecuteReader();
                compInfo.Load(reader, LoadOption.OverwriteChanges, "company"); // this overwrites the name of the datatable(s)
            }
            // for some reason, wants a primary key set
            DataColumn[] keyCols = new DataColumn[1];
            keyCols[0] = compInfo.Tables[0].Columns["client_id"];
            compInfo.Tables[0].PrimaryKey = keyCols;

            // add data to User object
            if (compInfo.Tables[0].Rows.Count > 0) {
                foreach (DataRow row in compInfo.Tables[0].Rows) {
                    Company comp = new Company();
                    comp = BuildCompany(row);
                    companies.Add(comp);
                }
            }

            return companies;
        }


        // get the tax years that a company filed ACA with us
        // used only HUB, despite its name
        public static List<Int32> GetAcaTaxYrsForCompany(Int32 compId) {
            List<Int32> taxYrs = new List<Int32>();
            SqlDataReader reader;
            DataSet taxYrData = new DataSet();
            using (SqlConnection conn = DataManager.BuildConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetAcaTaxYrForCompany", conn);
                cmd.Parameters.Add("@compId", SqlDbType.Int).Value = String.IsNullOrEmpty(compId.ToString()) ?
                    DBNull.Value : (object)compId;
                // execute it
                reader = cmd.ExecuteReader();
                taxYrData.Load(reader, LoadOption.OverwriteChanges, "tax-yrs"); // this overwrites the name of the datatable(s)
            }

            // add data to list
            if (taxYrData.Tables[0].Rows.Count > 0) {
                foreach (DataRow row in taxYrData.Tables[0].Rows) {
                    taxYrs.Add(Convert.ToInt32(row["tax_yr"]));
                }
            }

            return taxYrs;
        }


        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // DASHBOARD STATS
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        // get total count of ACA subs for a given company
        public static int GetTotACASubCount(User user) {
            // get count of non-test submissions for a specific tax year
            int count = 0;
            int taxYr = 2016; // TODO: make this a settable variable
            if (!user.IsClient) { // get them all
                SqlDataReader reader;
                DataSet results = new DataSet();
                using (SqlConnection conn = DataManager.BuildConnection()) {
                    conn.Open();
                    SqlCommand cmd = DataManager.PrepareCommand("p_GetTotCountAcaSubs", conn);
                    cmd.Parameters.Add("@taxYr", SqlDbType.Int).Value = taxYr;
                    // execute it
                    reader = cmd.ExecuteReader();
                    results.Load(reader, LoadOption.OverwriteChanges, "count"); // this overwrites the name of the datatable(s)
                }
                // get the count
                if (results.Tables[0].Rows.Count > 0) {
                    foreach (DataRow row in results.Tables[0].Rows) {
                        count = Convert.ToInt32(row["count"].ToString());
                    }
                }
            } // TODO: may not need to get ALL subs in case some are for clients of a subscriber. Gotta figure out what to do if the user is a subscriber. How to marry up clients from user_mgmt db and companies from aca db
            return count;
        }


        // 
        public static string GetBasicSubStats(User user) {
            string s = "";
            int taxYr = 2016; // TODO: make this a settable variable
            if (!user.IsClient) { // get them all
                SqlDataReader reader;
                DataSet results = new DataSet();
                using (SqlConnection conn = DataManager.BuildConnection()) {
                    conn.Open();
                    SqlCommand cmd = DataManager.PrepareCommand("p_GetBasicSubStats", conn);
                    cmd.Parameters.Add("@taxYr", SqlDbType.Int).Value = taxYr;
                    // execute it
                    reader = cmd.ExecuteReader();
                    results.Load(reader, LoadOption.OverwriteChanges, "totalSubs", "accepted1094s", "accepted1095s"); // this overwrites the name of the datatable(s)
                }
                // get the count
                if (results.Tables[0].Rows.Count > 0) {
                    foreach (DataRow row in results.Tables[0].Rows) {
                        s += row["count"].ToString() + ",";
                    }
                }
                if (results.Tables[1].Rows.Count > 0) {
                    foreach (DataRow row in results.Tables[1].Rows) {
                        s += row["count"].ToString() + ",";
                    }
                }
                if (results.Tables[2].Rows.Count > 0) {
                    foreach (DataRow row in results.Tables[2].Rows) {
                        s += row["count"].ToString() + ",";
                    }
                }
                s = s.TrimEnd(',');
            } // TODO: may not need to get ALL subs in case some are for clients of a subscriber. Gotta figure out what to do if the user is a subscriber. How to marry up clients from user_mgmt db and companies from aca db

            return s;
        }


        // 
        public static string GetLastLoginChart(User user) {
            string s = "";
            SqlDataReader reader;
            DataSet results = new DataSet();
            using (SqlConnection conn = DataManager.BuildUserMgmtConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetLastLoginAllUsers", conn);
                // execute it
                reader = cmd.ExecuteReader();
                results.Load(reader, LoadOption.OverwriteChanges, "lastLogin"); // this overwrites the name of the datatable(s)
            }
            if (results.Tables.Count > 0) {
                s = compileLastLoginData(results.Tables[0], user);
            }

            return s;
        }

        private static string compileLastLoginData(DataTable table, User user) {
            string data = "";
            // put each user in 1 of 3 groups:
            List<User> withinWeek = new List<User>();
            List<User> withinMonth = new List<User>();
            List<User> longer = new List<User>();
            if (table.Rows.Count > 0) {
                foreach (DataRow row in table.Rows) {
                    var validUser = false; // which users should we proceed with?
                    // if the user is an EBS admin, give them the count for all users
                    if (user.IsAdmin && !user.IsClient) {
                        validUser = true;
                    } else {
                        // otherwise, only give them the user info on users within their own company
                        if (row["client_id"].ToString() == user.Company.ID.ToString()) {
                            validUser = true;
                        }
                    }
                    // if this is a valid user, add it to the appropriate list
                    if (validUser) {
                        User u = buildUserObj(row);
                        DateTime lastLogin = Convert.ToDateTime("2016-01-01 12:00:00"); // earliest date in the system
                        if (!String.IsNullOrEmpty(row["last_login_time"].ToString())) {
                            lastLogin = Convert.ToDateTime(row["last_login_time"]);
                        }
                        DateTime now = DateTime.Now;
                        if (lastLogin >= now.AddDays(-7)) { // if within last week...
                            withinWeek.Add(u);
                        } else if (lastLogin >= now.AddDays(-30)) { // if within last month
                            withinMonth.Add(u);
                        } else {
                            longer.Add(u);
                        }
                    }
                } // end foreach
            } // end if

            if (withinWeek.Count > 0) {
                data += withinWeek.Count.ToString() + ",";
            } else {
                data += "0,";
            }
            if (withinMonth.Count > 0) {
                data += withinMonth.Count.ToString() + ",";
            } else {
                data += "0,";
            }
            if (longer.Count > 0) {
                data += longer.Count.ToString() + ",";
            } else {
                data += "0";
            }

            return data;
        }



        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // VAULT FUNCTIONS
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        // get all vault items for a user
        public static List<VaultItem> GetVaultByUser(User currUser) {
            // hit the db for the entries that this user can access

            // do I need to pass a parameter?  If the currUser is an EBS admin, then we need everything.
            // how do we display it so it makes sense?  Have root-level folders that correspond to different companies?
            // what about cases where a company has a client? -- future stuff??
            // If user is not an EBS admin then pass company id

            List<VaultItem> items = new List<VaultItem>();

            SqlDataReader reader;
            DataSet results = new DataSet();
            using (var conn = DataManager.BuildUserMgmtConnection()) {
                conn.Open();
                var cmd = DataManager.PrepareCommand("p_GetVaultItems", conn);
                // if the user isn't an EBS admin (superadmin), supply their company id
                if (currUser.IsClient || (!currUser.IsClient && !currUser.IsAdmin)) {
                    cmd.Parameters.Add("@compId", SqlDbType.Int).Value = currUser.Company.ID;
                }
                // submit the call
                reader = cmd.ExecuteReader();
                results.Load(reader, LoadOption.OverwriteChanges, "vault");
            }

            // populate vault items list
            if (results != null && results.Tables.Count > 0) {
                items = buildVaultItemList(results.Tables[0]);
                items = sortVaultItems(items, currUser);
            }

            return items;
        }


        /// <summary>
        /// insert a new item into the Vault
        /// </summary>
        /// <param name="name">This parameter can NOT be null or empty</param>
        /// <param name="isFile">This parameter can NOT be null</param>
        /// <param name="isAdminLvl">This parameter can NOT be null</param>
        /// <param name="vaultUrl">This parameter can NOT be null or empty</param>
        /// <param name="compId"></param>
        /// <param name="clientId"></param>
        /// <param name="parentId"></param>
        /// <param name="currUser">Used to determine if the user is an EBS Admin or not. In cases where a parentId is submitted, an empty User object can be inserted here.</param>
        /// <param name="isRoot">Optional parameter. Only set to 'true' when it's a system-generated folder at the root-level</param>
        /// <param name="isSysGenerated">Optional parameter. Only set to true when it's a system-generated folder that users can't delete.</param>
        public static VaultItem InsertVaultItem(string name, bool isFile, bool isAdminLvl, string vaultUrl, string fileExt, int compId, int? clientId, 
            int? parentId, User currUser, bool isRoot = false, bool isSysGenerated = false) {

            bool isEbsAdmin = false;
            if (currUser.IsAdmin && !currUser.IsClient) {
                isEbsAdmin = true;
            }

            SqlDataReader reader;
            DataSet results = new DataSet();
            using (var conn = DataManager.BuildUserMgmtConnection()) {
                conn.Open();
                var cmd = DataManager.PrepareCommand("p_InsertVaultItem", conn);
                // add parameters
                cmd.Parameters.Add("@name", SqlDbType.NVarChar).Value = name;
                cmd.Parameters.Add("@isFile", SqlDbType.Bit).Value = isFile;
                cmd.Parameters.Add("@isAdminLvl", SqlDbType.Bit).Value = isAdminLvl;
                cmd.Parameters.Add("@vaultUrl", SqlDbType.NVarChar).Value = vaultUrl;
                cmd.Parameters.Add("@compId", SqlDbType.Int).Value = String.IsNullOrEmpty(compId.ToString()) ? DBNull.Value : (object)compId;
                cmd.Parameters.Add("@clientId", SqlDbType.Int).Value = String.IsNullOrEmpty(clientId.ToString()) ? DBNull.Value : (object)clientId;
                cmd.Parameters.Add("@parentId", SqlDbType.Int).Value = String.IsNullOrEmpty(parentId.ToString()) ? DBNull.Value : (object)parentId;
                cmd.Parameters.Add("@fileExt", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(fileExt) ? DBNull.Value : (object)fileExt;
                cmd.Parameters.Add("@isRoot", SqlDbType.Bit).Value = isRoot;
                cmd.Parameters.Add("@isSysGenerated", SqlDbType.Bit).Value = isSysGenerated;
                cmd.Parameters.Add("@isEbsAdmin", SqlDbType.Bit).Value = isEbsAdmin;
                // execute
                reader = cmd.ExecuteReader();
                results.Load(reader, LoadOption.OverwriteChanges, "vault");
            }

            VaultItem item = new VaultItem();
            if (results != null && results.Tables.Count > 0 && results.Tables[0].Rows.Count > 0) {
                foreach (DataRow row in results.Tables[0].Rows) { // (there's only one)
                    item.ID = Convert.ToInt32(row[0]);
                    item.Name = name;
                    item.IsFile = isFile;
                    item.IsAdminLevel = isAdminLvl;
                    item.VaultURL = vaultUrl;
                    item.FileExt = fileExt;
                    item.CompID = compId;
                    if (!String.IsNullOrEmpty(clientId.ToString())) {
                        item.ClientID = (int)clientId;
                    }
                    if (!String.IsNullOrEmpty(parentId.ToString())) {
                        item.ParentVaultItemID = (int)parentId;
                    }
                }
            }
            return item;
        }

        public static void DeleteVaultItemFromDb(string vaultUrl) {
            SqlDataReader reader;
            using (var conn = DataManager.BuildUserMgmtConnection()) {
                conn.Open();
                var cmd = DataManager.PrepareCommand("p_DeleteVaultItemByUrl", conn);
                // if the user isn't an EBS admin (superadmin), supply their company id
                cmd.Parameters.Add("@url", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(vaultUrl) ? DBNull.Value : (object)vaultUrl;
                // submit the call
                reader = cmd.ExecuteReader();
            }
        }


        public static VaultItem GetVaultItemByVaultUrl(string url) {
            VaultItem i = new VaultItem();
            List<VaultItem> items = new List<VaultItem>();

            SqlDataReader reader;
            DataSet results = new DataSet();
            using (var conn = DataManager.BuildUserMgmtConnection()) {
                conn.Open();
                var cmd = DataManager.PrepareCommand("p_GetVaultItemByUrl", conn);
                // if the user isn't an EBS admin (superadmin), supply their company id
                cmd.Parameters.Add("@url", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(url) ? DBNull.Value : (object)url;
                // submit the call
                reader = cmd.ExecuteReader();
                results.Load(reader, LoadOption.OverwriteChanges, "vault");
            }

            // populate vault items list
            if (results != null && results.Tables.Count > 0) {
                items = buildVaultItemList(results.Tables[0]);
            }

            if (items != null && items.Count > 0) {
                i = items.First();
            }

            return i;
        }


        public static VaultItem GetSubscriberVaultFolder(Int32 clientId) {
            VaultItem i = new VaultItem();
            List<VaultItem> items = new List<VaultItem>();

            SqlDataReader reader;
            DataSet results = new DataSet();
            using (var conn = DataManager.BuildUserMgmtConnection()) {
                conn.Open();
                var cmd = DataManager.PrepareCommand("p_GetSubscriberVaultFolder", conn);
                cmd.Parameters.Add("@clientId", SqlDbType.Int).Value = String.IsNullOrEmpty(clientId.ToString()) ? DBNull.Value : (object)clientId;
                // submit the call
                reader = cmd.ExecuteReader();
                results.Load(reader, LoadOption.OverwriteChanges, "vault");
            }

            // populate vault items list
            if (results != null && results.Tables.Count > 0) {
                items = buildVaultItemList(results.Tables[0]);
            }

            if (items !=  null && items.Count > 0) {
                i = items.First();
            }

            return i;
        }


        // get the URL for a company's FormBuilder folder based on a user's email address
        public static VaultItem GetFormBuilderFolderFromUserEmail(string email) {
            VaultItem folder = new VaultItem();
            List<VaultItem> items = new List<VaultItem>();
            SqlDataReader reader;
            DataSet results = new DataSet();
            using (var conn = DataManager.BuildUserMgmtConnection()) {
                conn.Open();
                var cmd = DataManager.PrepareCommand("p_GetFormBuilderFolderUrlFromUserEmail", conn);
                cmd.Parameters.Add("@email", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(email) ? DBNull.Value : (object)email;
                // submit the call
                reader = cmd.ExecuteReader();
                results.Load(reader, LoadOption.OverwriteChanges, "vault");
            }

            // populate vault items list
            if (results != null && results.Tables.Count > 0 && results.Tables[0].Rows != null && results.Tables[0].Rows.Count > 0) {
                items = buildVaultItemList(results.Tables[0]);
                if (items != null && items.Count > 0) {
                    folder = items.First();
                }
            }

            return folder;
        }


        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // OBJECT BUILDERS
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        // build Person object (used in all applications)
        public static Person BuildPerson(DataRow row) {
            Person p = new Person();
            if (row.Table.Columns.Contains("first_name")) {
                if (row.Table.Columns.Contains("person_id") && !String.IsNullOrEmpty(row["person_id"].ToString())) {
                    p.ID = Convert.ToInt32(row["person_id"].ToString());
                }
                if (!String.IsNullOrEmpty(row["first_name"].ToString())) {
                    p.FirstName = row["first_name"].ToString();
                }
                if (row.Table.Columns.Contains("middle_name") && !String.IsNullOrEmpty(row["middle_name"].ToString())) {
                    p.MiddleName = row["middle_name"].ToString();
                }
                if (!String.IsNullOrEmpty(row["last_name"].ToString())) {
                    p.LastName = row["last_name"].ToString();
                }
                if (row.Table.Columns.Contains("suffix") && !String.IsNullOrEmpty(row["suffix"].ToString())) {
                    p.Suffix = row["suffix"].ToString();
                }
                if (row.Table.Columns.Contains("lastn") && !String.IsNullOrEmpty(row["lastn"].ToString())) {
                    p.LastFourSSN = row["lastn"].ToString();
                }
            }

            return p;
        }


        public static Company BuildCompany(DataRow row) {
            var comp = new Company();
            var address = new Address();
            // company details
            // TODO: change out column names to match acafiling db
            if (row.Table.Columns.Contains("client_id")) {
                if (!String.IsNullOrEmpty(row["client_id"].ToString())) {
                    comp.ID = Convert.ToInt32(row["client_id"].ToString());
                }
            }
            if (row.Table.Columns.Contains("name") && !String.IsNullOrEmpty(row["name"].ToString())) {
                comp.Name = row["name"].ToString();
            }
            if (row.Table.Columns.Contains("name1") && !String.IsNullOrEmpty(row["name1"].ToString())) {
                comp.Name = row["name1"].ToString();
            }
            if (row.Table.Columns.Contains("name2") && !String.IsNullOrEmpty(row["name2"].ToString())) {
                comp.Name2 = row["name2"].ToString();
            }
            if (row.Table.Columns.Contains("display_name") && !String.IsNullOrEmpty(row["display_name"].ToString())) {
                comp.DisplayName = row["display_name"].ToString();
            } else {
                comp.DisplayName = DataManager.LimitName(comp.Name);
            }
            if (row.Table.Columns.Contains("ein") && !String.IsNullOrEmpty(row["ein"].ToString())) {
                comp.EIN = row["ein"].ToString();
            }
            if (row.Table.Columns.Contains("contact_phone") && !String.IsNullOrEmpty(row["contact_phone"].ToString())) {
                comp.ContactPhone = row["contact_phone"].ToString();
            }

            if (row.Table.Columns.Contains("client_is_active") && !String.IsNullOrEmpty(row["client_is_active"].ToString())) {
                comp.IsActive = Convert.ToBoolean(row["client_is_active"].ToString());
            }
            if (row.Table.Columns.Contains("client_created") && !String.IsNullOrEmpty(row["client_created"].ToString())) {
                comp.Created = Convert.ToDateTime(row["client_created"].ToString());
            }
            if (row.Table.Columns.Contains("client_last_updated") && !String.IsNullOrEmpty(row["client_last_updated"].ToString())) {
                comp.LastUpdated = Convert.ToDateTime(row["client_last_updated"].ToString());
            }
            if (row.Table.Columns.Contains("last_tax_yr") && !String.IsNullOrEmpty(row["last_tax_yr"].ToString())) {
                comp.LastAcaTaxYr = Convert.ToInt32(row["last_tax_yr"].ToString());
            }
            // add apps
            if (row.Table.Columns.Contains("app_name") && !String.IsNullOrEmpty(row["app_name"].ToString())) {
                List<App> apps = new List<App>();
                App a = new App();
                a.App_Name = row["app_name"].ToString();
                a.App_Slug = row["app_slug"].ToString();
                apps.Add(a);
                comp.Apps = apps;
            }

            // address details
            if (row.Table.Columns.Contains("street1")) {
                if (!String.IsNullOrEmpty(row["address_id"].ToString())) {
                    address.ID = Convert.ToInt32(row["address_id"].ToString());
                }
                if (!String.IsNullOrEmpty(row["street1"].ToString())) {
                    address.Address1 = row["street1"].ToString();
                }
                if (!String.IsNullOrEmpty(row["street2"].ToString())) {
                    address.Address2 = row["street2"].ToString();
                }
                if (!String.IsNullOrEmpty(row["city"].ToString())) {
                    address.City = row["city"].ToString();
                }
                if (!String.IsNullOrEmpty(row["state"].ToString())) {
                    address.State = row["state"].ToString();
                }
                if (!String.IsNullOrEmpty(row["zip"].ToString())) {
                    address.Zip = Convert.ToInt32(row["zip"].ToString());
                }
                if (!String.IsNullOrEmpty(row["zip_ext"].ToString())) {
                    address.ZipExt = Convert.ToInt32(row["zip_ext"].ToString());
                }
                if (!String.IsNullOrEmpty(row["country"].ToString())) {
                    address.Country = row["country"].ToString();
                }
                if (row.Table.Columns.Contains("is_primary") && !String.IsNullOrEmpty(row["is_primary"].ToString())) {
                    address.IsPrimary = Convert.ToBoolean(row["is_primary"].ToString());
                }
                if (row.Table.Columns.Contains("is_billing") && !String.IsNullOrEmpty(row["is_billing"].ToString())) {
                    address.IsBilling = Convert.ToBoolean(row["is_billing"].ToString());
                }
                if (row.Table.Columns.Contains("is_physical") && !String.IsNullOrEmpty(row["is_physical"].ToString())) {
                    address.IsPhysical = Convert.ToBoolean(row["is_physical"].ToString());
                }
                if (row.Table.Columns.Contains("branch_id") && !String.IsNullOrEmpty(row["branch_id"].ToString())) {
                    address.BranchId = row["branch_id"].ToString();
                }
            }

            comp.Address = address;

            // add in contact
            comp.ContactPerson = BuildPerson(row);

            return comp;
        }


        public static List<Company> BuildCompanyList(DataTable table, bool includeEBS = true) {
            // go through each row and separate them by email address
            // we'll get more than one row per email in many cases, because of the apps they're assigned to
            // once a Company obj has been built for each collection, combine them back into one list and return
            List<string> eins = new List<string>();
            List<Company> uniqueComps = new List<Company>();
            List<Company> duplicateComps = new List<Company>();
            List<Company> finalComps = new List<Company>();

            if (table != null && table.Rows != null && table.Rows.Count > 0) {
                foreach (DataRow row in table.Rows) {
                    if (!String.IsNullOrEmpty(row["ein"].ToString())) {
                        if (eins.Contains(row["ein"].ToString())) {
                            // duplicate
                            duplicateComps.Add(BuildCompany(row));
                        } else {
                            // unique
                            uniqueComps.Add(BuildCompany(row));
                            // capture the email
                            eins.Add(row["ein"].ToString());
                        }
                    } // end if 
                } // end foreach

                // combine the separate lists (which means we'll consolidate the apps)
                foreach (Company c in uniqueComps) {
                    if (!c.Name.Contains("Employer Benefits Services") || includeEBS) { // only include EBS if requested
                        foreach (Company dc in duplicateComps) {
                            if (c.EIN == dc.EIN) {
                                c.Apps.AddRange(dc.Apps);
                                Company tempc = hasAccess(c);
                                c.HasAcaAccess = tempc.HasAcaAccess;
                                c.HasBenEnrollAccess = tempc.HasBenEnrollAccess;
                            }
                        } // end foreach duplicate company
                        finalComps.Add(c);
                    }
                }

            } // end if table exists, etc

            return finalComps;
        }


        // build user from data
        // this function is not called directly, except by the BuildUserList function
        private static User buildUserObj(DataRow row) {
            User user = new User();
            Person person = new Person();

            // person data
            if (!String.IsNullOrEmpty(row["first_name"].ToString())) {
                person.FirstName = row["first_name"].ToString();
            }
            if (row.Table.Columns.Contains("middle_name") && !String.IsNullOrEmpty(row["middle_name"].ToString())) {
                person.MiddleName = row["middle_name"].ToString();
            }
            if (!String.IsNullOrEmpty(row["last_name"].ToString())) {
                person.LastName = row["last_name"].ToString();
            }
            user.Person = person;

            // basic user data
            if (!String.IsNullOrEmpty(row["user_id"].ToString())) {
                user.ID = Convert.ToInt32(row["user_id"].ToString());
            }
            if (!String.IsNullOrEmpty(row["email"].ToString())) {
                user.Email = row["email"].ToString();
            }
            if (!String.IsNullOrEmpty(row["password"].ToString())) {
                user.Password = row["password"].ToString();
            }
            if (!String.IsNullOrEmpty(row["role_id"].ToString()) && (row["role_id"].ToString() == "4" || row["role_id"].ToString() == "2")) {
                user.IsAdmin = true;
            } else {
                user.IsAdmin = false;
            }
            if (!String.IsNullOrEmpty(row["role_id"].ToString()) && (row["role_id"].ToString() == "1" || row["role_id"].ToString() == "2")) {
                user.IsClient = true;
            } else {
                user.IsClient = false;
            }
            
            if (row.Table.Columns.Contains("role") && !String.IsNullOrEmpty(row["role"].ToString())) {
                user.Role = DataManager.CamelCase(row["role"].ToString());
            }
            if (row.Table.Columns.Contains("is_active") && !String.IsNullOrEmpty(row["is_active"].ToString())) {
                user.IsActive = Convert.ToBoolean(row["is_active"].ToString());
            }

            // company data
            Company comp = BuildCompany(row);
            user.Company = comp;

            // application data
            if (row.Table.Columns.Contains("app_name") && !String.IsNullOrEmpty(row["app_name"].ToString())) {
                App app = new App();
                app.App_Name = row["app_name"].ToString();
                app.App_Slug = row["app_slug"].ToString();
                List<App> apps = new List<App>();
                apps.Add(app);
                user.Apps = apps;
            }
            if (row.Table.Columns.Contains("app_slug") && !String.IsNullOrEmpty(row["app_slug"].ToString()) && row["app_slug"].ToString() == "acafiling") {
                user.HasAppAccess = true;
            } 
            if (row.Table.Columns.Contains("app_slug") && !String.IsNullOrEmpty(row["app_slug"].ToString()) && row["app_slug"].ToString() == "benefits") {
                user.HasBenEnrollAccess = true;
            } 

            return user;
        }

        // build list of users
        // most important aspect of this function is that it consolidates all the apps a user has access to
        // this should be used in all "Get User" functionality, even if it's for a single User
        public static List<User> BuildUserList(DataTable table) {
            // go through each row and separate them by email address
            // we'll get more than one row per email in many cases, because of the apps they're assigned to
            // once a User obj has been built for each collection, combine them back into one list and return
            List<string> emails = new List<string>();
            List<User> uniqueUsers = new List<User>();
            List<User> duplicateUsers = new List<User>();

            if (table != null && table.Rows != null && table.Rows.Count > 0) {
                foreach (DataRow row in table.Rows) {
                    if (!String.IsNullOrEmpty(row["email"].ToString())) {
                        if (emails.Contains(row["email"].ToString())) {
                            // duplicate
                            duplicateUsers.Add(buildUserObj(row));
                        } else {
                            // unique
                            uniqueUsers.Add(buildUserObj(row));
                            // capture the email
                            emails.Add(row["email"].ToString());
                        }
                    } // end if 
                } // end foreach

                // combine the separate lists (which means we'll consolidate the apps)
                foreach (User u in uniqueUsers) {
                    foreach (User du in duplicateUsers) {
                        if (u.Email == du.Email) {
                            u.Apps.AddRange(du.Apps);
                            User tempu = hasAccess(u);
                            u.HasAppAccess = tempu.HasAppAccess;
                            u.HasBenEnrollAccess = tempu.HasBenEnrollAccess;
                        }
                    }
                }

            } // end if table exists, etc

            return uniqueUsers;
        }

        private static User hasAccess(User u) {
            if (u != null) {
                foreach (App a in u.Apps) {
                    if (a.App_Slug == "acafiling") {
                        u.HasAppAccess = true;
                    } else if (a.App_Slug == "benefits") {
                        u.HasBenEnrollAccess = true;
                    }
                }
            }
            return u;
        }

        private static Company hasAccess(Company c) {
            if (c != null) {
                if (c.Apps != null && c.Apps.Count > 0) {
                    foreach (App a in c.Apps) {
                        if (a.App_Slug == "acafiling") {
                            c.HasAcaAccess = true;
                        } else if (a.App_Slug == "benefits") {
                            c.HasBenEnrollAccess = true;
                        }
                    }
                }
            }
            return c;
        }


        // build client list
        private static List<Company> buildClientList(DataTable table) {
            List<Company> clients = new List<Company>();

            if (table.Rows.Count > 0) {
                foreach (DataRow row in table.Rows) {
                    Company comp = new Company();
                    comp.ID = Convert.ToInt32(row["id"].ToString());
                    comp.Name = row["name1"].ToString();
                    comp.Name2 = row["name2"].ToString();
                    comp.EIN = row["ein"].ToString();
                    clients.Add(comp);
                }
            }

            return clients;
        }

        // build app list
        private static List<App> buildAppList(DataTable table) {
            List<App> apps = new List<App>();

            if (table.Rows.Count > 0) {
                foreach (DataRow row in table.Rows) {
                    App a = new App();
                    a.App_Name = row["app_name"].ToString();
                    a.App_Slug = row["app_slug"].ToString();
                    apps.Add(a);
                }
            }

            return apps;
        }


        // build activity
        private static Activity buildActivityObj(DataRow row, string activity) {
            Activity a = new Activity();

            User user = new User();
            User impersonatingAs = new User();

            user.ID = Convert.ToInt32(row["u_id"].ToString());
            Person up = new Person();
            up.FirstName = row["u_first_name"].ToString();
            up.LastName = row["u_last_name"].ToString();
            user.Person = up;
            user.Role = row["u_role"].ToString();
            user.Email = row["u_email"].ToString();
            Company comp = new Company();
            comp.Name = row["u_comp_name1"].ToString();
            if (!String.IsNullOrEmpty(row["u_comp_display_name"].ToString())) {
                comp.DisplayName = row["u_comp_display_name"].ToString();
            }
            user.Company = comp;

            if (!String.IsNullOrEmpty(row["imp_id"].ToString())) {
                user.IsImpersonating = true;
                impersonatingAs.ID = Convert.ToInt32(row["imp_id"].ToString());
                Person imp = new Person();
                imp.FirstName = row["imp_first_name"].ToString();
                imp.LastName = row["imp_last_name"].ToString();
                impersonatingAs.Person = imp;
                impersonatingAs.Role = row["imp_role"].ToString();
                Company impComp = new Company();
                impComp.Name = row["imp_comp_name1"].ToString();
                if (!String.IsNullOrEmpty(row["imp_comp_display_name"].ToString())) {
                    comp.DisplayName = row["imp_comp_display_name"].ToString();
                }
            } else {
                user.IsImpersonating = false;
            }

            a.User = user;
            a.ImpersonatingAsUser = impersonatingAs;
            a.ActivitySlug = row["activity_slug"].ToString();
            a.ActivityDesc = row["activity_desc"].ToString();
            a.Notes = row["notes"].ToString();
            a.ObjId = row["object_id"].ToString();
            a.ObjType = row["object_type"].ToString();
            a.Timestamp = Convert.ToDateTime(row["timestamp"].ToString());

            a.FullText = buildActivityFullText(a, activity);

            return a;
        }


        private static string buildActivityFullText(Activity a, string activity) {
            string msg = "";
            if (activity == "all") {
                if (a.ActivitySlug.Contains("error") || a.ActivitySlug.Contains("log") || a.ActivitySlug.Contains("report")) {
                    msg = a.ActivityDesc;
                } else if (a.ActivitySlug.Contains("submission")) {
                    msg = a.ActivityDesc + ": Receipt ID " + a.ObjId;
                } else if (a.ActivitySlug.Contains("status_update")) {
                    msg = a.ActivityDesc + ": Status Update ID " + a.ObjId;
                } else if (a.ActivitySlug.Contains("user")) {
                    msg = a.ActivityDesc + ": User ID " + a.ObjId;
                } else if (a.ActivitySlug.Contains("company")) {
                    msg = a.ActivityDesc + ": Company ID " + a.ObjId;
                }
            } else if (activity == "login-history") {
                msg = a.ActivityDesc;
            } else if (activity == "user-edits") {
                msg = a.ActivityDesc + ": User ID " + a.ObjId;
            } else if (activity == "company-edits") {
                msg = a.ActivityDesc + ": Company ID " + a.ObjId;
            } else if (activity == "reports-run") {
                msg = a.ActivityDesc;
            } else if (activity == "submission-attempts") {
                msg = a.ActivityDesc + ": Receipt ID " + a.ObjId;
            } else if (activity == "submissions-archived") {
                msg = a.ActivityDesc + ": Receipt ID " + a.ObjId;
            } else if (activity == "update-attempts") {
                msg = a.ActivityDesc + ": Status Update ID " + a.ObjId;
            } else if (activity == "errors") {
                msg = a.ActivityDesc;
            }

            return msg;
        }


        private static List<VaultItem> sortVaultItems(List<VaultItem> items, User currUser) {
            List<VaultItem> sorted = new List<VaultItem>();
            // sort child records into a separate list for now
            List<VaultItem> children = new List<VaultItem>();
            if (items != null && items.Count > 0) {
                foreach (VaultItem i in items) {
                    if (!String.IsNullOrEmpty(i.ParentVaultItemID.ToString()) && i.ParentVaultItemID > 0) {
                        children.Add(i);
                    }
                } // end foreach
            } // end if

            // now we need to add children back in.
            // keep a list of item IDs to help us track which records are being assigned. If we run across
            // an item and their list is in the ID collection, then we know that we've already put it back
            // in the sorted list.
            List<int> usedIds = new List<int>();

            if (items != null && items.Count > 0) {
                foreach (VaultItem vi in items) {
                    // make sure this isn't a child and we haven't already come across it


                    if ((String.IsNullOrEmpty(vi.ParentVaultItemID.ToString()) || vi.ParentVaultItemID == 0) && !usedIds.Contains(vi.ID)) {

                        sorted.Add(vi);
                        usedIds.Add(vi.ID);

                        // see if any children belong to this item
                        if (children != null && children.Count > 0) {
                            addVaultChildren(children, vi.ID, ref sorted);
                        } // end if children

                    } // end if
                } // end foreach items
            } // end if

            return sorted;
        }

        private static void addVaultChildren(List<VaultItem> children, int parentId, ref List<VaultItem> sorted) {
            foreach (VaultItem i in children) {
                if (i.ParentVaultItemID == parentId) {
                    sorted.Add(i);
                    addVaultChildren(children, i.ID, ref sorted);
                }
            }
        }


        // build list of vault items
        private static List<VaultItem> buildVaultItemList(DataTable table) {
            List<VaultItem> items = new List<VaultItem>();
            if (table != null && table.Rows != null && table.Rows.Count > 0) {
                foreach (DataRow row in table.Rows) {
                    var i = buildVaultItem(row);
                    items.Add(i);
                }
            }
            return items;
        }

        // build single vault item
        private static VaultItem buildVaultItem(DataRow row) {
            VaultItem item = new VaultItem();

            if (!String.IsNullOrEmpty(row["id"].ToString())) {
                item.ID = Convert.ToInt32(row["id"].ToString());
            }
            if (!String.IsNullOrEmpty(row["name"].ToString())) {
                item.Name = row["name"].ToString();
            }
            if (!String.IsNullOrEmpty(row["is_file"].ToString())) {
                item.IsFile = Convert.ToBoolean(row["is_file"].ToString());
            }
            if (!String.IsNullOrEmpty(row["is_admin_level"].ToString())) {
                item.IsAdminLevel = Convert.ToBoolean(row["is_admin_level"].ToString());
            }
            if (!String.IsNullOrEmpty(row["vault_url"].ToString())) {
                item.VaultURL = row["vault_url"].ToString();
            }
            if (!String.IsNullOrEmpty(row["comp_id"].ToString())) {
                item.CompID = Convert.ToInt32(row["comp_id"].ToString());
            }
            if (!String.IsNullOrEmpty(row["client_id"].ToString())) {
                item.ClientID = Convert.ToInt32(row["client_id"].ToString());
            }
            if (!String.IsNullOrEmpty(row["parent_vault_item_id"].ToString())) {
                item.ParentVaultItemID = Convert.ToInt32(row["parent_vault_item_id"].ToString());
            }
            if (!String.IsNullOrEmpty(row["root_parent_id"].ToString())) {
                item.RootParentVaultItemID = Convert.ToInt32(row["root_parent_id"].ToString());
            }
            if (!String.IsNullOrEmpty(row["file_ext"].ToString())) {
                item.FileExt = row["file_ext"].ToString();
            }
            if (!String.IsNullOrEmpty(row["is_root"].ToString())) {
                item.IsRootLvl = Convert.ToBoolean(row["is_root"].ToString());
            }
            if (!String.IsNullOrEmpty(row["is_sys_generated"].ToString())) {
                item.IsSysGenerated = Convert.ToBoolean(row["is_sys_generated"].ToString());
            }

            return item;
        }


        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // HELPER FUNCTIONS
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


    } // end class
} // end namespace