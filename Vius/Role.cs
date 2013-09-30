using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Security;
using Mono.Data.Sqlite;

namespace Vius
{
	public class Role:List<string>,IDataElement<string>
	{

		private static object staticLocker = new object();

		#region Config
		private bool immediatesave;
		public bool ImmediateSave {
			get {
				return immediatesave;
			}
			set {
				//if the value hasn't changed, don't do anything
				if(value == immediatesave){
					return;
				}

				lock(CapabilityChanged){
					if(value == immediatesave){
						return;
					}

					immediatesave = value;
					if (immediatesave) {
						this.CapabilityChanged += ImmediateSaver;
					} else {
						this.CapabilityChanged -= ImmediateSaver;
					}
				}

			}
		}

		private static RoleManagerSection config = null;
		protected static RoleManagerSection Config {
			get {
				if(config == null){
					lock(staticLocker){
						if(config == null){
							// Initialize values from web.config.
							object tmp = ConfigurationManager.GetSection("system.web/roleManager");
							//object tmp = ConfigurationManager.GetSection("roleManager");
							config = tmp as RoleManagerSection;
							if (config == null) {
								throw new Exception("Error retrieving Role Configuration");
							}
						}
					}
				}
				return config;
			}
		}
		#endregion

		#region Roles
		private string name;
		public string Name {
			get {
				return name;
			}
			protected set {
				name = value;
			}
		}

		public static implicit operator string (Role role)  
		{ 
			return role.Name;
		}

		public static Role GetRole (string name)
		{
			return new Role(name);
		}

		public Role (string name)
		{
			Initialize(name);
			Load(name);
		}

		private static string cnnstr = null;
		private static string CnnStr {
			get {
				if(cnnstr == null){
					// Initialize SqliteConnection.
					var tmpProvider = Config.Providers[Config.DefaultProvider];
					ProviderSettings provider = tmpProvider as ProviderSettings;
					ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[provider.Parameters["connectionStringName"]];

					if (connectionStringSettings==null || string.IsNullOrEmpty(connectionStringSettings.ConnectionString)) {
						throw new Exception("Connection string is empty for SqliteRoleProvider. Check the web configuration file (web.config).");
					}

					cnnstr = connectionStringSettings.ConnectionString;

				}
				return cnnstr;
			}
		}
		#endregion


		#region ICollection

		public event EventHandler<EventArgs> CapabilityChanged;

		private void ImmediateSaver (object owner, EventArgs args)
		{
			this.Save();
		}

		public new void Add (string capability)
		{
			if (Capabilities.AllCapabilities.Contains(capability)) {
				throw new Exception("Invalid Capability");
			}
			base.Add(capability);
			CapabilityChanged(this,new EventArgs());
		}

		public new void Clear ()
		{
			base.Clear();
			CapabilityChanged(this,new EventArgs());
		}

		public new bool Remove (string capability)
		{
			var rtn = base.Remove(capability);
			CapabilityChanged(this,new EventArgs());
			return rtn;
		}
		#endregion

		#region IDataElement
		public bool IsValid {
			get {
				try{
					Validate();
					return true;
				} catch {
					return false;
				}
			}
		}

		public bool IsNew {
			get {
				return Roles.RoleExists(this.Name);
			}
		}

		public bool IsDirty {
			get {
				throw new System.NotImplementedException();
			}
		}

		public void Dispose ()
		{
		}

		public void Validate ()
		{
			if (!Roles.RoleExists(this.Name)) {
				throw new Exception("Invalid role ("+this.Name+") specified");
			}
			foreach (var capability in this) {
				if(!Capabilities.Exists(capability)){
					throw new Exception("Capability ("+capability+") does not exist");
				}
			}
		}

