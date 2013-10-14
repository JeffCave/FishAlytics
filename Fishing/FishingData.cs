using System;

namespace Vius.Fishing.Data
{
	public class FishingData
	{
		public static object staticLocker = new object();
		public static FishingData instance;
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

		protected FishingData ()
		{
			catches = new Vius.Fishing.Data.Catches();
		}

		private Vius.Fishing.Data.Catches catches = null;
		public Vius.Fishing.Data.Catches Catches {
			get {
				return catches;
			}
		}
	}
}

