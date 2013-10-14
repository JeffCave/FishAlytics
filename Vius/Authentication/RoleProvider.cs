using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.Common;
using System.Web.Security;

namespace Vius.Authentication
{
	/// <summary>
	/// Provides a Role implementation whose data is stored in a Sqlite database.
	/// </summary>
	public sealed class SqliteRoleProvider : RoleProvider
	{
		#region Private Fields

		private static readonly Vius.Data.DataProvider Db = Vius.Data.DataProvider.Instance;

		private const string HTTP_TRANSACTION_ID = "SqliteTran";
		private const string APP_TB_NAME = "[aspnet_Applications]";
		private const string ROLE_TB_NAME = "[aspnet_Roles]";
		private const string USER_TB_NAME = "[aspnet_Users]";
		private const string USERS_IN_ROLES_TB_NAME = "[aspnet_UsersInRoles]";
		private const int MAX_USERNAME_LENGTH = 256;
		private const int MAX_ROLENAME_LENGTH = 256;
		private const int MAX_APPLICATION_NAME_LENGTH = 256;

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the name of the application to store and retrieve role information for.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// The name of the application to store and retrieve role information for.
		/// </returns>
		public override string ApplicationName {
			get { return "Vius"; }
			set {}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Initializes the provider.
		/// </summary>
		/// <param name="name">The friendly name of the provider.</param>
		/// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// The name of the provider is null.
		/// </exception>
		/// <exception cref="T:System.ArgumentException">
		/// The name of the provider has a length of zero.
		/// </exception>
		/// <exception cref="T:System.InvalidOperationException">
		/// An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.
		/// </exception>
		public override void Initialize (string name, NameValueCollection config)
		{
			// Initialize values from web.config.
			if (config == null)
				throw new ArgumentNullException("config");

			if (name == null || name.Length == 0)
				name = "SqliteRoleProvider";

			if (String.IsNullOrEmpty(config["description"])) {
				config.Remove("description");
				config.Add("description", "Sqlite Role provider");
			}

			// Initialize the abstract base class.
			base.Initialize(name, config);

		}

		/// <summary>
		/// Adds the specified user names to the specified roles for the configured applicationName.
		/// </summary>
		/// <param name="usernames">A string array of user names to be added to the specified roles.</param>
		/// <param name="roleNames">A string array of the role names to add the specified user names to.</param>
		public override void AddUsersToRoles (string[] usernames, string[] roleNames)
		{
			foreach (string roleName in roleNames) {
				if (!RoleExists(roleName)) {
					throw new ProviderException("Role name not found.");
				}
			}

			foreach (var username in usernames) {
				if(null == Membership.GetUser(username)){
					throw new ProviderException("User not found");
				}
			}

			Db.UseCommand(cmd => {
					cmd.CommandText = 
						"INSERT INTO " + USERS_IN_ROLES_TB_NAME + " (UserId, RoleId)"
						+ " SELECT u.UserId, r.RoleId"
						+ " FROM " + USER_TB_NAME + " u, " + ROLE_TB_NAME + " r"
						+ " WHERE (u.LoweredUsername = $Username)"
						+ " AND (r.LoweredRoleName = $RoleName) ";

					var p = cmd.CreateParameter();
					p.ParameterName = "$Username";
					p.DbType = DbType.String;
					p.Size = MAX_USERNAME_LENGTH;
					cmd.Parameters.Add(p);
					IDbDataParameter userParam = p;

					p = cmd.CreateParameter();
					p.ParameterName = "$RoleName";
					p.DbType = DbType.String;
					p.Size = MAX_ROLENAME_LENGTH;
					cmd.Parameters.Add(p);
					IDbDataParameter roleParam = p;

					foreach (string username in usernames) {
						userParam.Value = username.ToLowerInvariant();
						foreach (string roleName in roleNames) {
							roleParam.Value = roleName.ToLowerInvariant();
							cmd.ExecuteNonQuery();
						}
					}


			});
		}

		/// <summary>
		/// Adds a new role to the data source for the configured applicationName.
		/// </summary>
		/// <param name="roleName">The name of the role to create.</param>
		public override void CreateRole (string roleName)
		{
			if (roleName.IndexOf(',') > 0) {
				throw new ArgumentException("Role names cannot contain commas.");
			}

			if (RoleExists(roleName)) {
				throw new ProviderException("Role name already exists.");
			}

			if (!SecUtility.ValidateParameter(ref roleName, true, true, false, MAX_ROLENAME_LENGTH)) {
				throw new ProviderException(String.Format("The role name is too long: it must not exceed {0} chars in length.", MAX_ROLENAME_LENGTH));
			}

			Db.UseCommand(cmd=>{

					IDataParameter p;
					cmd.CommandText = 
						"INSERT INTO " + ROLE_TB_NAME + "(RoleId, RoleName, LoweredRoleName) " +
						"Values ($RoleId, $RoleName, $LoweredRoleName)";

					p = cmd.CreateParameter();
					p.ParameterName = "$RoleId";
					p.Value = Guid.NewGuid().ToString();
					cmd.Parameters.Add(p);

					p = cmd.CreateParameter();
					p.ParameterName = "$RoleName";
					p.Value = roleName;
					cmd.Parameters.Add(p);

					p = cmd.CreateParameter();
					p.ParameterName = "$LoweredRoleName";
					p.Value = roleName.ToLowerInvariant();
					cmd.Parameters.Add(p);

					cmd.ExecuteNonQuery();
			});
		}

		/// <summary>
		/// Removes a role from the data source for the configured applicationName.
		/// </summary>
		/// <param name="roleName">The name of the role to delete.</param>
		/// <param name="throwOnPopulatedRole">If true, throw an exception if <paramref name="roleName"/> has one or more members and do not delete <paramref name="roleName"/>.</param>
		/// <returns>
		/// true if the role was successfully deleted; otherwise, false.
		/// </returns>
		public override bool DeleteRole (string roleName, bool throwOnPopulatedRole)
		{
			if (!RoleExists(roleName)) {
				throw new ProviderException("Role does not exist.");
			}

			if (throwOnPopulatedRole && GetUsersInRole(roleName).Length > 0) {
				throw new ProviderException("Cannot delete a populated role.");
			}

			Db.UseCommand(cmd => {
				IDataParameter p = cmd.CreateParameter();
				p.ParameterName = "$RoleName";
				p.Value = roleName.ToLowerInvariant();
				cmd.Parameters.Add(p);

				cmd.CommandText = 
					"DELETE " +
					"FROM   " + USERS_IN_ROLES_TB_NAME + " " +
					"WHERE  RoleId IN (" +
					"           SELECT RoleId " +
					"           FROM   " + ROLE_TB_NAME + " " +
					"           WHERE  LoweredRoleName = $RoleName" +
					"        )";
				
				cmd.ExecuteNonQuery();

				cmd.CommandText = "DELETE FROM " + ROLE_TB_NAME + " WHERE LoweredRoleName = $RoleName AND ApplicationId = $ApplicationId";
				cmd.ExecuteNonQuery();

			});
			return true;
		}

		/// <summary>
		/// Gets a list of all the roles for the configured applicationName.
		/// </summary>
		/// <returns>
		/// A string array containing the names of all the roles stored in the data source for the configured applicationName.
		/// </returns>
		public override string[] GetAllRoles ()
		{
			var tmpRoleNames = new List<string>();

			Db.UseCommand(cmd => {
				cmd.CommandText = 
					"SELECT RoleName " +
					"FROM   " + ROLE_TB_NAME + " "
					;

				using (var dr = cmd.ExecuteReader()) {
					while (dr.Read()) {
						tmpRoleNames.Add(dr.GetString(0));
					}
				}
			});

			return tmpRoleNames.ToArray();
		}

		/// <summary>
		/// Gets a list of the roles that a specified user is in for the configured applicationName.
		/// </summary>
		/// <param name="username">The user to return a list of roles for.</param>
		/// <returns>
		/// A string array containing the names of all the roles that the specified user is in for the configured applicationName.
		/// </returns>
		public override string[] GetRolesForUser(string username)
		{
			var tmpRoleNames = new List<string>();

			Db.UseCommand(cmd => {
				cmd.CommandText = 
					"SELECT r.RoleName " +
					"FROM   " + ROLE_TB_NAME + " r " +
					"       INNER JOIN " + USERS_IN_ROLES_TB_NAME + " uir ON r.RoleId = uir.RoleId INNER JOIN " + USER_TB_NAME + " u ON uir.UserId = u.UserId " +
					"WHERE  u.LoweredUsername = $Username ";

				var p = cmd.CreateParameter();
				p.ParameterName = "$Username";
				p.Value = username.ToLowerInvariant();
				cmd.Parameters.Add(p);

				using (var dr = cmd.ExecuteReader())
				{
					while (dr.Read()){
						tmpRoleNames.Add(dr.GetString(0));
					}
				}

			});

			return tmpRoleNames.ToArray();
		}

		/// <summary>
		/// Gets the users in role.
		/// </summary>
		/// <param name="roleName">Name of the role.</param>
		/// <returns>Returns the users in role.</returns>
		public override string[] GetUsersInRole(string roleName)
		{
			var tmpUserNames = new List<string>();

			Db.UseCommand(cmd => {
				cmd.CommandText = 
					"SELECT u.Username " +
					"FROM   " + USER_TB_NAME + " u " +
					"       INNER JOIN " + USERS_IN_ROLES_TB_NAME + " uir ON u.UserId = uir.UserId INNER JOIN " + ROLE_TB_NAME + " r ON uir.RoleId = r.RoleId  " +
					"WHERE  r.LoweredRoleName = $RoleName ";

				var p = cmd.CreateParameter();
				p.ParameterName = "$RoleName";
				p.Value = roleName.ToLowerInvariant();
				cmd.Parameters.Add(p);


					using (var dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
						tmpUserNames.Add(dr.GetString(0));
						}
					}
			});

			return tmpUserNames.ToArray();
		}

