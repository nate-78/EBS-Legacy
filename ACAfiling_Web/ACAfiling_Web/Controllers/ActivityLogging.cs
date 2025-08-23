using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using EbsClassPkg.Controllers;

namespace ACAfiling_Web.Controllers {
    public class ActivityLogging {
        // this class handles the functionality for logging various events within the application
        // the log tables are held within the user_management db

        // the activity_log table must be passed the following parameters:
        //      user_id, 
        //      activity_slug (identifier for the activity): 
        //      notes
        //      object_id (the id of whatever object is also being affected: submission, company, user, etc)
        //      object_type (the type of id that is being supplied)
        public static void LogActivity(int userId, string activitySlug, string notes, int? objId, string objType) {
            // get connection to user_management db
            SqlDataReader reader;
            using (SqlConnection conn = DataManager.BuildUserMgmtConnection()) {
                conn.Open();
                // call LogActivity stored proc
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "p_LogActivity";
                cmd.Connection = conn;

                // add params
                cmd.Parameters.Add("@user_id", SqlDbType.Int).Value = String.IsNullOrEmpty(userId.ToString()) ? 
                    DBNull.Value : (object)userId;
                cmd.Parameters.Add("@activity_slug", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(activitySlug) ? 
                    DBNull.Value : (object)activitySlug;
                cmd.Parameters.Add("@notes", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(notes) ? DBNull.Value : (object)notes;
                cmd.Parameters.Add("@object_id", SqlDbType.Int).Value = String.IsNullOrEmpty(objId.ToString()) ? DBNull.Value : (object)objId;
                cmd.Parameters.Add("@object_type", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(objType) ? DBNull.Value : (object)objType;
                // execute
                reader = cmd.ExecuteReader();
            } // end using statement
             
        } // end LogActivity()

    } // end class
} // end namespace