using System;

namespace AQOONHUB_SMS.Modules.Reports
{
    public partial class LibraryReports : ReportCategoryBase
    {
        protected override string CategoryKey { get { return ReportAuthorization.Library; } }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthorizeCategory()) return;
            RenderCards(litCards, litDataSource);
        }
    }
}
