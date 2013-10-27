using System;
using System.Web;
using System.Data;
using System.Data.Common;
using System.Configuration;

namespace Vius.Data
{
	/// <summary>
	/// Data provider.
	/// </summary>
	/// <remarks>
	/// $Id$
	/// $URL$
	/// </remarks>
	public class DataProvider
	{
		private object locker = new object();
		private static DbTransaction tran = null;

		#region Singleton
		private static object staticLocker = new object();
		private static DataProvider instance = null;
		public static DataProvider Instance {
			get {
				lock (staticLocker) {
					if (instance == null) {
						instance = new DataProvider();
					}
					return instance;
				}
			}
		}
		#endregion

		public delegate void CommandUsage(IDbCommand cmd);

		private ConnectionStringSettings _settings = new ConnectionStringSettings("Vius","","Npgsql");

		/// <summary>Gets the connection string.</summary>
		/// <value>The connection string.</value>
		public string ConnectionString {
			get {
				lock(_settings){
					if(string.IsNullOrEmpty(_settings.ConnectionString)){
						_settings = ConfigurationManager.ConnectionStrings["Vius"];
						if (_settings==null || string.IsNullOrEmpty(_settings.ConnectionString)) {
							throw new Exception("Connection string is empty for 'Vius'. Check the configuration file (web.config).");
						}
					}
					return _settings.ConnectionString;
				}
			}
			set{
				_settings.ConnectionString = value;
			}
		}

		/// <summary>
		/// Gets the connection.
		/// </summary>
		/// <value>
		/// The connection.
		/// </value>
		public DbConnection GetConnection ()
		{
			var cnn = Factory.CreateConnection();
			cnn.ConnectionString = this.ConnectionString;
			return cnn;
		}

		private QuickDict parsedConnectionString = null;
		protected QuickDict ParsedConnectionString {
			get {
				lock(locker){
					if(parsedConnectionString == null){
						parsedConnectionString = new QuickDict();
						string[] elems = ConnectionString.Split(";".ToCharArray());
						foreach (var elem in elems) {
							if(!string.IsNullOrEmpty(elem)){
								var pair = elem.Split("=".ToCharArray());
								parsedConnectionString.Add(pair[0].ToLower(), pair[1]);
							}
						}
					}
					return parsedConnectionString;
				}
			}
		}

		private DbProviderFactory factory = null;
		public DbProviderFactory Factory {
			get {
				lock(locker){
					if(factory == null){
						factory = System.Data.Common.DbProviderFactories.GetFactory(_settings.ProviderName);
					}
					return factory;
				}
			}
		}

		/// <summary>
		/// Uses the command.
		/// </summary>
		/// <param name='usage'>
		/// Usage.
		/// </param>
		public void UseCommand (CommandUsage usage)
		{
			string sql = "";
			//get the connection
			using (var cnn = GetConnection()) {
				cnn.Open();
				//start a transaction
				using(var tran = cnn.BeginTransaction()){
					try{
						//run the users commands
						using (var cmd = cnn.CreateCommand()) {
							try{
								usage(cmd);
							} finally{
								sql = cmd.CommandText;
							}
						}
						//commit the transaction
						tran.Commit();
					} catch (Exception ex){
						//failure == rollback
						tran.Rollback();
						var e = new Exception(
							ex.Message + "\n" + sql,
							ex);
						throw e;
					}
				}
				//close out
				cnn.Close();
			}
		}

		public void ExecuteCommand (string Command)
		{
			var cmds = new System.Collections.Generic.List<string>();
			cmds.Add(Command);
			ExecuteCommand(cmds);
		}


		public void ExecuteCommand (System.Collections.Generic.IEnumerable<string> Commands)
		{
			UseCommand(cmd => {
				string errsql="";
				try {
					foreach (var sql in Commands) {
						errsql = sql;
						cmd.CommandText = sql;
						cmd.ExecuteNonQuery();
					}
					errsql = "";
				} catch (System.Data.Common.DbException ex) {
					var e = new Exception(
						ex.Message + "\n" + errsql,
						ex);
					throw e;
				}
			});
		}

		public bool TableExists (string tablename)
		{
			var illegalchar = "\"'.,";
			foreach (var ch in illegalchar.ToCharArray()) {
				tablename = tablename.Replace(ch.ToString(), "");
			}
			bool doesexist = false;
			this.UseCommand(cmd => {
				cmd.CommandText = "select * from " + tablename + " where 1=2";
				try{
					using(var rs = cmd.ExecuteReader(CommandBehavior.SingleRow)){
						rs.Read();
					}
					doesexist = true;
				} catch {
					doesexist = false;
				}
			});
			return doesexist;

		}

		public bool IsTransactionInProgress ()
		{
			if (System.Web.HttpContext.Current != null){
				tran = (DbTransaction)System.Web.HttpContext.Current.Items[_settings.ToString()];
			}


			if ((tran != null) && String.Equals(tran.Connection.ConnectionString, this.ConnectionString)) {
				return true;
			} else {
				return false;
			}
		}

	}
}
