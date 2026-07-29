using System;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Finance
{
    public partial class CreateInvoice : System.Web.UI.Page
    {
        readonly FeeRepository r = new FeeRepository();

        DataTable CurrentItems
        {
            get { return ViewState["items"] as DataTable; }
            set { ViewState["items"] = value; }
        }

        protected void Page_Load(object s, EventArgs e)
        {
            if (Session["UserID"] == null) Response.Redirect("~/Modules/Authentication/Login.aspx");
            if (!IsPostBack)
            {
                student.DataSource = r.GetStudents();
                student.DataTextField = "StudentName";
                student.DataValueField = "StudentID";
                student.DataBind();
                student.Items.Insert(0, new ListItem("Select Student", ""));
                invoiceDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
                dueDate.Text = DateTime.Today.AddDays(14).ToString("yyyy-MM-dd");
                RefreshSummary();
            }
        }

        protected void btnFindStudent_Click(object s, EventArgs e)
        {
            string key = (txtStudentSearch.Text ?? string.Empty).Trim();
            if (key == "")
            {
                msg.Visible = true;
                msgText.Text = "Enter a student ID or code to search.";
                return;
            }

            // Student codes look like AQH-2026-0001. Allow searching by the trailing
            // number alone (e.g. "1" or "0001") as well as the full code or the raw ID.
            int keyNum;
            bool keyIsNumber = int.TryParse(key.TrimStart('0') == "" ? "0" : key.TrimStart('0'), out keyNum);

            DataTable students = r.GetStudents();
            DataRow found = null;
            foreach (DataRow row in students.Rows)
            {
                string id = Convert.ToString(row["StudentID"]);
                string code = Convert.ToString(row["StudentCode"]);

                if (string.Equals(id, key, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(code, key, StringComparison.OrdinalIgnoreCase))
                {
                    found = row;
                    break;
                }

                if (keyIsNumber && !string.IsNullOrEmpty(code))
                {
                    string lastSeg = code.Substring(code.LastIndexOf('-') + 1);
                    int segNum;
                    if (int.TryParse(lastSeg, out segNum) && segNum == keyNum)
                    {
                        found = row;
                        break;
                    }
                }
            }

            if (found == null)
            {
                msg.Visible = true;
                msgText.Text = HttpUtility.HtmlEncode("No student found for \"" + key + "\".");
                student.SelectedIndex = 0;
                LoadItemsForStudent(null);
                return;
            }

            msg.Visible = false;
            string sid = Convert.ToString(found["StudentID"]);
            ListItem item = student.Items.FindByValue(sid);
            if (item != null) { student.ClearSelection(); item.Selected = true; }
            LoadItemsForStudent(int.Parse(sid));
        }

        protected void student_Changed(object s, EventArgs e)
        {
            if (student.SelectedValue == "")
            {
                LoadItemsForStudent(null);
                return;
            }
            LoadItemsForStudent(int.Parse(student.SelectedValue));
        }

        void LoadItemsForStudent(int? studentId)
        {
            if (studentId == null)
            {
                CurrentItems = null;
                items.DataSource = null;
                items.DataBind();
                RefreshSummary();
                return;
            }
            DataTable d = r.GetApplicableStructures(studentId.Value);
            d.Columns.Add("TotalAmount", typeof(decimal));
            foreach (DataRow x in d.Rows)
                x["TotalAmount"] = Convert.ToDecimal(x["Amount"]) - Convert.ToDecimal(x["DiscountAmount"]);
            CurrentItems = d;
            items.DataSource = d;
            items.DataBind();
            RefreshSummary();
            if (d.Rows.Count == 0)
            {
                msg.Visible = true;
                msgText.Text = "This student has no active fee structure for their class/academic year. Create one under Fee Structures first.";
            }
        }

        protected void discount_Changed(object s, EventArgs e) { RefreshSummary(); }

        void RefreshSummary()
        {
            decimal subtotal = 0;
            int count = 0;
            if (CurrentItems != null)
            {
                count = CurrentItems.Rows.Count;
                foreach (DataRow row in CurrentItems.Rows)
                    subtotal += Convert.ToDecimal(row["TotalAmount"]);
            }
            decimal dis;
            decimal.TryParse(discount.Text, out dis);
            if (dis < 0) dis = 0;
            decimal total = Math.Max(0, subtotal - dis);
            litItemCount.Text = count.ToString();
            litSubtotal.Text = subtotal.ToString("N2");
            litDiscount.Text = dis.ToString("N2");
            litTotal.Text = total.ToString("N2");
        }

        protected void create_Click(object s, EventArgs e)
        {
            try
            {
                if (student.SelectedValue == "")
                    throw new InvalidOperationException("Please select a student.");
                decimal dis;
                decimal.TryParse(discount.Text, out dis);
                if (dis < 0) dis = 0;
                if (CurrentItems == null || CurrentItems.Rows.Count == 0)
                    throw new InvalidOperationException("Select a student with an active fee structure.");
                int id = r.CreateInvoice(int.Parse(student.SelectedValue), DateTime.Parse(invoiceDate.Text),
                    DateTime.Parse(dueDate.Text), invoiceType.SelectedValue, dis, remarks.Text.Trim(),
                    instructions.Text.Trim(), Convert.ToInt32(Session["UserID"]), CurrentItems);
                Response.Redirect("ViewInvoice.aspx?id=" + id);
            }
            catch (System.Threading.ThreadAbortException) { throw; }
            catch (Exception ex)
            {
                msg.Visible = true;
                msgText.Text = HttpUtility.HtmlEncode(ex.Message);
            }
        }
    }
}
