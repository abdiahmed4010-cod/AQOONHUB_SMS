using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Attendance
{
    public partial class ImportAttendance : System.Web.UI.Page
    {
        private readonly AttendanceRepository _repo = new AttendanceRepository();

        private const int MaxBytes = 2 * 1024 * 1024;   // 2 MB
        private const int MaxRows = 5000;

        private string Role { get { return Convert.ToString(Session["Role"]); } }
        private int UserId { get { int v; return int.TryParse(Convert.ToString(Session["UserID"]), out v) ? v : 0; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack)
            {
                txtFixedDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
                BindYears(); BindTerms(); BindClasses(); BindSections(); BindSubjects();
                BindHistory();
            }
        }

        private bool Authorize()
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Modules/Authentication/Login.aspx", true); return false; }
            // Stage 4 policy: only management roles may bulk import. Enforced on load AND on every action.
            if (!_repo.UserCanImportAttendance(Role)) { Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true); return false; }
            return true;
        }

        private int YearV() { int v; return int.TryParse(ddlYear.SelectedValue, out v) ? v : 0; }
        private int TermV() { int v; return int.TryParse(ddlTerm.SelectedValue, out v) ? v : 0; }
        private int ClassV() { int v; return int.TryParse(ddlClass.SelectedValue, out v) ? v : 0; }
        private int SectionV() { int v; return int.TryParse(ddlSection.SelectedValue, out v) ? v : 0; }
        private int SubjectV() { int v; return int.TryParse(ddlSubject.SelectedValue, out v) ? v : 0; }
        private string SessionTypeV { get { return ddlSessionType.SelectedValue; } }
        private bool IsSubject { get { return string.Equals(SessionTypeV, "Subject", StringComparison.OrdinalIgnoreCase); } }
        private bool UseFixedDate { get { return ddlDateMode.SelectedValue == "fixed"; } }
        private DateTime FixedDateV() { DateTime d; return DateTime.TryParse(txtFixedDate.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out d) ? d.Date : DateTime.Today; }

        protected string VStyle(string v)
        {
            switch ((v ?? "").ToLowerInvariant())
            {
                case "valid": return "background:#DCFCE7;color:#15803D";
                case "warning": return "background:#FEF3C7;color:#B45309";
                case "error": return "background:#FEE2E2;color:#DC2626";
                default: return "background:#F1F5F9;color:#64748B";
            }
        }

        private AttendanceRepository.ImportOptions Options()
        {
            return new AttendanceRepository.ImportOptions
            {
                AcademicYearID = YearV(), TermID = TermV() > 0 ? TermV() : (int?)null,
                ClassID = ClassV(), SectionID = SectionV(),
                SubjectID = IsSubject && SubjectV() > 0 ? SubjectV() : (int?)null, SessionType = SessionTypeV,
                UseFixedDate = UseFixedDate, FixedDate = FixedDateV(),
                ImportAsSubmitted = ddlImportAs.SelectedValue == "submitted",
                UpdateExistingDraft = ddlMode.SelectedValue == "update"
            };
        }

        // ---------- binders ----------
        private void BindYears()
        {
            ddlYear.Items.Clear();
            foreach (DataRow r in _repo.GetAcademicYears().Rows) ddlYear.Items.Add(new ListItem(Convert.ToString(r["YearName"]), Convert.ToString(r["AcademicYearID"])));
            int a = _repo.GetActiveAcademicYearId(); if (a > 0 && ddlYear.Items.FindByValue(a.ToString()) != null) ddlYear.SelectedValue = a.ToString();
        }
        private void BindTerms() { ddlTerm.Items.Clear(); ddlTerm.Items.Add(new ListItem("— None —", "0")); foreach (DataRow r in _repo.GetTerms(YearV()).Rows) ddlTerm.Items.Add(new ListItem(Convert.ToString(r["TermName"]), Convert.ToString(r["TermID"]))); }
        private void BindClasses() { ddlClass.Items.Clear(); ddlClass.Items.Add(new ListItem("— Select class —", "0")); foreach (DataRow r in _repo.GetClasses(YearV()).Rows) ddlClass.Items.Add(new ListItem(Convert.ToString(r["ClassName"]), Convert.ToString(r["ClassID"]))); }
        private void BindSections() { ddlSection.Items.Clear(); ddlSection.Items.Add(new ListItem("— Select section —", "0")); if (ClassV() > 0) foreach (DataRow r in _repo.GetSectionsForClass(ClassV()).Rows) ddlSection.Items.Add(new ListItem(Convert.ToString(r["SectionName"]), Convert.ToString(r["SectionID"]))); }
        private void BindSubjects()
        {
            ddlSubject.Items.Clear(); ddlSubject.Items.Add(new ListItem("— Select subject —", "0")); ddlSubject.Enabled = IsSubject;
            if (IsSubject && ClassV() > 0) foreach (DataRow r in _repo.GetSubjectsForClass(ClassV(), YearV()).Rows) ddlSubject.Items.Add(new ListItem(Convert.ToString(r["SubjectName"]), Convert.ToString(r["SubjectID"])));
        }
        private void BindHistory() { gvHistory.DataSource = _repo.GetAttendanceImportHistory(10); gvHistory.DataBind(); }

        protected void ddlYear_Changed(object sender, EventArgs e) { BindTerms(); BindClasses(); BindSections(); BindSubjects(); HidePreview(); }
        protected void ddlClass_Changed(object sender, EventArgs e) { BindSections(); BindSubjects(); HidePreview(); }
        protected void ddlSessionType_Changed(object sender, EventArgs e) { BindSubjects(); HidePreview(); }
        protected void ddlDateMode_Changed(object sender, EventArgs e) { txtFixedDate.Enabled = UseFixedDate; HidePreview(); }

        protected void btnReset_Click(object sender, EventArgs e) { HidePreview(); Session.Remove("ImportCsv"); msg.Visible = false; }

        private void HidePreview() { pnlPreview.Visible = false; }

        // ---------- template ----------
        protected void btnTemplate_Click(object sender, EventArgs e)
        {
            string[] headers = { "AttendanceDate", "AcademicYear", "Class", "Section", "StudentCode", "Status", "CheckInTime", "Remarks", "SubjectCode", "SessionType" };
            StringBuilder b = new StringBuilder();
            for (int i = 0; i < headers.Length; i++) { b.Append(AttendanceUi.Csv(headers[i])); if (i < headers.Length - 1) b.Append(','); }
            b.AppendLine();
            // one clearly-labelled placeholder example row (no real attendance data, no formulas)
            string[] example = { "2026-05-30", "2025-2026", "Form 5", "Form 5A", "STUDENT-CODE", "Present", "07:05", "example only", "", "Daily" };
            for (int i = 0; i < example.Length; i++) { b.Append(AttendanceUi.Csv(example[i])); if (i < example.Length - 1) b.Append(','); }
            b.AppendLine();
            AttendanceUi.WriteCsv(Response, "attendance-import-template.csv", b.ToString());
        }

        // ---------- file upload + parse ----------
        private bool ReadUpload(out string content, out string hash, out string fileName, out string error)
        {
            content = null; hash = null; fileName = null; error = null;
            if (!fu.HasFile) { error = "Please choose a CSV file to upload."; return false; }
            fileName = System.IO.Path.GetFileName(fu.FileName);   // strip any path (no traversal)
            string ext = (System.IO.Path.GetExtension(fileName) ?? "").ToLowerInvariant();
            if (ext != ".csv") { error = "The uploaded file is not a valid CSV file. Only .csv is accepted."; return false; }
            byte[] bytes = fu.FileBytes;
            if (bytes == null || bytes.Length == 0) { error = "The uploaded file is empty."; return false; }
            if (bytes.Length > MaxBytes) { error = "The file exceeds the 2 MB limit."; return false; }

            // reject binary/executable content: a NUL byte indicates a non-text (renamed binary) file
            int scan = Math.Min(bytes.Length, 8192);
            for (int i = 0; i < scan; i++) if (bytes[i] == 0) { error = "The uploaded file is not a valid CSV file."; return false; }

            using (SHA256 sha = SHA256.Create())
            {
                byte[] h = sha.ComputeHash(bytes);
                StringBuilder sb = new StringBuilder(h.Length * 2);
                foreach (byte x in h) sb.Append(x.ToString("x2"));
                hash = sb.ToString();
            }
            content = new UTF8Encoding(false).GetString(bytes);
            return true;
        }

        protected void btnPreview_Click(object sender, EventArgs e)
        {
            if (!_repo.UserCanImportAttendance(Role)) { Show(false, "You are not authorized to import attendance."); return; }
            string content, hash, fileName, error;
            if (!ReadUpload(out content, out hash, out fileName, out error)) { HidePreview(); Show(false, error); return; }

            List<string[]> csv = AttendanceUi.ParseCsv(content);
            if (csv.Count == 0) { HidePreview(); Show(false, "The uploaded file is not a valid CSV file."); return; }
            if (csv.Count - 1 > MaxRows) { HidePreview(); Show(false, "The file exceeds the maximum of " + MaxRows + " data rows."); return; }

            AttendanceRepository.ImportPreview pv;
            try { pv = _repo.GetAttendanceImportPreview(csv, Options(), UserId, Role); }
            catch (Exception ex) { HidePreview(); Show(false, ex.Message); return; }

            if (!string.IsNullOrEmpty(pv.HeaderError)) { HidePreview(); Show(false, pv.HeaderError); return; }

            // Stash the validated content for the Import step (server-side; not trusted from client).
            Session["ImportCsv"] = content;
            Session["ImportHash"] = hash;
            Session["ImportFile"] = fileName;

            gv.DataSource = pv.Rows; gv.DataBind();
            litTotal.Text = pv.Total.ToString(); litValid.Text = pv.Valid.ToString(); litWarn.Text = pv.Warning.ToString();
            litErr.Text = pv.Error.ToString(); litCreate.Text = pv.SessionsToCreate.ToString(); litUpdate.Text = pv.SessionsToUpdate.ToString();
            btnImport.Enabled = pv.CanImport;
            pnlPreview.Visible = true;
            if (!pv.CanImport) Show(false, "The file has validation errors. Fix them and re-upload before importing.");
            else Show(true, "Validation complete. Review the preview, then click Import.");
        }

        protected void btnImport_Click(object sender, EventArgs e)
        {
            if (!_repo.UserCanImportAttendance(Role)) { Show(false, "You are not authorized to import attendance."); return; }
            string content = Convert.ToString(Session["ImportCsv"]);
            string hash = Convert.ToString(Session["ImportHash"]);
            string fileName = Convert.ToString(Session["ImportFile"]);
            if (string.IsNullOrEmpty(content)) { HidePreview(); Show(false, "Please upload and preview a file first."); return; }

            List<string[]> csv = AttendanceUi.ParseCsv(content);
            try
            {
                int batchId = _repo.ImportAttendanceBatch(csv, Options(), fileName, hash, UserId, Role);
                Session.Remove("ImportCsv"); Session.Remove("ImportHash"); Session.Remove("ImportFile");
                HidePreview(); BindHistory();
                Show(true, "Import complete. Batch #" + batchId + " saved.");
            }
            catch (Exception ex) { Show(false, ex.Message); }
        }

        private void Show(bool ok, string text)
        {
            msg.Visible = true;
            msg.CssClass = "rounded-lg p-3 mb-4 text-sm " + (ok ? "bg-emerald-50 text-emerald-800 border border-emerald-200" : "bg-amber-50 text-amber-800 border border-amber-200");
            msgText.Text = HttpUtility.HtmlEncode(text);
        }
    }
}