		public void Save ()
		{
			string sql = "";
			if (IsNew) {
				Roles.CreateRole(this.Name);
			}

			//first thing to do is delete all the items that have been removed
			var sqlIn = new System.Text.StringBuilder();
			foreach (var capability in this) {
				sqlIn.AppendFormat(",'{0}'",capability);
			}
			sql = sqlIn.ToString().Substring(1);
			ExecDataCmd((cmd)=>{
				cmd.CommandText =
					 "delete \n" +
					 "from  tAuthRoleCapabilities \n" +
					 "where role = @role and \n" +
					 "      capability not in ("+sql+") \n";

				var param = cmd.CreateParameter();
				param.DbType = System.Data.DbType.AnsiString;
				param.ParameterName = "@role";
				param.Value = Name;

				cmd.Parameters.Add(param);
				cmd.ExecuteNonQuery();
			});

			//then we insert all the items that have been added
			sqlIn = new StringBuilder(
				"insert into tAuthRoleCapabilities ");
			foreach (var capability in this) {
				sqlIn.AppendFormat("values('{0}','{0}')",Name,capability);
			}
			ExecDataCmd((cmd)=>{
				cmd.CommandText = sqlIn.ToString();
				cmd.ExecuteNonQuery();
			});

		}

		public void Create ()
		{
			ExecDataCmd(cmd=>{
				cmd.CommandText = 
					"create table tAuthRoleCapabilities (RoleId varchar(255), Capability varchar(255))";
				cmd.ExecuteNonQuery();
			});
		}

		public void Delete(){
			Roles.DeleteRole(this.name);
			ExecDataCmd(cmd=>{
				cmd.CommandText = 
					"delete " +
					"from tAuthRoleCapabilities " +
					"where role not in (select roles from roletable)";
				cmd.ExecuteNonQuery();
			});
		}

		public void Load(){
			base.Clear();
			ExecDataCmd(cmd=>{
				cmd.CommandText = 
					"select capability " +
					"from tAuthRoleCapabilities " +
					"where role = <?role?>";
				
				var param = cmd.CreateParameter();
				param.DbType = System.Data.DbType.AnsiString;
				param.ParameterName = "<?role?>";
				param.Value = Name;

				using(var rs = cmd.ExecuteReader()){
					while(rs.Read()){
						base.Add(rs.GetString(0));
					}
					rs.Close();
				}
			});
		}

		public void Load (string role)
		{
			Name = role;
			Load();
		}
		#endregion

		#region Statics
		/// <summary>
		/// Adds the specified user names to the specified roles for the configured applicationName.
		/// </summary>
		/// <param name="usernames">A string array of user names to be added to the specified roles.</param>
		/// <param name="roleNames">A string array of the role names to add the specified user names to.</param>
		public static void AddRoleToCapability (string[] capabilities, string[] roleNames)
		{
			foreach (string roleName in roleNames) {
				if (Roles.RoleExists(roleName)) {
					var role = Role.GetRole(roleName);

					//turn off "immediate" saving for this batch process
					bool immediatestate = role.ImmediateSave;
					role.ImmediateSave = false;

					//add each capability to the given role
					foreach (string capability in capabilities) {
						if(Capabilities.Exists(capability)){
							role.Add(capability);
						}
					}
					role.Save();

					//set "immeidate" back to whatever it was before
					role.ImmediateSave = immediatestate;
				}
			}
		}
		public static void AddRoleToCapability (string[] capabilities, string roleName){
			string[] roles = new string[]{roleName};
			AddRoleToCapability(capabilities, roles);
		}
		public static void AddRoleToCapability (string capability, string[] roleNames){
			string[] capabilities = new string[]{capability};
			AddRoleToCapability(capabilities, roleNames);
		}
		public static void AddRoleToCapability (string capability, string roleName){
			string[] roles = new string[]{roleName};
			string[] capabilities = new string[]{capability};
			AddRoleToCapability(capabilities, roles);
		}

		private delegate void DataCmd(System.Data.Common.DbCommand cmd);
		private static void ExecDataCmd (DataCmd cmdfunc)
		{
			using (DbConnection cnn = GetDbConnectionForCapabilities()) {
				using(DbCommand cmd= cnn.CreateCommand()){
					if(cnn.State == System.Data.ConnectionState.Closed){
						cnn.Open();
					}
					cmdfunc(cmd);
					cnn.Close();
				}
			}
		}

		#endregion

		#region Private Fields

		private const string HTTP_TRANSACTION_ID = "SqliteTran";
		private const string APP_TB_NAME = "[aspnet_Applications]";
		private const string CAPABILITY_TB_NAME = "[aspnet_RoleCapabilities]";
		private const string ROLE_TB_NAME = "[aspnet_Roles]";

		#endregion

		#region Public Methods

