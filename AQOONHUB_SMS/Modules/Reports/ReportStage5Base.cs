using System;
namespace AQOONHUB_SMS.Modules.Reports
{
 public abstract class ReportStage5Base:System.Web.UI.Page
 {
  protected readonly ReportsRepository Repo=new ReportsRepository(); protected string CurrentRole{get{return Convert.ToString(Session["Role"]);}} protected int CurrentUserId{get{int i;return int.TryParse(Convert.ToString(Session["UserID"]),out i)?i:0;}}
  protected bool Require(string feature){if(CurrentUserId==0){Response.Redirect("~/Modules/Authentication/Login.aspx",true);return false;}if(!ReportAuthorization.CanUseStage5(CurrentRole,feature)){Response.Redirect("~/Modules/Dashboard/Dashboard.aspx",true);return false;}return true;}
  protected string Ip(){return string.IsNullOrWhiteSpace(Request.UserHostAddress)?null:Request.UserHostAddress;}
  protected void Audit(string action,string key,string name,string category,string summary="Configuration details withheld",string status="Success"){Repo.LogAudit(CurrentUserId,action,key,name,category,summary,status,Ip());}
 }
}
