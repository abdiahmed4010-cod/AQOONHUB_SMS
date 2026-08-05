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
            // Public home page. The AQOONHUB landing page is shown at the site root to every
            // visitor — anonymous and authenticated alike. No automatic redirect to Login or
            // Dashboard happens here; users reach protected areas only via the Sign In links.
            // (Authenticated users can still open their dashboard from those links; protected
            //  modules remain secured by the Web.config <location path="Modules"> rule.)
        }
    }
}