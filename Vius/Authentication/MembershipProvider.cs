using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Security;

namespace Vius.Authentication
{
	/// <summary>
	/// Provides a Membership implementation whose data is stored in a Sqlite database.
	/// </summary>
	public sealed class ViusMembershipProvider : MembershipProvider
	{
		#region Private Fields

		private static readonly Vius.Data.DataProvider Db = Vius.Data.DataProvider.Instance;

		internal static class TbNames
		{
			public const string User = "AuthUsers";
			public const string Profile = "AuthUserProfile";
			public const string Roles = ViusRoleProvider.TbNames.Roles;
		}

		private const int NEW_PASSWORD_LENGTH = 8;
		private const int MAX_APPLICATION_NAME_LENGTH = 256;
		private const int MAX_USERNAME_LENGTH = 256;
		private const int MAX_PASSWORD_LENGTH = 128;
		private const int MAX_PASSWORD_ANSWER_LENGTH = 128;
		private const int MAX_EMAIL_LENGTH = 256;
		private const int MAX_PASSWORD_QUESTION_LENGTH = 256;

		private bool _enablePasswordReset;
		private bool _requiresQuestionAndAnswer;
		private bool _requiresUniqueEmail;
		private int _maxInvalidPasswordAttempts;
		private int _passwordAttemptWindow;
		private int _minRequiredNonAlphanumericCharacters;
		private int _minRequiredPasswordLength;
		private string _passwordStrengthRegularExpression;
		private readonly DateTime _minDate = DateTime.ParseExact ("01/01/1753", "d", CultureInfo.InvariantCulture);

		#endregion

		#region Public Properties

		/// <summary>
		/// The name of the application using the custom membership provider.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// The name of the application using the custom membership provider.
		/// </returns>
		public override string ApplicationName {
			get { return "Vius"; }
			set {}
		}

		/// <summary>
		/// Indicates whether the membership provider is configured to allow users to reset their passwords.
		/// </summary>
		/// <value></value>
		/// <returns>true if the membership provider supports password reset; otherwise, false. The default is true.
		/// </returns>
		public override bool EnablePasswordReset {
			get { return _enablePasswordReset; }
		}

		/// <summary>
		/// The database layer encrypts all passwords, therefore this is always false
		/// </summary>
		/// <value></value>
		/// <returns>true if the membership provider is configured to support password retrieval; otherwise, false. The default is false.
		/// </returns>
		public override bool EnablePasswordRetrieval {
			get { return false; }
		}

		/// <summary>
		/// Gets a value indicating whether the membership provider is configured to require the user to answer a password question for password reset and retrieval.
		/// </summary>
		/// <value></value>
		/// <returns>true if a password answer is required for password reset and retrieval; otherwise, false. The default is true.
		/// </returns>
		public override bool RequiresQuestionAndAnswer {
			get { return _requiresQuestionAndAnswer; }
		}

		/// <summary>
		/// Gets a value indicating whether the membership provider is configured to require a unique e-mail address for each user name.
		/// </summary>
		/// <value></value>
		/// <returns>true if the membership provider requires a unique e-mail address; otherwise, false. The default is true.
		/// </returns>
		public override bool RequiresUniqueEmail {
			get { return _requiresUniqueEmail; }
		}

		/// <summary>
		/// Gets the number of invalid password or password-answer attempts allowed before the membership user is locked out.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// The number of invalid password or password-answer attempts allowed before the membership user is locked out.
		/// </returns>
		public override int MaxInvalidPasswordAttempts {
			get { return _maxInvalidPasswordAttempts; }
		}

		/// <summary>
		/// Gets the number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// The number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
		/// </returns>
		public override int PasswordAttemptWindow {
			get { return _passwordAttemptWindow; }
		}

		/// <summary>
		/// Gets a value indicating the format for storing passwords in the membership data store.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// One of the <see cref="T:System.Web.Security.MembershipPasswordFormat"/> values indicating the format for storing passwords in the data store.
		/// </returns>
		public override MembershipPasswordFormat PasswordFormat {
			get { return MembershipPasswordFormat.Hashed; }
		}

		/// <summary>
		/// Gets the minimum number of special characters that must be present in a valid password.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// The minimum number of special characters that must be present in a valid password.
		/// </returns>
		public override int MinRequiredNonAlphanumericCharacters {
			get { return _minRequiredNonAlphanumericCharacters; }
		}

		/// <summary>
		/// Gets the minimum length required for a password.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// The minimum length required for a password.
		/// </returns>
		public override int MinRequiredPasswordLength {
			get { return _minRequiredPasswordLength; }
		}

		/// <summary>
		/// Gets the regular expression used to evaluate a password.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// A regular expression used to evaluate a password.
		/// </returns>
		public override string PasswordStrengthRegularExpression {
			get { return _passwordStrengthRegularExpression; }
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
				throw new ArgumentNullException ("config");

			if (string.IsNullOrEmpty (name))
				name = "ViusMembershipProvider";

			if (String.IsNullOrEmpty (config ["description"])) {
				config.Remove ("description");
				config.Add ("description", "Vius Postgres Membership provider");
			}

			// Initialize the abstract base class.
			base.Initialize (name, config);

			_maxInvalidPasswordAttempts = Convert.ToInt32 (GetConfigValue (config ["maxInvalidPasswordAttempts"], "50"));
			_passwordAttemptWindow = Convert.ToInt32 (GetConfigValue (config ["passwordAttemptWindow"], "10"));
			_minRequiredNonAlphanumericCharacters = Convert.ToInt32 (GetConfigValue (config ["minRequiredNonalphanumericCharacters"], "1"));
			_minRequiredPasswordLength = Convert.ToInt32 (GetConfigValue (config ["minRequiredPasswordLength"], "7"));
			_passwordStrengthRegularExpression = Convert.ToString (GetConfigValue (config ["passwordStrengthRegularExpression"], ""));
			_enablePasswordReset = Convert.ToBoolean (GetConfigValue (config ["enablePasswordReset"], "true"));
			_requiresQuestionAndAnswer = Convert.ToBoolean (GetConfigValue (config ["requiresQuestionAndAnswer"], "false"));
			_requiresUniqueEmail = Convert.ToBoolean (GetConfigValue (config ["requiresUniqueEmail"], "false"));

			ValidatePwdStrengthRegularExpression ();

			if (_minRequiredNonAlphanumericCharacters > _minRequiredPasswordLength) {
				_minRequiredNonAlphanumericCharacters = _minRequiredPasswordLength;
			}

		}

		/// <summary>
		/// Processes a request to update the password for a membership user.
		/// </summary>
		/// <param name="username">The user to update the password for.</param>
		/// <param name="oldPassword">The current password for the specified user.</param>
		/// <param name="newPassword">The new password for the specified user.</param>
		/// <returns>
		/// true if the password was updated successfully; otherwise, false.
		/// </returns>
		public override bool ChangePassword (string username, string oldPassword, string newPassword)
		{
			SecUtility.CheckParameter (ref username, true, true, true, MAX_USERNAME_LENGTH, "username");
			SecUtility.CheckParameter (ref oldPassword, true, true, false, MAX_PASSWORD_LENGTH, "oldPassword");
			SecUtility.CheckParameter (ref newPassword, true, true, false, MAX_PASSWORD_LENGTH, "newPassword");

			if (!CheckPassword (username, oldPassword, true))
				return false;

			if (newPassword.Length < this.MinRequiredPasswordLength) {
				throw new ArgumentException (String.Format (CultureInfo.CurrentCulture, "The password must be at least {0} characters.", this.MinRequiredPasswordLength));
			}

			int numNonAlphaNumericChars = 0;
			for (int i = 0; i < newPassword.Length; i++) {
				if (!char.IsLetterOrDigit (newPassword, i)) {
					numNonAlphaNumericChars++;
				}
			}
			if (numNonAlphaNumericChars < this.MinRequiredNonAlphanumericCharacters) {
				throw new ArgumentException (String.Format (CultureInfo.CurrentCulture, "There must be at least {0} non alpha numeric characters.", this.MinRequiredNonAlphanumericCharacters));
			}
			if ((this.PasswordStrengthRegularExpression.Length > 0) && !Regex.IsMatch (newPassword, this.PasswordStrengthRegularExpression)) {
				throw new ArgumentException ("The password does not match the regular expression in the config file.");
			}

			ValidatePasswordEventArgs args = new ValidatePasswordEventArgs (username, newPassword, false);

			OnValidatingPassword (args);

			if (args.Cancel) {
				if (args.FailureInformation != null)
					throw args.FailureInformation;
				else
					throw new MembershipPasswordException ("Change password canceled due to new password validation failure.");
			}

			DbConnection cn = GetDBConnectionForMembership ();
			try {
				using (var cmd = cn.CreateCommand()) {
					cmd.CommandText = 
						"UPDATE " + TbNames.User +
						"SET    Password = :newpass, " +
						"       LastPasswordChangedDate = current_timestamp " +
						"WHERE  LoweredUsername = :Username and \n" +
						"       :oldpass = crypt(password,:oldpass)";

					DbParameter param;

					param = cmd.CreateParameter();
					param.ParameterName = ":newpass";
					param.DbType = DbType.AnsiString;
					param.Value = newPassword;
					cmd.Parameters.Add(param);

					param = cmd.CreateParameter();
					param.ParameterName = ":oldpass";
					param.DbType = DbType.AnsiString;
					param.Value = oldPassword;
					cmd.Parameters.Add(param);

					param = cmd.CreateParameter();
					param.ParameterName = ":username";
					param.DbType = DbType.AnsiString;
					param.Value = username.ToLower();
					cmd.Parameters.Add(param);

					if (cn.State == ConnectionState.Closed){
						cn.Open ();
					}

					return (cmd.ExecuteNonQuery() > 0);
				}
			} finally {
				if (!IsTransactionInProgress()){
					cn.Dispose();
				}
			}
		}

