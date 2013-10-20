using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

using Vius.Data;

namespace Vius.Web
{
	/// <summary>
	/// Site map provider.
	/// </summary>
	/// <remarks>
	/// $Id$
	/// $URL$
	/// </remarks>
	public class DbSiteMapNode: Vius.Web.SiteMapNode
	{
		internal long? ParentKey = null;

		public DbSiteMapNode(DbSiteMapProvider provider, string key)
			:base(provider, key)
		{

		}

		public override SiteMapNodeCollection ChildNodes {
			get {
				return base.ChildNodes;
			}
			set {
				base.ChildNodes = value;
			}
		}

		public override System.Web.SiteMapNode ParentNode {
			get {
				return base.ParentNode;
			}
			set {
				base.ParentNode = value;
			}
		}

	}
}

