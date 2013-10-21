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
			// Create an instance of the XmlSiteMapProvider class.
			var allpages = new PageSiteMapProvider();

			var providerAttributes = new System.Collections.Specialized.NameValueCollection();

			// Initialize the provider with a provider name and file name.
			allpages.Initialize("testProvider", providerAttributes);

			// Call the BuildSiteMap to load the site map information into memory.
			//allpages.BuildSiteMap();

			SiteMapDataSource data = new SiteMapDataSource();
			data.Provider = allpages;

			AvailablePages.DataSource = data;
			AvailablePages.DataBind();
		}
	}
}

