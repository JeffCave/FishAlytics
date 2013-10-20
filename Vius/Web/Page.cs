using System;
using System.Web.UI;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Vius.Web
{
	/// <summary>
	/// Page.
	/// </summary>
	/// <remarks>
	/// $Id$
	/// $URL$
	/// </remarks>
	public abstract class Page:System.Web.UI.Page
	{
		private string pSitePageTitle = "";
		private string pSiteHTMLTitle = "";
		private ReadOnlyCollection<string> pRequirements = new ReadOnlyCollection<string>(new List<string>());

		private static object staticlocker = new object();

		private static SiteMapNode menuinformation=null;
		public static SiteMapNode MenuInformation {
			get {
				lock(staticlocker){
					if(menuinformation == null){
						menuinformation = CreateSiteMapNode();
					}
					return menuinformation;
				}
			}
		}
		protected static SiteMapNode CreateSiteMapNode(){
			return null;
		}

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
							pSiteHTMLTitle = PageTitle;
						}
					}
				}
				return pSiteHTMLTitle;
			}
			set {
				pSiteHTMLTitle = value;
			}
		}

		/// <summary>
		/// Gets or sets the name of the page.
		/// </summary>
		/// <value>
		/// The name of the page.
		/// </value>
		public string PageTitle {
			get {
				lock(pSitePageTitle){
					if(string.IsNullOrEmpty(pSitePageTitle)){
						pSitePageTitle = this.GetType().Name;
					}
					return pSitePageTitle;
				}
			}
			set {
				pSitePageTitle = value;
			}
		}

		/// <summary>
		/// List of Capabilities required to view the page
		/// </summary>
		/// <value>
		/// The required capabilities.
		/// </value>
		public ReadOnlyCollection<string> Requirements {
			get {
				return pRequirements;
			}
			protected set {
				if(value == null){
					return;
				}
				pRequirements = value;
			}
		}

	}
}

