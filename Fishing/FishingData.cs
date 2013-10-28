using System;
using System.Data.Linq;

namespace Vius.Fishing.Data
{
	public class FishingData
	{
		public Table<Trip> Trips2;

		private static object staticLocker = new object();
		private object locker = new object();

		private static FishingData instance;
		public static FishingData Instance {
			get {
				if(instance == null){
					lock(staticLocker){
						if(instance == null){
							try{
								string cnnstr = Vius.Data.DataProvider.Instance.ConnectionString;
								cnnstr += ";DbLinqProvider=PostgreSql";
								instance = new FishingData();
							} catch(Exception e) {
								System.Console.Out.WriteLine(e.Message);
							}
						}
					}
				}
				return instance;
			}
		}

		#region Initialization
		protected FishingData ()
//			:base("DbLinqProvider=PostgreSql;" + Vius.Data.DataProvider.Instance.ConnectionString)
		{
		}

		protected FishingData(string connection)
//			:base(connection)
		{
		}
		#endregion

		private Vius.Fishing.Data.Catches catches = null;
		public Vius.Fishing.Data.Catches Catches {
			get {
				if(catches == null){
					lock(locker){
						if(catches == null){
							catches = new Vius.Fishing.Data.Catches();
						}
					}
				}
				return catches;
			}
		}

		private Vius.Fishing.Data.Trips trips = null;
		public Vius.Fishing.Data.Trips Trips {
			get {
				if(trips == null){
					lock(locker){
						if(trips == null){
							trips = new Vius.Fishing.Data.Trips();
						}
					}
				}
				return trips;
			}
		}

	}
}

