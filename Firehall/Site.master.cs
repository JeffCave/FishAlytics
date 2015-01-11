using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace Firehall{

	public partial class Site : System.Web.UI.MasterPage
	{
		public string Title {
			get {
				return this.BrowserTitle.Text;
			}
			set {
				//cast it to a string
				string val = value;

				//make sure it isn't empty
				val = val.Trim();
				if(string.IsNullOrEmpty(val)){
					return;
				}

				//apply to various portions of the page
				this.BrowserTitle.Text = val;
				this.SiteHtmlTitle.Text = Server.HtmlEncode(val);
			}
		}

		protected string LoginLink{
			get{
				if (Request.IsAuthenticated) {
					return "/logout.aspx";
				}
				return "/login.aspx?authority=google";
			}
		}

		protected string LoginHtml{
			get{
				string template = "<a href='{0}'><img  style='width: 120px; height: 40px;' src='https://developers.google.com/+/images/branding/sign-in-buttons/Red-signin_Medium_base_44dp.png' /></a>";
				if (Request.IsAuthenticated) {
					template = "Logged in as: {1} | <a href='{0}'>Logout</a>";
				}

				string name;
				try{
					name = this.Context.User.Identity.Name;
				}catch{
					name = "";
				}

				template = String.Format(template
						,LoginLink
						,name
					);
				return template;
			}
		}

    	protected void Page_Load (object sender, EventArgs e)
		{
			//make sure the values agree
			Title = Title;
			//default value for current page is whatever is in the sitemap
			try {
				if (SiteMap.CurrentNode == null) {
					PageHtmlTitle.Text = MainContent
							.Page
							.GetType ()
							.Name
							.Replace ("_aspx", "");
				} else {
					PageHtmlTitle.Text = SiteMap.CurrentNode.Title;
				}
			} catch {
				PageHtmlTitle.Text = "";
			}
		}
	}
}