		/// <summary>
		/// Gets a value indicating whether the specified user is in the specified role for the configured applicationName.
		/// </summary>
		/// <param name="username">The user name to search for.</param>
		/// <param name="roleName">The role to search in.</param>
		/// <returns>
		/// true if the specified user is in the specified role for the configured applicationName; otherwise, false.
		/// </returns>
		public override bool IsUserInRole(string username, string roleName)
		{
			bool userinrole = false;
			Db.UseCommand(cmd => {
				cmd.CommandText = 
					"SELECT COUNT(*) " +
					"FROM " + USERS_IN_ROLES_TB_NAME + " uir INNER JOIN "
						+ USER_TB_NAME + " u ON uir.UserId = u.UserId INNER JOIN " + ROLE_TB_NAME + " r ON uir.RoleId = r.RoleId "
						+ " WHERE u.LoweredUsername = $Username "
						+ " AND r.LoweredRoleName = $RoleName ";

				var p = cmd.CreateParameter();
				p.ParameterName = "$Username";
				p.Value = username.ToLowerInvariant();
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$RoleName";
				p.Value = roleName.ToLowerInvariant();
				cmd.Parameters.Add(p);

				userinrole = 0 < Convert.ToInt64(cmd.ExecuteScalar());
			});
			return userinrole;
		}

