using System;
using System.Data;
using System.Globalization;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Examinations
{
    public partial class ExamSchedule : System.Web.UI.Page
    {
        private readonly ExaminationsRepository _repo = new ExaminationsRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack)
            {
                BindFilterYears();
                BindFilterTerms();
                BindFilterExams();
                BindFormExams();
                BindRooms();
                BindInvigilators();
                int preExam;
                if (int.TryParse(Request.QueryString["exam"], out preExam) && preExam > 0 && ddlExam.Items.FindByValue(preExam.ToString()) != null)
                {
                    ddlExam.SelectedValue = preExam.ToString();
                    BindClassesForExam(); BindSectionsAndSubjects();
                    if (ddlFExam.Items.FindByValue(preExam.ToString()) != null) ddlFExam.SelectedValue = preExam.ToString();
                }
                LoadSummaryAndGrid();
            }
        }

        private bool Authorize()
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Modules/Authentication/Login.aspx", true); return false; }
            if (!_repo.CanManage(Convert.ToString(Session["Role"]))) { Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true); return false; }
            return true;
        }

        // ---- markup helpers ----
        protected string Time(object v)
        {
            if (v == null || v == DBNull.Value) return "—";
            return DateTime.Today.Add((TimeSpan)v).ToString("hh:mm tt", CultureInfo.InvariantCulture);
        }
        protected string StatusStyle(string s)
        {
            switch ((s ?? "").ToLowerInvariant())
            {
                case "scheduled": return "background:#DBEAFE;color:#2563EB";
                case "cancelled": return "background:#FEE2E2;color:#DC2626";
                default: return "background:#F1F5F9;color:#64748B";
            }
        }

        private int? FYear() { int v; return int.TryParse(ddlFYear.SelectedValue, out v) && v > 0 ? v : (int?)null; }
        private int? FTerm() { int v; return int.TryParse(ddlFTerm.SelectedValue, out v) && v > 0 ? v : (int?)null; }
        private int? FExam() { int v; return int.TryParse(ddlFExam.SelectedValue, out v) && v > 0 ? v : (int?)null; }

        // ---- filter binding ----
        private void BindFilterYears()
        {
            ddlFYear.Items.Clear(); ddlFYear.Items.Add(new ListItem("All Years", ""));
            foreach (DataRow r in _repo.GetAcademicYears().Rows) ddlFYear.Items.Add(new ListItem(Convert.ToString(r["YearName"]), Convert.ToString(r["AcademicYearID"])));
            int a = _repo.GetActiveAcademicYearId(); if (a > 0 && ddlFYear.Items.FindByValue(a.ToString()) != null) ddlFYear.SelectedValue = a.ToString();
        }
        private void BindFilterTerms()
        {
            ddlFTerm.Items.Clear(); ddlFTerm.Items.Add(new ListItem("All Terms", ""));
            foreach (DataRow r in _repo.GetTerms(FYear()).Rows) ddlFTerm.Items.Add(new ListItem(Convert.ToString(r["TermName"]), Convert.ToString(r["TermID"])));
        }
        private void BindFilterExams()
        {
            ddlFExam.Items.Clear(); ddlFExam.Items.Add(new ListItem("All Examinations", ""));
            foreach (DataRow r in _repo.GetExaminations(FYear(), FTerm(), null, "", "").Rows) ddlFExam.Items.Add(new ListItem(Convert.ToString(r["ExamName"]), Convert.ToString(r["ExamID"])));
        }

        protected void ddlFYear_Changed(object sender, EventArgs e) { BindFilterTerms(); BindFilterExams(); LoadSummaryAndGrid(); }
        protected void btnFilter_Click(object sender, EventArgs e) { LoadSummaryAndGrid(); }
        protected void btnReset_Click(object sender, EventArgs e)
        {
            int a = _repo.GetActiveAcademicYearId(); if (a > 0 && ddlFYear.Items.FindByValue(a.ToString()) != null) ddlFYear.SelectedValue = a.ToString();
            BindFilterTerms(); BindFilterExams(); ddlFStatus.SelectedIndex = 0; LoadSummaryAndGrid();
        }

        private void LoadSummaryAndGrid()
        {
            litTotalExams.Text = Convert.ToString(_repo.GetSummary(FYear(), FTerm())["TotalExams"]);
            litRooms.Text = _repo.GetExamRooms().Rows.Count.ToString();
            litInvigilators.Text = _repo.GetActiveInvigilators().Rows.Count.ToString();

            DataTable dt = _repo.GetExamSchedules(FYear(), FTerm(), FExam(), null, null, ddlFStatus.SelectedValue, "");
            gv.DataSource = dt; gv.DataBind();
            litScheduled.Text = dt.Rows.Count.ToString();
        }

        // ---- form dependent dropdowns ----
        private void BindFormExams()
        {
            ddlExam.Items.Clear(); ddlExam.Items.Add(new ListItem("— Select examination —", "0"));
            foreach (DataRow r in _repo.GetExamsForScheduling().Rows) ddlExam.Items.Add(new ListItem(Convert.ToString(r["ExamName"]), Convert.ToString(r["ExamID"])));
        }
        private void BindRooms()
        {
            ddlRoom.Items.Clear(); ddlRoom.Items.Add(new ListItem("— Select room —", "0"));
            foreach (DataRow r in _repo.GetExamRooms().Rows) ddlRoom.Items.Add(new ListItem(Convert.ToString(r["RoomName"]) + " (cap " + Convert.ToString(r["Capacity"]) + ")", Convert.ToString(r["ExamRoomID"])));
        }
        private void BindInvigilators()
        {
            ddlInvigilator.Items.Clear(); ddlInvigilator.Items.Add(new ListItem("— Select invigilator —", "0"));
            foreach (DataRow r in _repo.GetActiveInvigilators().Rows) ddlInvigilator.Items.Add(new ListItem(Convert.ToString(r["FullName"]), Convert.ToString(r["StaffID"])));
        }

        private int ExamV() { int v; return int.TryParse(ddlExam.SelectedValue, out v) ? v : 0; }
        private int ClassV() { int v; return int.TryParse(ddlClass.SelectedValue, out v) ? v : 0; }
        private int SectionV() { int v; return int.TryParse(ddlSection.SelectedValue, out v) ? v : 0; }
        private int RoomV() { int v; return int.TryParse(ddlRoom.SelectedValue, out v) ? v : 0; }

        private void BindClassesForExam()
        {
            ddlClass.Items.Clear(); ddlClass.Items.Add(new ListItem("— Select class —", "0"));
            if (ExamV() > 0)
                foreach (DataRow r in _repo.GetClassesForExam(ExamV()).Rows) ddlClass.Items.Add(new ListItem(Convert.ToString(r["ClassName"]), Convert.ToString(r["ClassID"])));
        }
        private void BindSectionsAndSubjects()
        {
            ddlSection.Items.Clear(); ddlSection.Items.Add(new ListItem("All Sections", "0"));
            ddlSubject.Items.Clear(); ddlSubject.Items.Add(new ListItem("— Select subject —", "0"));
            if (ClassV() > 0)
            {
                foreach (DataRow r in _repo.GetSectionsForClass(ClassV()).Rows) ddlSection.Items.Add(new ListItem(Convert.ToString(r["SectionName"]), Convert.ToString(r["SectionID"])));
                foreach (DataRow r in _repo.GetSubjectsForExamClass(ExamV(), ClassV()).Rows) ddlSubject.Items.Add(new ListItem(Convert.ToString(r["SubjectName"]), Convert.ToString(r["SubjectID"])));
            }
        }

        protected void ddlExam_Changed(object sender, EventArgs e) { BindClassesForExam(); BindSectionsAndSubjects(); CapacityCheck(); }
        protected void ddlClass_Changed(object sender, EventArgs e) { BindSectionsAndSubjects(); CapacityCheck(); }
        protected void ddlSection_Changed(object sender, EventArgs e) { CapacityCheck(); }
        protected void ddlRoom_Changed(object sender, EventArgs e) { CapacityCheck(); }

        private void CapacityCheck()
        {
            pnlCapacity.Visible = false;
            if (SectionV() > 0 && RoomV() > 0)
            {
                int students = _repo.GetSectionStudentCount(SectionV());
                DataRow room = _repo.GetExamRoom(RoomV());
                if (room != null)
                {
                    int cap = Convert.ToInt32(room["Capacity"]);
                    if (students > cap)
                    {
                        pnlCapacity.Visible = true;
                        litCapacity.Text = HttpUtility.HtmlEncode("Warning: the section has " + students + " students but the room capacity is " + cap + ".");
                    }
                }
            }
        }

        // ---- grid actions ----
        protected void gv_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id) || id <= 0) return;
            if (e.CommandName == "EditRow")
            {
                DataRow r = _repo.GetExamSchedule(id);
                if (r == null) return;
                hfId.Value = id.ToString();
                BindFormExams();
                if (ddlExam.Items.FindByValue(Convert.ToString(r["ExamID"])) != null) ddlExam.SelectedValue = Convert.ToString(r["ExamID"]);
                BindClassesForExam();
                if (ddlClass.Items.FindByValue(Convert.ToString(r["ClassID"])) != null) ddlClass.SelectedValue = Convert.ToString(r["ClassID"]);
                BindSectionsAndSubjects();
                string sec = r["SectionID"] == DBNull.Value ? "0" : Convert.ToString(r["SectionID"]);
                if (ddlSection.Items.FindByValue(sec) != null) ddlSection.SelectedValue = sec;
                if (ddlSubject.Items.FindByValue(Convert.ToString(r["SubjectID"])) != null) ddlSubject.SelectedValue = Convert.ToString(r["SubjectID"]);
                txtDate.Text = Convert.ToDateTime(r["ExamDate"]).ToString("yyyy-MM-dd");
                txtStart.Text = ((TimeSpan)r["StartTime"]).ToString(@"hh\:mm");
                txtEnd.Text = ((TimeSpan)r["EndTime"]).ToString(@"hh\:mm");
                if (r["ExamRoomID"] != DBNull.Value && ddlRoom.Items.FindByValue(Convert.ToString(r["ExamRoomID"])) != null) ddlRoom.SelectedValue = Convert.ToString(r["ExamRoomID"]);
                if (r["InvigilatorStaffID"] != DBNull.Value && ddlInvigilator.Items.FindByValue(Convert.ToString(r["InvigilatorStaffID"])) != null) ddlInvigilator.SelectedValue = Convert.ToString(r["InvigilatorStaffID"]);
                txtNotes.Text = r["Notes"] == DBNull.Value ? "" : Convert.ToString(r["Notes"]);
                ddlStatus.SelectedValue = r["Status"] == DBNull.Value ? "Scheduled" : Convert.ToString(r["Status"]);
                CapacityCheck();
            }
            else if (e.CommandName == "DeleteRow")
            {
                try { _repo.DeleteExamScheduleSafely(id); Show(true, "Schedule removed."); LoadSummaryAndGrid(); }
                catch (Exception ex) { Show(false, ex.Message); }
            }
        }

        protected void btnClear_Click(object sender, EventArgs e) { ResetForm(); }

        private void ResetForm()
        {
            hfId.Value = "0"; txtDate.Text = txtStart.Text = txtEnd.Text = txtNotes.Text = "";
            ddlExam.SelectedIndex = 0; BindClassesForExam(); BindSectionsAndSubjects();
            ddlRoom.SelectedIndex = 0; ddlInvigilator.SelectedIndex = 0; ddlStatus.SelectedValue = "Scheduled";
            pnlCapacity.Visible = false;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int id = int.Parse(hfId.Value);
            int exam = ExamV(), cls = ClassV(), section = SectionV(), room = RoomV(), invig;
            int subject; int.TryParse(ddlSubject.SelectedValue, out subject);
            int.TryParse(ddlInvigilator.SelectedValue, out invig);
            DateTime date; TimeSpan start, end;

            if (exam <= 0) { Show(false, "Please select an examination."); return; }
            if (cls <= 0) { Show(false, "Please select a class."); return; }
            if (subject <= 0) { Show(false, "Please select a subject."); return; }
            if (!DateTime.TryParse(txtDate.Text, out date)) { Show(false, "Please provide a valid date."); return; }
            if (!TimeSpan.TryParse(txtStart.Text, out start)) { Show(false, "Please provide a valid start time."); return; }
            if (!TimeSpan.TryParse(txtEnd.Text, out end)) { Show(false, "Please provide a valid end time."); return; }
            if (room <= 0) { Show(false, "Please select a room."); return; }
            if (invig <= 0) { Show(false, "Please select an invigilator."); return; }

            try
            {
                _repo.SaveExamSchedule(id, exam, cls, section > 0 ? section : (int?)null, subject, date, start, end,
                    room, invig, txtNotes.Text.Trim(), ddlStatus.SelectedValue);
                Show(true, "Schedule saved.");
                ResetForm();
                LoadSummaryAndGrid();
            }
            catch (Exception ex) { Show(false, ex.Message); }
        }

        protected void btnPublish_Click(object sender, EventArgs e)
        {
            int exam = ExamV();
            if (exam <= 0) { Show(false, "Select an examination in the form to publish its schedule."); return; }
            try { _repo.PublishExamSchedule(exam); Show(true, "Schedule published. The examination is now Scheduled and locked."); BindFormExams(); LoadSummaryAndGrid(); }
            catch (Exception ex) { Show(false, ex.Message); }
        }

        private void Show(bool ok, string text)
        {
            msg.Visible = true;
            msg.CssClass = "rounded-lg p-3 mb-4 text-sm " + (ok
                ? "bg-emerald-50 text-emerald-800 border border-emerald-200"
                : "bg-amber-50 text-amber-800 border border-amber-200");
            msgText.Text = HttpUtility.HtmlEncode(text);
        }
    }
}