		/// <summary>
		/// Initialize the specified name and config.
		/// </summary>
		/// <param name='name'>
		/// Name.
		/// </param>
		/// <param name='config'>
		/// Config.
		/// </param>
		public void Initialize (string name)
		{
			if (!Roles.RoleExists(name)) {
				throw new ArgumentOutOfRangeException("Role does not exist");
			}

			this.Name = name;

			ImmediateSave = true;
			Load();
		}

		/// <summary>
		/// Gets the users in role.
		/// </summary>
		/// <param name="roleName">Name of the role.</param>
		/// <returns>Returns the users in role.</returns>
		public List<Role> GetRolesInCapability (string capability)
		{
			var roles = new List<Role>();

			if (!Capabilities.Exists(capability)) {
				throw new ArgumentOutOfRangeException("capability","Capability does not exist");
			}

			SqliteConnection cn = GetDbConnectionForCapabilities();
			try {
				using (SqliteCommand cmd = cn.CreateCommand()) {
					cmd.CommandText = 
						"SELECT RoleId " +
						"FROM   " + ROLE_TB_NAME + " r " +
						"       INNER JOIN " + CAPABILITY_TB_NAME + " cr ON r.RoleId = cr.RoleId " +
						"WHERE  cr.Capability = $CapabilityName";
					cmd.Parameters.AddWithValue("$CapabilityName", capability);

					if (cn.State == ConnectionState.Closed){
						cn.Open();
					}

					using (SqliteDataReader dr = cmd.ExecuteReader()) {
						while (dr.Read()) {
							roles.Add(Role.GetRole(dr.GetString(0)));
						}
					}
				}
			} finally {
				if (!IsTransactionInProgress())
					cn.Dispose();
			}

			return roles;
		}
		#endregion

		#region Private Methods

		/// <summary>
		/// Get a reference to the database connection used for Role. If a transaction is currently in progress, and the
		/// connection string of the transaction connection is the same as the connection string for the Role provider,
		/// then the connection associated with the transaction is returned, and it will already be open. If no transaction is in progress,
		/// a new <see cref="SqliteConnection"/> is created and returned. It will be closed and must be opened by the caller
		/// before using.
		/// </summary>
		/// <returns>A <see cref="SqliteConnection"/> object.</returns>
		/// <remarks>The transaction is stored in <see cref="System.Web.HttpContext.Current"/>. That means transaction support is limited
		/// to web applications. For other types of applications, there is no transaction support unless this code is modified.</remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		protected static SqliteConnection GetDbConnectionForCapabilities ()
		{
			// Look in the HTTP context bag for a previously created connection and transaction. Return if found and its connection
			// string matches that of the Role connection string; otherwise return a fresh connection.
			if (System.Web.HttpContext.Current != null) {
				const string HTTP_TRANSACTION_ID = "SqliteTran";
				SqliteTransaction tran = (SqliteTransaction)System.Web.HttpContext.Current.Items[HTTP_TRANSACTION_ID];
				if ((tran != null) && (String.Equals(tran.Connection.ConnectionString, CnnStr)))
					return tran.Connection;
			}

			return new SqliteConnection(CnnStr);
		}

		/// <summary>
		/// Determines whether a database transaction is in progress for the Role provider.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if a database transaction is in progress; otherwise, <c>false</c>.
		/// </returns>
		/// <remarks>A transaction is considered in progress if an instance of <see cref="SqliteTransaction"/> is found in the
		/// <see cref="System.Web.HttpContext.Current"/> Items property and its connection string is equal to the Role 
		/// provider's connection string. Note that this implementation of <see cref="SqliteRoleProvider"/> never adds a 
		/// <see cref="SqliteTransaction"/> to <see cref="System.Web.HttpContext.Current"/>, but it is possible that 
		/// another data provider in this application does. This may be because other data is also stored in this Sqlite database,
		/// and the application author wants to provide transaction support across the individual providers. If an instance of
		/// <see cref="System.Web.HttpContext.Current"/> does not exist (for example, if the calling application is not a web application),
		/// this method always returns false.</remarks>
		private static bool IsTransactionInProgress()
		{
			if (System.Web.HttpContext.Current == null)
				return false;

			SqliteTransaction tran = (SqliteTransaction)System.Web.HttpContext.Current.Items[HTTP_TRANSACTION_ID];

			if ((tran != null) && (String.Equals(tran.Connection.ConnectionString, CnnStr)))
				return true;
			else
				return false;
		}	
		#endregion

	}
}