		/// <summary>
		/// Removes the specified user names from the specified roles for the configured applicationName.
		/// </summary>
		/// <param name="usernames">A string array of user names to be removed from the specified roles.</param>
		/// <param name="roleNames">A string array of role names to remove the specified user names from.</param>
		public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
		{
			Db.UseCommand(cmd => {
				cmd.CommandText = 
					"DELETE " +
					"FROM   " + USERS_IN_ROLES_TB_NAME + " " +
					"WHERE  UserId = (" +
					"           SELECT UserId " +
					"           FROM " + USER_TB_NAME + " " +
					"           WHERE LoweredUsername = $Username " +
					"       ) AND " +
					"       RoleId = (" +
					"           SELECT RoleId " +
					"           FROM " + ROLE_TB_NAME + " " +
					"           WHERE LoweredRoleName = $RoleName " +
					"       )";

				var  userParm = cmd.CreateParameter();
				userParm.ParameterName = "$Username";
				userParm.DbType = DbType.String;
				userParm.Size = MAX_USERNAME_LENGTH;
				cmd.Parameters.Add(userParm);

				var roleParm = cmd.CreateParameter();
				roleParm.ParameterName = "$RoleName";
				roleParm.DbType = DbType.String;
				roleParm.Size = MAX_ROLENAME_LENGTH;
				cmd.Parameters.Add(roleParm);

				foreach (string username in usernames){
					userParm.Value = username.ToLowerInvariant();
					foreach (string roleName in roleNames){
						roleParm.Value = roleName.ToLowerInvariant();
						cmd.ExecuteNonQuery();
					}
				}
			});
		}

