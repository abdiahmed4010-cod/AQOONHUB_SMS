using System;

namespace AQOONHUB_SMS.Modules.Reports
{
    public partial class StudentReports : ReportCategoryBase
    {
        protected override string CategoryKey { get { return ReportAuthorization.Student; } }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthorizeCategory()) return;
            RenderCards(litCards, litDataSource);
        }
    }
}
