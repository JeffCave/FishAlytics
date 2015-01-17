using System;
using System.Collections.Generic;
using System.Data.Linq;

namespace Vius.Fishing.Data
{
	public class FishingData:DataContext
	{
		public Table<Trip> Trips{ get; set;}
		public Table<Catch> Catches{ get;  set;}

		private static object staticLocker = new object();

		private static FishingData instance;
		public static FishingData Instance {
			get {
				lock(staticLocker){
					if(instance == null){
						try{
							var cnn = Vius.Data.DataProvider.Instance.GetConnection();
							instance = new FishingData(cnn);
						} catch(Exception e) {
							System.Console.Out.WriteLine(e.Message);
							System.Console.Out.WriteLine(e.StackTrace);
						}
					}
				}
				return instance;
			}
		}

		protected FishingData(System.Data.IDbConnection cnn)
			:base(cnn){
			Trips = this.GetTable<Trip>();
			Catches = this.GetTable<Catch>();
		}

	}
}

