using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

		private const string TbName = "WebPages";
		private const string TbChild = TbName + "Capabilities";

		private TimeSpan pExpiryRate = new TimeSpan(0,11,30,41,56);
		private DateTime NextLoad = DateTime.MinValue;

		private static readonly List<DbSiteMapProvider> instances = new List<DbSiteMapProvider>();
		public static ReadOnlyCollection<DbSiteMapProvider> Instances {
			get {
				return instances.AsReadOnly();
			}
		}

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

		public DbSiteMapProvider ():
			base()
		{
			instances.Add(this);
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
				"create table if not exists " + TbName + "( \n" +
				"    PageId bigserial, \n" +
				"    MenuLabel varchar(20) not null, \n" +
				"    Url varchar(255) not null, \n" +
				"    Parent bigint, \n" +
				"    DescLabel text, \n" +
				"    Meta text, \n " +
				"    MenuOrder smallint, \n" +
				"    primary key(PageId), \n" + 
				"    foreign key(Parent) references " + TbName + "(PageId) \n" +
				")\n"
				,
				//create the permissions table
				"create table if not exists " + TbChild + "( \n" +
				"    PageId bigint, \n" +
				"    Capability varchar(20), \n" +
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

			//load all of the map nodes from the database
			var allnodes = new List<DbSiteMapNode>();
			Db.UseCommand(cmd => {
				cmd.CommandText = 
					"select * \n" +
					"from   " + TbName + " \n" +
					"order by parent, \n" +
					"         menuorder \n";
				using (var rs = cmd.ExecuteReader()) {
					while (rs.Read()) {
						allnodes.Add(LoadNode(rs));
					}
					rs.Close();
				}
			});

			//for each node
			foreach (var node in allnodes) {
				//link up its parent node
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
				//add the node with its parent
				AddNode(node,parent);
			}

			LoadNodeCapabilities(allnodes);
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
				switch(rec.GetName(fld).ToLower()){
					case "menulabel":
						node.Title = rec.GetString(fld);
						break;
					case "url":
						node.Url = rec.GetString(fld);
						break;
					case "parent":
						node.ParentKey = rec.GetInt64(fld);
						break;
					case "desclabel":
						node.Description = rec.GetString(fld);
						break;
				}
			}

			return node;
		}

		private void LoadNodeCapabilities (List<DbSiteMapNode> nodes)
		{
			Db.UseCommand(cmd => {
				cmd.CommandText = 
					"select * \n" +
					"from   " + TbChild + " \n" +
					"order by PageId \n";
				using (var rs = cmd.ExecuteReader()) {
					using(var ns = nodes.OrderBy(o=>{return int.Parse(o.Key);}).GetEnumerator()){
						//initialize the loop
						bool nsFin = ns.MoveNext(); //move to the first item
						bool rsFin = rs.Read(); //move to the first item
						//check to see if either list is finished, then we are done
						while(!nsFin && !rsFin){
							int rsId = (int)rs["PageId"];
							int nodeId = int.Parse(ns.Current.Key);
							//if we have a match, use it
							if(nodeId == rsId){
								ns.Current.Capabilities.Add(rs["Capability"].ToString());
							}
							//increment the item in the list that is further behind
							if(nodeId >= rsId){
								rsFin = rs.Read();
							} else if(nodeId < rsId){
								nsFin = ns.MoveNext();
							}
						}
					}
					rs.Close();
				}
			});

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

		private bool isExpiring = false;
		private static readonly TimeSpan MinWaitExpire = new TimeSpan(0,0,30);
		public void Expire ()
		{
			var now = DateTime.Now;
			// if it was too recent, throw an error
			if (now < NextLoad.Subtract(ExpiryRate).Add(MinWaitExpire)) {
				throw new Exception("Last Expiry was too recent. Must wait at least 1 minute");
			}
			// if someone else is already doing the job, we can stop
			if (!isExpiring) {
				// lock it
				lock (locker) {
					// announce that we are doing the job
					isExpiring = true;

					// if there was a race, and this routine was waiting on 
					// a lock to get here, another routine may have already
					// done the job. We check again. This time, we don't
					// trhow an error, becuase the job would have been done
					// since this function was called
					if (now < NextLoad.Subtract(ExpiryRate).Add(MinWaitExpire)) {
						return;
					}

					//actually do the work
					var rate = ExpiryRate;
					ExpiryRate = new TimeSpan(0);
					System.Threading.Thread.Sleep(1);
					CheckDataFreshness();
					ExpiryRate = rate;

					//release the locking
					isExpiring = false;
				}
			}


		}

	}
}