		/// <summary>
		/// Gets a value indicating whether the specified role name already exists in the role data source for the configured applicationName.
		/// </summary>
		/// <param name="roleName">The name of the role to search for in the data source.</param>
		/// <returns>
		/// true if the role name already exists in the data source for the configured applicationName; otherwise, false.
		/// </returns>
		public override bool RoleExists(string roleName)
		{
			bool roleexists = false;

			Db.UseCommand(cmd => {
				cmd.CommandText = 
					"SELECT COUNT(*) " +
					"FROM   " + ROLE_TB_NAME +
					"WHERE  LoweredRoleName = $RoleName ";

				var p = cmd.CreateParameter();
				p.ParameterName = "$RoleName";
				p.Value = roleName.ToLowerInvariant();
				cmd.Parameters.Add(p);

				roleexists = (Convert.ToInt64(cmd.ExecuteScalar()) > 0);
			});
			return roleexists;
		}

		/// <summary>
		/// Gets an array of user names in a role where the user name contains the specified user name to match.
		/// </summary>
		/// <param name="roleName">The role to search in.</param>
		/// <param name="usernameToMatch">The user name to search for.</param>
		/// <returns>
		/// A string array containing the names of all the users where the user name matches <paramref name="usernameToMatch"/> and the user is a member of the specified role.
		/// </returns>
		public override string[] FindUsersInRole(string roleName, string usernameToMatch)
		{
			var tmpUserNames = new List<string>();

			Db.UseCommand(cmd =>{
				cmd.CommandText = 
					"SELECT u.Username " +
					"FROM   " + USERS_IN_ROLES_TB_NAME + " uir " +
					"       INNER JOIN " + USER_TB_NAME + " u ON uir.UserId = u.UserId INNER JOIN " + ROLE_TB_NAME + " r ON r.RoleId = uir.RoleId " +
					"WHERE  u.LoweredUsername LIKE $UsernameSearch AND " +
					"       r.LoweredRoleName = $RoleName";

				var p = cmd.CreateParameter();
				p.ParameterName = "$UsernameSearch";
				p.Value = usernameToMatch;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$RoleName";
				p.Value = roleName.ToLowerInvariant();
				cmd.Parameters.Add(p);

				using (var dr = cmd.ExecuteReader())
				{
					while (dr.Read()){
						tmpUserNames.Add(dr.GetString(0));
					}
				}
			});


			return tmpUserNames.ToArray();
		}

		#endregion

