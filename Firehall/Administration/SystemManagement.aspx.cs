using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Firehall.Administration
{
	public partial class SystemManagement : Firehall.Page
	{
		public void Page_Load ()
		{
			if (!IsPostBack) {
				SitemapReload.Click += HandleClick;
			}
		}

		void HandleClick (object s, EventArgs e)
		{
			var sender = s as LinkButton;
			switch (sender.ID) {
				case "SitemapReload":
					//find the provider
					foreach(var sitemap in Vius.Web.DbSiteMapProvider.Instances){
						try{sitemap.Expire();} catch {}
					}
					break;
			}
		}
	}
}

