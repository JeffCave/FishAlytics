using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Vius.Web;

namespace Firehall.Administration.Sitemap
{
	public partial class Sitemap : Firehall.Page
	{
		public PageSiteMapNode RootNode = (PageSiteMapNode)Globals.Providers.SiteMapper.RootNode;

		public void Page_Load ()
		{
			//if (!IsPostBack) {
			//	BindAvailablePages();
			//}
		}

//		private void BindAvailablePages(){
//			var allpages = Globals.Providers.SiteMapper;
//
//			var providerAttributes = new System.Collections.Specialized.NameValueCollection();
//
//			// Initialize the provider with a provider name and file name.
//			allpages.Initialize("testProvider", providerAttributes);
//
//			// Call the BuildSiteMap to load the site map information into memory.
//			//allpages.BuildSiteMap();
//
//			SiteMapDataSource data = new SiteMapDataSource();
//			data.Provider = allpages;
//
//			AvailablePages.ShowCheckBoxes = TreeNodeTypes.All;
//			AvailablePages.ShowLines = true;
//			AvailablePages.DataSource = data;
//			AvailablePages.DataBind();
//		}

		public void RenderTableRows(){
			RenderTableRows(this.RootNode);
		}

		protected void RenderTableRows(Vius.Web.SiteMapNode node){
			var tmpl = 
				"<tr id='{Key}' data-parentid='{ParentKey}'>" +
				" <td><span>{Expander}{Icon}<input type='checkbox' name='page' value='{Key}' />{Name}</span></td>" +
				" <td>{Label}</td>" +
				" <td>{URL}</td>" +
				" <td>{Order}</td>" +
				"</tr>";
			tmpl = tmpl
					.Replace("{Key}", node.Key)
					.Replace("{ParentKey}", node.ParentNode.Key)
				.Replace("{Label}", node.Title)
				;
			Response.Write(tmpl);
			foreach (var child in node.ChildNodes) {
				if (child is Vius.Web.SiteMapNode) {
					RenderTableRows((Vius.Web.SiteMapNode)child);
				} else {
					Console.Error.WriteLine("Invalid Site node type (" + node.Key + ")");
				}
			}
		}
	}
}

