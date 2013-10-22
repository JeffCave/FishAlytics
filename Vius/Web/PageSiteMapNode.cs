using System;

namespace Vius.Web
{
	public class PageSiteMapNode: Vius.Web.SiteMapNode
	{
		private object locker = new object();

		public PageSiteMapNode(PageSiteMapProvider provider, string key)
			:base(provider, key)
		{

		}

		public new PageSiteMapProvider Provider {
			get {
				return base.Provider as PageSiteMapProvider;
			}
		}

		public override System.Web.SiteMapNodeCollection ChildNodes {
			get {
				if(base.ChildNodes == null){
					lock(locker){
						base.ChildNodes = Provider.GetChildNodes(this);
					}
				}
				return base.ChildNodes;
			}
			set {
				base.ChildNodes = value;
			}
		}
	}


}

