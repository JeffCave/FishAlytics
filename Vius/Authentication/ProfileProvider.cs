using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web.Profile;
using System.Xml.Serialization;

namespace Vius.Authentication
{
	/// <summary>
	/// Provides a Profile implementation whose data is stored in a Sqlite database.
	/// </summary>
	public sealed class ViusProfileProvider : ProfileProvider
	{
		#region Private Fields

		private static readonly Vius.Data.DataProvider Db = Vius.Data.DataProvider.Instance;

		internal static class TbNames{
			public const string Profiles = "AuthProfile";
			public const string Users = ViusMembershipProvider.TbNames.User;
		}

		private const int MAX_APPLICATION_NAME_LENGTH = 256;

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the name of the currently running application.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// A <see cref="T:System.String"/> that contains the application's shortened name, which does not contain a full path or extension, for example, SimpleAppSettings.
		/// </returns>
		public override string ApplicationName
		{
			get { return "Vius"; }
			set
			{
			}
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
		public override void Initialize(string name, NameValueCollection config)
		{
			if (config == null)
				throw new ArgumentNullException("config");

			if (string.IsNullOrEmpty(name))
				name = "ViusProfileProvider";


			if (string.IsNullOrEmpty(config["description"]))
			{
				config.Remove("description");
				config.Add("description", "Vius Profile Provider");
			}

			base.Initialize(name, config);

		}

		/// <summary>
		/// Retrieves profile property information and values from a SQL Server profile database.
		/// </summary>
		/// <param name="sc">The <see cref="SettingsContext" /> that contains user profile information. </param>
		/// <param name="properties">A <see cref="SettingsPropertyCollection" /> containing profile information for the properties to be retrieved.</param>
		/// <returns>A <see cref="SettingsPropertyValueCollection" /> containing profile property information and values.</returns>
		public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext sc, SettingsPropertyCollection properties)
		{
			SettingsPropertyValueCollection svc = new SettingsPropertyValueCollection();
			if (properties.Count < 1)
				return svc;

			string username = (string)sc["UserName"];
			foreach (SettingsProperty prop in properties)
			{
				if (prop.SerializeAs == SettingsSerializeAs.ProviderSpecific)
				{
					if (prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string))
						prop.SerializeAs = SettingsSerializeAs.String;
					else
						prop.SerializeAs = SettingsSerializeAs.Xml;
				}
				svc.Add(new SettingsPropertyValue(prop));
			}

			if (!String.IsNullOrEmpty(username))
			{
				GetPropertyValuesFromDatabase(username, svc);
			}
			return svc;
		}

		/// <summary>
		/// Updates the Sqlite profile database with the specified property values. 
		/// </summary>
		/// <param name="sc">The <see cref="SettingsContext" /> that contains user profile information. </param>
		/// <param name="properties">A <see cref="SettingsPropertyValueCollection" /> containing profile information and 
		/// values for the properties to be updated. </param>
		public override void SetPropertyValues (SettingsContext sc, SettingsPropertyValueCollection properties)
		{
			string username = (string)sc["UserName"];
			bool userIsAuthenticated = (bool)sc["IsAuthenticated"];
			if (string.IsNullOrEmpty(username) || properties.Count < 1)
				return;

			string names = String.Empty;
			string values = String.Empty;
			byte[] buf = null;

			PrepareDataForSaving(ref names, ref values, ref buf, true, properties, userIsAuthenticated);

			if (names.Length == 0)
				return;

			Db.UseCommand(cmd => {
				cmd.CommandText = 
					"SELECT UserId " +
					"FROM " + TbNames.Users + " " +
					"WHERE LoweredUsername = $Username;";

				var p = cmd.CreateParameter();
				p.ParameterName = "$Username";
				p.Value = username.ToLowerInvariant();
				cmd.Parameters.Add(p);

				string userId = cmd.ExecuteScalar() as string;

				if ((userId == null) && (userIsAuthenticated)) {
					// User is logged on but no record exists in user table. This should never happen, but if it doesn, just exit.
					return; 
				}

				if (userId == null) {
					// User is anonymous and no record exists in user table. Add it.
					userId = Guid.NewGuid().ToString();

					CreateAnonymousUser(username, userId, cmd);
				}

				cmd.CommandText = 
					"SELECT COUNT(*) \n" +
					"FROM " + TbNames.Profiles + " \n" +
					"WHERE UserId = $UserId";
				cmd.Parameters.Clear();
				p = cmd.CreateParameter();
				p.ParameterName = "$UserId";
				p.Value = userId;

				if (Convert.ToInt64(cmd.ExecuteScalar()) > 0) {
					cmd.CommandText = "UPDATE " + TbNames.Profiles + " SET PropertyNames = $PropertyNames, PropertyValuesString = $PropertyValuesString, PropertyValuesBinary = $PropertyValuesBinary, LastUpdatedDate = $LastUpdatedDate WHERE UserId = $UserId";
				} else {
					cmd.CommandText = 
						"INSERT INTO " + TbNames.Profiles + " (" +
						"    UserId, " +
						"    PropertyNames, " +
						"    PropertyValuesString, " +
						"    PropertyValuesBinary, " +
						"    LastUpdatedDate " +
						") " +
						"VALUES (" +
						"    $UserId, " +
						"    $PropertyNames, " +
						"    $PropertyValuesString, " +
						"    $PropertyValuesBinary, " +
						"    $LastUpdatedDate" +
						")";
				}
				cmd.Parameters.Clear();
				p = cmd.CreateParameter();
				p.ParameterName = "$UserId";
				p.Value = userId;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$PropertyNames";
				p.Value = names;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$PropertyValuesString";
				p.Value = values;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$PropertyValuesBinary";
				p.Value = buf;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$LastUpdatedDate";
				p.Value = DateTime.UtcNow;
				cmd.Parameters.Add(p);

				cmd.ExecuteNonQuery();

				// Update activity field
				cmd.CommandText = 
					"UPDATE " + TbNames.Users + " " +
					"SET    LastActivityDate = $LastActivityDate " +
					"WHERE  UserId = $UserId";
				cmd.Parameters.Clear();

				p = cmd.CreateParameter();
				p.ParameterName = "$LastActivityDate";
				p.Value = DateTime.UtcNow;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$UserId";
				p.Value = userId;
				cmd.Parameters.Add(p);

				cmd.ExecuteNonQuery();

			}
			);
		}

