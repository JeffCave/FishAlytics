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
	public class DbSiteMapProvider:System.Web.StaticSiteMapProvider
	{
		private System.Web.SiteMapNode root = null;
		private object locker = new object();

		private const string TbName = "tWebPages";
		private const string TbChild = TbName + "Capabilities";

		private TimeSpan pExpiryRate = new TimeSpan(0,11,30,41,56);
		private DateTime NextLoad = DateTime.MinValue;

		public TimeSpan ExpiryRate {
			get {
				return pExpiryRate;
			}
			set {
				lock(locker){
					if(value < TimeSpan.Zero){
						return;
					}
					try{
						NextLoad = NextLoad.Subtract(pExpiryRate);
						NextLoad = NextLoad.Add(value);
					} catch {
						NextLoad = DateTime.MinValue;
					}
					pExpiryRate = value;
					CheckDataFreshness();
				}
			}
		}
		
		private DataProvider Db {
			get {
				return DataProvider.Instance;
			}
		}

		public DbSiteMapProvider ()
		{
		}

		/// <summary>
		/// Gets the root node core.
		/// </summary>
		/// <returns>
		/// The root node core.
		/// </returns>
		protected override System.Web.SiteMapNode GetRootNodeCore ()
		{
			return BuildSiteMap();
		}

		/// <summary>
		/// Create this instance.
		/// </summary>
		protected void CreateTable ()
		{
			//if the table already exists, we are done
			if (TableExists) {
				return;
			}

			List<string> cmds = new List<string>{
				//create the pages table
				"if not exists \n" +
				"create table " + TbName + "( \n" +
				"    PageId integer primary key autoincrement, \n" +
				"    MenuLabel varchar(20), \n" +
				"    Url varchar(255), \n" +
				"    Parent, \n" +
				"    Desc, \n" +
				"    Meta, \n " + 
				"    foreign key(Parent) references " + TbName + "(PageId) \n" +
				")\n"
				,
				//create the permissions table
				"if not exists \n" +
				"create table " + TbChild + "( \n" +
				"    PageId, \n" +
				"    Capability, \n" +
				"    primary key(PageId, Capability), \n" +
				"    foreign key(PageId) references " + TbName + "(PageId) \n" +
				")\n"
			};
			Db.ExecuteCommand(cmds);
		}

		public override System.Web.SiteMapNode BuildSiteMap ()
		{
			CheckDataFreshness();
			// Use a lock to provide thread safety
			if(root == null){
				lock (locker) {
					if (root == null) {
						NextLoad = DateTime.Now.Add(ExpiryRate);
						base.Clear();

						root = new DbSiteMapNode(this, "");
						root.Url = "~/default.aspx";
						root.Title = "Home";
						AddNode(root);

						LoadSiteMapNodes();
					}
				}
			}
			return root;
		}

        private void LoadSiteMapNodes ()
		{
			CreateTable();

			var allnodes = new List<DbSiteMapNode>();

			Db.UseCommand(cmd => {
				cmd.CommandText = "select * from " + TbName;
				using (var rs = cmd.ExecuteReader()) {
					while (rs.Read()) {
						allnodes.Add(LoadNode(rs));
					}
					rs.Close();
				}
			}
			);

			foreach (var node in allnodes) {
				System.Web.SiteMapNode parent = null;
				foreach(var n in allnodes){
					if(n.Key == node.ParentKey.ToString()){
						parent = n;
						break;
					}
				}
				if(parent == null){
					parent = root;
				}
				AddNode(node,parent);
			}
		}

		private bool? tableexists = null;
		protected bool TableExists {
			get {
				if(tableexists == null){
					tableexists = Db.TableExists(TbName);
				}
				return tableexists.Value;
			}
		}

		private DbSiteMapNode LoadNode(System.Data.IDataRecord rec)
		{
			string key = rec["PageId"].ToString();
			DbSiteMapNode node = new DbSiteMapNode(this,key);

			for (int fld=0; fld<rec.FieldCount; fld++) {
				if(rec.IsDBNull(fld)){
					continue;
				}
				switch(rec.GetName(fld)){
					case "MenuLabel":
						node.Title = rec.GetString(fld);
						break;
					case "Url":
						node.Url = rec.GetString(fld);
						break;
					case "Capabilities":

						break;
					case "Parent":
						node.ParentKey = rec.GetInt32(fld);
						break;
					case "Desc":
						node.Description = rec.GetString(fld);
						break;
				}
			}

			return node;
		}

		private void CheckDataFreshness ()
		{
			if (DateTime.Now > NextLoad) {
				lock(locker){
					root = null;
					base.Clear();
				}
			}
		}

	}
}

