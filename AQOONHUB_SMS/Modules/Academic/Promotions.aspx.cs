using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Academic
{
    public partial class Promotions : System.Web.UI.Page
    {
        private readonly AcademicsRepository _repo = new AcademicsRepository();

        private int Step
        {
            get { object o = ViewState["step"]; return o == null ? 1 : (int)o; }
            set { ViewState["step"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack)
            {
                BindYearFilters();
                BindClassFilter();
                LoadListAndSummary();
            }
        }

        private bool Authorize()
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx", true);
                return false;
            }
            if (!_repo.CanManage(Convert.ToString(Session["Role"])))
            {
                Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true);
                return false;
            }
            return true;
        }

        // ---------- markup helpers ----------
        protected string PromoLabel(object v)
        {
            string s = v == null || v == DBNull.Value ? "" : Convert.ToString(v);
            return string.IsNullOrEmpty(s) ? "Pending" : Server.HtmlEncode(s == "NotEligible" ? "Not Eligible" : s);
        }

        protected string StatusStyle(string status)
        {
            switch ((status ?? "").ToLowerInvariant())
            {
                case "promoted": return "background:#DCFCE7;color:#15803D";
                case "repeated": return "background:#FFEDD5;color:#C2410C";
                case "graduated": return "background:#E0E7FF;color:#4338CA";
                case "transferred": return "background:#E0F2FE;color:#0369A1";
                case "withdrawn": return "background:#FEE2E2;color:#DC2626";
                case "noteligible": return "background:#FEE2E2;color:#DC2626";
                default: return "background:#FEF3C7;color:#B45309"; // Pending
            }
        }

        // ---------- filters / list / summary ----------
        private void BindYearFilters()
        {
            FillYears(ddlFrom); FillYears(ddlTo);
            int active = _repo.GetActiveAcademicYearId();
            if (active > 0 && ddlFrom.Items.FindByValue(active.ToString()) != null) ddlFrom.SelectedValue = active.ToString();
        }

        private void BindClassFilter()
        {
            FillList(ddlClass, _repo.GetClassesLookup(), "ClassName", "ClassID", "All Classes");
            ddlSection.Items.Clear(); ddlSection.Items.Add(new ListItem("All Sections", ""));
        }

        protected void ddlClass_Changed(object sender, EventArgs e)
        {
            ddlSection.Items.Clear(); ddlSection.Items.Add(new ListItem("All Sections", ""));
            int c; if (int.TryParse(ddlClass.SelectedValue, out c) && c > 0)
                foreach (DataRow r in _repo.GetSectionsLookup(c).Rows)
                    ddlSection.Items.Add(new ListItem(Convert.ToString(r["SectionName"]), Convert.ToString(r["SectionID"])));
            LoadListAndSummary();
        }

        private int FromYear() { int v; return int.TryParse(ddlFrom.SelectedValue, out v) ? v : 0; }
        private int ToYear() { int v; return int.TryParse(ddlTo.SelectedValue, out v) ? v : 0; }
        private int? ClassF() { int v; return int.TryParse(ddlClass.SelectedValue, out v) && v > 0 ? v : (int?)null; }
        private int? SectionF() { int v; return int.TryParse(ddlSection.SelectedValue, out v) && v > 0 ? v : (int?)null; }

        private void LoadListAndSummary()
        {
            int from = FromYear(), to = ToYear();
            if (from <= 0) return;

            DataTable cand = _repo.GetPromotionCandidates(from, to > 0 ? to : from, ClassF(), SectionF(), txtSearch.Text.Trim());
            gv.DataSource = cand; gv.DataBind();

            if (to > 0)
            {
                DataRow s = _repo.GetPromotionSummary(from, to, ClassF(), SectionF());
                long total = Convert.ToInt64(s["TotalStudents"]);
                long processed = Convert.ToInt64(s["Processed"]);
                litTotal.Text = total.ToString();
                litPromoted.Text = Convert.ToString(s["Promoted"]);
                litRepeated.Text = Convert.ToString(s["Repeated"]);
                litGraduated.Text = Convert.ToString(s["Graduated"]);
                litNotEligible.Text = "0";
                litPending.Text = Math.Max(0, total - processed).ToString();
                litEligible.Text = Math.Max(0, total - processed).ToString();
            }
            else
            {
                litTotal.Text = cand.Rows.Count.ToString();
                litEligible.Text = cand.Rows.Count.ToString();
                litPending.Text = cand.Rows.Count.ToString();
                litPromoted.Text = litRepeated.Text = litGraduated.Text = litNotEligible.Text = "0";
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e) { LoadListAndSummary(); }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            ddlFrom.SelectedIndex = 0; ddlTo.SelectedIndex = 0; ddlClass.SelectedIndex = 0;
            ddlSection.Items.Clear(); ddlSection.Items.Add(new ListItem("All Sections", ""));
            txtSearch.Text = "";
            int active = _repo.GetActiveAcademicYearId();
            if (active > 0 && ddlFrom.Items.FindByValue(active.ToString()) != null) ddlFrom.SelectedValue = active.ToString();
            LoadListAndSummary();
        }

        // ---------- wizard ----------
        protected void btnPromote_Click(object sender, EventArgs e)
        {
            Step = 1;
            FillYears(wFrom); FillYears(wTo);
            if (FromYear() > 0 && wFrom.Items.FindByValue(FromYear().ToString()) != null) wFrom.SelectedValue = FromYear().ToString();
            if (ToYear() > 0 && wTo.Items.FindByValue(ToYear().ToString()) != null) wTo.SelectedValue = ToYear().ToString();
            FillList(wCurClass, _repo.GetClassesLookup(), "ClassName", "ClassID", "— Select class —");
            FillList(wTgtClass, _repo.GetClassesLookup(), "ClassName", "ClassID", "— Select class —");
            wCurSection.Items.Clear(); wCurSection.Items.Add(new ListItem("All sections", "0"));
            wTgtSection.Items.Clear(); wTgtSection.Items.Add(new ListItem("— Select section —", "0"));
            ShowStep();
            pnlWizard.Visible = true;
        }

        protected void btnCloseWiz_Click(object sender, EventArgs e) { pnlWizard.Visible = false; LoadListAndSummary(); }

        protected void wCurClass_Changed(object sender, EventArgs e)
        {
            wCurSection.Items.Clear(); wCurSection.Items.Add(new ListItem("All sections", "0"));
            int c; if (int.TryParse(wCurClass.SelectedValue, out c) && c > 0)
                foreach (DataRow r in _repo.GetSectionsLookup(c).Rows)
                    wCurSection.Items.Add(new ListItem(Convert.ToString(r["SectionName"]), Convert.ToString(r["SectionID"])));
            pnlWizard.Visible = true; ShowStep();
        }

        protected void wTgtClass_Changed(object sender, EventArgs e)
        {
            wTgtSection.Items.Clear(); wTgtSection.Items.Add(new ListItem("— Select section —", "0"));
            int c; if (int.TryParse(wTgtClass.SelectedValue, out c) && c > 0)
                foreach (DataRow r in _repo.GetSectionsLookup(c).Rows)
                    wTgtSection.Items.Add(new ListItem(Convert.ToString(r["SectionName"]), Convert.ToString(r["SectionID"])));
            pnlWizard.Visible = true; ShowStep();
        }

        protected void btnStep1Next_Click(object sender, EventArgs e)
        {
            pnlWizard.Visible = true;
            int from, to, curClass, tgtSection;
            int.TryParse(wFrom.SelectedValue, out from);
            int.TryParse(wTo.SelectedValue, out to);
            int.TryParse(wCurClass.SelectedValue, out curClass);
            int.TryParse(wTgtSection.SelectedValue, out tgtSection);

            if (from <= 0 || to <= 0) { WizMsg("Select both From and To academic years."); return; }
            if (from == to) { WizMsg("From and To academic years must be different."); return; }
            if (curClass <= 0) { WizMsg("Select the current class."); return; }
            if (tgtSection <= 0) { WizMsg("Select the target section."); return; }

            int curSection; int.TryParse(wCurSection.SelectedValue, out curSection);
            ViewState["wFrom"] = from; ViewState["wTo"] = to; ViewState["wTgtSection"] = tgtSection;

            DataTable cand = _repo.GetPromotionCandidates(from, to, curClass, curSection > 0 ? curSection : (int?)null, "");
            // exclude already-promoted to the target year
            DataView dv = new DataView(cand);
            DataTable filtered = cand.Clone();
            foreach (DataRow r in cand.Rows)
                if (r["PromotionStatus"] == DBNull.Value) filtered.ImportRow(r);

            gvReview.DataSource = filtered; gvReview.DataBind();
            // apply default decision
            foreach (GridViewRow gr in gvReview.Rows)
            {
                DropDownList d = (DropDownList)gr.FindControl("ddlDecision");
                if (d != null && d.Items.FindByValue(wDefault.SelectedValue) != null) d.SelectedValue = wDefault.SelectedValue;
            }
            Step = 2; ShowStep();
        }

        protected void chkAll_Changed(object sender, EventArgs e)
        {
            foreach (GridViewRow gr in gvReview.Rows)
            {
                CheckBox c = (CheckBox)gr.FindControl("chkSel");
                if (c != null) c.Checked = chkAll.Checked;
            }
            pnlWizard.Visible = true; Step = 2; ShowStep();
        }

        protected void btnStep2Back_Click(object sender, EventArgs e) { pnlWizard.Visible = true; Step = 1; ShowStep(); }

        protected void btnStep2Next_Click(object sender, EventArgs e)
        {
            pnlWizard.Visible = true;
            List<string> picks = new List<string>();
            var counts = new Dictionary<string, int>();
            foreach (GridViewRow gr in gvReview.Rows)
            {
                CheckBox c = (CheckBox)gr.FindControl("chkSel");
                if (c == null || !c.Checked) continue;
                int sid = Convert.ToInt32(gvReview.DataKeys[gr.RowIndex].Value);
                string dec = ((DropDownList)gr.FindControl("ddlDecision")).SelectedValue;
                picks.Add(sid + ":" + dec);
                counts[dec] = counts.ContainsKey(dec) ? counts[dec] + 1 : 1;
            }
            if (picks.Count == 0) { Step = 2; ShowStep(); WizMsg("Select at least one student."); return; }

            ViewState["picks"] = string.Join(",", picks);

            System.Text.StringBuilder b = new System.Text.StringBuilder();
            b.Append("<div class='text-sm space-y-1'>");
            b.Append("<div>From Year: <b>").Append(Server.HtmlEncode(wFrom.SelectedItem.Text)).Append("</b> → To Year: <b>").Append(Server.HtmlEncode(wTo.SelectedItem.Text)).Append("</b></div>");
            b.Append("<div>Target Section: <b>").Append(Server.HtmlEncode(wTgtClass.SelectedItem.Text)).Append(" / ").Append(Server.HtmlEncode(wTgtSection.SelectedItem.Text)).Append("</b></div>");
            b.Append("<div class='mt-2'>Selected: <b>").Append(picks.Count).Append("</b> student(s)</div>");
            foreach (var kv in counts) b.Append("<div>").Append(Server.HtmlEncode(kv.Key == "NotEligible" ? "Not Eligible" : kv.Key)).Append(": <b>").Append(kv.Value).Append("</b></div>");
            b.Append("</div>");
            litConfirm.Text = b.ToString();

            Step = 3; ShowStep();
        }

        protected void btnStep3Back_Click(object sender, EventArgs e) { pnlWizard.Visible = true; Step = 2; ShowStep(); }

        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            pnlWizard.Visible = true;
            int from = Convert.ToInt32(ViewState["wFrom"]);
            int to = Convert.ToInt32(ViewState["wTo"]);
            int tgtSection = Convert.ToInt32(ViewState["wTgtSection"]);
            string raw = Convert.ToString(ViewState["picks"]);

            var items = new List<AcademicsRepository.PromotionItem>();
            int skippedNotEligible = 0;
            foreach (string tok in raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = tok.Split(':');
                string dec = parts[1];
                // Not Eligible: record nothing (leave the student pending, do not block a future promotion)
                if (dec == "NotEligible") { skippedNotEligible++; continue; }
                bool needsSection = dec == "Promoted" || dec == "Repeated";
                items.Add(new AcademicsRepository.PromotionItem
                {
                    StudentID = int.Parse(parts[0]),
                    Status = dec,
                    ToSectionID = needsSection ? tgtSection : (int?)null
                });
            }

            try
            {
                if (items.Count > 0) _repo.PromoteStudents(from, to, items, CurrentUserId());

                int promoted = items.FindAll(x => x.Status == "Promoted").Count;
                int repeated = items.FindAll(x => x.Status == "Repeated").Count;
                int graduated = items.FindAll(x => x.Status == "Graduated").Count;
                int transferred = items.FindAll(x => x.Status == "Transferred").Count;
                int withdrawn = items.FindAll(x => x.Status == "Withdrawn").Count;

                System.Text.StringBuilder b = new System.Text.StringBuilder("<div class='text-sm text-gray-600 space-y-1'>");
                b.Append("<div>Processed: <b>").Append(items.Count).Append("</b></div>");
                b.Append("<div>Promoted: <b>").Append(promoted).Append("</b> · Repeated: <b>").Append(repeated).Append("</b> · Graduated: <b>").Append(graduated).Append("</b></div>");
                b.Append("<div>Transferred: <b>").Append(transferred).Append("</b> · Withdrawn: <b>").Append(withdrawn).Append("</b></div>");
                if (skippedNotEligible > 0) b.Append("<div>Left pending (Not Eligible): <b>").Append(skippedNotEligible).Append("</b></div>");
                b.Append("</div>");
                litComplete.Text = b.ToString();

                Step = 4; ShowStep();
            }
            catch (Exception ex)
            {
                Step = 3; ShowStep();
                WizMsg(ex.Message); // batch rolled back
            }
        }

        protected void btnViewList_Click(object sender, EventArgs e)
        {
            pnlWizard.Visible = false;
            if (ViewState["wFrom"] != null) { if (ddlFrom.Items.FindByValue(ViewState["wFrom"].ToString()) != null) ddlFrom.SelectedValue = ViewState["wFrom"].ToString(); }
            if (ViewState["wTo"] != null) { if (ddlTo.Items.FindByValue(ViewState["wTo"].ToString()) != null) ddlTo.SelectedValue = ViewState["wTo"].ToString(); }
            LoadListAndSummary();
        }

        private void ShowStep()
        {
            pnlStep1.Visible = Step == 1;
            pnlStep2.Visible = Step == 2;
            pnlStep3.Visible = Step == 3;
            pnlStep4.Visible = Step == 4;
            SetStepClass(s1, 1); SetStepClass(s2, 2); SetStepClass(s3, 3); SetStepClass(s4, 4);
            pnlWizMsg.Visible = false;
        }

        private void SetStepClass(System.Web.UI.HtmlControls.HtmlGenericControl el, int n)
        {
            el.Attributes["class"] = "step" + (Step == n ? " active" : (Step > n ? " done" : ""));
        }

        private void WizMsg(string text)
        {
            pnlWizMsg.Visible = true;
            wizMsgText.Text = HttpUtility.HtmlEncode(text);
        }

        private int? CurrentUserId()
        {
            int uid;
            return int.TryParse(Convert.ToString(Session["UserID"]), out uid) ? uid : (int?)null;
        }

        // ---------- helpers ----------
        private void FillYears(DropDownList ddl)
        {
            ddl.Items.Clear();
            ddl.Items.Add(new ListItem("— Select year —", "0"));
            foreach (DataRow r in _repo.GetAcademicYearsLookup().Rows)
                ddl.Items.Add(new ListItem(Convert.ToString(r["YearName"]), Convert.ToString(r["AcademicYearID"])));
        }

        private void FillList(DropDownList ddl, DataTable dt, string text, string val, string first)
        {
            ddl.Items.Clear();
            ddl.Items.Add(new ListItem(first, first.StartsWith("All") ? "" : "0"));
            foreach (DataRow r in dt.Rows)
                ddl.Items.Add(new ListItem(Convert.ToString(r[text]), Convert.ToString(r[val])));
        }
    }
}
