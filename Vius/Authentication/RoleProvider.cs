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
	public sealed class ViusRoleProvider : RoleProvider
	{
		#region Private Fields

		private static readonly Vius.Data.DataProvider Db = Vius.Data.DataProvider.Instance;

		internal class TbNames {
			public const string Roles = "AuthRoles";
			public const string Users = ViusMembershipProvider.TbNames.User;
			public const string UserRoles = "AuthUserRoles";
		}

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
				name = "ViusRoleProvider";

			if (String.IsNullOrEmpty(config["description"])) {
				config.Remove("description");
				config.Add("description", "Vius Role provider");
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
						"INSERT INTO " + TbNames.UserRoles + " ( \n" +
						"    UserId, \n" +
						"    RoleId \n" +
						") \n " +
						"SELECT u.UserId, \n" +
						"       r.RoleId \n" +
						"FROM   " + TbNames.Users + " u, \n" +
						"       " + TbNames.Roles + " r \n" +
						"WHERE u.UsernameLowered = lower(:Username) AND \n" +
						"      r.LoweredRoleName = lower(:RoleName) \n";

					var p = cmd.CreateParameter();
					p.ParameterName = ":Username";
					p.DbType = DbType.String;
					p.Size = MAX_USERNAME_LENGTH;
					cmd.Parameters.Add(p);
					IDbDataParameter userParam = p;

					p = cmd.CreateParameter();
					p.ParameterName = ":RoleName";
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
						"INSERT INTO " + TbNames.Roles + "(RoleId, RoleName, LoweredRoleName) " +
						"Values (:RoleId, :RoleName, :LoweredRoleName)";

					p = cmd.CreateParameter();
					p.ParameterName = ":RoleId";
					p.Value = Guid.NewGuid().ToString();
					cmd.Parameters.Add(p);

					p = cmd.CreateParameter();
					p.ParameterName = ":RoleName";
					p.Value = roleName;
					cmd.Parameters.Add(p);

					p = cmd.CreateParameter();
					p.ParameterName = ":LoweredRoleName";
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
				p.ParameterName = ":RoleName";
				p.Value = roleName.ToLowerInvariant();
				cmd.Parameters.Add(p);

				cmd.CommandText = 
					"DELETE " +
					"FROM   " + TbNames.UserRoles + " " +
					"WHERE  RoleId IN (" +
					"           SELECT RoleId " +
					"           FROM   " + TbNames.Roles + " " +
					"           WHERE  LoweredRoleName = lower(:RoleName)" +
					"        )";
				
				cmd.ExecuteNonQuery();

				cmd.CommandText = 
					"DELETE " +
					"FROM " + TbNames.Roles + " " +
					"WHERE LoweredRoleName = :RoleName ";
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
					"FROM   " + TbNames.Roles + " "
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
		public override string[] GetRolesForUser (string username)
		{
			var tmpRoleNames = new List<string>();

			if (string.IsNullOrEmpty(username)) {
				tmpRoleNames.ToArray();
			}

			Db.UseCommand(cmd => {
				cmd.CommandText = 
					"SELECT r.RoleName " +
					"FROM   " + TbNames.Roles + " r " +
					"       INNER JOIN " + TbNames.UserRoles + " uir ON r.RoleId = uir.RoleId " +
					"       INNER JOIN " + TbNames.Users + " u ON uir.UserId = u.UserId " +
					"WHERE  u.UsernameLowered = lower(@Username) ";

				var p = cmd.CreateParameter();
				p.ParameterName = "@Username";
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
					"FROM   " + TbNames.Users + " u " +
					"       INNER JOIN " + TbNames.UserRoles + " uir ON u.UserId = uir.UserId INNER JOIN " + TbNames.Roles + " r ON uir.RoleId = r.RoleId  " +
					"WHERE  r.LoweredRoleName = lower(:RoleName) ";

				var p = cmd.CreateParameter();
				p.ParameterName = ":RoleName";
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
					"FROM " + TbNames.UserRoles + " uir INNER JOIN "
						+ TbNames.Users + " u ON uir.UserId = u.UserId INNER JOIN " + TbNames.Roles + " r ON uir.RoleId = r.RoleId "
						+ " WHERE u.LoweredUsername = :Username "
						+ " AND r.LoweredRoleName = :RoleName ";

				var p = cmd.CreateParameter();
				p.ParameterName = ":Username";
				p.Value = username.ToLowerInvariant();
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = ":RoleName";
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
					"DELETE \n" +
					"FROM   " + TbNames.UserRoles + " \n" +
					"WHERE  UserId = ( \n" +
					"           SELECT UserId \n" +
					"           FROM " + TbNames.Users + " \n" +
					"           WHERE UsernameLowered = lower(:Username) \n" +
					"       ) AND \n" +
					"       RoleId = ( \n" +
					"           SELECT RoleId \n" +
					"           FROM " + TbNames.Roles + " \n" +
					"           WHERE LoweredRoleName = :RoleName \n" +
					"       ) \n";

				var  userParm = cmd.CreateParameter();
				userParm.ParameterName = ":Username";
				userParm.DbType = DbType.String;
				userParm.Size = MAX_USERNAME_LENGTH;
				cmd.Parameters.Add(userParm);

				var roleParm = cmd.CreateParameter();
				roleParm.ParameterName = ":RoleName";
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
					"SELECT COUNT(*) \n" +
					"FROM   " + TbNames.Roles + " \n" +
					"WHERE  LoweredRoleName = lower(:RoleName) \n";

				var p = cmd.CreateParameter();
				p.ParameterName = ":RoleName";
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
					"FROM   " + TbNames.UserRoles + " uir " +
					"       INNER JOIN " + TbNames.Users + " u ON uir.UserId = u.UserId INNER JOIN " + TbNames.Roles + " r ON r.RoleId = uir.RoleId " +
					"WHERE  u.LoweredUsername LIKE :UsernameSearch AND " +
					"       r.LoweredRoleName = :RoleName";

				var p = cmd.CreateParameter();
				p.ParameterName = ":UsernameSearch";
				p.Value = usernameToMatch;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = ":RoleName";
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

	}
}