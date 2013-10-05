using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

using Vius.Data;

namespace Vius.Web
{
	public class DbSiteMapNode: Vius.Web.SiteMapNode
	{
		internal int? ParentKey = null;

		internal DbSiteMapNode (DbSiteMapProvider provider, IDataRecord rec)
			:base(provider,"")
		{
			Load(rec);
		}

		public DbSiteMapNode(DbSiteMapProvider provider, string key)
			:base(provider, key)
		{

		}

		private string key = "";
		public new string Key {
			get {
				if(string.IsNullOrEmpty(key)){
					key = base.Key;
				}
				return key.ToString();
			}
			protected set {
				key = value;
			}
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

		internal void Load (System.Data.IDataRecord rec)
		{
			for (int fld=0; fld<rec.FieldCount; fld++) {
				switch(rec.GetName(fld).ToLower()){
					case "PageId":
						this.Key = rec.GetInt16(fld).ToString();
						break;
					case "MenuLabel":
						this.Title = rec.GetString(fld);
						break;
					case "Url":
						this.Url = rec.GetString(fld);
						break;
					case "Capabilities":

						break;
					case "Parent":
						ParentKey = rec.GetInt16(fld);
						break;
					case "Desc":
						this.Description = rec.GetString(fld);
						break;
				}
			}
		}

	}

	/// <summary>
	/// Site map provider.
	/// </summary>
	/// <remarks>
	/// $Id$
	/// $URL$
	/// </remarks>
	public class DbSiteMapProvider:System.Web.SiteMapProvider
	{
		private SiteMapNode root = null;
		private object locker = new object();

		private const string TbName = "tWebPages";
		private DataProvider Db {
			get {
				return DataProvider.Instance;
			}
		}

		public DbSiteMapProvider ()
		{
		}

		private List<DbSiteMapNode> allnodes = null;
		private object allnodeslocker = new object();
		internal List<DbSiteMapNode> AllNodes {
			get {
				if(allnodes == null){
					lock (allnodeslocker) {
						if (allnodes == null) {
							allnodes = new List<DbSiteMapNode>();
							Db.UseCommand(cmd => {
								cmd.CommandText = "select * from " + TbName + "";
								var rs = cmd.ExecuteReader();
								while (rs.Read()) {
									allnodes.Add(new DbSiteMapNode(this, rs));
								}
							}
							);

						}
					}
				}
				return allnodes;
			}
		}

		/// <summary>
		/// Finds the site map node.
		/// </summary>
		/// <returns>
		/// The site map node.
		/// </returns>
		/// <param name='rawUrl'>
		/// Raw URL.
		/// </param>
		public override System.Web.SiteMapNode FindSiteMapNode (string rawUrl)
		{
			foreach (var node in AllNodes) {
				if(node.Url == rawUrl){
					return node;
				}
			}
			return null;
		}

		/// <Docs>
		/// To be added.
		/// </Docs>
		/// <returns>
		/// To be added.
		/// </returns>
		/// <since version='.NET 2.0'>
		/// 
		/// </since>
		/// <summary>
		/// Gets the child nodes.
		/// </summary>
		/// <param name='node'>
		/// Node.
		/// </param>
		public override System.Web.SiteMapNodeCollection GetChildNodes (System.Web.SiteMapNode node)
		{
			var rtn = new SiteMapNodeCollection();
			var children = AllNodes.Where(child => { return child.ParentKey.ToString() == node.Key;});
			rtn.AddRange(children.ToArray());
			return rtn;
		}

		/// <Docs>
		/// To be added.
		/// </Docs>
		/// <returns>
		/// To be added.
		/// </returns>
		/// <since version='.NET 2.0'>
		/// 
		/// </since>
		/// <summary>
		/// Gets the parent node.
		/// </summary>
		/// <param name='node'>
		/// Node.
		/// </param>
		public override System.Web.SiteMapNode GetParentNode (System.Web.SiteMapNode node)
		{
			var pagenode = node as DbSiteMapNode;
			if (pagenode != null) {
				foreach(var it in AllNodes){
					if(it.Key == pagenode.ParentKey.ToString()){
						return it;
					}
				}
			}
			return null;
		}


		/// <summary>
		/// Gets the root node core.
		/// </summary>
		/// <returns>
		/// The root node core.
		/// </returns>
		protected override System.Web.SiteMapNode GetRootNodeCore ()
		{
			lock (locker) {
				if(root == null){
					root = new DbSiteMapNode(this,"");
				}
				return root;
			}
		}

		/// <summary>
		/// Create this instance.
		/// </summary>
		protected void Create ()
		{
			List<string> cmds = new List<string>{
				"create table tWebPages( \n" +
				"    PageId, \n" +
				"    MenuLabel varchar(20), \n" +
				"    Url varchar(255), \n" +
				"    Parent, \n" +
				"    Desc, \n" +
				"    Meta \n" +
				")\n"
				,
				"create table tWebPageCapabilities( \n" +
				"    PageId, \n" +
				"    Capability \n" +
				")\n"
				,
				"alter table tWebPageCapabilities add constraint (PageId) foreign key tWebPageCapabilities(PageId)"
			};
			Db.ExecuteCommand(cmds);
		}

		/// <summary>
		/// Adds the node.
		/// </summary>
		/// <param name='node'>
		/// Node.
		/// </param>
		protected override void AddNode (System.Web.SiteMapNode node)
		{
			DbSiteMapNode newnode = node as DbSiteMapNode;

			if (AllNodes.Contains(newnode)) {
				return;
			}
			AllNodes.Add(newnode);
			base.AddNode(node);
		}

	}
}