		/// <summary>
		/// Processes a request to update the password question and answer for a membership user.
		/// </summary>
		/// <param name="username">The user to change the password question and answer for.</param>
		/// <param name="password">The password for the specified user.</param>
		/// <param name="newPasswordQuestion">The new password question for the specified user.</param>
		/// <param name="newPasswordAnswer">The new password answer for the specified user.</param>
		/// <returns>
		/// true if the password question and answer are updated successfully; otherwise, false.
		/// </returns>
		public override bool ChangePasswordQuestionAndAnswer (string username, string password, string newPasswordQuestion, string newPasswordAnswer)
		{
			SecUtility.CheckParameter(ref username, true, true, true, MAX_USERNAME_LENGTH, "username");
			SecUtility.CheckParameter(ref password, true, true, false, MAX_PASSWORD_LENGTH, "password");

			if (!CheckPassword(username, password, true)) {
				return false;
			}

			SecUtility.CheckParameter (ref newPasswordQuestion, this.RequiresQuestionAndAnswer, this.RequiresQuestionAndAnswer, false, MAX_PASSWORD_QUESTION_LENGTH, "newPasswordQuestion");
			if (newPasswordAnswer != null) {
				newPasswordAnswer = newPasswordAnswer.Trim ();
			}

			SecUtility.CheckParameter (ref newPasswordAnswer, this.RequiresQuestionAndAnswer, this.RequiresQuestionAndAnswer, false, MAX_PASSWORD_ANSWER_LENGTH, "newPasswordAnswer");

			DbConnection cn = GetDBConnectionForMembership ();
			try {
				using (DbCommand cmd = cn.CreateCommand()) {
					cmd.CommandText = 
						"UPDATE " + TbNames.User + " \n" +
						"SET    PasswordQuestion = :Question, " +
						"       PasswordAnswer   = :Answer \n" +
						"WHERE  LoweredUsername  = lower(:Username) \n";

					var param = cmd.CreateParameter();
					param.DbType = DbType.String;
					param.Value = newPasswordQuestion;
					param.ParameterName = ":Question";
					cmd.Parameters.Add(param);

					param = cmd.CreateParameter();
					param.DbType = DbType.String;
					param.ParameterName = ":Answer";
					param.Value = newPasswordAnswer;
					cmd.Parameters.Add(param);

					param = cmd.CreateParameter();
					param.DbType = DbType.String;
					param.ParameterName = ":Username";
					param.Value = username.ToLower();
					cmd.Parameters.Add(param);

					if (cn.State == ConnectionState.Closed){
						cn.Open ();
					}

					return (cmd.ExecuteNonQuery () > 0);
				}
			} finally {
				if (!IsTransactionInProgress ())
					cn.Dispose ();
			}
		}

