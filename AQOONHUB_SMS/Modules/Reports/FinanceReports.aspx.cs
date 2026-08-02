using System;

namespace AQOONHUB_SMS.Modules.Reports
{
    public partial class FinanceReports : ReportCategoryBase
    {
        protected override string CategoryKey { get { return ReportAuthorization.Finance; } }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthorizeCategory()) return;
            RenderCards(litCards, litDataSource);
        }
    }
}
