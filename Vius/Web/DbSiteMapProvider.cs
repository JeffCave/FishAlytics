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

		/// <summary>
		/// Gets or sets the expiry rate.
		/// </summary>
		/// <value>The expiry rate.</value>
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

		/// <summary>
		/// Gets the db.
		/// </summary>
		/// <value>The db.</value>
		private DataProvider Db {
			get {
				return DataProvider.Instance;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Vius.Web.DbSiteMapProvider"/> class.
		/// </summary>
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

		/// <Docs>To be added.</Docs>
		/// <remarks>To be added.</remarks>
		/// <summary>
		/// Builds the site map.
		/// </summary>
		/// <returns>The site map.</returns>
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

		/// <summary>
		/// Loads the site map nodes.
		/// </summary>
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

		/// <summary>
		/// Gets a value indicating whether this <see cref="Vius.Web.DbSiteMapProvider"/> table exists.
		/// </summary>
		/// <value><c>true</c> if table exists; otherwise, <c>false</c>.</value>
		protected bool TableExists {
			get {
				if(!tableexistschecked){
					tableexists = Db.TableExists(TbName) && Db.TableExists(TbChild);
					tableexistschecked = true;
				}
				return tableexists;
			}
		}
		private bool tableexists = false;
		private bool tableexistschecked = false;

		/// <summary>
		/// Loads the node.
		/// </summary>
		/// <returns>The node.</returns>
		/// <param name="rec">Rec.</param>
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
						bool nsMore = ns.MoveNext(); //move to the first item
						bool rsMore = rs.Read(); //move to the first item
						//check to see if either list is finished, then we are done
						while(nsMore && rsMore){
							int rsId = (int)rs["PageId"];
							int nodeId = int.Parse(ns.Current.Key);
							//if we have a match, use it
							if(nodeId == rsId){
								ns.Current.Capabilities.Add(rs["Capability"].ToString());
							}
							//increment the item in the list that is further behind
							while(nodeId >= rsId){
								rsMore = rs.Read();
							} 
							while(nodeId < rsId){
								nsMore = ns.MoveNext();
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