		/// <summary>
		/// Adds a new membership user to the data source.
		/// </summary>
		/// <param name="username">The user name for the new user.</param>
		/// <param name="password">The password for the new user.</param>
		/// <param name="email">The e-mail address for the new user.</param>
		/// <param name="passwordQuestion">The password question for the new user.</param>
		/// <param name="passwordAnswer">The password answer for the new user</param>
		/// <param name="isApproved">Whether or not the new user is approved to be validated.</param>
		/// <param name="providerUserKey">The unique identifier from the membership data source for the user.</param>
		/// <param name="status">A <see cref="T:System.Web.Security.MembershipCreateStatus"/> enumeration value indicating whether the user was created successfully.</param>
		/// <returns>
		/// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the information for the newly created user.
		/// </returns>
		public override MembershipUser CreateUser (string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
		{
			#region Validation

			if (!SecUtility.ValidateParameter (ref password, true, true, false, MAX_PASSWORD_LENGTH)) {
				status = MembershipCreateStatus.InvalidPassword;
				return null;
			}

			if (password.Length > MAX_PASSWORD_LENGTH) {
				status = MembershipCreateStatus.InvalidPassword;
				return null;
			}

			if (!string.IsNullOrEmpty (passwordAnswer)) {
				passwordAnswer = passwordAnswer.Trim ();
			}
			if (!string.IsNullOrEmpty (passwordAnswer)) {
				if (passwordAnswer.Length > MAX_PASSWORD_ANSWER_LENGTH) {
					status = MembershipCreateStatus.InvalidAnswer;
					return null;
				}
			}

			if (!SecUtility.ValidateParameter (ref username, true, true, true, MAX_USERNAME_LENGTH)) {
				status = MembershipCreateStatus.InvalidUserName;
				return null;
			}

			if (!SecUtility.ValidateParameter (ref email, this.RequiresUniqueEmail, this.RequiresUniqueEmail, false, MAX_EMAIL_LENGTH)) {
				status = MembershipCreateStatus.InvalidEmail;
				return null;
			}

			if (!SecUtility.ValidateParameter (ref passwordQuestion, this.RequiresQuestionAndAnswer, true, false, MAX_PASSWORD_QUESTION_LENGTH)) {
				status = MembershipCreateStatus.InvalidQuestion;
				return null;
			}

			if ((providerUserKey != null) && !(providerUserKey is Guid)) {
				status = MembershipCreateStatus.InvalidProviderUserKey;
				return null;
			}

			if (password.Length < this.MinRequiredPasswordLength) {
				status = MembershipCreateStatus.InvalidPassword;
				return null;
			}

			int numNonAlphaNumericChars = 0;
			for (int i = 0; i < password.Length; i++) {
				if (!char.IsLetterOrDigit (password, i)) {
					numNonAlphaNumericChars++;
				}
			}

			if (numNonAlphaNumericChars < this.MinRequiredNonAlphanumericCharacters) {
				status = MembershipCreateStatus.InvalidPassword;
				return null;
			}

			if ((this.PasswordStrengthRegularExpression.Length > 0) && !Regex.IsMatch (password, this.PasswordStrengthRegularExpression)) {
				status = MembershipCreateStatus.InvalidPassword;
				return null;
			}

			#endregion

			ValidatePasswordEventArgs args = new ValidatePasswordEventArgs (username, password, true);
			OnValidatingPassword (args);
			if (args.Cancel) {
				status = MembershipCreateStatus.InvalidPassword;
				return null;
			}

			if (RequiresUniqueEmail && !String.IsNullOrEmpty (GetUserNameByEmail (email))) {
				status = MembershipCreateStatus.DuplicateEmail;
				return null;
			}

			MembershipUser u = GetUser (username, false);
			if (u != null) {
				status = MembershipCreateStatus.DuplicateUserName;
				return null;
			}

			DateTime createDate = DateTime.UtcNow;

			MembershipCreateStatus lStatus = MembershipCreateStatus.ProviderError;
			try{
				Db.UseCommand(cmd => {

					cmd.CommandText = 
						"INSERT INTO " + TbNames.User + "( \n" +
						"    Username, \n" +
						"    Email, \n" +
						"    Pass, \n" +
						"    PassQuestion, \n" +
						"    PassAnswer, \n" +
						"    IsApproved, \n" +
						"    IsAnonymous, \n" +
						"    LastActivityDate, \n" +
						"    LastLoginDate, \n" +
						"    LastPasswordChangedDate, \n" +
						"    FailedPasswordAttemptCount, \n" +
						"    FailedPasswordAttemptWindowStart, \n" +
						"    FailedPasswordAnswerAttemptCount, \n" +
						"    FailedPasswordAnswerAttemptWindowStart \n" +
						") \n" +
						"Values ( \n" +
						"    :Username, \n" +
						"    :Email, \n" +
						"    :Password, \n" +
						"    :PasswordQuestion, \n" +
						"    :PasswordAnswer, \n" +
						"    :IsApproved, \n" +
						"    :IsAnonymous, \n" +
						"    :LastActivityDate, \n" +
						"    :LastLoginDate, \n" +
						"    :LastPasswordChangedDate, \n" +
						"    :FailedPasswordAttemptCount, \n" +
						"    :FailedPasswordAttemptWindowStart, \n" +
						"    :FailedPasswordAnswerAttemptCount, \n" +
						"    :FailedPasswordAnswerAttemptWindowStart \n" +
						") \n";

					DateTime nullDate = _minDate;
					IDbDataParameter p;

					p = cmd.CreateParameter();
					p.ParameterName = ":Username";
					p.Value = username;
					cmd.Parameters.Add(p);

					p = cmd.CreateParameter();
					p.ParameterName = ":Email";
					p.Value = email;
					cmd.Parameters.Add(p);

					p = cmd.CreateParameter();
					p.ParameterName = ":Password";
					p.Value = password;
					cmd.Parameters.Add(p);

					p = cmd.CreateParameter();
					p.ParameterName = ":PasswordFormat";
					p.Value = PasswordFormat.ToString ();
					cmd.Parameters.Add(p);

					p = cmd.CreateParameter();
					p.ParameterName = ":PasswordQuestion";
					p.Value = passwordQuestion;
					cmd.Parameters.Add(p);

					p = cmd.CreateParameter();
					p.ParameterName = ":PasswordAnswer";
					p.Value = passwordAnswer;
					cmd.Parameters.Add(p);

					p = cmd.CreateParameter();
					p.ParameterName = ":IsApproved";
					p.Value = isApproved;
					cmd.Parameters.Add(p);

					p = cmd.CreateParameter();
					p.ParameterName = ":IsAnonymous";
					p.Value = false;
					cmd.Parameters.Add(p);

					p = cmd.CreateParameter();
					p.ParameterName = ":LastActivityDate";
					p.Value = createDate;
					cmd.Parameters.Add(p);

					p = cmd.CreateParameter();
					p.ParameterName = ":LastLoginDate";
					p.Value = createDate;
					cmd.Parameters.Add(p);

					p = cmd.CreateParameter();
					p.ParameterName = ":LastPasswordChangedDate";
					p.Value = createDate;
					cmd.Parameters.Add(p);

					p = cmd.CreateParameter();
					p.ParameterName = ":FailedPasswordAttemptCount";
					p.Value = 0;
					cmd.Parameters.Add(p);

					p = cmd.CreateParameter();
					p.ParameterName = ":FailedPasswordAttemptWindowStart";
					p.Value = nullDate;
					cmd.Parameters.Add(p);

					p = cmd.CreateParameter();
					p.ParameterName = ":FailedPasswordAnswerAttemptCount";
					p.Value = 0;
					cmd.Parameters.Add(p);

					p = cmd.CreateParameter();
					p.ParameterName = ":FailedPasswordAnswerAttemptWindowStart";
					p.Value = nullDate;
					cmd.Parameters.Add(p);

					if (cmd.ExecuteNonQuery () > 0) {
						lStatus = MembershipCreateStatus.Success;
					} else {
						lStatus = MembershipCreateStatus.UserRejected;
					}
				});
			} catch {
				status = MembershipCreateStatus.ProviderError;
				throw;
			}

			status = lStatus;
			return GetUser (username, false);
		}

		/// <summary>
		/// Removes a user from the membership data source.
		/// </summary>
		/// <param name="username">The name of the user to delete.</param>
		/// <param name="deleteAllRelatedData">true to delete data related to the user from the database; false to leave data related to the user in the database.</param>
		/// <returns>
		/// true if the user was successfully deleted; otherwise, false.
		/// </returns>
		public override bool DeleteUser (string username, bool deleteAllRelatedData)
		{
			DbConnection cn = GetDBConnectionForMembership ();
			try {
				DbParameter p;
				using (DbCommand cmd = cn.CreateCommand()) {
					if (cn.State == ConnectionState.Closed){
						cn.Open ();
					}

					// Get UserId if necessary.
					string userId = null;
					cmd.CommandText = 
						"SELECT UserId " +
						"FROM   " + TbNames.User + " " +
						"WHERE  LoweredUsername = :Username ";

					p = cmd.CreateParameter();
					p.ParameterName = ":Username";
					p.Value = username.ToLowerInvariant ();
					cmd.Parameters.Add(p);

					userId = cmd.ExecuteScalar () as string;

					cmd.Parameters.Clear();
					p = cmd.CreateParameter();
					p.ParameterName = ":UserId";
					p.Value = username.ToLowerInvariant ();
					cmd.Parameters.Add(p);

					// start deleting
					cmd.CommandText = 
						"DELETE " +
						"FROM   " + TbNames.User + " " +
						"WHERE  UserId = :UserId ";
					int rowsAffected = cmd.ExecuteNonQuery ();

					if (deleteAllRelatedData && (!String.IsNullOrEmpty ((userId)))) {
						// Delete from user/role relationship table.
						cmd.CommandText = 
							"DELETE " +
							"FROM   " + TbNames.Roles + " " +
							"WHERE  UserId = :UserId";
						cmd.ExecuteNonQuery ();

						// Delete from profile table.
						cmd.CommandText = 
							"DELETE " +
							"FROM   " + TbNames.Profile + " " +
							"WHERE  UserId = :UserId";
						cmd.ExecuteNonQuery ();
					}

					return (rowsAffected > 0);
				}
			} finally {
				if (!IsTransactionInProgress ())
					cn.Dispose ();
			}
		}

		/// <summary>
		/// Gets a collection of all the users in the data source in pages of data.
		/// </summary>
		/// <param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.</param>
		/// <param name="pageSize">The size of the page of results to return.</param>
		/// <param name="totalRecords">The total number of matched users.</param>
		/// <returns>
		/// A <see cref="T:System.Web.Security.MembershipUserCollection"/> collection that contains a page of <paramref name="pageSize"/><see cref="T:System.Web.Security.MembershipUser"/> objects beginning at the page specified by <paramref name="pageIndex"/>.
		/// </returns>
		public override MembershipUserCollection GetAllUsers (int pageIndex, int pageSize, out int totalRecords)
		{
			MembershipUserCollection users = new MembershipUserCollection();
			int total = 0;
			Db.UseCommand( cmd => {
				cmd.CommandText = 
					"SELECT Count(*) " +
					"FROM   " + TbNames.User + " " +
					"WHERE  IsAnonymous='0'";

				total = Convert.ToInt32(cmd.ExecuteScalar());
				if (total <= 0) {
					return;
				}

				cmd.CommandText = 
					"SELECT * \n" +
					"FROM   " + TbNames.User + "\n" +
					"WHERE  IsAnonymous=false \n" +
					"ORDER BY Username Asc \n";

				using (var reader = cmd.ExecuteReader()) {
					int counter = 0;
					int startIndex = pageSize * pageIndex;
					int endIndex = startIndex + pageSize - 1;

					while (reader.Read()) {
						if (counter >= startIndex) {
							MembershipUser u = GetUserFromReader(reader);
							users.Add(u);
						}
						if (counter >= endIndex) {
							cmd.Cancel();
						}
						counter++;
					}

				}
			});
			totalRecords = total;
			return users;
		}

		/// <summary>
		/// Gets the number of users currently accessing the application.
		/// </summary>
		/// <returns>
		/// The number of users currently accessing the application.
		/// </returns>
		public override int GetNumberOfUsersOnline ()
		{
			DbConnection cn = GetDBConnectionForMembership ();
			try {
				using (DbCommand cmd = cn.CreateCommand()) {
					cmd.CommandText = 
						"SELECT Count(*) " +
						"FROM   " + TbNames.User +
						"WHERE  LastActivityDate > :LastActivityDate AND " +
						"       ApplicationId = :ApplicationId";

					TimeSpan onlineSpan = new TimeSpan (0, Membership.UserIsOnlineTimeWindow, 0);
					DateTime compareTime = DateTime.UtcNow.Subtract (onlineSpan);

					var p = cmd.CreateParameter();
					p.ParameterName = ":LastActivityDate";
					p.Value = compareTime;
					cmd.Parameters.Add(p);

					if (cn.State == ConnectionState.Closed){
						cn.Open ();
					}

					return Convert.ToInt32(cmd.ExecuteScalar());
				}
			} finally {
				if (!IsTransactionInProgress ()){
					cn.Dispose ();
				}
			}
		}

		/// <summary>
		/// Gets the password for the specified user name from the data source.
		/// </summary>
		/// <param name="username">The user to retrieve the password for.</param>
		/// <param name="answer">The password answer for the user.</param>
		/// <returns>
		/// The password for the specified user name.
		/// </returns>
		public override string GetPassword (string username, string answer)
		{
			throw new ProviderException ("Cannot retrieve hashed passwords.");
		}

		/// <summary>
		/// Gets information from the data source for a user. Provides an option to update the last-activity date/time stamp for the user.
		/// </summary>
		/// <param name="username">The name of the user to get information for.</param>
		/// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
		/// <returns>
		/// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the specified user's information from the data source.
		/// </returns>
		public override MembershipUser GetUser (string username, bool userIsOnline)
		{
				MembershipUser user = null;
				IDbDataParameter p;
				Db.UseCommand(cmd =>{
					cmd.CommandText = 
						"SELECT UserId, \n" +
						"       Username, " +
						"       Email, \n" +
						"       PassQuestion, \n" +
						"       Comment, \n" +
						"       IsApproved, \n" +
						"       CreateDate, LastLoginDate, \n" +
						"       LastActivityDate, \n" +
						"       LastPasswordChangedDate \n" +
						"FROM   " + TbNames.User + " \n" +
						"WHERE  UsernameLowered = lower(:Username) \n";
					
					p = cmd.CreateParameter();
					p.ParameterName = ":Username";
					p.Value = username.ToLowerInvariant();
					cmd.Parameters.Add(p);
					
					using (var dr = cmd.ExecuteReader()) {
						if (dr.Read()) {
							user = GetUserFromReader (dr);
						}
					}
					
					if (userIsOnline) {
						cmd.CommandText =
							"UPDATE " + TbNames.User + " \n" +
							"SET    LastActivityDate = :LastActivityDate \n" +
							"WHERE  LoweredUsername = lower(:Username) \n";
						
						p = cmd.CreateParameter();
						p.ParameterName = ":LastActivityDate";
						p.Value = DateTime.UtcNow;
						cmd.Parameters.Add(p);
						
						cmd.ExecuteNonQuery();
					}
					
				});
				return user;
		}

		/// <summary>
		/// Gets user information from the data source based on the unique identifier for the membership user. Provides an option to update the last-activity date/time stamp for the user.
		/// </summary>
		/// <param name="providerUserKey">The unique identifier for the membership user to get information for.</param>
		/// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
		/// <returns>
		/// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the specified user's information from the data source.
		/// </returns>
		public override MembershipUser GetUser (object providerUserKey, bool userIsOnline)
		{
			DbParameter p;
			DbConnection cn = GetDBConnectionForMembership ();
			try {
				using (DbCommand cmd = cn.CreateCommand()) {
					cmd.CommandText = "SELECT UserId, Username, Email, PasswordQuestion,"
						+ " Comment, IsApproved, IsLockedOut, CreateDate, LastLoginDate,"
						+ " LastActivityDate, LastPasswordChangedDate, LastLockoutDate"
						+ " FROM " + TbNames.User + " WHERE UserId = :UserId";

					p = cmd.CreateParameter();
					p.DbType = DbType.Int32;
					p.ParameterName = ":UserId";
					p.Value = Convert.ToInt32(providerUserKey);
					cmd.Parameters.Add(p);

					MembershipUser user = null;

					if (cn.State == ConnectionState.Closed){
						cn.Open ();
					}

					using (DbDataReader dr = cmd.ExecuteReader()) {
						if (dr.HasRows) {
							dr.Read ();
							user = GetUserFromReader (dr);
						}
					}

					if (userIsOnline) {
						cmd.CommandText = 
							"UPDATE " + TbNames.User+ " " +
							"SET    LastActivityDate = :LastActivityDate " +
							"WHERE  UserId = :UserId";

						p = cmd.CreateParameter();
						p.ParameterName = ":LastActivityDate";
						p.Value = DateTime.UtcNow;
						cmd.Parameters.Add(p);

						cmd.ExecuteNonQuery ();
					}

					return user;
				}
			} finally {
				if (!IsTransactionInProgress()){
					cn.Dispose();
				}
			}
		}

		/// <summary>
		/// Unlocks the user.
		/// </summary>
		/// <param name="username">The username.</param>
		/// <returns>Returns true if user was unlocked; otherwise returns false.</returns>
		public override bool UnlockUser (string username)
		{
			DbParameter p;
			DbConnection cn = GetDBConnectionForMembership ();
			try {
				using (DbCommand cmd = cn.CreateCommand()) {
					cmd.CommandText = 
						"UPDATE " + TbNames.User + " " +
						"SET    IsLockedOut = '0', " +
						"       FailedPasswordAttemptCount = 0, " +
						"       FailedPasswordAttemptWindowStart = :MinDate, " +
						"       FailedPasswordAnswerAttemptCount = 0," +
						"       FailedPasswordAnswerAttemptWindowStart = :MinDate " +
						"WHERE  LoweredUsername = :Username";

					p = cmd.CreateParameter();
					p.ParameterName = ":MinDate";
					p.Value = _minDate;
					cmd.Parameters.Add(p);

					p = cmd.CreateParameter();
					p.ParameterName = ":Username";
					p.Value = username.ToLowerInvariant();
					cmd.Parameters.Add(p);

					if (cn.State == ConnectionState.Closed){
						cn.Open();
					}

					return (cmd.ExecuteNonQuery () > 0);
				}
			} finally {
				if (!IsTransactionInProgress()){
					cn.Dispose();
				}
			}
		}

		/// <summary>
		/// Gets the user name associated with the specified e-mail address.
		/// </summary>
		/// <param name="email">The e-mail address to search for.</param>
		/// <returns>
		/// The user name associated with the specified e-mail address. If no match is found, return null.
		/// </returns>
		public override string GetUserNameByEmail (string email)
		{
			if (email == null) {
				return null;
			}

			DbParameter p;
			DbConnection cn = GetDBConnectionForMembership ();
			try {
				using (DbCommand cmd = cn.CreateCommand()) {
					cmd.CommandText = 
						"SELECT Username" +
						"FROM   " + TbNames.User + " " +
						"WHERE LoweredEmail = :Email ";

					p = cmd.CreateParameter();
					p.ParameterName = ":Email";
					p.Value = email.ToLowerInvariant();
					cmd.Parameters.Add(p);

					if (cn.State == ConnectionState.Closed){
						cn.Open ();
					}

					return (cmd.ExecuteScalar () as string);
				}
			} finally {
				if (!IsTransactionInProgress ())
					cn.Dispose ();
			}
		}

		/// <summary>
		/// Resets a user's password to a new, automatically generated password.
		/// </summary>
		/// <param name="username">The user to reset the password for.</param>
		/// <param name="passwordAnswer">The password answer for the specified user.</param>
		/// <returns>The new password for the specified user.</returns>
		/// <exception cref="T:System.Configuration.Provider.ProviderException">username is not found in the membership database.- or -The 
		/// change password action was canceled by a subscriber to the System.Web.Security.Membership.ValidatePassword
		/// event and the <see cref="P:System.Web.Security.ValidatePasswordEventArgs.FailureInformation"></see> property was null.- or -An 
		/// error occurred while retrieving the password from the database. </exception>
		/// <exception cref="T:System.NotSupportedException"><see cref="P:System.Web.Security.SqlMembershipProvider.EnablePasswordReset"></see> 
		/// is set to false. </exception>
		/// <exception cref="T:System.ArgumentException">username is an empty string (""), contains a comma, or is longer than 256 characters.
		/// - or -passwordAnswer is an empty string or is longer than 128 characters and 
		/// <see cref="P:System.Web.Security.SqlMembershipProvider.RequiresQuestionAndAnswer"></see> is true.- or -passwordAnswer is longer 
		/// than 128 characters after encoding.</exception>
		/// <exception cref="T:System.ArgumentNullException">username is null.- or -passwordAnswer is null and 
		/// <see cref="P:System.Web.Security.SqlMembershipProvider.RequiresQuestionAndAnswer"></see> is true.</exception>
		/// <exception cref="T:System.Web.Security.MembershipPasswordException">passwordAnswer is invalid. - or -The user account is currently locked out.</exception>
		public override string ResetPassword (string username, string passwordAnswer)
		{
//			string salt;
//			MembershipPasswordFormat passwordFormat;
//			string passwordFromDb;
//			int failedPwdAttemptCount;
//			int failedPwdAnswerAttemptCount;
//			bool isApproved;
//			DateTime lastLoginDate;
//			DateTime lastActivityDate;
			if (!this.EnablePasswordReset) {
				throw new NotSupportedException ("This provider is not configured to allow password resets. To enable password reset, set enablePasswordReset to \"true\" in the configuration file.");
			}
			SecUtility.CheckParameter (ref username, true, true, true, 0x100, "username");

			if (passwordAnswer != null) {
				passwordAnswer = passwordAnswer.Trim ();
			}

			string newPassword = Membership.GeneratePassword (NEW_PASSWORD_LENGTH, MinRequiredNonAlphanumericCharacters);

			ValidatePasswordEventArgs e = new ValidatePasswordEventArgs (username, newPassword, false);
			this.OnValidatingPassword (e);
			if (e.Cancel) {
				if (e.FailureInformation != null) {
					throw e.FailureInformation;
				}
				throw new ProviderException ("The custom password validation failed.");
			}

			// From this point on the only logic I need to implement is that contained in aspnet_Membership_ResetPassword.
			Db.UseCommand(cmd => {
				cmd.CommandText = 
					"SELECT PasswordAnswer = crypt(:SuppliedAnswer,PasswordAnswer) as AnswerMatches, \n" +
					"       IsLockedOut \n" +
					"FROM " + TbNames.User + "\n" +
					"WHERE LoweredUsername = :Username ";

				var p = cmd.CreateParameter();
				p.ParameterName = ":Username";
				p.Value = username.ToLowerInvariant();
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = ":SuppliedAnswer";
				p.Value = passwordAnswer;
				cmd.Parameters.Add(p);

				using (var dr = cmd.ExecuteReader(CommandBehavior.SingleRow)) {
					if (dr.Read ()){
						if (dr.GetBoolean(1)){
							throw new MembershipPasswordException ("The supplied user is locked out.");
						}
						if (RequiresQuestionAndAnswer && !dr.GetBoolean(0)) {
							UpdateFailureCount (username, "passwordAnswer", false);
							throw new MembershipPasswordException ("Incorrect password answer.");
						}
					} else {
						throw new MembershipPasswordException ("The supplied user name is not found.");
					}
				}

				cmd.CommandText = 
					"UPDATE " + TbNames.User + " " +
					"SET    Password = :NewPass, " +
					"       FailedPasswordAttemptCount = 0, " +
					"       FailedPasswordAttemptWindowStart = :MinDate, " +
					"       FailedPasswordAnswerAttemptCount = 0, " +
					"       FailedPasswordAnswerAttemptWindowStart = :MinDate" +
					"WHERE  UsernameLowered = lower(:Username) AND " +
					"       IsLockedOut = false";

				cmd.Parameters.Clear ();

				p = cmd.CreateParameter();
				p.ParameterName = ":NewPass";
				p.Value = newPassword;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = ":MinDate";
				p.Value = _minDate;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = ":Username";
				p.Value = username;
				cmd.Parameters.Add(p);

				if (cmd.ExecuteNonQuery() < 1) {
					throw new MembershipPasswordException ("User not found, or user is locked out. Password not reset.");
				}
			});
			return newPassword;
		}

		/// <summary>
		/// Updates information about a user in the data source.
		/// </summary>
		/// <param name="user">A <see cref="T:System.Web.Security.MembershipUser"/> object that represents the user to update and the updated information for the user.</param>
		public override void UpdateUser (MembershipUser user)
		{
			DbParameter p;
			DbConnection cn = GetDBConnectionForMembership ();
			try {
				using (DbCommand cmd = cn.CreateCommand()) {
					cmd.CommandText = 
						"UPDATE " + TbNames.User + " " +
						"SET Email = :Email, LoweredEmail = :LoweredEmail, Comment = :Comment," +
						"    IsApproved = :IsApproved" +
						"WHERE LoweredUsername = :Username";

					p = cmd.CreateParameter();
					p.ParameterName = ":Email";
					p.Value = user.Email;
					cmd.Parameters.Add(p);

					p = cmd.CreateParameter();
					p.ParameterName = ":LoweredEmail";
					p.Value = user.Email.ToLowerInvariant();
					cmd.Parameters.Add(p);

					p = cmd.CreateParameter();
					p.ParameterName = ":Comment";
					p.Value = user.Comment;
					cmd.Parameters.Add(p);

					p = cmd.CreateParameter();
					p.ParameterName = ":IsApproved";
					p.Value = user.IsApproved;
					cmd.Parameters.Add(p);

					p = cmd.CreateParameter();
					p.ParameterName = ":Username";
					p.Value = user.UserName.ToLowerInvariant();
					cmd.Parameters.Add(p);

					if (cn.State == ConnectionState.Closed){
						cn.Open ();
					}
					cmd.ExecuteNonQuery ();
				}
			} finally {
				if (!IsTransactionInProgress ())
					cn.Dispose ();
			}
		}

		/// <summary>
		/// Verifies that the specified user name and password exist in the data source.
		/// </summary>
		/// <param name="username">The name of the user to validate.</param>
		/// <param name="password">The password for the specified user.</param>
		/// <returns>
		/// true if the specified username and password are valid; otherwise, false.
		/// </returns>
		public override bool ValidateUser (string username, string password)
		{
			if (!SecUtility.ValidateParameter (ref username, true, true, true, MAX_USERNAME_LENGTH) || !SecUtility.ValidateParameter (ref password, true, true, false, MAX_PASSWORD_LENGTH)) {
				return false;
			}

			bool isAuthenticated = CheckPassword (username, password, true);
			if (isAuthenticated) {
				// User is authenticated. Update last activity and last login dates.
				Db.UseCommand( cmd =>{
					cmd.CommandText = 
						"UPDATE " + TbNames.User + " \n" +
						"SET    LastActivityDate = current_timestamp, \n" +
						"       LastLoginDate = current_timestamp \n" + 
						"WHERE  UsernameLowered = lower(:Username) \n";

					var p = cmd.CreateParameter();
					p.ParameterName = ":Username";
					p.Value = username.ToLowerInvariant ();
					cmd.Parameters.Add(p);

					cmd.ExecuteNonQuery ();
				});
			}

			return isAuthenticated;
		}

		/// <summary>
		/// Gets a collection of membership users where the user name contains the specified user name to match.
		/// </summary>
		/// <param name="usernameToMatch">The user name to search for.</param>
		/// <param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.</param>
		/// <param name="pageSize">The size of the page of results to return.</param>
		/// <param name="totalRecords">The total number of matched users.</param>
		/// <returns>
		/// A <see cref="T:System.Web.Security.MembershipUserCollection"/> collection that contains a page of <paramref name="pageSize"/><see cref="T:System.Web.Security.MembershipUser"/> objects beginning at the page specified by <paramref name="pageIndex"/>.
		/// </returns>
		public override MembershipUserCollection FindUsersByName (string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			DbParameter p;
			DbConnection cn = GetDBConnectionForMembership ();
			try {
				using (DbCommand cmd = cn.CreateCommand()) {
					cmd.CommandText = 
						"SELECT Count(*) " +
						"FROM " + TbNames.User +
						"WHERE LoweredUsername LIKE :UsernameSearch";

					p = cmd.CreateParameter();
					p.ParameterName = ":UsernameSearch";
					p.Value = usernameToMatch.ToLowerInvariant ();
					cmd.Parameters.Add(p);

					if (cn.State == ConnectionState.Closed){
						cn.Open ();
					}

					totalRecords = Convert.ToInt32 (cmd.ExecuteScalar ());

					MembershipUserCollection users = new MembershipUserCollection ();

					if (totalRecords <= 0) {
						return users;
					}

					cmd.CommandText = 
						"SELECT UserId, " +
						"       Username, " +
						"       Email, " +
						"       PassQuestion," +
						"       Comment, " +
						"       IsApproved, " +
						"       IsLockedOut, " +
						"       CreateDate, " +
						"       LastLoginDate," +
						"       LastActivityDate, " +
						"       LastPasswordChangedDate, " +
						"       LastLockoutDate " + " " +
						"FROM   " + TbNames.User + " " +
						"WHERE  LoweredUsername LIKE :UsernameSearch " +
						"ORDER BY Username Asc";

					using (DbDataReader dr = cmd.ExecuteReader()) {
						int counter = 0;
						int startIndex = pageSize * pageIndex;
						int endIndex = startIndex + pageSize - 1;

						while (dr.Read()) {
							if (counter >= startIndex) {
								MembershipUser u = GetUserFromReader (dr);
								users.Add (u);
							}

							if (counter >= endIndex) {
								cmd.Cancel ();
							}

							counter++;
						}
					}

					return users;
				}
			} finally {
				if (!IsTransactionInProgress ())
					cn.Dispose ();
			}
		}

		/// <summary>
		/// Gets a collection of membership users where the e-mail address contains the specified e-mail address to match.
		/// </summary>
		/// <param name="emailToMatch">The e-mail address to search for.</param>
		/// <param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.</param>
		/// <param name="pageSize">The size of the page of results to return.</param>
		/// <param name="totalRecords">The total number of matched users.</param>
		/// <returns>
		/// A <see cref="T:System.Web.Security.MembershipUserCollection"/> collection that contains a page of <paramref name="pageSize"/><see cref="T:System.Web.Security.MembershipUser"/> objects beginning at the page specified by <paramref name="pageIndex"/>.
		/// </returns>
		public override MembershipUserCollection FindUsersByEmail (string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			DbParameter p;
			DbConnection cn = GetDBConnectionForMembership ();
			try {
				using (DbCommand cmd = cn.CreateCommand()) {
					cmd.CommandText = 
						"SELECT Count(*) " +
						"FROM   " + TbNames.User + " " +
						"WHERE  LoweredEmail LIKE :EmailSearch ";

					p = cmd.CreateParameter();
					p.ParameterName = ":EmailSearch";
					p.Value = emailToMatch.ToLowerInvariant();
					cmd.Parameters.Add(p);

					if (cn.State == ConnectionState.Closed){
						cn.Open();
					}

					totalRecords = Convert.ToInt32 (cmd.ExecuteScalar ());

					MembershipUserCollection users = new MembershipUserCollection ();

					if (totalRecords <= 0) {
						return users;
					}

					cmd.CommandText = 
						"SELECT UserId, Username, Email, PasswordQuestion,"
						+ " Comment, IsApproved, IsLockedOut, CreateDate, LastLoginDate,"
						+ " LastActivityDate, LastPasswordChangedDate, LastLockoutDate " +
							"FROM " + TbNames.User + " " +
							"WHERE LoweredEmail LIKE :EmailSearch "
						+ " ORDER BY Username Asc";

					using (DbDataReader dr = cmd.ExecuteReader()) {
						int counter = 0;
						int startIndex = pageSize * pageIndex;
						int endIndex = startIndex + pageSize - 1;

						while (dr.Read()) {
							if (counter >= startIndex) {
								MembershipUser u = GetUserFromReader (dr);
								users.Add (u);
							}

							if (counter >= endIndex) {
								cmd.Cancel ();
							}

							counter++;
						}
					}

					return users;
				}
			} finally {
				if (!IsTransactionInProgress ())
					cn.Dispose ();
			}
		}

		#endregion

		#region Private Methods

		private void ValidatePwdStrengthRegularExpression ()
		{
			// Validate regular expression, if supplied.
			if (_passwordStrengthRegularExpression == null)
				_passwordStrengthRegularExpression = String.Empty;

			_passwordStrengthRegularExpression = _passwordStrengthRegularExpression.Trim ();
			if (_passwordStrengthRegularExpression.Length > 0) {
				try {
					new Regex (_passwordStrengthRegularExpression);
				} catch (ArgumentException ex) {
					throw new ProviderException (ex.Message, ex);
				}
			}
		}

		private static string GetConfigValue (string configValue, string defaultValue)
		{
			// A helper function to retrieve config values from the configuration file.
			if (String.IsNullOrEmpty (configValue))
				return defaultValue;

			return configValue;
		}

		private MembershipUser GetUserFromReader (IDataRecord reader)
		{
			long providerUserKey = -1;
			string username = "";
			string email = "";
			string passwordQuestion = "";
			string comment = "";
			bool isApproved = true;
			bool isLockedOut = false;
			DateTime creationDate = DateTime.MinValue;
			DateTime lastLoginDate = DateTime.MinValue;
			DateTime lastActivityDate = DateTime.MinValue;
			DateTime lastPasswordChangedDate = DateTime.MinValue;
			DateTime lastLockedOutDate = DateTime.MinValue;

			for(var fld = 0; fld< reader.FieldCount; fld++){
				if(reader.IsDBNull(fld)){
					continue;
				}
				switch(reader.GetName(fld).ToLower()){
					case "userid":
						providerUserKey = reader.GetInt64(fld);
						break;
					case "username":
						username = reader.GetString(fld);
						break;
					case "email":
						email = reader.GetString(fld);
						break;
					case "passquestion":
						passwordQuestion = reader.GetString(fld);
						break;
					case "comment":
						comment = reader.GetString(fld);
						break;
					case "isapproved":
						isApproved = reader.GetBoolean(fld);
						break;
					case "lockedout":
						//given nulls are skipped, if we got here
						//there is a value, and they are locked out
						isLockedOut = true;
						//we also need to grab the value
						lastLockedOutDate = reader.GetDateTime(fld);
						break;
					case "createdate":
						creationDate = reader.GetDateTime(fld);
						break;
					case "lastlogindate":
						lastLoginDate = reader.GetDateTime(fld);
						break;
					case "lastactivitydate":
						lastActivityDate = reader.GetDateTime(fld);
						break;
					case "lastpasswordchangeddate":
						lastPasswordChangedDate = reader.GetDateTime (fld);
						break;
				}
			}

			MembershipUser user = new MembershipUser (
				this.Name,
				username,
				providerUserKey,
				email,
				passwordQuestion,
				comment,
				isApproved,
				isLockedOut,
				creationDate,
				lastLoginDate,
				lastActivityDate,
				lastPasswordChangedDate,
				lastLockedOutDate
			);

			return user;
		}

		private void UpdateFailureCount (string username, string failureType, bool isAuthenticated)
		{
			// A helper method that performs the checks and updates associated with password failure tracking.
			if (!((failureType == "password") || (failureType == "passwordAnswer"))) {
				throw new ArgumentException ("Invalid value for failureType parameter. Must be 'password' or 'passwordAnswer'.", "failureType");
			}

			IDbDataParameter p;
			Db.UseCommand( cmd => {
				int failedPasswordAttemptCount = 0;
				int failedPasswordAnswerAttemptCount = 0;
				DateTime failedPasswordAttemptWindowStart = _minDate;
				DateTime failedPasswordAnswerAttemptWindowStart = _minDate;
				bool isLockedOut = false;
				DateTime LockOutDate = _minDate;

				cmd.CommandText = 
					"SELECT FailedPasswordAttemptCount, \n" +
					"       FailedPasswordAttemptWindowStart, \n" +
					"       FailedPasswordAnswerAttemptCount, \n" +
					"       FailedPasswordAnswerAttemptWindowStart, \n" +
					"       LockedOut \n" +
					"FROM   " + TbNames.User + " \n" +
					"WHERE UsernameLowered = lower(:Username)";

				p = cmd.CreateParameter();
				p.ParameterName = ":Username";
				p.Value = username;
				cmd.Parameters.Add(p);

				using (var dr = cmd.ExecuteReader(CommandBehavior.SingleRow)) {
					if (dr.Read()) {
						for(var fld=0; fld<dr.FieldCount; fld++){
							if(dr.IsDBNull(fld)){
								continue;
							}
							switch(dr.GetName(fld).ToLower()){
								case "failedpasswordattemptcount":
									failedPasswordAttemptCount = dr.GetInt16(fld);
									break;
								case "failedpasswordattemptwindowstart":
									failedPasswordAttemptWindowStart = dr.GetDateTime (fld);
									break;
								case "failedpasswordanswerattemptcount":
									failedPasswordAnswerAttemptCount = dr.GetInt16(fld);
									break;
								case "failedpasswordanswerattemptwindowstart":
									failedPasswordAnswerAttemptWindowStart = dr.GetDateTime (fld);
									break;
								case "lockedout":
									isLockedOut = !dr.IsDBNull(fld);
									if(isLockedOut){
										LockOutDate = dr.GetDateTime(fld);
									}
									break;
							}
						}
					}
				}

				if (isLockedOut){
					// Just exit without updating any fields if user is 
					// locked out.
					return; 
				}

				cmd.CommandText = 
					"UPDATE " + TbNames.User + " \n" +
					"SET    FailedPasswordAttemptCount = :PassAttemptCount, \n" +
					"       FailedPasswordAttemptWindowStart = :PassAttemptStart, \n" +
					"       FailedPasswordAnswerAttemptCount = :AnswerAttemptCount, \n" +
					"       FailedPasswordAnswerAttemptWindowStart = :AnswerAttemptStart, \n" +
					"       LockedOut = :LockOut \n" +
					"WHERE  UsernameLowered = lower(:Username) \n";

				var pPassAttemptStart = p = cmd.CreateParameter();
				p.DbType = DbType.DateTime;
				p.ParameterName = ":PassAttemptStart";
				cmd.Parameters.Add(p);

				var pAnswerAttemptStart = p = cmd.CreateParameter();
				p.DbType = DbType.DateTime;
				p.ParameterName = ":AnswerAttemptStart";
				cmd.Parameters.Add(p);

				var pPassAttemptCount = p = cmd.CreateParameter();
				p.DbType = DbType.Int32;
				p.ParameterName = ":PassAttemptCount";
				cmd.Parameters.Add(p);

				var pAnswerAttemptCount = p = cmd.CreateParameter();
				p.DbType = DbType.Int32;
				p.ParameterName = ":AnswerAttemptCount";
				cmd.Parameters.Add(p);

				var pLockout = p = cmd.CreateParameter();
				p.DbType = DbType.DateTime;
				p.ParameterName = ":LockOut";
				cmd.Parameters.Add(p);

				//initialize all the values with whatever is currently in them
				pPassAttemptCount.Value = failedPasswordAttemptCount;
				pPassAttemptStart.Value = failedPasswordAttemptWindowStart;
				pAnswerAttemptCount.Value = failedPasswordAnswerAttemptCount;
				pAnswerAttemptStart.Value = failedPasswordAnswerAttemptWindowStart;
				pLockout.Value = isLockedOut?LockOutDate:_minDate;

				//if they are validly authenticated, we should reset
				//everything
				if (isAuthenticated) {
					pPassAttemptCount.Value = 0;
					pPassAttemptStart.Value = null;
					pAnswerAttemptCount.Value = 0;
					pAnswerAttemptStart.Value = DBNull.Value;
					pLockout.Value = null;
				}
				// If we get here that means isAuthenticated = false, which means the user did not log on successfully.
				// Log the failure and possibly lock out the user if she exceeded the number of allowed attempts.
				else {
					IDbDataParameter pAttemptCount=null;
					IDbDataParameter pAttemptStart=null;

					//determine the type of failure
					if (failureType == "password") {
						pAttemptCount = pPassAttemptCount;
						pAttemptStart = pPassAttemptStart;
					} else if (failureType == "passwordAnswer") {
						pAttemptCount = pAnswerAttemptCount;
						pAttemptStart = pAnswerAttemptStart;
					}
					DateTime windowEnd = 
						((DateTime)pAnswerAttemptStart.Value)
							.AddMinutes(PasswordAttemptWindow);
					if(DateTime.UtcNow > windowEnd){
						pAttemptCount.Value = 0;
					}

					//The have failed, so increment the failure count
					pAttemptCount.Value = ((int)pAttemptCount.Value)+1;
					//if its their first time
					if(((int)pAttemptCount.Value) ==1){
						pAttemptStart.Value = DateTime.UtcNow;
						pAnswerAttemptStart.Value = DateTime.UtcNow;
					} 
					//time to lock them out?
					else if(((int)pAttemptCount.Value)>=MaxInvalidPasswordAttempts){
						pLockout.Value = DateTime.UtcNow;
					}
				}

				//sanitize any db values (mostly watch for null markers)
				if((DateTime)pLockout.Value == _minDate){
					pLockout.Value = null;
				}
				//run it
				cmd.ExecuteNonQuery();
			});
		}

		private bool CheckPassword (string username, string password, bool failIfNotApproved =true)
		{
			bool isAuthenticated = false;

			Db.UseCommand(cmd => {
				cmd.CommandText = 
					"select count(*) \n" +
					"from   " + TbNames.User + " \n" +
					"where  UsernameLowered = lower(:username) and \n" +
					"       pass = crypt(:password,pass) and \n" +
					"       failedpasswordattemptcount < :maxfailedpass and \n" +
					"       (IsApproved or not :failnotapprove) \n";

				var p = cmd.CreateParameter();
				p.ParameterName = ":username";
				p.DbType = DbType.String;
				p.Value = username;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = ":password";
				p.DbType = DbType.String;
				p.Value = password;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = ":maxfailedpass";
				p.DbType = DbType.Int32;
				p.Value = MaxInvalidPasswordAttempts;
				cmd.Parameters.Add(p);

				p = cmd.CreateParameter();
				p.ParameterName = ":failnotapprove";
				p.DbType = DbType.Boolean;
				p.Value = failIfNotApproved;
				cmd.Parameters.Add(p);

				var count = Convert.ToInt32(cmd.ExecuteScalar());
				isAuthenticated = (1==count);
			});

			UpdateFailureCount (username, "password", isAuthenticated);
			return isAuthenticated;
		}

		/// <summary>
		/// Gets several pieces of information for a user from the database.
		/// </summary>
		/// <param name="username">The username to search for.</param>
		/// <param name="status">The return status of the method. Possible values are: 0 = User is found and not locked; 
		/// 1 = User not found; 99 = User is locked. These values match the return values of the corresponding method in 
		/// SqlMembershipProvider, so don't blame me for this goofy implementation.</param>
		/// <param name="password">The password as stored in the database. If it is stored encrypted, the encrypted value
		/// is returned. The calling method is responsible for decrypting it.</param>
		/// <param name="passwordFormat">The password format as stored in the database. Possible values: Clear, Hashed, Encrypted.</param>
		/// <param name="passwordSalt">The password salt as stored in the database.</param>
		/// <param name="failedPasswordAttemptCount">The failed password attempt count as stored in the database.</param>
		/// <param name="failedPasswordAnswerAttemptCount">The failed password answer attempt count as stored in the database.</param>
		/// <param name="isApproved">if set to <c>true</c> the user is approved (not locked out).</param>
		/// <param name="lastLoginDate">The last login date.</param>
		/// <param name="lastActivityDate">The last activity date.</param>
		private static void GetPasswordWithFormat (string username, out int status, out string password, out MembershipPasswordFormat passwordFormat, out string passwordSalt, out int failedPasswordAttemptCount, out int failedPasswordAnswerAttemptCount, out bool isApproved, out DateTime lastLoginDate, out DateTime lastActivityDate)
		{
			DbParameter p;
			DbConnection cn = GetDBConnectionForMembership ();
			try {
				using (DbCommand cmd = cn.CreateCommand()) {
					cmd.CommandText = 
						"SELECT Password, PasswordFormat, PasswordSalt, FailedPasswordAttemptCount," +
						"       FailedPasswordAnswerAttemptCount, IsApproved, IsLockedOut, LastLoginDate, LastActivityDate" +
						"FROM   " + TbNames.User + " " +
						"WHERE LoweredUsername = :Username";

					p = cmd.CreateParameter();
					p.ParameterName = ":Username";
					p.Value = username.ToLowerInvariant();
					cmd.Parameters.Add(p);

					if (cn.State == ConnectionState.Closed){
						cn.Open();
					}

					using (DbDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleRow)) {
						if (dr.HasRows) {
							dr.Read ();

							password = dr.GetString (0);
							passwordFormat = (MembershipPasswordFormat)Enum.Parse (typeof(MembershipPasswordFormat), dr.GetString (1));
							passwordSalt = dr.GetString (2);
							failedPasswordAttemptCount = dr.GetInt32 (3);
							failedPasswordAnswerAttemptCount = dr.GetInt32 (4);
							isApproved = dr.GetBoolean (5);
							status = dr.GetBoolean (6) ? 99 : 0; // 99 = User is locked; 0 = User is found & not locked
							lastLoginDate = dr.GetDateTime (7);
							lastActivityDate = dr.GetDateTime (8);
						} else {
							status = 1; // User not found
							password = null;
							passwordFormat = MembershipPasswordFormat.Clear;
							passwordSalt = null;
							failedPasswordAttemptCount = 0;
							failedPasswordAnswerAttemptCount = 0;
							isApproved = false;
							lastLoginDate = DateTime.UtcNow;
							lastActivityDate = DateTime.UtcNow;
						}
					}
				}
			} finally {
				if (!IsTransactionInProgress ())
					cn.Dispose ();
			}
		}

