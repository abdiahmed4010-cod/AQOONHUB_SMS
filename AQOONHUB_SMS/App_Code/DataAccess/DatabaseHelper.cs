using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace AQOONHUB_SMS.App_Code.DataAccess
{
    public class DatabaseHelper
    {
        private string connectionString;

        public DatabaseHelper()
        {
            connectionString = ConfigurationManager.ConnectionStrings["AQOONHUB_DB"].ConnectionString;
        }

        public DataTable ExecuteQuery(string query, SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        public DataTable ExecuteQuery(string query)
        {
            return ExecuteQuery(query, null);
        }

        public int ExecuteNonQuery(string query, SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public int ExecuteNonQuery(string query)
        {
            return ExecuteNonQuery(query, null);
        }

        public object ExecuteScalar(string query, SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    conn.Open();
                    return cmd.ExecuteScalar();
                }
            }
        }

        public object ExecuteScalar(string query)
        {
            return ExecuteScalar(query, null);
        }

        public DataTable ExecuteStoredProcedure(string spName, SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(spName, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        return dt;
                    }
                }
            }
        }
    }
}