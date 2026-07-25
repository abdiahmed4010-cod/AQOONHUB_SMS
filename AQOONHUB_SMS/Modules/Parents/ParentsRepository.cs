using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AQOONHUB_SMS.Modules.Parents
{
    /// <summary>
    /// Self-contained ADO.NET repository for the Parents module. Does not use,
    /// reference, or depend on DatabaseHelper.cs or any legacy DAL/BLL class —
    /// every Parents page also carries its own copies of the basic Execute*
    /// helpers so this file is optional convenience, not a hard dependency.
    /// </summary>
    public class ParentsRepository
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["AQOONHUB_DB"].ConnectionString;

        public DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteScalar();
            }
        }

        public int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Same normalized-role comparison used throughout the Students/Admission modules.</summary>
        public static string NormalizeRole(string role)
        {
            if (string.IsNullOrEmpty(role)) return string.Empty;
            return role.Trim().Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// Finds possible duplicate Guardians by phone or email — used to warn (not block)
        /// when adding/creating a Guardian that might already exist.
        /// </summary>
        public DataTable FindPossibleDuplicateGuardians(string phone, string email)
        {
            string query = @"
                SELECT GuardianID, FullName, Phone, Email
                FROM Guardians
                WHERE (Phone = @Phone) OR (@Email IS NOT NULL AND Email = @Email)";
            return ExecuteQuery(query, new[]
            {
                new SqlParameter("@Phone", (object)phone ?? DBNull.Value),
                new SqlParameter("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email)
            });
        }
    }
}
