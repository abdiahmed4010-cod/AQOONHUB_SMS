using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Payroll
{
    public partial class CreatePayRun : System.Web.UI.Page
    {
        private readonly PayrollRepository _repo = new PayrollRepository();

        private int Step
        {
            get { return ViewState["Step"] == null ? 1 : (int)ViewState["Step"]; }
            set { ViewState["Step"] = value; }
        }

        private DataTable Components
        {
            get { return ViewState["Comp"] as DataTable; }
            set { ViewState["Comp"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthorizeFinance()) return;

            if (!IsPostBack)
            {
                BindPeriods();
                BindDepartments();
                UpdatePayDate();
                ShowStep(1);
            }
        }

        #region Authorization

        private bool AuthorizeFinance()
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx", true);
                return false;
            }
            string role = _repo.NormalizeRole(Convert.ToString(Session["Role"]));
            if (role != "superadmin" && role != "admin" && role != "accountant" && role != "finance")
            {
                Response.Redirect("~/Default.aspx", true);
                return false;
            }
            return true;
        }

        #endregion

        #region Binders

        private void BindPeriods()
        {
            DataTable dt = _repo.GetPayrollPeriods();
            ddlPeriod.Items.Clear();
            ddlPeriod.Items.Add(new ListItem("Select Pay Period", ""));
            foreach (DataRow row in dt.Rows)
            {
                string status = Convert.ToString(row["Status"]);
                if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                    status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                    continue; // cannot use closed periods
                ddlPeriod.Items.Add(new ListItem(
                    Convert.ToString(row["PeriodName"]) + " (" + status + ")",
                    Convert.ToString(row["PayrollPeriodID"])));
            }
        }

        private void BindDepartments()
        {
            DataTable staff = _repo.GetEligibleStaff();
            SortedSet<string> depts = new SortedSet<string>(StringComparer.CurrentCultureIgnoreCase);
            foreach (DataRow row in staff.Rows)
            {
                string d = Convert.ToString(row["Department"]);
                if (!string.IsNullOrWhiteSpace(d)) depts.Add(d.Trim());
            }
            ddlDept.Items.Clear();
            ddlDept.Items.Add(new ListItem("All Departments", ""));
            foreach (string d in depts) ddlDept.Items.Add(new ListItem(d, d));
        }

        private void UpdatePayDate()
        {
            litPayDate.Text = "—";
            int pid;
            if (int.TryParse(ddlPeriod.SelectedValue, out pid) && pid > 0)
            {
                PayrollPeriodData p = _repo.GetPayrollPeriod(pid);
                if (p != null && p.PaymentDate.HasValue)
                    litPayDate.Text = p.PaymentDate.Value.ToString("dd MMM yyyy");
                else if (p != null)
                    litPayDate.Text = p.EndDate.ToString("dd MMM yyyy");
            }
        }

        protected void ddlPeriod_Changed(object sender, EventArgs e) { UpdatePayDate(); }

        private void BindStaff()
        {
            DataTable staff = _repo.GetEligibleStaff();
            string dept = ddlDept.SelectedValue;
            string search = (txtStaffSearch.Text ?? string.Empty).Trim();

            DataView dv = staff.DefaultView;
            List<string> filters = new List<string>();
            if (!string.IsNullOrEmpty(dept))
                filters.Add("Department = '" + dept.Replace("'", "''") + "'");
            if (!string.IsNullOrEmpty(search))
            {
                string s = search.Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]");
                filters.Add("(EmployeeID LIKE '%" + s + "%' OR Department LIKE '%" + s + "%' OR Position LIKE '%" + s + "%')");
            }
            dv.RowFilter = string.Join(" AND ", filters);

            gvStaff.DataSource = dv;
            gvStaff.DataBind();
            RecheckStaffSelection();
        }

        // Re-check staff rows already in the Components table (when navigating Back).
        private void RecheckStaffSelection()
        {
            DataTable comp = Components;
            if (comp == null) return;
            HashSet<int> ids = new HashSet<int>();
            foreach (DataRow r in comp.Rows) ids.Add(Convert.ToInt32(r["StaffID"]));
            foreach (GridViewRow row in gvStaff.Rows)
            {
                int staffId = Convert.ToInt32(gvStaff.DataKeys[row.RowIndex].Value);
                CheckBox chk = row.FindControl("chkSel") as CheckBox;
                if (chk != null) chk.Checked = ids.Contains(staffId);
            }
        }

        #endregion

        #region Step navigation

        private void ShowStep(int step)
        {
            Step = step;
            pnlStep1.Visible = step == 1;
            pnlStep2.Visible = step == 2;
            pnlStep3.Visible = step == 3;
            pnlStep4.Visible = step == 4;

            SetStepClass(stp1, 1, step);
            SetStepClass(stp2, 2, step);
            SetStepClass(stp3, 3, step);
            SetStepClass(stp4, 4, step);
        }

        private void SetStepClass(Panel p, int index, int current)
        {
            p.CssClass = "st" + (index == current ? " active" : (index < current ? " done" : ""));
        }

        protected void btnNext1_Click(object sender, EventArgs e)
        {
            int pid;
            if (string.IsNullOrWhiteSpace(txtPayRunName.Text)) { Warn("Pay Run Name is required."); return; }
            if (!int.TryParse(ddlPeriod.SelectedValue, out pid) || pid <= 0) { Warn("Please select a pay period."); return; }

            ViewState["Name"] = txtPayRunName.Text.Trim();
            ViewState["PeriodId"] = pid;
            ViewState["PeriodName"] = ddlPeriod.SelectedItem.Text;
            ViewState["PayDate"] = litPayDate.Text;
            ViewState["Method"] = ddlMethod.SelectedValue;

            BindStaff();
            ShowStep(2);
        }

        protected void btnStaffFilter_Click(object sender, EventArgs e) { BindStaff(); }

        protected void btnBack2_Click(object sender, EventArgs e) { ShowStep(1); }

        protected void btnNext2_Click(object sender, EventArgs e)
        {
            DataTable comp = BuildComponentsTable();
            foreach (GridViewRow row in gvStaff.Rows)
            {
                CheckBox chk = row.FindControl("chkSel") as CheckBox;
                if (chk == null || !chk.Checked) continue;
                int staffId = Convert.ToInt32(gvStaff.DataKeys[row.RowIndex].Value);
                DataRow dr = comp.NewRow();
                dr["StaffID"] = staffId;
                dr["EmployeeID"] = row.Cells[1].Text;
                dr["Department"] = row.Cells[2].Text;
                dr["Position"] = row.Cells[3].Text;
                dr["BasicSalary"] = ParseCell(row.Cells[4].Text);
                dr["OtherAllowance"] = 0m;
                dr["Bonus"] = 0m;
                dr["TaxDeduction"] = 0m;
                dr["OtherDeduction"] = 0m;
                comp.Rows.Add(dr);
            }

            if (comp.Rows.Count == 0) { Warn("Select at least one employee."); return; }

            Components = comp;
            gvComponents.DataSource = comp;
            gvComponents.DataBind();
            ShowStep(3);
        }

        protected void btnBack3_Click(object sender, EventArgs e)
        {
            BindStaff();
            ShowStep(2);
        }

        protected void btnNext3_Click(object sender, EventArgs e)
        {
            if (!ReadComponentInputs()) return;
            BuildReview();
            ShowStep(4);
        }

        protected void btnBack4_Click(object sender, EventArgs e)
        {
            gvComponents.DataSource = Components;
            gvComponents.DataBind();
            ShowStep(3);
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            DataTable comp = Components;
            if (comp == null || comp.Rows.Count == 0) { Warn("No employees selected."); return; }
            int pid = Convert.ToInt32(ViewState["PeriodId"]);

            List<PayRunStaffComponent> list = new List<PayRunStaffComponent>();
            foreach (DataRow r in comp.Rows)
            {
                list.Add(new PayRunStaffComponent
                {
                    StaffID = Convert.ToInt32(r["StaffID"]),
                    OtherAllowance = Convert.ToDecimal(r["OtherAllowance"]),
                    Bonus = Convert.ToDecimal(r["Bonus"]),
                    TaxDeduction = Convert.ToDecimal(r["TaxDeduction"]),
                    OtherDeduction = Convert.ToDecimal(r["OtherDeduction"])
                });
            }

            try
            {
                int? userId = null;
                int uid;
                if (int.TryParse(Convert.ToString(Session["UserID"]), out uid)) userId = uid;

                int created = _repo.GeneratePayRun(pid, list, Convert.ToString(ViewState["Method"]), userId);
                Response.Redirect("Payroll.aspx?created=" + created, true);
            }
            catch (System.Threading.ThreadAbortException) { throw; }
            catch (Exception ex)
            {
                Warn(ex.Message);
                ShowStep(4);
            }
        }

        #endregion

        #region Components grid

        private DataTable BuildComponentsTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("StaffID", typeof(int));
            dt.Columns.Add("EmployeeID", typeof(string));
            dt.Columns.Add("Department", typeof(string));
            dt.Columns.Add("Position", typeof(string));
            dt.Columns.Add("BasicSalary", typeof(decimal));
            dt.Columns.Add("OtherAllowance", typeof(decimal));
            dt.Columns.Add("Bonus", typeof(decimal));
            dt.Columns.Add("TaxDeduction", typeof(decimal));
            dt.Columns.Add("OtherDeduction", typeof(decimal));
            return dt;
        }

        protected void gvComponents_RowDataBound(object sender, GridViewRowEventArgs e) { }

        // Read edited textbox values from the components grid back into the ViewState table.
        private bool ReadComponentInputs()
        {
            DataTable comp = Components;
            if (comp == null) { Warn("No employees selected."); return false; }

            foreach (GridViewRow row in gvComponents.Rows)
            {
                int staffId = Convert.ToInt32(gvComponents.DataKeys[row.RowIndex].Value);
                DataRow[] match = comp.Select("StaffID = " + staffId);
                if (match.Length == 0) continue;
                DataRow dr = match[0];

                decimal other = ParseInput(row, "txtOther");
                decimal bonus = ParseInput(row, "txtBonus");
                decimal tax = ParseInput(row, "txtTax");
                decimal oded = ParseInput(row, "txtOtherDed");

                decimal basic = Convert.ToDecimal(dr["BasicSalary"]);
                if (basic + other + bonus - tax - oded < 0m)
                {
                    Warn("Deductions exceed gross salary for " + Convert.ToString(dr["EmployeeID"]) + ". Net salary cannot be negative.");
                    return false;
                }

                dr["OtherAllowance"] = other;
                dr["Bonus"] = bonus;
                dr["TaxDeduction"] = tax;
                dr["OtherDeduction"] = oded;
            }
            Components = comp;
            return true;
        }

        private decimal ParseInput(GridViewRow row, string id)
        {
            TextBox tb = row.FindControl(id) as TextBox;
            decimal v;
            if (tb != null && decimal.TryParse(tb.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out v) && v >= 0m)
                return v;
            return 0m;
        }

        #endregion

        #region Review

        private void BuildReview()
        {
            DataTable comp = Components;
            DataTable review = comp.Clone();
            review.Columns.Add("GrossSalary", typeof(decimal));
            review.Columns.Add("TotalDeductions", typeof(decimal));
            review.Columns.Add("NetSalary", typeof(decimal));

            decimal tBasic = 0, tOther = 0, tBonus = 0, tGross = 0, tTax = 0, tOtherDed = 0, tDed = 0, tNet = 0;
            SortedSet<string> depts = new SortedSet<string>(StringComparer.CurrentCultureIgnoreCase);

            foreach (DataRow r in comp.Rows)
            {
                decimal basic = Convert.ToDecimal(r["BasicSalary"]);
                decimal other = Convert.ToDecimal(r["OtherAllowance"]);
                decimal bonus = Convert.ToDecimal(r["Bonus"]);
                decimal tax = Convert.ToDecimal(r["TaxDeduction"]);
                decimal oded = Convert.ToDecimal(r["OtherDeduction"]);
                decimal gross = basic + other + bonus;
                decimal ded = tax + oded;
                decimal net = gross - ded;

                DataRow rr = review.NewRow();
                rr.ItemArray = r.ItemArray.Clone() as object[];
                rr["GrossSalary"] = gross;
                rr["TotalDeductions"] = ded;
                rr["NetSalary"] = net;
                review.Rows.Add(rr);

                tBasic += basic; tOther += other; tBonus += bonus; tGross += gross;
                tTax += tax; tOtherDed += oded; tDed += ded; tNet += net;
                string d = Convert.ToString(r["Department"]);
                if (!string.IsNullOrWhiteSpace(d)) depts.Add(d.Trim());
            }

            gvReview.DataSource = review;
            gvReview.DataBind();

            litRName.Text = Enc(ViewState["Name"]);
            litRPeriod.Text = Enc(ViewState["PeriodName"]);
            litRPayDate.Text = Enc(ViewState["PayDate"]);
            litRCount.Text = comp.Rows.Count.ToString();
            litRMethod.Text = Enc(ViewState["Method"]);
            litRBasic.Text = PayrollFormat.Money(tBasic);
            litROther.Text = PayrollFormat.Money(tOther);
            litRBonus.Text = PayrollFormat.Money(tBonus);
            litRGross.Text = PayrollFormat.Money(tGross);
            litRDeductions.Text = PayrollFormat.Money(tDed);
            litRNet.Text = PayrollFormat.Money(tNet);
        }

        #endregion

        #region Helpers

        private decimal ParseCell(string text)
        {
            decimal v;
            return decimal.TryParse((text ?? "").Replace("&nbsp;", "").Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out v) ? v : 0m;
        }

        private string Enc(object v) { return HttpUtility.HtmlEncode(v == null ? "" : v.ToString()); }

        private void Warn(string message)
        {
            pnlMsg.Visible = true;
            lblMsg.Text = HttpUtility.HtmlEncode(message);
        }

        #endregion
    }
}
