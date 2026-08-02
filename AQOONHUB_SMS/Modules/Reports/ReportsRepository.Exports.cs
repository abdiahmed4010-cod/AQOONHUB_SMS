using System;
using System.Data;
namespace AQOONHUB_SMS.Modules.Reports
{
 public sealed partial class ReportsRepository
 {
  public DataTable GetExportHistory(int uid,string role){return ExecuteDataTable(@"SELECT e.ReportExportID,e.ReportName,e.ReportKey,e.Category,e.ExportFormat,COALESCE(u.FullName,'System') GeneratedBy,e.GeneratedAt,e.FilterSummary,e.FileSize,e.ExpiresAt,CASE WHEN e.FilePath IS NULL OR e.FilePath='' THEN 'Metadata Only' WHEN e.ExpiresAt<GETDATE() THEN 'Expired' ELSE e.Status END FileStatus FROM ReportExports e LEFT JOIN Users u ON u.UserID=e.GeneratedBy WHERE (@admin=1 OR e.GeneratedBy=@uid) ORDER BY e.GeneratedAt DESC",new[]{P("@admin",ReportAuthorization.IsStage5Admin(role)),P("@uid",uid)});}
  public DataRow GetExportRecord(int id,int uid,string role){var d=ExecuteDataTable("SELECT TOP 1 * FROM ReportExports WHERE ReportExportID=@id AND (GeneratedBy=@u OR @admin=1)",new[]{P("@id",id),P("@u",uid),P("@admin",ReportAuthorization.IsStage5Admin(role))});return d.Rows.Count==0?null:d.Rows[0];}
  public bool DeleteExportMetadata(int id,int uid,string role){if(GetExportRecord(id,uid,role)==null)return false;ExecuteNonQuery("DELETE ReportExports WHERE ReportExportID=@id",new[]{P("@id",id)});return true;}
 }
}