		/// <summary>
		/// Deletes profile properties and information for the supplied list of profiles.
		/// </summary>
		/// <param name="profiles">A <see cref="T:System.Web.Profile.ProfileInfoCollection"/>  of information about profiles that are to be deleted.</param>
		/// <returns>
		/// The number of profiles deleted from the data source.
		/// </returns>
		public override int DeleteProfiles (ProfileInfoCollection profiles)
		{
			if (profiles == null)
				throw new ArgumentNullException("profiles");

			if (profiles.Count < 1)
				throw new ArgumentException("Profiles collection is empty", "profiles");

			var usernames = new List<string>();
			foreach (ProfileInfo profile in profiles) {
				usernames.Add(profile.UserName);
			}
			return DeleteProfiles(usernames);
		}

		/// <summary>
		/// When overridden in a derived class, deletes profile properties and information for profiles that match the supplied list of user names.
		/// </summary>
		/// <param name="usernames">A string array of user names for profiles to be deleted.</param>
		/// <returns>
		/// The number of profiles deleted from the data source.
		/// </returns>
		public override int DeleteProfiles(string[] usernames)
		{
			return DeleteProfiles(usernames as ICollection<string>);
		}

		/// <summary>
		/// Deletes all user-profile data for profiles in which the last activity date occurred before the specified date.
		/// </summary>
		/// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are deleted.</param>
		/// <param name="userInactiveSinceDate">A <see cref="T:System.DateTime"/> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"/>  value of a user profile occurs on or before this date and time, the profile is considered inactive.</param>
		/// <returns>
		/// The number of profiles deleted from the data source.
		/// </returns>
		public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			int profiles = 0;
				Db.UseCommand(cmd => 
				{
					cmd.CommandText = 
						"DELETE " +
						"FROM   " + TbNames.Profiles + " " +
						"WHERE  UserId IN (" +
						"           SELECT UserId " +
						"           FROM " + TbNames.Users + " " +
						"           WHERE LastActivityDate <= $LastActivityDate" + GetClauseForAuthenticationOptions(authenticationOption) + 
						"       ) ";

					var p = cmd.CreateParameter();
					p.ParameterName = "$LastActivityDate";
					p.Value = userInactiveSinceDate;
					cmd.Parameters.Add(p);

					profiles = cmd.ExecuteNonQuery();
			});
			return profiles;
		}

		/// <summary>
		/// Returns the number of profiles in which the last activity date occurred on or before the specified date.
		/// </summary>
		/// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param>
		/// <param name="userInactiveSinceDate">A <see cref="T:System.DateTime"/> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"/>  of a user profile occurs on or before this date and time, the profile is considered inactive.</param>
		/// <returns>
		/// The number of profiles in which the last activity date occurred on or before the specified date.
		/// </returns>
		public override int GetNumberOfInactiveProfiles (ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			int numProfiles = 0;

				Db.UseCommand( cmd => {
					cmd.CommandText = 
						"SELECT COUNT(*) " +
						"FROM " + TbNames.Users + " u, " + TbNames.Profiles + " p " +
						"WHERE u.LastActivityDate <= $LastActivityDate AND " +
						"u.UserId = p.UserId" + 
						GetClauseForAuthenticationOptions(authenticationOption);

					var p = cmd.CreateParameter();
					p.ParameterName = "$LastActivityDate";
					p.Value = userInactiveSinceDate;
					cmd.Parameters.Add(p);

					numProfiles = cmd.ExecuteNonQuery();
			});
			return numProfiles;
		}

		/// <summary>
		/// Retrieves user profile data for all profiles in the data source.
		/// </summary>
		/// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param>
		/// <param name="pageIndex">The index of the page of results to return.</param>
		/// <param name="pageSize">The size of the page of results to return.</param>
		/// <param name="totalRecords">When this method returns, contains the total number of profiles.</param>
		/// <returns>
		/// A <see cref="T:System.Web.Profile.ProfileInfoCollection"/> containing user-profile information for all profiles in the data source.
		/// </returns>
		public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
		{
			string sqlQuery = 
				"SELECT u.UserName, " +
				"       u.IsAnonymous, " +
				"       u.LastActivityDate, " +
				"       p.LastUpdatedDate, " +
				"       length(p.PropertyNames) + " +
				"       length(p.PropertyValuesString) " +
				"FROM   " + TbNames.Users + " u, " +
				"       " + TbNames.Profiles + " p " +
				"WHERE  u.ApplicationId = $ApplicationId AND u.UserId = p.UserId "
												+ GetClauseForAuthenticationOptions(authenticationOption);

			DbParameter[] args = new DbParameter[0];
			return GetProfilesForQuery(sqlQuery, args, pageIndex, pageSize, out totalRecords);
		}

		/// <summary>
		/// Retrieves user-profile data from the data source for profiles in which the last activity date occurred on or before the specified date.
		/// </summary>
		/// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param>
		/// <param name="userInactiveSinceDate">A <see cref="T:System.DateTime"/> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"/>  of a user profile occurs on or before this date and time, the profile is considered inactive.</param>
		/// <param name="pageIndex">The index of the page of results to return.</param>
		/// <param name="pageSize">The size of the page of results to return.</param>
		/// <param name="totalRecords">When this method returns, contains the total number of profiles.</param>
		/// <returns>
		/// A <see cref="T:System.Web.Profile.ProfileInfoCollection"/> containing user-profile information about the inactive profiles.
		/// </returns>
		public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
		{
			string sqlQuery = 
				"SELECT u.UserName, u.IsAnonymous, u.LastActivityDate, p.LastUpdatedDate, length(p.PropertyNames) + length(p.PropertyValuesString) " +
				"FROM   " + TbNames.Users + " u, " +
				"       " + TbNames.Profiles + " p " +
				"WHERE  u.UserId = p.UserId AND " +
				"       u.LastActivityDate <= $LastActivityDate"
				+ GetClauseForAuthenticationOptions(authenticationOption);

			DbParameter p = Db.Factory.CreateParameter();
			p.ParameterName = "$LastActivityDate";
			p.DbType = DbType.DateTime;
			p.Value = userInactiveSinceDate;

			var args = new System.Collections.Generic.List<DbParameter>(){
				p
			};

			return GetProfilesForQuery(sqlQuery, args.ToArray(), pageIndex, pageSize, out totalRecords);
		}

		/// <summary>
		/// Retrieves profile information for profiles in which the user name matches the specified user names.
		/// </summary>
		/// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param>
		/// <param name="usernameToMatch">The user name to search for.</param>
		/// <param name="pageIndex">The index of the page of results to return.</param>
		/// <param name="pageSize">The size of the page of results to return.</param>
		/// <param name="totalRecords">When this method returns, contains the total number of profiles.</param>
		/// <returns>
		/// A <see cref="T:System.Web.Profile.ProfileInfoCollection"/> containing user-profile information for profiles where the user name matches the supplied <paramref name="usernameToMatch"/> parameter.
		/// </returns>
		public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			string sqlQuery = "SELECT u.UserName, u.IsAnonymous, u.LastActivityDate, p.LastUpdatedDate, length(p.PropertyNames) + length(p.PropertyValuesString) FROM "
												+ TbNames.Users + " u, " + TbNames.Profiles + " p WHERE u.ApplicationId = $ApplicationId AND u.UserId = p.UserId AND u.LoweredUserName LIKE $UserName"
												+ GetClauseForAuthenticationOptions(authenticationOption);

			var p = Db.Factory.CreateParameter();
			p.ParameterName = "$UserName";
			p.DbType = DbType.String;
			p.Size = 256;
			p.Value = usernameToMatch.ToLowerInvariant();

			var args = new List<DbParameter>(){
				p
			};

			return GetProfilesForQuery(sqlQuery, args.ToArray(), pageIndex, pageSize, out totalRecords);
		}

		/// <summary>
		/// Retrieves profile information for profiles in which the last activity date occurred on or before the specified date and the user name matches the specified user name.
		/// </summary>
		/// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param>
		/// <param name="usernameToMatch">The user name to search for.</param>
		/// <param name="userInactiveSinceDate">A <see cref="T:System.DateTime"/> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"/> value of a user profile occurs on or before this date and time, the profile is considered inactive.</param>
		/// <param name="pageIndex">The index of the page of results to return.</param>
		/// <param name="pageSize">The size of the page of results to return.</param>
		/// <param name="totalRecords">When this method returns, contains the total number of profiles.</param>
		/// <returns>
		/// A <see cref="T:System.Web.Profile.ProfileInfoCollection"/> containing user profile information for inactive profiles where the user name matches the supplied <paramref name="usernameToMatch"/> parameter.
		/// </returns>
		public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
		{
			string sqlQuery = "SELECT u.UserName, u.IsAnonymous, u.LastActivityDate, p.LastUpdatedDate, length(p.PropertyNames) + length(p.PropertyValuesString) FROM "
												+ TbNames.Users + " u, " + TbNames.Profiles + " p WHERE u.UserId = p.UserId AND u.UserName LIKE $UserName AND u.LastActivityDate <= $LastActivityDate"
												+ GetClauseForAuthenticationOptions(authenticationOption);

			var args = new List<DbParameter>();

			DbParameter p;
			p = Db.Factory.CreateParameter();
			p.ParameterName = "$UserName";
			p.DbType = DbType.String;
			p.Size = 256;
			p.Value = usernameToMatch.ToLowerInvariant();
			args.Add(p);

			p = Db.Factory.CreateParameter();
			p.ParameterName = "$LastActivityDate";
			p.DbType = DbType.DateTime;
			p.Value = userInactiveSinceDate;
			args.Add(p);

			return GetProfilesForQuery(sqlQuery, args.ToArray(), pageIndex, pageSize, out totalRecords);
		}

		#endregion

		#region Private Methods

		private static void CreateAnonymousUser(string username, string userId, IDbCommand cmd)
		{
				cmd.CommandText = 
						"INSERT INTO " + TbNames.Users + "(" +
						"    UserId, " +
						"    Username, " +
						"    LoweredUsername, " +
						"    Email, " +
						"    LoweredEmail, " +
						"    Comment, " +
						"    Password," + 
						"    PasswordFormat, " +
						"    PasswordSalt, " +
						"    PasswordQuestion," + 
						"    PasswordAnswer, " +
						"    IsApproved, " +
						"    IsAnonymous," + 
						"    CreateDate, " +
						"    LastPasswordChangedDate, " +
						"    LastActivityDate," + 
						"    LastLoginDate, " +
						"    IsLockedOut, " +
						"    LastLockoutDate," + 
						"    FailedPasswordAttemptCount, " +
						"    FailedPasswordAttemptWindowStart," + 
						"    FailedPasswordAnswerAttemptCount, " +
						"    FailedPasswordAnswerAttemptWindowStart)" + 
						"Values(" +
						"    $UserId, " +
						"    $Username, " +
						"    $LoweredUsername, " +
						"    $Email, " +
						"    $LoweredEmail, " +
						"    $Comment, " +
						"    $Password," + 
						"    $PasswordFormat, " +
						"    $PasswordSalt, " +
						"    $PasswordQuestion, " +
						"    $PasswordAnswer, " +
						"    $IsApproved, " +
						"    $IsAnonymous, " +
						"    $CreateDate, " +
						"    $LastPasswordChangedDate," +
						"    $LastActivityDate, " +
						"    $LastLoginDate, " +
						"    $IsLockedOut, " +
						"    $LastLockoutDate," + 
						"    $FailedPasswordAttemptCount, " +
						"    $FailedPasswordAttemptWindowStart," + 
						"    $FailedPasswordAnswerAttemptCount, " +
						"    $FailedPasswordAnswerAttemptWindowStart" +
						")";

				DateTime nullDate = DateTime.MinValue;
				DateTime nowDate = DateTime.UtcNow;
				IDbDataParameter p;

				p = cmd.CreateParameter();
				p.ParameterName = "$UserId";
				p.DbType = DbType.String;
				p.Value = userId;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$Username";
				p.DbType = DbType.String;
				p.Size = 256;
				p.Value = username;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$LoweredUsername";
				p.DbType = DbType.String;
				p.Size = 256;
				p.Value = username.ToLowerInvariant();
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$Email";
				p.DbType = DbType.String;
				p.Size = 256;
				p.Value = String.Empty;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$LoweredEmail";
				p.DbType = DbType.String;
				p.Size = 256;
				p.Value = String.Empty;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$Comment";
				p.DbType = DbType.String;
				p.Size = 3000;
				p.Value = null;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$Password";
				p.DbType = DbType.String;
				p.Size = 128;
				p.Value = Guid.NewGuid().ToString();
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$PasswordFormat";
				p.DbType = DbType.String;
				p.Size = 128;
				p.Value = System.Web.Security.Membership.Provider.PasswordFormat.ToString();
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$PasswordSalt";
				p.DbType = DbType.String;
				p.Size = 128;
				p.Value = String.Empty;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$PasswordQuestion";
				p.DbType = DbType.String;
				p.Size = 256;
				p.Value = null;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$PasswordAnswer";
				p.DbType = DbType.String;
				p.Size = 128;
				p.Value = null;
				cmd.Parameters.Add(p);


				p = cmd.CreateParameter();
				p.ParameterName = "$IsApproved";
				p.DbType = DbType.Boolean;
				p.Value = true;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$IsAnonymous";
				p.DbType = DbType.Boolean;
				p.Value = true;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$CreateDate";
				p.DbType = DbType.DateTime;
				p.Value = nowDate;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$LastPasswordChangedDate";
				p.DbType = DbType.DateTime;
				p.Value = nullDate;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$LastActivityDate";
				p.DbType = DbType.DateTime;
				p.Value = nowDate;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$LastLoginDate";
				p.DbType = DbType.DateTime;
				p.Value = nullDate;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$IsLockedOut";
				p.DbType = DbType.Boolean;
				p.Value = false;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$LastLockoutDate";
				p.DbType = DbType.DateTime;
				p.Value = nullDate;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$FailedPasswordAttemptCount";
				p.DbType = DbType.Int32;
				p.Value = 0;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$FailedPasswordAttemptWindowStart";
				p.DbType = DbType.DateTime;
				p.Value = nullDate;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$FailedPasswordAnswerAttemptCount";
				p.DbType = DbType.Int32;
				p.Value = 0;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = "$FailedPasswordAnswerAttemptWindowStart";
				p.DbType = DbType.DateTime;
				p.Value = nullDate;
				cmd.Parameters.Add(p);

				cmd.ExecuteNonQuery();
		}

		private static void ParseDataFromDb(string[] names, string values, byte[] buf, SettingsPropertyValueCollection properties)
		{
			if (names == null || values == null || buf == null || properties == null)
				return;

			for (int iter = 0; iter < names.Length / 4; iter++)
			{
				string name = names[iter * 4];
				SettingsPropertyValue pp = properties[name];

				if (pp == null) // property not found
					continue;

				int startPos = Int32.Parse(names[iter * 4 + 2], CultureInfo.InvariantCulture);
				int length = Int32.Parse(names[iter * 4 + 3], CultureInfo.InvariantCulture);

				if (length == -1 && !pp.Property.PropertyType.IsValueType) // Null Value
				{
					pp.PropertyValue = null;
					pp.IsDirty = false;
					pp.Deserialized = true;
				}
				if (names[iter * 4 + 1] == "S" && startPos >= 0 && length > 0 && values.Length >= startPos + length)
				{
					pp.PropertyValue = Deserialize(pp, values.Substring(startPos, length));
				}

				if (names[iter * 4 + 1] == "B" && startPos >= 0 && length > 0 && buf.Length >= startPos + length)
				{
					byte[] buf2 = new byte[length];

					Buffer.BlockCopy(buf, startPos, buf2, 0, length);
					pp.PropertyValue = Deserialize(pp, buf2);
				}
			}
		}

		private static void GetPropertyValuesFromDatabase (string username, SettingsPropertyValueCollection svc)
		{
			string[] names = null;
			string values = null;
			byte[] buffer = null;

			Db.UseCommand(cmd => {
				
				// User exists?
				cmd.CommandText = 
						"SELECT PropertyNames, PropertyValuesString, PropertyValuesBinary " +
					"FROM   " + TbNames.Profiles + " " +
					"WHERE  UserId in (SELECT UserId FROM " + TbNames.Users + " WHERE LoweredUsername = $UserName)";

				var p = cmd.CreateParameter();
				p.ParameterName = "$UserName";
				p.Value = username.ToLowerInvariant();
				cmd.Parameters.Add(p);


				using (var dr = cmd.ExecuteReader()) {
					if (dr.Read()) {
						names = dr.GetString(0).Split(':');
						values = dr.GetString(1);
						int length = (int)dr.GetBytes(2, 0L, null, 0, 0);
						buffer = new byte[length];
						dr.GetBytes(2, 0L, buffer, 0, length);
					}
				}

				cmd.CommandText = 
							"UPDATE " + TbNames.Users + " " +
					"SET LastActivityDate = $LastActivityDate " +
					"WHERE UserId in (SELECT UserId FROM " + TbNames.Users + " WHERE LoweredUsername = $UserName)";
				p = cmd.CreateParameter();
				p.ParameterName = "$LastActivityDate";
				p.Value = DateTime.UtcNow;
				cmd.Parameters.Add(p);

				cmd.ExecuteNonQuery();
			});

			if (names != null && names.Length > 0) {
				ParseDataFromDb(names, values, buffer, svc);
			}
		}

		private static ProfileInfoCollection GetProfilesForQuery(string sqlQuery, DbParameter[] args, int pageIndex, int pageSize, out int totalRecords)
		{
			if (pageIndex < 0)
				throw new ArgumentException("Page index must be non-negative", "pageIndex");

			if (pageSize < 1)
				throw new ArgumentException("Page size must be positive", "pageSize");

			long lBound = (long)pageIndex * pageSize;
			long uBound = lBound + pageSize - 1;

			if (uBound > Int32.MaxValue)
			{
				throw new ArgumentException("pageIndex*pageSize too large");
			}

			ProfileInfoCollection profiles = new ProfileInfoCollection();
			int total = 0;
			Db.UseCommand( cmd =>
			{
				cmd.CommandText = sqlQuery;

				foreach (var param in args){
					cmd.Parameters.Add(param);
				}

				using (var dr = cmd.ExecuteReader()){
					while (dr.Read()){
						total++;
						if ((total - 1 < lBound) || (total - 1 > uBound))
							continue;

						string username = dr.GetString(0);
						bool isAnon = dr.GetBoolean(1);
						DateTime dtLastActivity = dr.GetDateTime(2);
						DateTime dtLastUpdated = dr.GetDateTime(3);
						int size = dr.GetInt32(4);
						profiles.Add(new ProfileInfo(username, isAnon, dtLastActivity, dtLastUpdated, size));
					}
				}
			});

			//return our two values
			totalRecords = total;
			return profiles;
		}

		private static bool DeleteProfile (string username)
		{
			var usernames = new List<string>(){
				username
			};
			return (1==DeleteProfiles(usernames));
		}

		private static int DeleteProfiles (ICollection<string> usernames)
		{
			int deletes = 0;

			Db.UseCommand(cmd=>{
				cmd.CommandText = 
					"DELETE \n" +
					"FROM   " + TbNames.Profiles + " \n" +
					"WHERE  UserId in ( \n" +
					"           SELECT UserId \n" +
					"           FROM   " + TbNames.Users + " \n" +
					"           WHERE  LoweredUsername = $Username \n" +
					"       ) \n";

				var pUserName = cmd.CreateParameter();
				pUserName.ParameterName = "$Username";
				cmd.Parameters.Add(pUserName);

				foreach(var username in usernames){
					pUserName.Value = username.ToLowerInvariant();
					deletes += (cmd.ExecuteNonQuery() != 0)?1:0;
				}

			});
			return (deletes);
		}

		private static object Deserialize(SettingsPropertyValue prop, object obj)
		{
			object val = null;

			//////////////////////////////////////////////
			// Step 1: Try creating from Serialized value
			if (obj != null)
			{
				if (obj is string)
				{
					val = GetObjectFromString(prop.Property.PropertyType, prop.Property.SerializeAs, (string)obj);
				}
				else
				{
					MemoryStream ms = new MemoryStream((byte[])obj);
					try
					{
						val = (new BinaryFormatter()).Deserialize(ms);
					}
					finally
					{
						ms.Close();
					}
				}

				if (val != null && !prop.Property.PropertyType.IsAssignableFrom(val.GetType())) // is it the correct type
					val = null;
			}

			//////////////////////////////////////////////
			// Step 2: Try creating from default value
			if (val == null)
			{
				if (prop.Property.DefaultValue == null || prop.Property.DefaultValue.ToString() == "[null]")
				{
					if (prop.Property.PropertyType.IsValueType)
						return Activator.CreateInstance(prop.Property.PropertyType);
					else
						return null;
				}
				if (!(prop.Property.DefaultValue is string))
				{
					val = prop.Property.DefaultValue;
				}
				else
				{
					val = GetObjectFromString(prop.Property.PropertyType, prop.Property.SerializeAs, (string)prop.Property.DefaultValue);
				}

				if (val != null && !prop.Property.PropertyType.IsAssignableFrom(val.GetType())) // is it the correct type
					throw new ArgumentException("Could not create from default value for property: " + prop.Property.Name);
			}

			//////////////////////////////////////////////
			// Step 3: Create a new one by calling the parameterless constructor
			if (val == null)
			{
				if (prop.Property.PropertyType == typeof(string))
					val = "";
				else
					val = Activator.CreateInstance(prop.Property.PropertyType);
			}
			return val;
		}

		private static void PrepareDataForSaving(ref string allNames, ref string allValues, ref byte[] buf, bool binarySupported, SettingsPropertyValueCollection properties, bool userIsAuthenticated)
		{
			StringBuilder names = new StringBuilder();
			StringBuilder values = new StringBuilder();

			MemoryStream ms = (binarySupported ? new MemoryStream() : null);
			try
			{
				bool anyItemsToSave = false;

				foreach (SettingsPropertyValue pp in properties)
				{
					if (pp.IsDirty)
					{
						if (!userIsAuthenticated)
						{
							bool allowAnonymous = (bool)pp.Property.Attributes["AllowAnonymous"];
							if (!allowAnonymous)
								continue;
						}
						anyItemsToSave = true;
						break;
					}
				}

				if (!anyItemsToSave)
					return;

				foreach (SettingsPropertyValue pp in properties)
				{
					if (!userIsAuthenticated)
					{
						bool allowAnonymous = (bool)pp.Property.Attributes["AllowAnonymous"];
						if (!allowAnonymous)
							continue;
					}

					if (!pp.IsDirty && pp.UsingDefaultValue) // Not fetched from DB and not written to
						continue;

					int len, startPos = 0;
					string propValue = null;

					if (pp.Deserialized && pp.PropertyValue == null)
					{
						len = -1;
					}
					else
					{
						object sVal = SerializePropertyValue(pp);

						if (sVal == null)
						{
							len = -1;
						}
						else
						{
							if (!(sVal is string) && !binarySupported)
							{
								sVal = Convert.ToBase64String((byte[])sVal);
							}

							if (sVal is string)
							{
								propValue = (string)sVal;
								len = propValue.Length;
								startPos = values.Length;
							}
							else
							{
								byte[] b2 = (byte[])sVal;

								if (ms != null)
								{
									startPos = (int)ms.Position;
									ms.Write(b2, 0, b2.Length);
									ms.Position = startPos + b2.Length;
								}

								len = b2.Length;
							}
						}
					}

					names.Append(pp.Name + ":" + ((propValue != null) ? "S" : "B") + ":" + startPos.ToString(CultureInfo.InvariantCulture) + ":" + len.ToString(CultureInfo.InvariantCulture) + ":");

					if (propValue != null)
						values.Append(propValue);
				}

				if (binarySupported)
				{
					buf = ms.ToArray();
				}
			}
			finally
			{
				if (ms != null)
					ms.Close();
			}

			allNames = names.ToString();
			allValues = values.ToString();
		}

		private static string ConvertObjectToString(object propValue, Type type, SettingsSerializeAs serializeAs, bool throwOnError)
		{
			if (serializeAs == SettingsSerializeAs.ProviderSpecific)
			{
				if (type == typeof(string) || type.IsPrimitive)
					serializeAs = SettingsSerializeAs.String;
				else
					serializeAs = SettingsSerializeAs.Xml;
			}

			try
			{
				switch (serializeAs)
				{
					case SettingsSerializeAs.String:
						TypeConverter converter = TypeDescriptor.GetConverter(type);
						if (converter != null && converter.CanConvertTo(typeof(String)) && converter.CanConvertFrom(typeof(String)))
							return converter.ConvertToString(propValue);
						throw new ArgumentException("Unable to convert type " + type.ToString() + " to string", "type");
					case SettingsSerializeAs.Binary:
						MemoryStream ms = new MemoryStream();
						try
						{
							BinaryFormatter bf = new BinaryFormatter();
							bf.Serialize(ms, propValue);
							byte[] buffer = ms.ToArray();
							return Convert.ToBase64String(buffer);
						}
						finally
						{
							ms.Close();
						}

					case SettingsSerializeAs.Xml:
						XmlSerializer xs = new XmlSerializer(type);
						StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);

						xs.Serialize(sw, propValue);
						return sw.ToString();
				}
			}
			catch (Exception)
			{
				if (throwOnError)
					throw;
			}
			return null;
		}

		private static object SerializePropertyValue(SettingsPropertyValue prop)
		{
			object val = prop.PropertyValue;
			if (val == null)
				return null;

			if (prop.Property.SerializeAs != SettingsSerializeAs.Binary)
				return ConvertObjectToString(val, prop.Property.PropertyType, prop.Property.SerializeAs, prop.Property.ThrowOnErrorSerializing);

			MemoryStream ms = new MemoryStream();
			try
			{
				BinaryFormatter bf = new BinaryFormatter();
				bf.Serialize(ms, val);
				return ms.ToArray();
			}
			finally
			{
				ms.Close();
			}
		}

		private static object GetObjectFromString(Type type, SettingsSerializeAs serializeAs, string attValue)
		{
			// Deal with string types
			if (type == typeof(string) && (string.IsNullOrEmpty(attValue) || serializeAs == SettingsSerializeAs.String))
				return attValue;

			// Return null if there is nothing to convert
			if (string.IsNullOrEmpty(attValue))
				return null;

			// Convert based on the serialized type
			switch (serializeAs)
			{
				case SettingsSerializeAs.Binary:
					byte[] buf = Convert.FromBase64String(attValue);
					MemoryStream ms = null;
					try
					{
						ms = new MemoryStream(buf);
						return (new BinaryFormatter()).Deserialize(ms);
					}
					finally
					{
						if (ms != null)
							ms.Close();
					}

				case SettingsSerializeAs.Xml:
					StringReader sr = new StringReader(attValue);
					XmlSerializer xs = new XmlSerializer(type);
					return xs.Deserialize(sr);

				case SettingsSerializeAs.String:
					TypeConverter converter = TypeDescriptor.GetConverter(type);
					if (converter != null && converter.CanConvertTo(typeof(String)) && converter.CanConvertFrom(typeof(String)))
						return converter.ConvertFromString(attValue);
					throw new ArgumentException("Unable to convert type: " + type.ToString() + " from string", "type");

				default:
					return null;
			}
		}

		private static string GetClauseForAuthenticationOptions(ProfileAuthenticationOption authenticationOption)
		{
			switch (authenticationOption)
			{
				case ProfileAuthenticationOption.Anonymous:
					return " AND IsAnonymous='1' ";

				case ProfileAuthenticationOption.Authenticated:
					return " AND IsAnonymous='0' ";

				case ProfileAuthenticationOption.All:
					return " ";

				default: throw new InvalidEnumArgumentException(String.Format("Unknown ProfileAuthenticationOption value: {0}.", authenticationOption.ToString()));
			}
		}

		/// <summary>
		/// Get a reference to the database connection used for profile. If a transaction is currently in progress, and the
		/// connection string of the transaction connection is the same as the connection string for the profile provider,
		/// then the connection associated with the transaction is returned, and it will already be open. If no transaction is in progress,
		/// a new <see cref="DbConnection"/> is created and returned. It will be closed and must be opened by the caller
		/// before using.
		/// </summary>
		/// <returns>A <see cref="DbConnection"/> object.</returns>
		/// <remarks>The transaction is stored in <see cref="System.Web.HttpContext.Current"/>. That means transaction support is limited
		/// to web applications. For other types of applications, there is no transaction support unless this code is modified.</remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		private static IDbConnection GetDbConnectionForProfile()
		{
			return Db.GetConnection();
		}

		/// <summary>
		/// Determines whether a database transaction is in progress for the Profile provider.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if a database transaction is in progress; otherwise, <c>false</c>.
		/// </returns>
		/// <remarks>A transaction is considered in progress if an instance of <see cref="DbTransaction"/> is found in the
		/// <see cref="System.Web.HttpContext.Current"/> Items property and its connection string is equal to the Profile 
		/// provider's connection string. Note that this implementation of <see cref="SqliteProfileProvider"/> never adds a 
		/// <see cref="DbTransaction"/> to <see cref="System.Web.HttpContext.Current"/>, but it is possible that 
		/// another data provider in this application does. This may be because other data is also stored in this Sqlite database,
		/// and the application author wants to provide transaction support across the individual providers. If an instance of
		/// <see cref="System.Web.HttpContext.Current"/> does not exist (for example, if the calling application is not a web application),
		/// this method always returns false.</remarks>
		[Obsolete("Use 'Db.IsTransactionInProgress' instead")]
		private static bool IsTransactionInProgress()
		{
			return Vius.Data.DataProvider.Instance.IsTransactionInProgress();
		}

		#endregion

	}
}
