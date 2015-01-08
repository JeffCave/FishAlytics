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
				return "/LoginGoogle.aspx";
			}
		}

		protected string LoginHtml{
			get{
				//Console.WriteLine(this.Page.User.Identity is FormsIdentity);
				if (Request.IsAuthenticated) {
					return "<a href='"+ LoginLink+"'>Logout</a>";
				}
				return "<a href='" + LoginLink + "'>Login with Google</a>";
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