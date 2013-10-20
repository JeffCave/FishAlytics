using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Security;
using Mono.Data.Sqlite;

namespace Vius.Authentication
{
	public class Role:HashSet<string>,IDataElement<string>
	{
		private object localLocker = new object();
		private static object staticLocker = new object();
		private List<string> origCapabilities = new List<string>();

		private static Vius.Data.DataProvider Db {
			get {
				return Vius.Data.DataProvider.Instance;
			}
		}

		#region Config
		private bool immediatesave=false;
		public bool ImmediateSave {
			get {
				return immediatesave;
			}
			set {
				lock(localLocker){
					//if the value hasn't changed, don't do anything
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

		#region Initialization
		protected Role (string name)
		{
			Initialize(name);
		}

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

		private int? _id;
		private object idlocker = new object();
		public int Id {
			get{
				if(!_id.HasValue){
					lock(idlocker){
						if(!_id.HasValue){
							Db.UseCommand(cmd=>{
								cmd.CommandText = 
									"select roleid " +
									"from " + ROLE_TB_NAME + " " +
									"where rolename = :rolename";
								var p = cmd.CreateParameter();
								p.DbType = DbType.String;
								p.ParameterName = ":rolename";
								p.Value = this.Name;
								cmd.Parameters.Add(p);

								_id = (int)cmd.ExecuteScalar();

							});
						}
					}
				}
				return _id.Value;
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

		#endregion

		#region Operators And Casting
		public static Role operator +(Role role, string capability) 
		{
			role.Add(capability);
			return role;
		}

		public static Role operator - (Role role, string capability)
		{
			role.Remove(capability);
			return role;
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
			//is there actually something to do?
			if (base.Contains(capability)) {
				return;
			}

			//is it a valid thing to add?
			if (!Vius.Authentication.Capabilities.AllCapabilities.Contains(capability)) {
				throw new Exception("Invalid Capability");
			}

			//do the actual work
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
			//is there actually something to do?
			if (!base.Contains(capability)) {
				return true;
			}

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
				return !Roles.RoleExists(this.Name);
			}
		}

		public bool IsDirty {
			get {
				return !origCapabilities.Equals(this);
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
				if(!Vius.Authentication.Capabilities.Exists(capability)){
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

			//if there hasn't been a change, then don't do anything
			if (!IsDirty) {
				return;
			}

			if (!TableExists) {
				Create();
			}

			//first thing to do is delete all the items that have been removed
			var sqlIn = new System.Text.StringBuilder();
			foreach (var capability in this) {
				sqlIn.AppendFormat(",'{0}'", capability);
			}
			sql = sqlIn.ToString();
			Db.UseCommand((cmd) => {
				cmd.CommandText =
					"delete \n" +
					"from  " + CAPABILITY_TB_NAME + " \n" +
					"where RoleId = @role and \n" +
					"      capability not in (''" + sql + ") \n";

				var param = cmd.CreateParameter();
				param.DbType = DbType.Int32;
				param.ParameterName = "@role";
				param.Value = Id;

				cmd.Parameters.Add(param);
				cmd.ExecuteNonQuery();
			}
			);

			//then we insert all the items that have been added
			sqlIn = new StringBuilder(
				"insert into "+CAPABILITY_TB_NAME+"(RoleId,Capability) values \n");
			//create list of adds
			var InsertExists = false;
			foreach (var capability in this) {
				//don't include capabilities that were already in the list
				if (!origCapabilities.Contains(capability)) {
					sqlIn.AppendFormat("    ({0},'{1}'),\n", Id, capability);
					InsertExists = true;
				}
			}

			if (InsertExists) {
				sql = sqlIn.ToString();
				sql = sql.Substring(0, sql.Length - ",\n".Length);
				Db.UseCommand(cmd => {
					cmd.CommandText = sql;
					cmd.ExecuteNonQuery();
				}
				);
			}

			origCapabilities.Clear();
			origCapabilities.AddRange(this);
		}

		public void Create ()
		{
			if (TableExists) {
				return;
			}

			Db.UseCommand(cmd => {
				cmd.CommandText = 
					"create table " + CAPABILITY_TB_NAME + " ( \n" +
					"    RoleId int references(RoleId), \n" +
					"    Capability varchar(255), \n" +
					"    primary key (RoleId, Capability) \n" +
					") \n";
				cmd.ExecuteNonQuery();
			});

		}

		public void Delete ()
		{
			Roles.DeleteRole(this.name);

			//table doesn't exist? nothing to delete
			if (!TableExists) {
				return;
			}

			Db.UseCommand(cmd=>{
				cmd.CommandText = 
					"delete \n" +
					"from " + CAPABILITY_TB_NAME + " \n" +
					"where roleid not in (select roleid from roletable) \n";
				cmd.ExecuteNonQuery();
			});
		}

		public void Load ()
		{
			base.Clear();

			//no table? nothing to load
			if (!TableExists) {
				return;
			}

			Db.UseCommand(cmd=>{
				cmd.CommandText = 
					"select capability \n" +
					"from " + CAPABILITY_TB_NAME + " \n" +
					"where RoleId = :roleid \n";
				
				var param = cmd.CreateParameter();
				param.DbType = System.Data.DbType.Int32;
				param.ParameterName = ":roleid";
				param.Value = this.Id;
				cmd.Parameters.Add(param);

				if(cmd.Connection.State != System.Data.ConnectionState.Open){
					cmd.Connection.Open();
				}
				using(var rs = cmd.ExecuteReader()){
					while(rs.Read()){
						var capability = rs.GetString(0);
						if(!base.Contains(capability) && Capabilities.AllCapabilities.Contains(capability)){
							base.Add(capability);
						}
					}
					rs.Close();
				}
			});

			origCapabilities.Clear();
			origCapabilities.AddRange(this.ToArray());
		}

		public void Load (string role)
		{
			Name = role;
			Load();
		}
		/// <summary>
		/// Gets the users in role.
		/// </summary>
		/// <param name="roleName">Name of the role.</param>
		/// <returns>Returns the users in role.</returns>
		public static List<Role> GetRolesInCapability (string capability)
		{
			var roles = new List<Role>();

			if (!Vius.Authentication.Capabilities.Exists(capability)) {
				throw new ArgumentOutOfRangeException("capability", "Capability does not exist");
			}

			//no table? nothing to look up
			if (!TableExists) {
				return roles;
			}

			Db.UseCommand(cmd =>{
				cmd.CommandText = 
					"SELECT RoleId " +
					"FROM   " + ROLE_TB_NAME + " r " +
					"       INNER JOIN " + CAPABILITY_TB_NAME + " cr ON r.RoleId = cr.RoleId " +
					"WHERE  cr.Capability = :CapabilityName";
				var p = cmd.CreateParameter();
				p.DbType = DbType.String;
				p.ParameterName = ":CapabilityName";
				p.Value = capability;
				cmd.Parameters.Add(p);

				using (var dr = cmd.ExecuteReader()) {
					while (dr.Read()) {
						roles.Add(Role.GetRole(dr.GetString(0)));
					}
				}
			});

			return roles;
		}

		private static bool tableExists = false;
		private static bool TableExists {
			get {
				lock(staticLocker){
					if(!tableExists){
						tableExists = Db.TableExists(CAPABILITY_TB_NAME);
					}
				}
				return tableExists;
			}
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

		#endregion

		#region Private Fields

		private const string CAPABILITY_TB_NAME = "AuthRoleCapabilities";
		private const string ROLE_TB_NAME = "AuthRoles";

		#endregion


		#region Private Methods

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
			return Vius.Data.DataProvider.Instance.IsTransactionInProgress();
		}	
		#endregion

	}
}

