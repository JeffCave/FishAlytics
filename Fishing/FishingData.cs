using System;
using System.Collections.Generic;
using DbLinq.Util;

using DbLinq.PostgreSql;
using System.Data.Linq;

namespace Vius.Fishing.Data
{
	public class FishingData:System.Data.Linq.DataContext
	{
		public Table<Trip> Trips{ get{return this.GetTable<Trip>();}}
		public Table<Catch> Catches{ get{return this.GetTable<Catch>();}}

		private static object staticLocker = new object();

		private static FishingData instance;
		public static FishingData Instance {
			get {
				lock(staticLocker){
					if(instance == null){
						instance = GetContext();
					}
				}
				return instance;
			}
		}

		public static FishingData GetContext(){
			FishingData inst = null;
			try{
				var cnn = Vius.Data.DataProvider.Instance.GetConnection();
				//cnn.ConnectionString += ";DbLinqProvider=Npgsql";
				var cnnstr = 
					"DbLinqProvider=PostgreSql;"
					+ "DbLinqConnectionType=Npgsql.NpgsqlConnection, Npgsql;"
					+ cnn.ConnectionString
					;
				inst = new FishingData(cnnstr);
			} catch(Exception e) {
				System.Console.Out.WriteLine(e.Message);
				System.Console.Out.WriteLine(e.StackTrace);
			}
			return inst;
		}

		public FishingData(string cnnstr):base(cnnstr)
		{
		}

	}
}

