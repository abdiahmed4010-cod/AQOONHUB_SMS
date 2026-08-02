using System;
using System.Data;
using System.Text;
using System.Web;

namespace AQOONHUB_SMS.Modules.Reports
{
    /// <summary>Central CSV export for the Reports module: builds injection-safe UTF-8 CSV from the exact
    /// on-screen DataTable, records export metadata (never the file bytes) and writes an audit row.</summary>
    public sealed class ReportExportService
    {
        private readonly ReportsRepository _repo;
        public ReportExportService(ReportsRepository repo) { _repo = repo; }

        /// <summary>Serialise a DataTable to CSV text (headers + rows), escaping + neutralising injection.</summary>
        public static string BuildCsv(DataTable data)
        {
            var b = new StringBuilder();
            if (data == null) return "";
            for (int i = 0; i < data.Columns.Count; i++)
            {
                b.Append(ReportUi.Csv(data.Columns[i].ColumnName));
                if (i < data.Columns.Count - 1) b.Append(',');
            }
            b.AppendLine();
            foreach (DataRow r in data.Rows)
            {
                for (int i = 0; i < data.Columns.Count; i++)
                {
                    object v = r[i];
                    string s = v == null || v == DBNull.Value ? "" : Convert.ToString(v);
                    b.Append(ReportUi.Csv(s));
                    if (i < data.Columns.Count - 1) b.Append(',');
                }
                b.AppendLine();
            }
            return b.ToString();
        }

        /// <summary>Write the CSV to the response, record export metadata + audit, and end the response.</summary>
        public void Export(HttpResponse response, ReportDefinition def, DataTable data, int? userId, string filterSummary, string ip)
        {
            string content = BuildCsv(data);
            string fileName = def.ExportName + "-" + DateTime.Now.ToString("yyyyMMdd") + ".csv";
            long size = Encoding.UTF8.GetByteCount(content) + 3; // +BOM

            _repo.RecordExport(def.Key, def.Title, def.Category, "CSV", filterSummary, fileName, size, userId);
            _repo.LogAudit(userId, "Exported", def.Key, def.Title, def.Category, filterSummary, "Success", ip);

            ReportUi.WriteCsv(response, fileName, content);
        }
    }
}