		private static bool IsStatusDueToBadPassword (int status)
		{
			return (((status >= 2) && (status <= 6)) || (status == 99));
		}

		private static string GetExceptionText (int status)
		{
			string exceptionText;
			switch (status) {
			case 0:
				return string.Empty;

			case 1:
				exceptionText = "The user was not found.";
				break;

			case 2:
				exceptionText = "The password supplied is wrong.";
				break;

			case 3:
				exceptionText = "The password-answer supplied is wrong.";
				break;

			case 4:
				exceptionText = "The password supplied is invalid.  Passwords must conform to the password strength requirements configured for the default provider.";
				break;

			case 5:
				exceptionText = "The password-question supplied is invalid.  Note that the current provider configuration requires a valid password question and answer.  As a result, a CreateUser overload that accepts question and answer parameters must also be used.";
				break;

			case 6:
				exceptionText = "The password-answer supplied is invalid.";
				break;

			case 7:
				exceptionText = "The E-mail supplied is invalid.";
				break;

			case 99:
				exceptionText = "The user account has been locked out.";
				break;

			default:
				exceptionText = "The Provider encountered an unknown error.";
				break;
			}
			return exceptionText;
		}

		/// <summary>
		/// Get a reference to the database connection used for membership. If a transaction is currently in progress, and the
		/// connection string of the transaction connection is the same as the connection string for the membership provider,
		/// then the connection associated with the transaction is returned, and it will already be open. If no transaction is in progress,
		/// a new <see cref="DbConnection"/> is created and returned. It will be closed and must be opened by the caller
		/// before using.
		/// </summary>
		/// <returns>A <see cref="DbConnection"/> object.</returns>
		/// <remarks>The transaction is stored in <see cref="System.Web.HttpContext.Current"/>. That means transaction support is limited
		/// to web applications. For other types of applications, there is no transaction support unless this code is modified.</remarks>
		//[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		private static DbConnection GetDBConnectionForMembership ()
		{
			return Db.GetConnection();
		}

