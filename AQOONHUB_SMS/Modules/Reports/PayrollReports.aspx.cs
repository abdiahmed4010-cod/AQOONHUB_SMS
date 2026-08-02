using System;

namespace AQOONHUB_SMS.Modules.Reports
{
    public partial class PayrollReports : ReportCategoryBase
    {
        protected override string CategoryKey { get { return ReportAuthorization.Payroll; } }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthorizeCategory()) return;
            RenderCards(litCards, litDataSource);
        }
    }
}
