using System;
using System.Globalization;
using System.Web;

namespace AQOONHUB_SMS.Modules.Attendance
{
    /// <summary>Shared presentation + CSV helpers for the Attendance report pages (no data access).</summary>
    public static class AttendanceUi
    {
        public static string StatusStyle(string s)
        {
            switch ((s ?? "").ToLowerInvariant())
            {
                case "present": return "background:#DCFCE7;color:#15803D";
                case "absent": return "background:#FEE2E2;color:#DC2626";
                case "late": return "background:#FEF3C7;color:#B45309";
                case "excused": return "background:#EDE9FE;color:#6D28D9";
                default: return "background:#F1F5F9;color:#64748B";
            }
        }

        public static string SessionStyle(string s)
        {
            switch ((s ?? "").ToLowerInvariant())
            {
                case "submitted": return "background:#DCFCE7;color:#15803D";
                case "draft": return "background:#FEF3C7;color:#B45309";
                case "locked": return "background:#E0E7FF;color:#3730A3";
                case "cancelled": return "background:#FEE2E2;color:#DC2626";
                default: return "background:#F1F5F9;color:#64748B";
            }
        }

        public static string RiskStyle(string s)
        {
            switch ((s ?? "").ToLowerInvariant())
            {
                case "good": return "background:#DCFCE7;color:#15803D";
                case "watch": return "background:#FEF3C7;color:#B45309";
                case "at risk": return "background:#FEE2E2;color:#DC2626";
                default: return "background:#F1F5F9;color:#64748B";
            }
        }

        public static string FormatTime(object t)
        {
            if (t == null || t == DBNull.Value) return "—";
            TimeSpan ts = (TimeSpan)t;
            return new DateTime(ts.Ticks).ToString("hh:mm tt", CultureInfo.InvariantCulture);
        }

        public static string FormatTimeCsv(object t)
        {
            if (t == null || t == DBNull.Value) return "";
            TimeSpan ts = (TimeSpan)t;
            return new DateTime(ts.Ticks).ToString("HH:mm", CultureInfo.InvariantCulture);
        }

        /// <summary>CSV cell: quote always, escape quotes, and neutralise CSV-injection leaders.</summary>
        public static string Csv(string value)
        {
            string s = value ?? string.Empty;
            if (s.Length > 0 && "=+-@\t\r".IndexOf(s[0]) >= 0) s = "'" + s;
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }

        /// <summary>Sanitise a filename fragment to a safe slug.</summary>
        public static string Slug(string value)
        {
            if (string.IsNullOrEmpty(value)) return "export";
            var sb = new System.Text.StringBuilder();
            foreach (char c in value)
                sb.Append(char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : '-');
            string r = sb.ToString();
            while (r.Contains("--")) r = r.Replace("--", "-");
            return r.Trim('-');
        }

        /// <summary>RFC-4180-style CSV parser: handles quoted fields, commas/newlines inside quotes,
        /// escaped double-quotes (""), UTF-8 BOM, and both CRLF and LF line endings. Returns non-empty rows.</summary>
        public static System.Collections.Generic.List<string[]> ParseCsv(string content)
        {
            var rows = new System.Collections.Generic.List<string[]>();
            if (content == null) return rows;
            if (content.Length > 0 && content[0] == '﻿') content = content.Substring(1); // strip BOM

            var field = new System.Text.StringBuilder();
            var current = new System.Collections.Generic.List<string>();
            bool inQuotes = false;
            int i = 0, n = content.Length;
            while (i < n)
            {
                char c = content[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < n && content[i + 1] == '"') { field.Append('"'); i += 2; continue; }
                        inQuotes = false; i++; continue;
                    }
                    field.Append(c); i++; continue;
                }
                if (c == '"') { inQuotes = true; i++; continue; }
                if (c == ',') { current.Add(field.ToString()); field.Clear(); i++; continue; }
                if (c == '\r') { i++; continue; }
                if (c == '\n')
                {
                    current.Add(field.ToString()); field.Clear();
                    if (!IsBlankRow(current)) rows.Add(current.ToArray());
                    current = new System.Collections.Generic.List<string>(); i++; continue;
                }
                field.Append(c); i++;
            }
            // last field/row
            current.Add(field.ToString());
            if (!IsBlankRow(current)) rows.Add(current.ToArray());
            return rows;
        }

        private static bool IsBlankRow(System.Collections.Generic.List<string> cells)
        {
            foreach (string c in cells) if (!string.IsNullOrWhiteSpace(c)) return false;
            return true;
        }

        public static void WriteCsv(HttpResponse response, string fileName, string content)
        {
            response.Clear();
            response.ContentType = "text/csv";
            response.ContentEncoding = System.Text.Encoding.UTF8;
            response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            response.Write("﻿"); // UTF-8 BOM so Excel opens correctly
            response.Write(content);
            response.End();
        }
    }
}
