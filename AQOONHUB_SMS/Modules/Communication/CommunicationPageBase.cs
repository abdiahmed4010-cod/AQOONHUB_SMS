using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

namespace AQOONHUB_SMS.Modules.Communication
{
    public abstract class CommunicationPageBase : Page
    {
        protected readonly CommunicationRepository Repo=new CommunicationRepository();
        protected int UserId { get { int v; return int.TryParse(Convert.ToString(Session["UserID"]),out v)?v:0; } }
        protected string Role { get { return Convert.ToString(Session["Role"]); } }
        protected string Ip { get { return Request.UserHostAddress ?? ""; } }
        protected override void OnInit(EventArgs e){base.OnInit(e);CommunicationAuthorization.Demand(this);}
        protected DateTime? DateValue(string value){DateTime d;return DateTime.TryParse(value,out d)?d:(DateTime?)null;}
        protected int PageSize(string value){int n;return int.TryParse(value,out n)&&Array.IndexOf(new[]{10,25,50,100},n)>=0?n:10;}
        protected static string H(object value){return HttpUtility.HtmlEncode(Convert.ToString(value));}
        protected static string Csv(object value){string s=Convert.ToString(value);if(s.Length>0&&"=+-@\t\r".IndexOf(s[0])>=0)s="'"+s;return "\""+s.Replace("\"","\"\"")+"\"";}
        protected static void ValidateText(string title,string body){if(string.IsNullOrWhiteSpace(title)||title.Trim().Length>200)throw new InvalidOperationException("Title is required and must be at most 200 characters.");if(string.IsNullOrWhiteSpace(body)||body.Trim().Length>4000)throw new InvalidOperationException("Message is required and must be at most 4,000 characters.");}
        protected static void ValidatePlaceholders(string text){foreach(Match m in Regex.Matches(text??"",@"\{\{([A-Za-z]+)\}\}")){string[] allowed={"StudentName","ParentName","ClassName","SectionName","AmountDue","DueDate","ExamDate","SchoolName"};if(Array.IndexOf(allowed,m.Groups[1].Value)<0)throw new InvalidOperationException("Unknown template placeholder: "+m.Value);}}
    }
}