		#region Private Methods
		/*
		private static string GetApplicationId(string appName)
		{
			DbConnection cn = GetDbConnectionForRole();
			try
			{
				using (SqliteCommand cmd = cn.CreateCommand())
				{
					cmd.CommandText = "SELECT ApplicationId FROM aspnet_Applications WHERE ApplicationName = $AppName";
					cmd.Parameters.AddWithValue("$AppName", appName);

					if (cn.State == ConnectionState.Closed)
						cn.Open();

					return cmd.ExecuteScalar() as string;
				}
			}
			finally
			{
				if (!IsTransactionInProgress())
					cn.Dispose();
			}
		}

		private void VerifyApplication()
		{
			// Verify a record exists in the application table.
			if (String.IsNullOrEmpty(_applicationId) || String.IsNullOrEmpty(_membershipApplicationName))
			{
				// No record exists in the application table for either the role application and/or the membership application. Create it.
				DbConnection cn = GetDbConnectionForRole();
				try
				{
					using (DbCommand cmd = cn.CreateCommand())
					{
						cmd.CommandText = "INSERT INTO " + APP_TB_NAME + " (ApplicationId, ApplicationName, Description) VALUES ($ApplicationId, $ApplicationName, $Description)";

						string roleApplicationId = Guid.NewGuid().ToString();

						cmd.Parameters.AddWithValue("$ApplicationId", roleApplicationId);
						cmd.Parameters.AddWithValue("$ApplicationName", _applicationName);
						cmd.Parameters.AddWithValue("$Description", String.Empty);

						if (cn.State == ConnectionState.Closed)
							cn.Open();

						// Insert record for the role application.
						if (String.IsNullOrEmpty(_applicationId))
						{
							cmd.ExecuteNonQuery();

							_applicationId = roleApplicationId;
						}

						if (String.IsNullOrEmpty(_membershipApplicationId))
						{
							if (_applicationName == _membershipApplicationName)
							{
								// Use the app name for the membership app name.
								MembershipApplicationName = ApplicationName;
							}
							else
							{
								// Need to insert record for the membership application.
								_membershipApplicationId = Guid.NewGuid().ToString();

								cmd.Parameters["$ApplicationId"].Value = _membershipApplicationId;
								cmd.Parameters["$ApplicationName"].Value = _membershipApplicationName;

								cmd.ExecuteNonQuery();
							}
						}
					}
				}
				finally
				{
					if (!IsTransactionInProgress())
						cn.Dispose();
				}
			}
		}
*/
		/// <summary>
		/// Get a reference to the database connection used for Role. If a transaction is currently in progress, and the
		/// connection string of the transaction connection is the same as the connection string for the Role provider,
		/// then the connection associated with the transaction is returned, and it will already be open. If no transaction is in progress,
		/// a new <see cref="DbConnection"/> is created and returned. It will be closed and must be opened by the caller
		/// before using.
		/// </summary>
		/// <returns>A <see cref="DbConnection"/> object.</returns>
		/// <remarks>The transaction is stored in <see cref="System.Web.HttpContext.Current"/>. That means transaction support is limited
		/// to web applications. For other types of applications, there is no transaction support unless this code is modified.</remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		[Obsolete("User 'Db.GetConnection' instead")]
		private static DbConnection GetDbConnectionForRole()
		{
			return Db.GetConnection();
		}

		/// <summary>
		/// Determines whether a database transaction is in progress for the Role provider.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if a database transaction is in progress; otherwise, <c>false</c>.
		/// </returns>
		/// <remarks>A transaction is considered in progress if an instance of <see cref="DbTransaction"/> is found in the
		/// <see cref="System.Web.HttpContext.Current"/> Items property and its connection string is equal to the Role 
		/// provider's connection string. Note that this implementation of <see cref="SqliteRoleProvider"/> never adds a 
		/// <see cref="DbTransaction"/> to <see cref="System.Web.HttpContext.Current"/>, but it is possible that 
		/// another data provider in this application does. This may be because other data is also stored in this Sqlite database,
		/// and the application author wants to provide transaction support across the individual providers. If an instance of
		/// <see cref="System.Web.HttpContext.Current"/> does not exist (for example, if the calling application is not a web application),
		/// this method always returns false.</remarks>
		[Obsolete("Use 'Db.IsTransactionInProgress' instead")]
		private static bool IsTransactionInProgress()
		{
			return Db.IsTransactionInProgress();
		}

		#endregion
	}
}