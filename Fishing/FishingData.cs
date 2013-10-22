using System;

namespace Vius.Fishing.Data
{
	public class FishingData
	{
		private static object staticLocker = new object();
		private object locker = new object();

		private static FishingData instance;
		public static FishingData Instance {
			get {
				if(instance == null){
					lock(staticLocker){
						if(instance == null){
							instance = new FishingData();
						}
					}
				}
				return instance;
			}
		}

		#region Initialization
		protected FishingData ()
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

