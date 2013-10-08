using System;
using System.Web;
using System.Data;
using System.Data.Common;
using System.Configuration;

namespace Vius.Data
{
	public class DataProvider
	{
		private object locker = new object();

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
		protected DbProviderFactory Factory {
			get {
				lock(locker){
					if(factory == null){
						var name = ParsedConnectionString["provider"];
						//var type = Type.GetType(name);
						var type = typeof(Mono.Data.Sqlite.SqliteFactory);
						factory = Activator.CreateInstance(type) as DbProviderFactory;
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
				string errsql="";
				using (var trans = cmd.Connection.BeginTransaction()) {
					try {
						foreach (var sql in Commands) {
							errsql = sql;
							cmd.CommandText = sql;
							cmd.ExecuteNonQuery();
						}
						errsql = "";
						trans.Commit();
					} catch (System.Data.Common.DbException ex) {
						trans.Rollback();
						Console.Out.WriteLine("Invalid SQL Command:");
						Console.Out.WriteLine(errsql);
						Console.Out.WriteLine(ex.Message);
						Console.Out.WriteLine(ex.StackTrace);
						throw ex;
					}
				}
			}
			);
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

	}
}
