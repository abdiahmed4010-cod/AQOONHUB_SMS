using System;
using System.Data;
using System.IO;
using System.Web;

namespace AQOONHUB_SMS.Modules.Communication
{
    public partial class AttachmentDownload : CommunicationPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            long id;
            if (!long.TryParse(Request.QueryString["id"], out id) || id <= 0) { Unavailable(400); return; }
            DataRow row=Repo.AuthorizedAttachment(id,UserId,Role);
            if(row==null){Unavailable(403);return;}
            string relative=Convert.ToString(row["RelativePath"]);
            string stored=Convert.ToString(row["StoredFileName"]);
            string original=Path.GetFileName(Convert.ToString(row["OriginalFileName"]));
            string ext=Path.GetExtension(stored).ToLowerInvariant();
            string[] allowed={".pdf",".docx",".xlsx",".jpg",".jpeg",".png"};
            if(Array.IndexOf(allowed,ext)<0||Path.GetFileName(stored)!=stored||!relative.StartsWith("~/Uploads/Communication/",StringComparison.OrdinalIgnoreCase)){Unavailable(404);return;}
            string root=Path.GetFullPath(Server.MapPath("~/Uploads/Communication/"));
            string physical=Path.GetFullPath(Server.MapPath(relative));
            if(!physical.StartsWith(root,StringComparison.OrdinalIgnoreCase)||!File.Exists(physical)){Unavailable(404);return;}
            string mime=SafeMime(ext);
            Repo.Log(UserId,"ATTACHMENT_DOWNLOADED","Communication attachment "+id+" downloaded",Ip);
            Response.Clear(); Response.BufferOutput=false; Response.ContentType=mime;
            Response.AddHeader("X-Content-Type-Options","nosniff");
            Response.AddHeader("Content-Disposition","attachment; filename*=UTF-8''"+HttpUtility.UrlEncode(string.IsNullOrWhiteSpace(original)?("attachment"+ext):original).Replace("+","%20"));
            Response.TransmitFile(physical); Response.End();
        }
        static string SafeMime(string ext){switch(ext){case ".pdf":return "application/pdf";case ".docx":return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";case ".xlsx":return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";case ".jpg":case ".jpeg":return "image/jpeg";default:return "image/png";}}
        void Unavailable(int status){Response.StatusCode=status;Response.TrySkipIisCustomErrors=true;Response.ContentType="text/plain";Response.Write("The requested attachment is unavailable.");Context.ApplicationInstance.CompleteRequest();}
    }
}