		/// <summary>
		/// Determines whether a database transaction is in progress for the Membership provider.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if a database transaction is in progress; otherwise, <c>false</c>.
		/// </returns>
		/// <remarks>A transaction is considered in progress if an instance of <see cref="DbTransaction"/> is found in the
		/// <see cref="System.Web.HttpContext.Current"/> Items property and its connection string is equal to the Membership 
		/// provider's connection string. Note that this implementation of <see cref="SqliteMembershipProvider"/> never adds a 
		/// <see cref="DbTransaction"/> to <see cref="System.Web.HttpContext.Current"/>, but it is possible that 
		/// another data provider in this application does. This may be because other data is also stored in this Sqlite database,
		/// and the application author wants to provide transaction support across the individual providers. If an instance of
		/// <see cref="System.Web.HttpContext.Current"/> does not exist (for example, if the calling application is not a web application),
		/// this method always returns false.</remarks>
		private static bool IsTransactionInProgress ()
		{
			return Db.IsTransactionInProgress();
		}

		#endregion
	}

	/// <summary>
	/// Provides general purpose validation functionality.
	/// </summary>
	internal class SecUtility
	{
		/// <summary>
		/// Checks the parameter and throws an exception if one or more rules are violated.
		/// </summary>
		/// <param name="param">The parameter to check.</param>
		/// <param name="checkForNull">When <c>true</c>, verify <paramref name="param"/> is not null.</param>
		/// <param name="checkIfEmpty">When <c>true</c> verify <paramref name="param"/> is not an empty string.</param>
		/// <param name="checkForCommas">When <c>true</c> verify <paramref name="param"/> does not contain a comma.</param>
		/// <param name="maxSize">The maximum allowed length of <paramref name="param"/>.</param>
		/// <param name="paramName">Name of the parameter to check. This is passed to the exception if one is thrown.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="param"/> is null and <paramref name="checkForNull"/> is true.</exception>
		/// <exception cref="ArgumentException">Thrown if <paramref name="param"/> does not satisfy one of the remaining requirements.</exception>
		/// <remarks>This method performs the same implementation as Microsoft's version at System.Web.Util.SecUtility.</remarks>
		internal static void CheckParameter (ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize, string paramName)
		{
			if (param == null) {
				if (checkForNull) {
					throw new ArgumentNullException (paramName);
				}
			} else {
				param = param.Trim ();
				if (checkIfEmpty && (param.Length < 1)) {
					throw new ArgumentException (String.Format ("The parameter '{0}' must not be empty.", paramName), paramName);
				}
				if ((maxSize > 0) && (param.Length > maxSize)) {
					throw new ArgumentException (String.Format ("The parameter '{0}' is too long: it must not exceed {1} chars in length.", paramName, maxSize.ToString (CultureInfo.InvariantCulture)), paramName);
				}
				if (checkForCommas && param.Contains (",")) {
					throw new ArgumentException (String.Format ("The parameter '{0}' must not contain commas.", paramName), paramName);
				}
			}
		}

		/// <summary>
		/// Verifies that <paramref name="param"/> conforms to all requirements.
		/// </summary>
		/// <param name="param">The parameter to check.</param>
		/// <param name="checkForNull">When <c>true</c>, verify <paramref name="param"/> is not null.</param>
		/// <param name="checkIfEmpty">When <c>true</c> verify <paramref name="param"/> is not an empty string.</param>
		/// <param name="checkForCommas">When <c>true</c> verify <paramref name="param"/> does not contain a comma.</param>
		/// <param name="maxSize">The maximum allowed length of <paramref name="param"/>.</param>
		/// <returns>Returns <c>true</c> if all requirements are met; otherwise returns <c>false</c>.</returns>
		internal static bool ValidateParameter (ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize)
		{
			if (param == null) {
				return !checkForNull;
			}
			param = param.Trim ();
			return (((!checkIfEmpty || (param.Length >= 1)) && ((maxSize <= 0) || (param.Length <= maxSize))) && (!checkForCommas || !param.Contains (",")));
		}

	}

}