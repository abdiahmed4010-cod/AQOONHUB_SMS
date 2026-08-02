using System;
using System.Data;
using System.Data.SqlClient;

namespace AQOONHUB_SMS.Modules.Reports
{
    public sealed partial class ReportsRepository
    {
        public DataTable GetSavedReports(int userId,string role){bool admin=ReportAuthorization.IsStage5Admin(role);return ExecuteDataTable(@"SELECT s.SavedReportID,s.ReportName,s.ReportKey DataSource,s.Category,COALESCE(u.FullName,'System') Owner,s.CreatedAt,s.UpdatedAt,s.LastRunAt,s.Visibility,CASE WHEN s.IsActive=1 THEN 'Active' ELSE 'Inactive' END Status FROM SavedReports s LEFT JOIN Users u ON u.UserID=s.OwnerUserID WHERE (@admin=1 OR s.OwnerUserID=@uid OR (s.Visibility='Role-Based' AND s.Category IN ('Student','Academic','Examination','Attendance','Enrollment','Guardian','Finance','Payroll','TeacherStaff','Performance'))) ORDER BY s.CreatedAt DESC",new[]{P("@admin",admin),P("@uid",userId)});}
        public DataRow GetSavedReport(int id){var t=ExecuteDataTable("SELECT TOP 1 * FROM SavedReports WHERE SavedReportID=@id",new[]{P("@id",id)});return t.Rows.Count==0?null:t.Rows[0];}
        public int CreateSavedReport(string name,string source,string category,string json,string visibility,int owner){object o=ExecuteScalar(@"INSERT SavedReports(ReportName,ReportKey,Category,ConfigurationJson,Visibility,OwnerUserID,IsActive,CreatedAt) VALUES(@n,@k,@c,@j,@v,@u,1,GETDATE());SELECT SCOPE_IDENTITY()",new[]{P("@n",name),P("@k",source),P("@c",category),P("@j",json),P("@v",visibility),P("@u",owner)});return Convert.ToInt32(o);}
        public bool UpdateSavedReport(int id,string name,string json,string visibility,int userId,string role){return ExecuteOwned(@"UPDATE SavedReports SET ReportName=@n,ConfigurationJson=@j,Visibility=@v,UpdatedAt=GETDATE() WHERE SavedReportID=@id",id,userId,role,new[]{P("@n",name),P("@j",json),P("@v",visibility)});}
        public bool SetSavedReportActive(int id,bool active,int userId,string role){return ExecuteOwned("UPDATE SavedReports SET IsActive=@a,UpdatedAt=GETDATE() WHERE SavedReportID=@id",id,userId,role,new[]{P("@a",active)});}
        public bool DeleteSavedReport(int id,int userId,string role){return ExecuteOwned("DELETE SavedReports WHERE SavedReportID=@id",id,userId,role,null);}
        public void MarkSavedRun(int id){ExecuteNonQuery("UPDATE SavedReports SET LastRunAt=GETDATE() WHERE SavedReportID=@id",new[]{P("@id",id)});}
        private bool ExecuteOwned(string sql,int id,int userId,string role,SqlParameter[] extra){var row=GetSavedReport(id);if(row==null)return false;bool owner=Convert.ToInt32(row["OwnerUserID"])==userId;if(!owner&&!ReportAuthorization.IsStage5Admin(role))return false;var ps=new System.Collections.Generic.List<SqlParameter>{P("@id",id)};if(extra!=null)ps.AddRange(extra);ExecuteNonQuery(sql,ps.ToArray());return true;}
    }
}
