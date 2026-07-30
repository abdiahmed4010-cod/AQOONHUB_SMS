using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Entry point: send authenticated users to the dashboard, others to login.
            if (Session["UserID"] != null)
                Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true);
            else
                Response.Redirect("~/Modules/Authentication/Login.aspx", true);
        }
    }
}