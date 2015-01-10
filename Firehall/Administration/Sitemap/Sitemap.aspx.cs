using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Vius.Web;

namespace Firehall.Administration.Sitemap
{
	public partial class Sitemap : Firehall.Page
	{
		public void Page_Load ()
		{
			if (!IsPostBack) {
				BindAvailablePages();
			}
		}

		private void BindAvailablePages(){
			var allpages = Globals.Providers.SiteMapper;

			var providerAttributes = new System.Collections.Specialized.NameValueCollection();

			// Initialize the provider with a provider name and file name.
			allpages.Initialize("testProvider", providerAttributes);

			// Call the BuildSiteMap to load the site map information into memory.
			//allpages.BuildSiteMap();

			SiteMapDataSource data = new SiteMapDataSource();
			data.Provider = allpages;

			AvailablePages.ShowCheckBoxes = TreeNodeTypes.All;
			AvailablePages.ShowLines = true;
			AvailablePages.DataSource = data;
			AvailablePages.DataBind();
		}
	}
}

