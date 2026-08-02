using System;
using System.Globalization;
using System.Web;

namespace AQOONHUB_SMS.Modules.Reports
{
    /// <summary>Shared presentation + CSV helpers for the Reports module (no data access).</summary>
    public static class ReportUi
    {
        /// <summary>CSV cell: quote always, escape quotes, neutralise CSV-injection leaders.</summary>
        public static string Csv(string value)
        {
            string s = value ?? string.Empty;
            if (s.Length > 0 && "=+-@\t\r".IndexOf(s[0]) >= 0) s = "'" + s;
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }

        public static string Slug(string value)
        {
            if (string.IsNullOrEmpty(value)) return "report";
            var sb = new System.Text.StringBuilder();
            foreach (char c in value) sb.Append(char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : '-');
            string r = sb.ToString();
            while (r.Contains("--")) r = r.Replace("--", "-");
            return r.Trim('-');
        }

        public static void WriteCsv(HttpResponse response, string fileName, string content)
        {
            response.Clear();
            response.ContentType = "text/csv";
            response.ContentEncoding = System.Text.Encoding.UTF8;
            response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            response.Write("﻿"); // UTF-8 BOM
            // Clear the text write above and emit the exact UTF-8 BOM bytes followed by UTF-8 content.
            response.Clear();
            response.ContentType = "text/csv";
            response.ContentEncoding = System.Text.Encoding.UTF8;
            response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());
            response.BinaryWrite(System.Text.Encoding.UTF8.GetBytes(content ?? string.Empty));
            response.End();
        }

        public static string StatusStyle(string s)
        {
            switch ((s ?? "").ToLowerInvariant())
            {
                case "active": case "success": case "generated": case "published": return "background:#DCFCE7;color:#15803D";
                case "pending": case "pending scheduler configuration": case "warning": return "background:#FEF3C7;color:#B45309";
                case "failed": case "error": case "inactive": return "background:#FEE2E2;color:#DC2626";
                default: return "background:#F1F5F9;color:#64748B";
            }
        }

        public static string Enc(object o) { return HttpUtility.HtmlEncode(o == null ? "" : o.ToString()); }

        /// <summary>Render report cards for a category: available+authorized reports link to the viewer;
        /// unavailable/unauthorized ones show a disabled card with an honest reason.</summary>
        public static string RenderReportCards(string category, string role, Func<string, string> resolveUrl)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var def in ReportCatalog.ForCategory(category))
            {
                bool authorized = ReportAuthorization.CanRunReport(role, def);
                bool clickable = def.Available && authorized;
                string title = Enc(def.Title);
                string desc = Enc(def.Description);
                if (clickable)
                {
                    string url = resolveUrl("~/Modules/Reports/ReportViewer.aspx?report=" + HttpUtility.UrlEncode(def.Key));
                    sb.Append("<a class='rc' href='").Append(HttpUtility.HtmlEncode(url)).Append("'>")
                      .Append("<div class='rc-t'>").Append(title);
                    if (def.Sensitive) sb.Append(" <span class='rc-lock' title='Restricted report'><i data-lucide='shield-check' class='inline w-3 h-3' aria-hidden='true'></i><span class='sr-only'>Restricted report</span></span>");
                    sb.Append("</div><div class='rc-d'>").Append(desc).Append("</div>")
                      .Append("<div class='rc-a'><span class='badge' style='background:#DCFCE7;color:#15803D'>Available</span> Open report &rarr;</div></a>");
                }
                else
                {
                    string reason = !def.Available ? "Data source unavailable" : "Restricted";
                    sb.Append("<div class='rc rc-off' title='").Append(HttpUtility.HtmlEncode(reason)).Append("'>")
                      .Append("<div class='rc-t'>").Append(title).Append("</div>")
                      .Append("<div class='rc-d'>").Append(desc).Append("</div>")
                      .Append("<div class='rc-a rc-off-a'><span class='badge' style='background:#FEF3C7;color:#92400E'>Unavailable</span> ").Append(Enc(reason)).Append("</div></div>");
                }
            }
            return sb.ToString();
        }

        public static string Money(object o)
        {
            if (o == null || o == DBNull.Value) return "0.00";
            return Convert.ToDecimal(o).ToString("#,##0.00", CultureInfo.InvariantCulture);
        }
    }
}
