using System;
using System.Web.UI;

namespace Firehall
{
	public class Page:System.Web.UI.Page
	{	
		public string siteSection = "";
		public string sitePage = "";
		private string pSiteHTMLTitle = "";
		public string SitePageTitle = "";
		public string Permissions;

		/// ----------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the site html title.
		/// </summary>
		/// <value>
		/// The site html title.
		/// </value>
		public string SiteHtmlTitle {
			get {
				if (string.IsNullOrEmpty(pSiteHTMLTitle)) {
					lock (pSiteHTMLTitle) {
						if (string.IsNullOrEmpty(pSiteHTMLTitle)) {
							//TODO: set up a central configruation manager
							//pSiteHTMLTitle = Sys.Settings ["sys.name"] + " &gt; " + this.pSiteHTMLTitle;
						}
					}
				}
				return pSiteHTMLTitle;
			}
			set {
				pSiteHTMLTitle = value;
			}
		}

		public Page ()
		{
		}
	}
}

