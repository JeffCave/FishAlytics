using System;
using System.Web;
using System.Data;
using System.Data.Common;
using System.Configuration;

namespace Vius.Data
{
	public class DataProvider
	{
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

		private string pConnectionString = "";

		/// <summary>Gets the connection string.</summary>
		/// <value>The connection string.</value>
		public string ConnectionString {
			get {
				lock(pConnectionString){
					if(string.IsNullOrEmpty(pConnectionString)){
						ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["Vius"];
						if (settings==null || string.IsNullOrEmpty(settings.ConnectionString)) {
							throw new Exception("Connection string is empty for 'Vius'. Check the configuration file (web.config).");
						}
						pConnectionString = settings.ConnectionString;
					}
					return pConnectionString;
				}
			}
			set{
				pConnectionString = value;
			}
		}

		/// <summary>
		/// Gets the connection.
		/// </summary>
		/// <value>
		/// The connection.
		/// </value>
		private IDbConnection GetConnection ()
		{
			var cnn = Factory.CreateConnection();
			cnn.ConnectionString = this.ConnectionString;
			return cnn;
		}

		private System.Collections.Generic.Dictionary<string,string> ParsedConnectionString {
			get {
				QuickDict dict = new QuickDict();
				foreach (var elem in ConnectionString.Split(";".ToCharArray())) {
					var pair = elem.Split("=".ToCharArray());
					dict.Add(pair[0], pair[1]);
				}
				return dict;
			}
		}

		private DbProviderFactory Factory {
			get {
				DbProviderFactory factory;

				factory = Activator.CreateInstance(Type.GetType(ParsedConnectionString["Provider"])) as DbProviderFactory;
				return factory;
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
			using (var cnn = GetConnection()) {
				cnn.Open();
				using (var cmd = cnn.CreateCommand()) {
					usage(cmd);
				}
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
				using (var trans = cmd.Connection.BeginTransaction()) {
					try {
						foreach (var sql in Commands) {
							cmd.CommandText = sql;
							cmd.ExecuteNonQuery();
						}
						trans.Commit();
					} catch (Exception ex) {
						trans.Rollback();
						throw ex;
					}
				}
			}
			);
		}
	}
}
